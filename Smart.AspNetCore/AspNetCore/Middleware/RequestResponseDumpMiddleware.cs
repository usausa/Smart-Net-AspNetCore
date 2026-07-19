namespace Smart.AspNetCore.Middleware;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class RequestResponseDumpMiddleware
{
    private static readonly Encoding TextEncoding = Encoding.UTF8;

    private readonly RequestDelegate next;

    private readonly ILogger<RequestResponseDumpMiddleware> log;

    private readonly RequestResponseDumpOptions options;

    public RequestResponseDumpMiddleware(RequestDelegate next, ILogger<RequestResponseDumpMiddleware> log, IOptions<RequestResponseDumpOptions> options)
    {
        this.next = next;
        this.log = log;
        this.options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        if (log.IsEnabled(LogLevel.Debug))
        {
            if (IsDumpTarget(context.Request.ContentType))
            {
                await DumpRequestAsync(context.Request).ConfigureAwait(false);
            }

            var originalBodyStream = context.Response.Body;
            var captureStream = new CaptureStream(originalBodyStream, options.MaxDumpBytes);
            context.Response.Body = captureStream;

            try
            {
                await next(context).ConfigureAwait(false);

                if (IsDumpTarget(context.Response.ContentType))
                {
                    var captured = captureStream.Captured;
                    if (captured.Length > 0)
                    {
                        log.DebugResponseDump(TextEncoding.GetString(captured));
                    }
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
                await captureStream.DisposeAsync().ConfigureAwait(false);
            }
        }
        else
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private async ValueTask DumpRequestAsync(HttpRequest request)
    {
        request.EnableBuffering();

        var maxBytes = options.MaxDumpBytes;
        if (maxBytes <= 0)
        {
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(maxBytes);
        try
        {
            var read = 0;
            while (read < maxBytes)
            {
                var count = await request.Body.ReadAsync(buffer.AsMemory(read, maxBytes - read)).ConfigureAwait(false);
                if (count == 0)
                {
                    break;
                }

                read += count;
            }

            request.Body.Seek(0, SeekOrigin.Begin);

            if ((read > 0) && log.IsEnabled(LogLevel.Debug))
            {
                log.DebugRequestDump(TextEncoding.GetString(buffer.AsSpan(0, read)));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private bool IsDumpTarget(string? contentType)
    {
        if (String.IsNullOrEmpty(contentType))
        {
            return false;
        }

        foreach (var targetType in options.TargetTypes)
        {
            if (contentType.StartsWith(targetType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class CaptureStream : Stream
    {
#pragma warning disable CA2213
        private readonly Stream inner;
#pragma warning restore CA2213

        private readonly byte[] head;

        private readonly int capacity;

        private int captured;

        private long written;

        private bool disposed;

        public CaptureStream(Stream inner, int maxBytes)
        {
            this.inner = inner;
            capacity = maxBytes > 0 ? maxBytes : 0;
            head = capacity > 0 ? ArrayPool<byte>.Shared.Rent(capacity) : [];
        }

        public ReadOnlySpan<byte> Captured => head.AsSpan(0, captured);

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => written;

        public override long Position
        {
            get => written;
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);

        public override void Write(byte[] buffer, int offset, int count)
        {
            Capture(buffer.AsSpan(offset, count));
            inner.Write(buffer, offset, count);
            written += count;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Capture(buffer);
            inner.Write(buffer);
            written += buffer.Length;
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Capture(buffer.Span);
            written += buffer.Length;
            return inner.WriteAsync(buffer, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        private void Capture(ReadOnlySpan<byte> data)
        {
            if (captured >= capacity)
            {
                return;
            }

            var count = Math.Min(capacity - captured, data.Length);
            data[..count].CopyTo(head.AsSpan(captured));
            captured += count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                if (capacity > 0)
                {
                    ArrayPool<byte>.Shared.Return(head);
                }
            }

            base.Dispose(disposing);
        }
    }
}

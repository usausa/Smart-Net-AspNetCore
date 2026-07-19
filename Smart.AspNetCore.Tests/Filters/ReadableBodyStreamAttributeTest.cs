namespace Smart.AspNetCore.Filters;

using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public sealed class ReadableBodyStreamAttributeTest
{
    [Fact]
    public void CreateInstanceReturnsReusableReadableBodyStreamFilter()
    {
        // Arrange
        var attribute = new ReadableBodyStreamAttribute();
        using var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var filter = attribute.CreateInstance(services);

        // Assert
        Assert.True(attribute.IsReusable);
        Assert.IsType<ReadableBodyStreamAttribute.ReadableBodyStreamFilter>(filter);
    }

    [Fact]
    public async Task OnAuthorizationEnablesBufferingSoBodyCanBeReadFromStartAgain()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var payload = "sample body content"u8.ToArray();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Body = new NonSeekableStream(new MemoryStream(payload)),
                ContentLength = payload.Length
            }
        };

        var filterContext = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            []);
        var filter = new ReadableBodyStreamAttribute.ReadableBodyStreamFilter();

        Assert.False(httpContext.Request.Body.CanSeek);

        // Act
        filter.OnAuthorization(filterContext);

        // Assert
        Assert.True(httpContext.Request.Body.CanSeek);

        var first = await ReadAllAsync(httpContext.Request.Body, ct);
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        var second = await ReadAllAsync(httpContext.Request.Body, ct);

        Assert.Equal(payload, first);
        Assert.Equal(first, second);
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        return memory.ToArray();
    }

    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream inner;

        public NonSeekableStream(Stream inner)
        {
            this.inner = inner;
        }

        public override bool CanRead => inner.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            inner.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

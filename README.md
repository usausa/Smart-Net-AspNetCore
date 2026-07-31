# Smart.AspNetCore .NET - ASP.NET Core support library

[![NuGet](https://img.shields.io/nuget/v/Usa.Smart.AspNetCore.svg)](https://www.nuget.org/packages/Usa.Smart.AspNetCore)

## Features

* Action constraints (`AjaxOnlyAttribute`, `FormParameterAttribute`, `QueryParameterAttribute`)
* Data annotations (`AnyRequiredAttribute`, `RequiredWhenAttribute`, `CompareToAttribute`, `DuplicateAttribute`, `ElementRequiredAttribute`)
* Filters (`TimeLoggingFilter`, `AjaxModelStateAttribute`, `ReadableBodyStreamAttribute`)
* Routing (`SubAreaAttribute`, `ConvertAttribute`)
* Application model conventions (kebab-case and lowercase controller naming)
* Tag helpers (`ConditionTagHelper`, `RequiredIfTagHelper`)
* Select list rendering (`SelectListBuilder`, `ToSelectList`, `WithEmpty`, `GetDisplayName`)
* Model binding (`LocalDateTimeModelBinder`)
* Middleware (`RequestResponseDumpMiddleware`)
* Mvc results (`DeletePhysicalFileResult`, `PushStreamResult`)
* JSON converters (`DateTimeFormatConverter`, `StrictEnumConverter`)
* Binder source generator (`BindAttribute`, `DefaultStringConverter`)

## Binder source generator

`[Bind]` generates the implementation of a partial method that binds a source collection to a target type.

```csharp
internal static partial class RequestBinder
{
    [Bind]
    public static partial SearchRequest BindSearch(IQueryCollection query);
}
```

Three shapes are supported:

```csharp
[Bind] public static partial T Bind(IQueryCollection query);              // creates the instance
[Bind] public static partial T Bind(IQueryCollection query, T target);   // binds to an existing instance and returns it
[Bind] public static partial void Bind(IQueryCollection query, T target); // binds to an existing instance
```

Supported sources are `IQueryCollection`, `IFormCollection`, `IHeaderDictionary`, and
`Dictionary<string, string>` / `Dictionary<string, StringValues>` (including the `IDictionary` and
`IReadOnlyDictionary` variants).

### Conversion

Values are converted by `DefaultStringConverter` using `InvariantCulture`, so binding does not depend
on the server locale. By default a conversion failure falls back to the type default. Set `Strict` to
reject invalid input instead:

```csharp
[Bind(Strict = true)]
public static partial SearchRequest BindSearch(IQueryCollection query);
```

With `Strict`, a failed conversion throws `FormatException` instead of silently producing a default
value, which lets invalid input be distinguished from a legitimate default. `DefaultStringConverter`
exposes both forms directly as well: `ToInt32(span)` (lenient) and `TryToInt32(span, out var value)`
(strict).

A custom converter can be supplied with `[BindConverter(typeof(...))]`. Custom converters are not
affected by `Strict`, because they return the converted value directly and cannot report failure.

### Diagnostics

| Id | Description |
| --- | --- |
| SAN0001 | Method must be static partial |
| SAN0002 | Method must have one supported string collection parameter |
| SAN0003 | Property has no available converter and is not bound |
| SAN0004 | Containing type must be partial |
| SAN0005 | Containing type must be a top-level type |
| SAN0006 | Target type must not be abstract |
| SAN0007 | Target type requires an accessible parameterless constructor |
| SAN0008 | Bind method must not be generic |

## RequestResponseDumpMiddleware

Writes request and response bodies to the log. The middleware is completely inert unless the `Debug`
log level is enabled, and only content types listed in `RequestResponseDumpOptions.TargetTypes` are
dumped. Only the first `MaxDumpBytes` bytes are captured; the response is streamed through to the
original body, so large responses are not buffered in memory.

> **Do not enable the `Debug` level for this middleware in production.** Request bodies frequently
> contain passwords, tokens, and personal data, and dumping them writes those values to the log in
> plain text. This feature is intended for development use only.

## Notes

`RouteValues` uses reflection and dynamic delegate creation. It is annotated with
`[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` and is not compatible with trimming or
Native AOT.

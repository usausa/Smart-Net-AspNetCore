# Smart.AspNetCore .NET - ASP.NET Core support library

[![NuGet](https://img.shields.io/nuget/v/Usa.Smart.AspNetCore.svg)](https://www.nuget.org/packages/Usa.Smart.AspNetCore)

## Features

* Action constraints (`AjaxOnlyAttribute`, `FormParameterAttribute`, `QueryParameterAttribute`)
* Data annotations (`AnyRequiredAttribute`, `RequiredWhenAttribute`, `CompareToAttribute`, `DuplicateAttribute`, `ElementRequiredAttribute`)
* Filters (`TimeLoggingFilter`, `AjaxModelStateAttribute`, `ReadableBodyStreamAttribute`)
* Routing (`SubAreaAttribute`, `ConvertAttribute`)
* Application model conventions (kebab-case and lowercase controller naming)
* Tag helpers (`ConditionTagHelper`, `RequiredIfTagHelper`)
* Model binding (`LocalDateTimeModelBinder`)
* Middleware (`RequestResponseDumpMiddleware`)
* Mvc results (`DeletePhysicalFileResult`, `PushStreamResult`)
* JSON converters (`DateTimeFormatConverter`, `StrictEnumConverter`)

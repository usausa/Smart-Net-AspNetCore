# Diagnostics

| ID | Severity | Description | How to fix |
|---|---|---|---|
| SAN0001 | ❌ Error | Bind method is not `static partial` | Declare the method as `static partial` |
| SAN0002 | ❌ Error | Bind method does not take exactly one supported string collection parameter | Take a single supported string collection parameter |
| SAN0003 | ⚠️ Warning | Property has no available converter and is silently skipped | Provide a converter, or exclude the property from binding |
| SAN0004 | ❌ Error | Type containing the bind method is not declared partial | Declare the containing type as `partial` |
| SAN0005 | ❌ Error | Type containing the bind method is a nested type | Move the containing type to the top level |
| SAN0006 | ❌ Error | Bind method creates the target instance, but the target type is abstract | Use a non-abstract target type |
| SAN0007 | ❌ Error | Bind method creates the target instance, but the target type has no accessible parameterless constructor | Add an accessible parameterless constructor to the target type |
| SAN0008 | ❌ Error | Bind method has type parameters | Remove the type parameters from the bind method |

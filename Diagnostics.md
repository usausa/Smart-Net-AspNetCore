# Diagnostics

| ID | Severity | Description | How to fix |
|---|---|---|---|
| SAN0001 | ❌ Error | `[Bind]` method is not `static partial`, or has an implementation written | Declare the method as `static partial` without an implementation |
| SAN0002 | ❌ Error | `[Bind]` method does not take exactly one supported string collection parameter | Take a single supported string collection parameter |
| SAN0003 | ⚠️ Warning | Property has no available converter and is silently skipped | Provide a converter, or exclude the property from binding |
| SAN0004 | ❌ Error | Type containing the `[Bind]` method is not declared partial | Declare the containing type as `partial` |
| SAN0005 | ❌ Error | Type containing the `[Bind]` method is a nested or file-local type | Move the containing type to the top level, and do not declare it `file` |
| SAN0006 | ❌ Error | Bind method creates the target instance, but the target type is abstract | Use a non-abstract target type |
| SAN0007 | ❌ Error | Bind method creates the target instance, but the target type has no accessible parameterless constructor, or has required members | Add an accessible parameterless constructor (with `[SetsRequiredMembers]` when the type has required members) |
| SAN0008 | ❌ Error | `[Bind]` method has type parameters | Remove the type parameters from the bind method |
| SAN0009 | ❌ Error | Type name differs only in case from another type containing `[Bind]` methods, so the generated file names collide; only the first type (in ordinal order) is generated | Rename one of the types |

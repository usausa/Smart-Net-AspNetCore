namespace Smart.AspNetCore.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

public sealed class LowercaseControllerModelConvention : IControllerModelConvention
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Ignore")]
    public void Apply(ControllerModel controller)
    {
        controller.ControllerName = controller.ControllerName.ToLowerInvariant();
        foreach (var action in controller.Actions)
        {
            action.ActionName = action.ActionName.ToLowerInvariant();
        }
    }
}

namespace Smart.AspNetCore.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

public sealed class LowercaseControllerModelConvention : IControllerModelConvention
{
#pragma warning disable CA1308
    public void Apply(ControllerModel controller)
    {
        controller.ControllerName = controller.ControllerName.ToLowerInvariant();
        foreach (var action in controller.Actions)
        {
            action.ActionName = action.ActionName.ToLowerInvariant();
        }
    }
#pragma warning restore CA1308
}

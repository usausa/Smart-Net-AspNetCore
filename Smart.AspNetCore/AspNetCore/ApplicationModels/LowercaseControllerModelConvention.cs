namespace Smart.AspNetCore.ApplicationModels
{
    using System.Globalization;

    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public sealed class LowercaseControllerModelConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            controller.ControllerName = controller.ControllerName.ToLowerInvariant();
            foreach (var action in controller.Actions)
            {
                action.ActionName = action.ActionName.ToLowerInvariant();
            }
        }
    }
}

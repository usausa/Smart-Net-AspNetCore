namespace Smart.AspNetCore.ApplicationModels
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public sealed class LowercaseControllerModelConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            controller.ControllerName = controller.ControllerName.ToLower();
            foreach (var action in controller.Actions)
            {
                action.ActionName = action.ActionName.ToLower();
            }
        }
    }
}

namespace Smart.AspNetCore.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

using Smart.Text;

public sealed class KebabControllerModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        controller.ControllerName = Inflector.Kebab(controller.ControllerName);
        foreach (var action in controller.Actions)
        {
            action.ActionName = Inflector.Kebab(action.ActionName);
        }
    }
}

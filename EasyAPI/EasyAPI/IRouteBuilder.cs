using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyAPICore
{
    public interface IRouteBuilder
    {
        string Build(string areaName, string controllerName, ActionModel action);
    }
}

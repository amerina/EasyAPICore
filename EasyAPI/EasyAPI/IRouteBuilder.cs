using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyAPICore
{
    /// <summary>
    /// 路由接口
    /// </summary>
    public interface IRouteBuilder
    {
        string Build(string areaName, string controllerName, ActionModel action);
    }
}

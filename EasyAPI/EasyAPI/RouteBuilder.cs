using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyAPICore
{
    internal class RouteBuilder : IRouteBuilder
    {
        protected virtual string GetApiPreFix(ActionModel action)
        {
            var getValueSuccess = AppConsts.AssemblyApiOptions
                .TryGetValue(action.Controller.ControllerType.Assembly, out AssemblyApiOptions assemblyApiOptions);
            if (getValueSuccess && !string.IsNullOrWhiteSpace(assemblyApiOptions?.ApiPrefix))
            {
                return assemblyApiOptions.ApiPrefix;
            }

            return AppConsts.DefaultApiPreFix;
        }

        public string Build(string areaName, string controllerName, ActionModel action)
        {
            var apiPreFix = GetApiPreFix(action);
            var routeStr = $"{apiPreFix}/{areaName}/{controllerName}/{action.ActionName}".Replace("//", "/");
            return routeStr;
        }
    }
}

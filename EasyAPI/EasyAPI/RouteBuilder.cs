using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyAPICore
{
    internal class RouteBuilder : IRouteBuilder
    {
        /// <summary>
        /// 获取API前缀
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 生成路由
        /// </summary>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public string Build(string areaName, string controllerName, ActionModel action)
        {
            var apiPreFix = GetApiPreFix(action);
            var routeStr = $"{apiPreFix}/{areaName}/{controllerName}/{action.ActionName}".Replace("//", "/");
            return routeStr;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyAPICore
{
    public static class AppConsts
    {
        /// <summary>
        /// 默认根路由
        /// </summary>
        public const string DefaultRootPath = "app";

        /// <summary>
        /// 默认HTTP Verb
        /// </summary>
        public static string DefaultHttpVerb { get; set; }

        /// <summary>
        /// 默认AreaName
        /// </summary>
        public static string DefaultAreaName { get; set; }

        /// <summary>
        /// 默认API前缀
        /// </summary>
        public static string DefaultApiPreFix { get; set; }

        /// <summary>
        /// 需要移除的Service后缀
        /// </summary>
        public static List<string> ControllerPostfixes { get; set; }

        /// <summary>
        /// 需要移除的Action后缀
        /// </summary>
        public static List<string> ActionPostfixes { get; set; }

        /// <summary>
        /// 是否从Body获取参数
        /// </summary>
        public static List<Type> FormBodyBindingIgnoredTypes { get; set; }

        public static Dictionary<string, string> HttpVerbs { get; set; }

        public static Func<string, string> GetRestFulActionName { get; set; }

        public static Dictionary<Assembly, AssemblyApiOptions> AssemblyApiOptions { get; set; }

        static AppConsts()
        {
            HttpVerbs = new Dictionary<string, string>()
            {
                ["add"] = "POST",
                ["create"] = "POST",
                ["post"] = "POST",

                ["get"] = "GET",
                ["find"] = "GET",
                ["fetch"] = "GET",
                ["query"] = "GET",

                ["update"] = "PUT",
                ["put"] = "PUT",

                ["delete"] = "DELETE",
                ["remove"] = "DELETE",
            };
        }
    }
}

using System.Reflection;

namespace EasyAPICore
{
    /// <summary>
    /// 判断一个类是否需要自动生成Controller
    /// </summary>
    public class ControllerFeatureProvider : Microsoft.AspNetCore.Mvc.Controllers.ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo type)
        {
            if (!type.IsPublic || type.IsAbstract || type.IsGenericType)
            {
                return false;
            }

            var remoteServiceAttr = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(type);
            if (remoteServiceAttr != null)
            {
                return remoteServiceAttr.IsEnabledFor(type);
            }

            if (typeof(IEasyAPI).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }
    }
}

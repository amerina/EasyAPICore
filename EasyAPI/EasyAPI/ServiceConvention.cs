using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyAPICore
{
    public class ServiceConvention : IApplicationModelConvention
    {
        protected IRouteBuilder RouteBuilder { get; }
        public ServiceConvention(IRouteBuilder routeBuilder)
        {
            RouteBuilder = routeBuilder;
        }
        public void Apply(ApplicationModel application)
        {
            ApplyForControllers(application);
        }

        protected virtual void ApplyForControllers(ApplicationModel application)
        {
            RemoveDuplicateControllers(application);

            foreach (var controller in application.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();

                //是否实现IRemoteService接口
                if (ImplementsRemoteServiceInterface(controllerType))
                {
                    controller.ControllerName = controller.ControllerName.RemovePostFix(AppConsts.ControllerPostfixes.ToArray());
                    ConfigureRemoteService(controller);
                }
                else
                {
                    var remoteServiceAttr = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(controllerType.GetTypeInfo());
                    if (remoteServiceAttr != null && remoteServiceAttr.IsEnabledFor(controllerType))
                    {
                        ConfigureRemoteService(controller);
                    }
                }
            }
        }

        protected virtual void RemoveDuplicateControllers(ApplicationModel application)
        {
            var controllerModelsToRemove = new List<ControllerModel>();

            foreach (var controllerModel in application.Controllers)
            {
                var baseControllerTypes = controllerModel.ControllerType
                    .GetBaseClasses(typeof(Controller), includeObject: false)
                    .Where(t => !t.IsAbstract)
                    .ToArray();

                if (baseControllerTypes.Length == 0)
                {
                    continue;
                }

                var baseControllerModels = application.Controllers
                    .Where(cm => baseControllerTypes.Contains(cm.ControllerType))
                    .ToArray();

                if (baseControllerModels.Length == 0)
                {
                    continue;
                }

                controllerModelsToRemove.Add(controllerModel);
            }
            foreach (var controllerModel in controllerModelsToRemove)
            {
                application.Controllers.Remove(controllerModel);
            }
        }

        protected virtual bool ImplementsRemoteServiceInterface(Type controllerType)
        {
            return typeof(IEasyAPI).GetTypeInfo().IsAssignableFrom(controllerType);
        }

        protected virtual void ConfigureRemoteService(ControllerModel controller)
        {
            ConfigureApiExplorer(controller);
            ConfigureSelector(controller);
            ConfigureParameters(controller);
        }
        #region ConfigureApiExplorer
        protected virtual void ConfigureApiExplorer(ControllerModel controller)
        {
            if (controller.ApiExplorer.GroupName.IsNullOrEmpty())
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }

            if (controller.ApiExplorer.IsVisible == null)
            {
                controller.ApiExplorer.IsVisible = IsVisibleRemoteService(controller.ControllerType);
            }

            foreach (var action in controller.Actions)
            {
                ConfigureApiExplorer(action);
            }
        }

        protected virtual bool IsVisibleRemoteService(Type controllerType)
        {
            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(controllerType);
            if (attribute == null)
            {
                return true;
            }

            return attribute.IsEnabledFor(controllerType) &&
                   attribute.IsMetadataEnabledFor(controllerType);
        }

        protected virtual void ConfigureApiExplorer(ActionModel action)
        {
            if (action.ApiExplorer.IsVisible != null)
            {
                return;
            }

            var visible = IsVisibleRemoteServiceMethod(action.ActionMethod);
            if (visible == null)
            {
                return;
            }

            action.ApiExplorer.IsVisible = visible;
        }

        /// <summary>
        /// RemoteService特性标记在方法上
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected virtual bool? IsVisibleRemoteServiceMethod(MethodInfo method)
        {
            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(method);
            if (attribute == null)
            {
                return null;
            }

            return attribute.IsEnabledFor(method) &&
                   attribute.IsMetadataEnabledFor(method);
        }

        protected virtual void ConfigureSelector(ControllerModel controller)
        {
            var controllerType = controller.ControllerType.AsType();
            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(controllerType.GetTypeInfo());
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabledFor(controllerType))
            {
                return;
            }

            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
            {
                return;
            }

            var rootPath = GetRootPathOrDefault(controller.ControllerType.AsType());

            foreach (var action in controller.Actions)
            {
                ConfigureSelector(rootPath, controller.ControllerName, action);
            }
        }


        protected virtual string GetRootPathOrDefault(Type controllerType)
        {
            var areaAttribute = controllerType.GetCustomAttributes().OfType<AreaAttribute>().FirstOrDefault();
            if (areaAttribute?.RouteValue != null)
            {
                return areaAttribute.RouteValue;
            }

            return AppConsts.DefaultRootPath;
        }
        #endregion

        #region ConfigureSelector
        protected virtual void ConfigureSelector(string rootPath, string controllerName, ActionModel action)
        {
            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<EasyAPIAttribute>(action.ActionMethod);
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabledFor(action.ActionMethod))
            {
                return;
            }

            if (!action.Selectors.Any())
            {
                AddServiceSelector(rootPath, controllerName, action);
            }
            else
            {
                NormalizeSelectorRoutes(rootPath, controllerName, action);
            }
        }

        protected virtual void AddServiceSelector(string rootPath, string controllerName, ActionModel action)
        {
            var httpMethod = SelectHttpMethod(action);

            var abpServiceSelectorModel = new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(RouteBuilder.Build(rootPath, controllerName, action))),
                ActionConstraints = { new HttpMethodActionConstraint(new[] { httpMethod }) }
            };

            action.Selectors.Add(abpServiceSelectorModel);
        }

        protected virtual string SelectHttpMethod(ActionModel action)
        {
            return HttpMethodHelper.GetConventionalVerbForMethodName(action.ActionName);
        }

        protected virtual void NormalizeSelectorRoutes(string rootPath, string controllerName, ActionModel action)
        {
            foreach (var selector in action.Selectors)
            {
                var httpMethod = selector.ActionConstraints
                    .OfType<HttpMethodActionConstraint>()
                    .FirstOrDefault()?
                    .HttpMethods?
                    .FirstOrDefault();

                if (httpMethod == null)
                {
                    httpMethod = SelectHttpMethod(action);
                }

                if (selector.AttributeRouteModel == null)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(RouteBuilder.Build(rootPath, controllerName, action)));
                }

                if (!selector.ActionConstraints.OfType<HttpMethodActionConstraint>().Any())
                {
                    selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));
                }
            }
        }
        #endregion

        #region ConfigureParameters
        protected virtual void ConfigureParameters(ControllerModel controller)
        {
            /* Default binding system of Asp.Net Core for a parameter
             * 1. Form values
             * 2. Route values.
             * 3. Query string.
             */

            foreach (var action in controller.Actions)
            {
                foreach (var prm in action.Parameters)
                {
                    if (prm.BindingInfo != null)
                    {
                        continue;
                    }

                    if (!TypeHelper.IsPrimitiveExtendedIncludingNullable(prm.ParameterInfo.ParameterType))
                    {
                        if (CanUseFormBodyBinding(action, prm))
                        {
                            prm.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                        }
                    }
                }
            }
        }

        protected virtual bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
        {

            if (AppConsts.FormBodyBindingIgnoredTypes.Any(t => t.IsAssignableFrom(parameter.ParameterInfo.ParameterType)))
            {
                return false;
            }

            foreach (var selector in action.Selectors)
            {
                if (selector.ActionConstraints == null)
                {
                    continue;
                }

                foreach (var actionConstraint in selector.ActionConstraints)
                {
                    var httpMethodActionConstraint = actionConstraint as HttpMethodActionConstraint;
                    if (httpMethodActionConstraint == null)
                    {
                        continue;
                    }

                    if (httpMethodActionConstraint.HttpMethods.All(hm => hm.IsIn("GET", "DELETE", "TRACE", "HEAD")))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

    }
}

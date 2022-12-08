using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace EasyAPI
{
    /// <summary>
    /// Add Dynamic WebApi
    /// </summary>
    public static class ApiExtensions
    {
        public static IApplicationBuilder UseEasyApi(this IApplicationBuilder application, Action<IServiceProvider, ApiOptions> optionsAction)
        {
            var options = new ApiOptions();

            optionsAction?.Invoke(application.ApplicationServices, options);

            options.Valid();

            AppConsts.DefaultAreaName = options.DefaultAreaName;
            AppConsts.DefaultHttpVerb = options.DefaultHttpVerb;
            AppConsts.DefaultApiPreFix = options.DefaultApiPrefix;
            AppConsts.ControllerPostfixes = options.RemoveControllerPostfixes;
            AppConsts.ActionPostfixes = options.RemoveActionPostfixes;
            AppConsts.FormBodyBindingIgnoredTypes = options.FormBodyBindingIgnoredTypes;
            AppConsts.GetRestFulActionName = options.GetRestFulActionName;
            AppConsts.AssemblyApiOptions = options.AssemblyApiOptions;

            var partManager = application.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            // Add a custom controller checker
            var featureProviders = application.ApplicationServices.GetRequiredService<ControllerFeatureProvider>();
            partManager.FeatureProviders.Add(featureProviders);

            foreach (var assembly in options.AssemblyApiOptions.Keys)
            {
                var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

                foreach (var part in partFactory.GetApplicationParts(assembly))
                {
                    partManager.ApplicationParts.Add(part);
                }
            }


            var mvcOptions = application.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>();
            var serviceConvention = application.ApplicationServices.GetRequiredService<ServiceConvention>();

            mvcOptions.Value.Conventions.Add(serviceConvention);

            return application;
        }


        public static IServiceCollection AddEasyApi(this IServiceCollection services)
        {
            services.AddSingleton<ServiceConvention>();
            services.AddSingleton<ControllerFeatureProvider>();
            return services;
        }

        /// <summary>
        /// Add EasyApi Container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddEasyApi(this IServiceCollection services, ApiOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException(nameof(options));
            }

            options.Valid();

            AppConsts.DefaultAreaName = options.DefaultAreaName;
            AppConsts.DefaultHttpVerb = options.DefaultHttpVerb;
            AppConsts.DefaultApiPreFix = options.DefaultApiPrefix;
            AppConsts.ControllerPostfixes = options.RemoveControllerPostfixes;
            AppConsts.ActionPostfixes = options.RemoveActionPostfixes;
            AppConsts.FormBodyBindingIgnoredTypes = options.FormBodyBindingIgnoredTypes;
            AppConsts.GetRestFulActionName = options.GetRestFulActionName;
            AppConsts.AssemblyApiOptions = options.AssemblyApiOptions;

            var partManager = services.GetSingletonInstanceOrNull<ApplicationPartManager>();

            if (partManager == null)
            {
                throw new InvalidOperationException("\"AddEasyApi\" must be after \"AddMvc\".");
            }

            // Add a custom controller checker
            partManager.FeatureProviders.Add(new ControllerFeatureProvider());

            services.Configure<MvcOptions>(o =>
            {
                // Register Controller Routing Information Converter
                o.Conventions.Add(new ServiceConvention(options.RouteBuilder));
            });

            services.AddEasyApi();

            return services;
        }

        public static IServiceCollection AddEasyApi(this IServiceCollection services, Action<ApiOptions> optionsAction)
        {
            var ApiOptions = new ApiOptions();

            optionsAction?.Invoke(ApiOptions);

            return AddEasyApi(services, ApiOptions);
        }

    }
}

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyAPI
{
    public interface IRouteBuilder
    {
        string Build(string areaName, string controllerName, ActionModel action);
    }
}

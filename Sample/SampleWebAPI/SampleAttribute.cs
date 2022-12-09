using EasyAPICore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebAPI
{
    [EasyAPI]
    public class SampleAttribute
    {
        public string APIMethod()
        {
            return $"Hello, Easy API Core.";
        }

        public string NotAPIMethod()
        {
            return $"Hello World.";
        }
    }


}

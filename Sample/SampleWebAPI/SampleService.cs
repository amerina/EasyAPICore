using EasyAPICore;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebAPI
{
    [EasyAPI]
    public class SampleService : IEasyAPI
    {
        public string Get()
        {
            return $"Hello, EasyAPICore";
        }

        [HttpPost]
        public string Post()
        {
            return $"Hello, EasyAPICore";
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JSanitizer.Tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JSanitizerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<JSanitizerController> _logger;

        private string _xmlValue { set; get; }

        public JSanitizerController(ILogger<JSanitizerController> logger)
        {
            _logger = logger;

            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "_test.xml");

            _xmlValue = System.IO.File.ReadAllText(path);
        }

        [HttpGet, Route("GetWithoutOptions")]
        public IActionResult Get()
        {
          
            Data data = new Data()
            {
                JsonArrayResult = "[{\"password\":\"password@412\", \"data\": \"sample\" }]".SanitizeJsonValue(),
                Name = "{\"name\":\"John\", \"age\":30, \"password\":\"password@123\" } ]}".SanitizeJsonValue(),
                XMLResult = _xmlValue.SanitizeXmlValue()
            };

            return Ok(data);
        }

        [HttpGet, Route("GetWithOptions")]
        public IActionResult GetWithOptions()
        {
            Data data = new Data()
            {
                XMLResult = _xmlValue.SanitizeXmlWithOptions(new Sanitizer.JOptions()
                {
                    DefaultMaskValue = "####-####",
                    Sensitivity = new List<string>() { "password" }
                }),
            };

            return Ok(data);
        }

        public class Data
        {
            public string IdCard { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string XMLResult { set; get; }
            public string JsonResult { set; get; }
            public string JsonArrayResult { set; get; }
        }
    }
}

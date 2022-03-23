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
            string configDir = $"{Directory.GetCurrentDirectory()}\\JSOptions\\SanitizerOptions.json";

            Data data = new Data()
            {
                JsonArrayResult = "[{\"password\":\"password@412\", \"data\": \"sample\" }]".SanitizeJsonValue(configDir),
                Name = "{\"name\":\"John\", \"age\":30, \"password\":\"password@123\" }".SanitizeJsonValue(configDir),
                XMLResult = _xmlValue.SanitizeXmlValue(configDir)
            };

            return Ok(data);
        }

        [HttpGet, Route("GetWithOptions")]
        public IActionResult GetWithOptions()
        {
            Data data = new Data()
            {
                XMLResult = _xmlValue.SanitizeXmlValue(new XmlMask()
                {
                    MaskValue = "#",
                    IsFullMasking = false,
                    Sensitivity = new List<Sensitivity>()
                    {
                      new Sensitivity()
                      {
                          TargetProperties = new List<string> {
                             "Password",
                             "password",
                             "PASSWROD"
                          },
                          Positions = new MaskPosition ()
                          {
                              Left = 3,
                              Center = 2,
                              Right = 2
                          }
                    }
                }
                })
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

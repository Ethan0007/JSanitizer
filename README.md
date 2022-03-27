[![NuGet Downloads](https://img.shields.io/nuget/dt/JSanitizer.svg)](https://github.com/Ethan0007/JSanitizer)
[![NuGet Version](https://img.shields.io/nuget/v/JSanitizer.svg)](https://github.com/Ethan0007/JSanitizer)

# JSanitizer 
Configurable sanitizer for XML and JSON through extension method.   

# Features  
* Default sanitizer to replace password value
* Sanitize with options
    - Default value replacer
    - Add property to sanitize value

# Requirements   
 * .NET Core 3.1  
 * Visual Studio or VS Code
 
# How to use:  
* Go to Tools and select Manage Nuget Packages and Search for JSanitizer library   
```Install-Package JSanitizer -Version 3.0.0```   

**Extension:**   
```.SanitizeJsonValue(configPath)```   
```.SanitizeXmlValue(configPath)```   
```.SanitizeJsonValue(options)```   
```.SanitizeXmlValue(options)```

## How to create your custom JSON configurations.
* Create JSON configuration file under your project and follow the object schema below.

##### {Project.Name}/JSOptions/SanitizerOptions.json
```
{
  "DefaultMaskValue": "#",
  "ConfigurationValue": [
    {
      "id": 1,
      "XmlMask": {
        "MaskValue": "#",
        "IsFullMasking": false,
        "Sensitivity": [
          {
            "TargetProperties": [
              "Password",
              "password",
              "PASSWORD"
            ],
            "Positions": {
              "Left": 1,
              "Center": 1,
              "Right": 1
            }
          }
        ]
      },
      "JsonMask": {
        "MaskValue": "#",
        "IsFullMasking": false,
        "Sensitivity": [
          {
            "TargetProperties": [
              "Password",
              "password",
              "PASSWORD"
            ],
            "Positions": {
              "Left": 3,
              "Center": 3,
              "Right": 3
            }
          }
        ]
      }
    }
  ]
}
```
#### note: It will throw an exception if you use the default json or xml extension methods without providing default json configurations.
*remarks*: You can add more properties under Sensitivity for value masking.


1. Sanitize without options 
```
    [HttpGet, Route("GetWithJsonConfiguration")]
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
```

2. Sanitize withoptions 
```
    [HttpGet, Route("GetWithClassConfigurations")]
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
```
  
# License 
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
  Copyright (c) 2020 [Joever Monceda](https://github.com/Ethan0007)

Linkedin: [Joever Monceda](https://www.linkedin.com/in/joever-monceda-55242779/)  
  Medium: [Joever Monceda](https://medium.com/@joever.monceda/new-net-core-vuejs-vuex-router-webpack-starter-kit-e94b6fdb7481)  
  Twitter [@_EthanHunt07](https://twitter.com/_EthanHunt07)  
  Facebook: [Ethan Hunt](https://www.facebook.com/nethan.hound.3/)

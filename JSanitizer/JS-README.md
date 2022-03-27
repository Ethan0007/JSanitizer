# JSanitizer 
Configurable sanitizer for XML and JSON string value through extension method.

## How to create your custom JSON configurations.
* Create JSON configuration file under your project and follow the object schema below.

##### {Project.Name}/SanitizerOptions.json
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

# License 
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
  Copyright (c) 2020 [Joever Monceda](https://github.com/Ethan0007)

  Linkedin: [Joever Monceda](https://www.linkedin.com/in/joever-monceda-55242779/)  
  Medium: [Joever Monceda](https://medium.com/@joever.monceda/new-net-core-vuejs-vuex-router-webpack-starter-kit-e94b6fdb7481)  
  Twitter [@_EthanHunt07](https://twitter.com/_EthanHunt07)  
  Facebook: [Ethan Hunt](https://www.facebook.com/nethan.hound.3/)

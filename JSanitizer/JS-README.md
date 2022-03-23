# JSanitizer 
Configurable sanitizer for XML and JSON string value through extension method.

## How to create your custom JSON configurations.
* Create folder with name **JSOptions** under your project then create json file with name **SanitizerOptions.json** under the said folder as your default configuration and follow the object format below.

##### {Project.Name}/JSOptions/SanitizerOptions.json
```
{
  "DefaultMaskValue": "####-####",
  "ConfigurationValue": [
    {
      "id": 1,
      "XmlMask": {
        "MaskValue": "###-###-###",
        "Sensitivity": [
          "Password",
          "password",
          "PASSWROD"
        ]
      },
      "JsonMask": {
        "MaskValue": "###-###-###",
        "Sensitivity": [
          "Password",
          "password",
          "PASSWROD"
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

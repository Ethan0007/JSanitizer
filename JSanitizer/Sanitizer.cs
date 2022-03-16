using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace JSanitizer
{
    public static class Sanitizer
    {
        private static void XmlValueReplacer(XmlNode node, string sensitivity, string maskVal)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.SelectSingleNode(sensitivity) != null)
                {
                    child.SelectSingleNode(sensitivity).InnerText = maskVal;
                }
            }
        }

        private static LogConfig GetConfig()
        {
            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "JSOptions/SanitizerOptions.json");

                string jsonValue = File.ReadAllText(filePath);

                return JsonConvert.DeserializeObject<LogConfig>(jsonValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to find JSOptions/SanitizerOptions.json as a default configuration. Please read README.md under JSOptions folder!", ex);
            }
        }

        public class JOptions
        {
            public JOptions ()
            {
                this.Sensitivity = new List<string>();
            }

            public string DefaultMaskValue { set; get; } = "###-###";
            public List<string> Sensitivity { set; get; }
        }

        public static string SanitizeXmlValue(this string target, JOptions options)
        {

            if (string.IsNullOrEmpty(target))
                return null;

            if (!target.IsValidXML())
                return target;

            XmlDocument targetXMLDoc = new XmlDocument();

            targetXMLDoc.LoadXml(target);

            if (options.Sensitivity.Count == 0)
            {
                options.Sensitivity = new List<string>()
                {
                    "password"
                };
            }

            foreach (string sensitiveItem in options.Sensitivity)
            {
                XmlNodeList nodes = targetXMLDoc.DocumentElement.ChildNodes;

                foreach (XmlNode node in nodes)
                {
                    XmlValueReplacer(node, sensitiveItem, options.DefaultMaskValue);
                }
            }

            return targetXMLDoc.InnerXml;
        }

        public static string SanitizeXmlValue(this string target)
        {

            if (string.IsNullOrEmpty(target))
                return null;

            if (!target.IsValidXML())
                return target;

            LogConfig logConfig = GetConfig();

            XmlDocument targetXMLDoc = new XmlDocument();

            targetXMLDoc.LoadXml(target);

            logConfig.ConfigurationValue.ForEach(extLog =>
            {
                extLog.XmlMask.Sensitivity.ForEach(sensitiveItem =>
                {
                    XmlNodeList nodes = targetXMLDoc.DocumentElement.ChildNodes;

                    foreach (XmlNode node in nodes)
                    {
                        XmlValueReplacer(node, sensitiveItem, extLog.XmlMask.MaskValue);
                    }
                });
            });

            return targetXMLDoc.InnerXml;
        }


        public static string SanitizeJsonValue(this string target, JOptions options)
        {

            if (string.IsNullOrEmpty(target)) return null;

            if (!target.IsValidJson())
                return target;

            if(options.Sensitivity.Count == 0)
            {
                options.Sensitivity = new List<string>()
                {
                    "password"
                };
            }

            foreach (string sensitiveValue in options.Sensitivity)
            {
                target = JsonAction(target, sensitiveValue, options.DefaultMaskValue);
            }

            return target;
        }

        public static string SanitizeJsonValue(this string target)
        {
            LogConfig logConfig = GetConfig();

            if (string.IsNullOrEmpty(target)) return null;

            if (!target.IsValidJson())
                return target;

            logConfig.ConfigurationValue.ForEach(extLog =>
            {
                extLog.JsonMask.Sensitivity.ForEach(sensitiveValue =>
                {
                    target = JsonAction(target, sensitiveValue, extLog.JsonMask.MaskValue);
                });

            });

            return target;
        }

        private static string JsonAction(string propVal, string senItem, string maskValue)
        {
            JToken node = JToken.Parse(propVal);

            CheckJsonNode(node, n =>
            {
                JToken token = n[senItem];

                if (token != null && token.Type == JTokenType.String)
                {
                    propVal = propVal.Replace(token.ToString(), maskValue);
                }
            });

            return propVal;
        }

        private static void CheckJsonNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    CheckJsonNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    CheckJsonNode(child, action);
                }
            }
        }

        public static bool IsValidXML(this string xml)
        {
            if (!string.IsNullOrEmpty(xml) && xml.TrimStart().StartsWith("<"))
            {
                try
                {
                    XDocument.Parse(xml);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else return false;
        }

        public static bool IsValidJson(this string json)
        {
            if (string.IsNullOrEmpty(json)) { return false; }

            json = json.Trim();

            if ((json.StartsWith("{") && json.EndsWith("}")) ||
                (json.StartsWith("[") && json.EndsWith("]")))
            {
                try
                {
                    JToken.Parse(json);

                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


    }

    public class LogConfig
    {
        public LogConfig()
        {
            this.ConfigurationValue = new List<ConfigurationValue>();
        }

        public string DefaultMaskValue { set; get; }
        public List<ConfigurationValue> ConfigurationValue { set; get; }
    }

    public class ConfigurationValue
    {
        public ConfigurationValue()
        {
            this.XmlMask = new XmlMask();
            this.JsonMask = new JsonMask();
        }

        public int Id { set; get; }
        public string ServiceName { set; get; }

        public XmlMask XmlMask { set; get; }

        public JsonMask JsonMask { set; get; }
    }

    public class XmlMask
    {
        public XmlMask()
        {
            this.Sensitivity = new List<string>();
        }

        public string MaskValue { set; get; }
        public List<string> Sensitivity { set; get; }
    }

    public class JsonMask
    {
        public JsonMask()
        {
            this.Sensitivity = new List<string>();
        }

        public string MaskValue { set; get; }
        public List<string> Sensitivity { set; get; }
    }
}

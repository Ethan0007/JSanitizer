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
        private static void XmlValueReplacer(XmlNode node, string sensitivity, string maskVal, XmlMask xm, MaskPosition ps)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.SelectSingleNode(sensitivity) != null)
                {
                    string input = child.SelectSingleNode(sensitivity).InnerText;

                    string copyInput = input;

                    int length = input.Length - ps.Left;

                    bool isMasked = false;

                    if (!xm.IsFullMasking && ps.Left > 0 && length > ps.Left)
                    {
                        string masking = string.Empty;

                        for (int i = 0; i < ps.Left; i++) masking += xm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty;

                        for (int i = length; i < input.Length; i++)
                        {
                            toBeReplaced += chars[i].ToString();
                        }

                        input = input.Replace(toBeReplaced, masking);

                        isMasked = true;
                    }

                    if (!xm.IsFullMasking && ps.Right > 0 && length > ps.Right)
                    {
                        string masking = string.Empty;

                        for (int i = 0; i < ps.Right; i++) masking += xm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty;

                        for (int i = 0; i < ps.Right; i++)
                        {
                            toBeReplaced += input[i].ToString();
                        }

                        input = input.Replace(toBeReplaced, masking);

                        isMasked = true;
                    }

                    if (!xm.IsFullMasking && ps.Center > 0 && length > ps.Center)
                    {
                        string masking = string.Empty;

                        int nl = length / 2;

                        for (int i = 0; i < ps.Center; i++) masking += xm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty;

                        int c = 0;

                        for (int i = nl; i < length; i++)
                        {
                            if (c < ps.Center)
                            {
                                toBeReplaced += input[i].ToString();
                                c++;
                            }
                        }

                        input = input.Replace(toBeReplaced, masking);

                        isMasked = true;
                    }

                    if (isMasked)
                    {
                        child.SelectSingleNode(sensitivity).InnerText = input;
                    }
                    else
                    {
                        child.SelectSingleNode(sensitivity).InnerText = maskVal;
                    }
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
                throw new Exception("Unable to find JSOptions/SanitizerOptions.json as a default configuration. Please read JS-README.md under JSOptions folder!", ex);
            }
        }

        //public class JOptions
        //{
        //    public JOptions ()
        //    {
        //        this.Sensitivity = new List<string>();
        //    }

        //    public string DefaultMaskValue { set; get; } = "###-###";
        //    public List<string> Sensitivity { set; get; }
        //}

        public static string SanitizeXmlValue(this string target, XmlMask ml)
        {

            if (string.IsNullOrEmpty(target))
                return null;

            if (!target.IsValidXML())
                return target;

            XmlDocument targetXMLDoc = new XmlDocument();

            targetXMLDoc.LoadXml(target);

            foreach (var item in ml.Sensitivity)
            {
                if (item.TargetProperties.Count == 0)
                {
                    item.TargetProperties.Add("password");
                }
            }

            ml.Sensitivity.ForEach(sensitiveValues =>
            {
                XmlNodeList nodes = targetXMLDoc.DocumentElement.ChildNodes;

                sensitiveValues.TargetProperties.ForEach(sensitivityItem =>
                {
                    foreach (XmlNode node in nodes)
                    {
                        XmlValueReplacer(node, sensitivityItem, ml.MaskValue , ml , sensitiveValues.Positions);
                    }
                });
            });

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

                    sensitiveItem.TargetProperties.ForEach(sensitivityItem =>
                    {
                        foreach (XmlNode node in nodes)
                        {
                            XmlValueReplacer(node, sensitivityItem, extLog.XmlMask.MaskValue  , extLog.XmlMask, sensitiveItem.Positions);
                        }
                    });
                });
            });

            return targetXMLDoc.InnerXml;
        }


        public static string SanitizeJsonValue(this string target, JsonMask jm)
        {

            if (string.IsNullOrEmpty(target)) return null;

            if (!target.IsValidJson())
                return target;


            foreach (var item in jm.Sensitivity)
            {
                if(item.TargetProperties.Count == 0)
                {
                    item.TargetProperties.Add("password");
                }
            }


            jm.Sensitivity.ForEach(sensitiveValues =>
            {
                sensitiveValues.TargetProperties.ForEach(sensitivityItem =>
                {
                    target = JsonAction(target, sensitivityItem, jm, sensitiveValues.Positions);
                });
            });

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
                extLog.JsonMask.Sensitivity.ForEach(sensitiveValues =>
                {
                    sensitiveValues.TargetProperties.ForEach(sensitivityItem =>
                    {
                        target = JsonAction(target, sensitivityItem, 
                                            extLog.JsonMask, 
                                            sensitiveValues.Positions);
                    });
                });

            });

            return target;
        }

        private static string JsonAction(string propVal, string senItem, JsonMask jm , MaskPosition ps)
        {
            JToken node = JToken.Parse(propVal);

            bool isMasked = false;

            CheckJsonNode(node, n =>
            {
                JToken token = n[senItem];

                if (token != null && token.Type == JTokenType.String)
                {
                    string input = token.ToString();

                    string copyInput = token.ToString();

                    int length = input.Length - ps.Left;

                    if (!jm.IsFullMasking && jm.IsFullMasking && ps.Left > 0 && length > ps.Left)
                    {
                        string masking = string.Empty;

                        for (int i = 0; i < ps.Left; i++) masking += jm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty ;

                        for (int i = length; i < input.Length; i++)
                        {
                            toBeReplaced += chars[i].ToString();
                        }

                        input = input.Replace(toBeReplaced, masking);
                         
                        isMasked = true;
                    }

                    if  (!jm.IsFullMasking && ps.Right > 0 && length > ps.Right)
                    {
                        string masking = string.Empty;

                        for (int i = 0; i < ps.Right; i++) masking += jm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty;

                        for (int i = 0; i < ps.Right; i++)
                        {
                            toBeReplaced += input[i].ToString();
                        }

                        input = input.Replace(toBeReplaced, masking);

                        isMasked = true;
                    }

                    if (!jm.IsFullMasking && ps.Center > 0 && length > ps.Center)
                    {
                        string masking = string.Empty;

                        int nl = length / 2;

                        for (int i = 0; i < ps.Center; i++) masking += jm.MaskValue;

                        char[] chars = input.ToCharArray();

                        string toBeReplaced = string.Empty;

                        int c = 0;

                        for (int i = nl; i < length; i++)
                        {
                            if(c < ps.Center)
                            {
                                toBeReplaced += input[i].ToString();
                                c++;
                            }
                        }

                        input = input.Replace(toBeReplaced, masking);

                        isMasked = true;
                    }

                    if (isMasked)
                    {
                        propVal = propVal.Replace(copyInput, input);
                    }
                    else
                    {
                        propVal = propVal.Replace(token.ToString(), jm.MaskValue);
                    }
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

        public XmlMask XmlMask { set; get; }

        public JsonMask JsonMask { set; get; }
    }

    public class XmlMask
    {
        public XmlMask()
        {
            this.Sensitivity = new List<Sensitivity>();
        }
        public bool IsFullMasking { set; get; }
        public string MaskValue { set; get; }
        public List<Sensitivity> Sensitivity { set; get; }
    }

    public class MaskPosition
    {
        public int Left { set; get; }
        public int Center { set; get; }
        public int Right { set; get; }
    }

    public class JsonMask
    {
        public JsonMask()
        {
            this.Sensitivity = new List<Sensitivity>();
        }
        public bool IsFullMasking { set; get; }
        public string MaskValue { set; get; }
        public List<Sensitivity> Sensitivity { set; get; }
    }

    public class Sensitivity
    {
        public List<string> TargetProperties { set; get; }
        public MaskPosition Positions { set; get; }
    }
}

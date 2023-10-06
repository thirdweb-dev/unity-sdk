#if UNITY_EDITOR && UNITY_ANDROID
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using System.Xml;
using System.Text;

namespace Thirdweb
{
    public class ThirdwebPostprocessBuild : IPostGenerateGradleAndroidProject
    {
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            var androidManifestPath = $"{basePath}/src/main/AndroidManifest.xml";

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(androidManifestPath);
            
            var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            nsManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            var applicationNode = xmlDocument.SelectSingleNode("/manifest/application", nsManager) as XmlElement;

            // Identify the UnityPlayerActivity
            if (applicationNode.SelectSingleNode("./activity[@android:name='com.unity3d.player.UnityPlayerActivity']", nsManager) is XmlElement activityNode)
            {
                // Overwrite UnityPlayerActivity with ThirdwebActivity
                activityNode.SetAttribute("name", nsManager.LookupNamespace("android"), "com.unity3d.player.ThirdwebActivity");

                // Add specified intent-filter if not already present
                var intentFilterNode = activityNode.SelectSingleNode("./intent-filter[data/@android:scheme='thirdweb']", nsManager) as XmlElement;
                if (intentFilterNode == null)
                {
                    intentFilterNode = xmlDocument.CreateElement("intent-filter");

                    var actionNode = xmlDocument.CreateElement("action");
                    actionNode.SetAttribute("name", nsManager.LookupNamespace("android"), "android.intent.action.VIEW");
                    intentFilterNode.AppendChild(actionNode);

                    var defaultCategoryNode = xmlDocument.CreateElement("category");
                    defaultCategoryNode.SetAttribute("name", nsManager.LookupNamespace("android"), "android.intent.category.DEFAULT");
                    intentFilterNode.AppendChild(defaultCategoryNode);

                    var browsableCategoryNode = xmlDocument.CreateElement("category");
                    browsableCategoryNode.SetAttribute("name", nsManager.LookupNamespace("android"), "android.intent.category.BROWSABLE");
                    intentFilterNode.AppendChild(browsableCategoryNode);

                    var dataNode = xmlDocument.CreateElement("data");
                    dataNode.SetAttribute("scheme", nsManager.LookupNamespace("android"), "thirdweb");
                    intentFilterNode.AppendChild(dataNode);

                    activityNode.AppendChild(intentFilterNode);
                }

                xmlDocument.Save(androidManifestPath);

                // Log the final manifest
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = Encoding.UTF8
                };
                using (var writer = XmlWriter.Create(sb, settings))
                {
                    xmlDocument.Save(writer);
                }
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.LogWarning("UnityPlayerActivity not found. Unable to overwrite activity name and add intent-filter.");
            }
        }

        public int callbackOrder => 2;
    }
}
#endif

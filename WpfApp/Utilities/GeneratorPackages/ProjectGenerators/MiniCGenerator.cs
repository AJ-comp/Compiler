using Parse.BackEnd.Target;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace WpfApp.Utilities.GeneratorPackages.ProjectGenerators
{
    public class MiniCGenerator : ProjectGenerator
    {
        private XmlDocument xDoc;

        public override string Extension { get; } = "mc";

        private XmlNode CreateDefaultPropertyNode(Configure conf, Target target)
        {
            XmlNode propertyGroup = xDoc.CreateElement("PropertyGroup");
            XmlNode configure = xDoc.CreateElement("Configuration");
            configure.InnerText = conf.ToString();
            propertyGroup.AppendChild(configure);

            XmlNode platform = xDoc.CreateElement("PlatForm");
            platform.InnerText = target.Name;
            propertyGroup.AppendChild(platform);

            XmlNode optimize = xDoc.CreateElement("Optimize");
            optimize.InnerText = "false";
            propertyGroup.AppendChild(optimize);

            return propertyGroup;
        }

        private XmlNode CreateReferenceNode(StringCollection items)
        {
            XmlNode referGroup = xDoc.CreateElement("ReferenceGroup");

            foreach (var item in items)
            {
                XmlNode referItem = xDoc.CreateElement("Item");
                referItem.InnerText = item;
                referGroup.AppendChild(referItem);
            }

            return referGroup;
        }

        private XmlNode CreateItemNode(string item)
        {
            XmlNode itemGroup = xDoc.CreateElement("ItemGroup");

            XmlNode itemNode = xDoc.CreateElement("Item");
            itemNode.InnerText = item;
            itemGroup.AppendChild(itemNode);

            return itemGroup;
        }


        public override void Generator(string projectPath, string projectName, Target target)
        {
            Directory.CreateDirectory(projectPath);

            this.xDoc = new XmlDocument();

            XmlNode root = xDoc.CreateElement("Project");
            XmlAttribute attr = xDoc.CreateAttribute("ToolVersion");
            attr.Value = "1.0";
            root.Attributes.Append(attr);

            string fileName = string.Format("main.{0}", this.Extension);
            root.AppendChild(this.CreateDefaultPropertyNode(Configure.Debug, target));
            root.AppendChild(this.CreateDefaultPropertyNode(Configure.Release, target));
            root.AppendChild(this.CreateReferenceNode(new StringCollection() { "System", "System.Data", "System.Collection" }));
            root.AppendChild(this.CreateItemNode(fileName));

            xDoc.AppendChild(root);

            File.Create(Path.Combine(projectPath, fileName));
            xDoc.Save(string.Format("{0}.{1}", Path.Combine(projectPath, projectName), this.Extension + "proj"));
        }
    }
}

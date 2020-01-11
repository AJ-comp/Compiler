using Parse.BackEnd.Target;
using System;
using System.IO;
using System.Xml;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Utilities.GeneratorPackages.ProjectLoaders
{
    public class ProjectLoader
    {

        public ProjectStruct LoadProject(string projectPath, string projectFileName)
        {
            ProjectStruct result = new ProjectStruct() { FullName = projectFileName };

            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(Path.Combine(projectPath, projectFileName));

            //XmlNodeList propertyNodes = xmlDoc.SelectNodes("//Project/PropertyGroup");
            //foreach (XmlNode propertyNode in propertyNodes)
            //{
            //    ProjectProperty property = new ProjectProperty()
            //    {
            //        Mode = (ProjectProperty.Configure)Enum.Parse(typeof(ProjectProperty.Configure), propertyNode.SelectSingleNode("Configuration").InnerText),
            //        Target = Activator.CreateInstance(Type.GetType(propertyNode.SelectSingleNode("PlatForm").InnerText)) as Target,
            //        OptimizeLevel = propertyNode.SelectSingleNode("Optimize").InnerText
            //    };

            //    XmlNode configureNode = 
            //    XmlNode fullPathNode = 
            //    XmlNode optimizeNode = 
            //}

            //XmlNodeList refGroupNodes = xmlDoc.SelectNodes("//Project/ReferenceGroup");
            //foreach (XmlNode refNode in refGroupNodes)
            //{
            //    XmlNodeList itemNodes = refNode.SelectNodes("Item");

            //    foreach(XmlNode item in itemNodes)
            //    {
            //        result.ReferenceFolder.ReferenceFiles.Add(new ReferenceFileStruct() { DisplayName = item.InnerText });
            //    }

            //}

            //XmlNodeList itemGroupNodes = xmlDoc.SelectNodes("//Project/ItemGroup");
            //foreach (XmlNode itemNode in itemGroupNodes)
            //{
            //    XmlNodeList itemNodes = itemNode.SelectNodes("Item");

            //    foreach (XmlNode item in itemNodes)
            //    {
            //        result.Items.Add(new FileStruct() { DisplayName = item.InnerText });
            //    }
            //}

            return result;
        }
    }
}

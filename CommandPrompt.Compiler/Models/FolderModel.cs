using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommandPrompt.Compiler.Models
{
    public class FolderModel
    {
        [XmlElement("Folder")]
        public Collection<FolderModel> Folders { get; } = new Collection<FolderModel>();

        [XmlElement("File")]
        public Collection<SourceModel> Files { get; } = new Collection<SourceModel>();

    }
}

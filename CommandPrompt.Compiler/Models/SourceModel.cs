using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommandPrompt.Compiler.Models
{
    public class SourceModel
    {

        [XmlIgnore] public static string Extension => ".aj";

    }
}

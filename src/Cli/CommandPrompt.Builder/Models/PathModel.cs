using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CommandPrompt.Builder.Models
{
    public class PathModel
    {
        public string Path { get; set; }
        public string FileName { get; set; }

        [XmlIgnore] public bool IsAbsolute => System.IO.Path.IsPathRooted(Path);
        [XmlIgnore] public string FullPath => System.IO.Path.Combine(Path, FileName);

        public PathModel()
        {
        }

        public PathModel(string fullPath)
        {
            Path = System.IO.Path.GetDirectoryName(fullPath);
            FileName = System.IO.Path.GetFileName(fullPath);
        }

        public PathModel(string path, string fileName)
        {
            Path = path;
            FileName = fileName;
        }

        public override bool Equals(object obj)
        {
            return obj is PathModel model &&
                   Path == model.Path &&
                   FileName == model.FileName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, FileName);
        }

        public static bool operator ==(PathModel left, PathModel right)
        {
            return EqualityComparer<PathModel>.Default.Equals(left, right);
        }

        public static bool operator !=(PathModel left, PathModel right)
        {
            return !(left == right);
        }
    }
}

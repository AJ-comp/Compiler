using System;

namespace Parse.FrontEnd.AJ.Data
{
    public class ProjectProperty
    {
        public enum Configure { Debug, Release }

        public Configure Mode { get; set; }
        public int OptimizeLevel { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ProjectProperty property &&
                   Mode == property.Mode &&
                   OptimizeLevel == property.OptimizeLevel;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Mode, OptimizeLevel);
        }
    }
}

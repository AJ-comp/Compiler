using Parse.BackEnd.Target;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class ProjectProperty
    {
        public enum Configure { Debug, Release }

        public Configure Mode { get; set; }
        public string Target { get; set; }
        public int OptimizeLevel { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ProjectProperty property &&
                   Mode == property.Mode &&
                   Target == property.Target &&
                   OptimizeLevel == property.OptimizeLevel;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Mode, Target, OptimizeLevel);
        }
    }
}

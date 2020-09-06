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
            var hashCode = -949592730;
            hashCode = hashCode * -1521134295 + Mode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + OptimizeLevel.GetHashCode();
            return hashCode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.AJ
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class AssemblyInfo
    {
        public string AssemblyName { get; }
        public HashSet<string> FileFullPaths { get; } = new HashSet<string>();
        public HashSet<AssemblyInfo> ReferenceAssemblies { get; } = new HashSet<AssemblyInfo>();


        public AssemblyInfo(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public override bool Equals(object obj)
        {
            return obj is AssemblyInfo info &&
                   AssemblyName == info.AssemblyName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssemblyName);
        }

        public static bool operator ==(AssemblyInfo left, AssemblyInfo right)
        {
            return EqualityComparer<AssemblyInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(AssemblyInfo left, AssemblyInfo right)
        {
            return !(left == right);
        }

        private string DebuggerDisplay
            => $"Assembly: {AssemblyName}, files count: {FileFullPaths.Count}";
    }
}

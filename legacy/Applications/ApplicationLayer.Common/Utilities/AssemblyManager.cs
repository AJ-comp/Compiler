using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Common.Utilities
{
    public class AssemblyManager
    {
        public static IEnumerable<Assembly> GetAllLoadedAssemblyList()
        {
            //                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            //                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            //                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            List<Assembly> result = new List<Assembly>();

            Parallel.ForEach(referencedPaths, (fileName) =>
            {
                try
                {
                    result.Add(Assembly.Load(AssemblyName.GetAssemblyName(fileName)));
                }
                catch
                {

                }
            });

            return result;
        }


        public static IEnumerable<Type> GetAllTypesOfLoadedAssemblies(IEnumerable<Assembly> loadedAssemblies)
        {
            List<Type> result = new List<Type>();

            foreach (Assembly assembly in loadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes()) result.Add(type);
            }

            return result;
        }


        public static object CreateInstanceFromClassName(string className)
        {
            var allType = GetAllTypesOfLoadedAssemblies(GetAllLoadedAssemblyList());

            object result = null;
            foreach(var type in allType)
            {
                if (type.Name != className) continue;

                result = Activator.CreateInstance(type);
                break;
            }

            return result;
        }
    }
}

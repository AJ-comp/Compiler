using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ApplicationLayer.Common.Utilities
{
    public class ClassHierarchyGenerator
    {
        private SortedList<string, Type> sortedList = new SortedList<string, Type>();

        private List<Type> GetChildrenType(Type data)
        {
            List<Type> result = new List<Type>();

            foreach (KeyValuePair<string, Type> keyValuePair in sortedList)
            {
                if (keyValuePair.Value.BaseType == data)
                    result.Add(keyValuePair.Value);
            }

            return result;
        }

        private ClassHierarchyData Populate(ClassHierarchyData parentTypeHier)
        {
            List<Type> childrenType = this.GetChildrenType(parentTypeHier.Data);
            if (childrenType.Count == 0) return parentTypeHier;

            foreach(Type childType in childrenType)
            {
                ClassHierarchyData childTypeHier = new ClassHierarchyData() { Data = childType };
                parentTypeHier.Items.Add(Populate(childTypeHier));
            }

            return parentTypeHier;
        }

        public ClassHierarchyData ToHierarchyData(Type rootType)
        {
            //                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            //                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            //                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            AssemblyName[] assemblyNameArray = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            List<Assembly> loadedAssemblyList = new List<Assembly>();

            foreach (var fileName in referencedPaths)
                loadedAssemblyList.Add(Assembly.Load(AssemblyName.GetAssemblyName(fileName)));

            this.sortedList.Add(rootType.Name, rootType);

            foreach (Assembly assembly in loadedAssemblyList)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsPublic && type.IsSubclassOf(rootType)) sortedList.Add(type.Name, type);
                }
            }

            ClassHierarchyData rootTypeHier = new ClassHierarchyData() { Data = rootType };
            return Populate(rootTypeHier);
        }
    }
}

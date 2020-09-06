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

        private IEnumerable<Assembly> GetAllLoadedAssemblyList()
        {
            //                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            //                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            //                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            List<Assembly> result = new List<Assembly>();

            foreach (var fileName in referencedPaths)
                result.Add(Assembly.Load(AssemblyName.GetAssemblyName(fileName)));

            return result;
        }

        private static ClassHierarchyData FindNode(HashSet<ClassHierarchyData> list, Type targetToFind)
        {
            ClassHierarchyData result = null;

            foreach (var item in list)
            {
                if(item.Data == targetToFind)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        public IReadOnlyList<Type> GetAllTypesOfLoadedAssemblies(IReadOnlyList<Assembly> loadedAssemblies)
        {
            List<Type> result = new List<Type>();

            foreach (Assembly assembly in loadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes()) result.Add(type);
            }

            return result;
        }

        public ClassHierarchyData ToHierarchyDataDirectionChild(Type rootType)
        {
            var loadedAssemblyList = this.GetAllLoadedAssemblyList();

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

        public static ClassHierarchyData ToHierarchyDataDirectionParent(Type type, Type toType = null)
        {
            ClassHierarchyData current = new ClassHierarchyData() { Data = type };

            while (true)
            {
                ClassHierarchyData parent = new ClassHierarchyData() { Data = current.Data.BaseType };

                bool breakCondition = (toType == null) ? current.Data.BaseType != null : current.Data == toType;
                if (breakCondition) break;

                parent.Items.Add(current);
                current = parent;
            }

            return current;
        }

        public static ClassHierarchyData ToHierarchyDataDirectionParent(HashSet<Type> typeList, Type toType = null)
        {
            HashSet<ClassHierarchyData> createdTypeList = new HashSet<ClassHierarchyData>();

            foreach (var type in typeList)
            {
                var classHierData = ToHierarchyDataDirectionParent(type, toType);
                int maxDepthLevel = classHierData.MaxDepthLevel;

                for (int i = 0; i < maxDepthLevel; i++)
                {
                    var parent = classHierData.GetFirstClassHierarchyDataByDepth(i);
                    var firstChild = classHierData.GetFirstClassHierarchyDataByDepth(i + 1);

                    if (parent == null) continue;

                    if (createdTypeList.Contains(parent) == false)
                    {
                        createdTypeList.Add(parent);

                        if (firstChild == null) continue;
                        createdTypeList.Add(firstChild);
                    }
                    else if(createdTypeList.Contains(firstChild) == false)
                    {
                        if (firstChild == null) continue;

                        createdTypeList.TryGetValue(parent, out var parentData);
                        createdTypeList.Add(firstChild);
                        if (parentData.Items.Contains(firstChild) == false) parentData.Items.Add(firstChild);
                    }
                }
            }

            return FindNode(createdTypeList, toType);
        }


    }
}

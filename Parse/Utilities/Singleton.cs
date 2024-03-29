﻿using System;
using System.Reflection;

namespace Parse.Utilities
{
    public class Singleton<T> where T : class
    {
        private static object _syncobj = new object();
        private static volatile T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            lock (_syncobj)
            {
                if (_instance == null)
                {
                    Type t = typeof(T);

                    // Ensure there are no public constructors...  
                    ConstructorInfo[] ctors = t.GetConstructors();
                    if (ctors.Length > 0)
                    {
                        throw new InvalidOperationException(String.Format("{0} has at least one accessible ctor making it impossible to enforce singleton behaviour", t.Name));
                    }

                    // Create an instance via the private constructor  
                    _instance = (T)Activator.CreateInstance(t, true);
                }
            }
        }
    }
}

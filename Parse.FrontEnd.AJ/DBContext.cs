using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parse.FrontEnd.AJ
{
    public class DBContext
    {
        public static DBContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock(_lockObject)
                    {
                        _instance = new DBContext();

                        // create database
                        using (var connection = new LiteDatabase(_dbFullPath))
                        {
                        }

                        _db = new LiteDatabase(ExpandableStream(_dbFullPath));
                    }
                }

                return _instance;
            }
        }

        public static void SetDBFullPath(string dbFullPath)
        {
            lock(_lockObject)
            {
                // it can allocation only once before calling the Instance property.
                if (_instance != null) return;

                _dbFullPath = dbFullPath;
                _instance = null;
            }
        }


        public void Insert<T>(T target)
        {
            var collection = _db.GetCollection<T>();
            collection.Insert(target);
        }

        public ILiteCollection<T> Select<T>() => _db.GetCollection<T>();



        private static DBContext _instance;
        private static LiteDatabase _db;
        private static string _dbFullPath = "aj.db";
        private static object _lockObject;




        private static MemoryStream ExpandableStream(string path)
        {
            using (var temp = new MemoryStream(File.ReadAllBytes(path)))
            {
                var ms = new MemoryStream();
                temp.CopyTo(ms);

                return ms;
            }
        }
    }
}

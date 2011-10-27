﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Kooboo.CMS.Content.Persistence
{
    public static class Serialization
    {
        public static void SerializeSettings<T>(T o, string filePath)
        {
            Serialize(o, filePath);
        }
        public static T DeserializeSettings<T>(string filePath)
        {
            return Deserialize<T>(filePath);
        }
        public static void Serialize<T>(T o, string filePath)
        {
            Serialize(o, new Type[] { typeof(T) }, filePath);
        }
        public static void Serialize<T>(T o, IEnumerable<Type> knownTypes, string filePath)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T), knownTypes);
            string folderPath = Path.GetDirectoryName(filePath);
            Kooboo.IO.IOUtility.EnsureDirectoryExists(folderPath);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                ser.WriteObject(stream, o);
            }
        }
        public static T Deserialize<T>(string filePath)
        {
            return Deserialize<T>(filePath, new[] { typeof(T) });
        }
        public static T Deserialize<T>(string filePath, IEnumerable<Type> knownTypes)
        {
            return (T)Deserialize(typeof(T), knownTypes, filePath);
        }
        public static object Deserialize(Type type, IEnumerable<Type> knownTypes, string filePath)
        {
            DataContractSerializer ser = new DataContractSerializer(type, knownTypes);
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ser.ReadObject(stream);
            }
        }
    }
}

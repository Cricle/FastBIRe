using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastBIRe.ModelGen
{
    internal static class Types
    {
        public static IReadOnlyList<Type> SupportDbTypes = new Type[]
        {
            typeof(bool),
            typeof(byte),
            typeof(short),
            typeof(char),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(Guid),
            typeof(string),
        };

        public static IReadOnlyList <string> SupportTypeFullNames = SupportDbTypes.Select(x=>x.FullName).Concat(new string[]
        {
            "bool",
            "byte",
            "short",
            "char",
            "int",
            "long",
            "float",
            "double",
            "string"
        }).ToArray();
    }
}

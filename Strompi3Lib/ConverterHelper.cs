using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Strompi3Lib
{
    public static class ConverterHelper
    {
        public static bool EnabledDisabledConverter(string argument, string argumentName)
        {
            bool result = false;

            if (argument == "1")
            {
                result = true;
            }
            else if (argument == "0")
            {
                result = false;
            }
            else
            {
                result = false;
                Console.WriteLine($"***error: {argumentName} not set = '{argument}'");
            }

            return result;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static int ToNumber(this bool argument)
        {
            var result = argument == false ? 0 : 1;
            return result;
        }
    }
}

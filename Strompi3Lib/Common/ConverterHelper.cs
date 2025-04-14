using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Strompi3Lib.Common;

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

    public static bool ToBool(this int argument)
    {
        bool result = argument != 0;
        return result;
    }


    /// <summary>
    /// reads keys from console until ENTER is pressed
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static int ReadInt(int min, int max, string msg)
    {
        int result;
        while (true)
        {
            Console.Write($"Set {msg} ({min} bis {max}) and press ENTER to continue: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out result) && result >= min && result <= max)
            {
                return result;
            }

            Console.WriteLine($"Ungültige Eingabe. Bitte eine Zahl zwischen {min} und {max} eingeben.");
        }
    }
}
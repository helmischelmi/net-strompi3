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
        Console.WriteLine($"Set {msg} and press ENTER to continue");

        bool bContinue = true;
        bool bError = false;
        string strResult = string.Empty;
        int result = -1;

        while (bContinue)
        {
            var keyInfo = Console.ReadKey();// Get user input

            if (keyInfo.Key == ConsoleKey.Enter && bError == false)
            {
                bContinue = false;
                continue;
            }

            if (!char.IsDigit(keyInfo.KeyChar)) continue;

            strResult += keyInfo.KeyChar.ToString();

            result = int.Parse(strResult);

            if (result >= min && result <= max)
            {
                bError = false;
                continue;
            }
            strResult = string.Empty;
            Console.CursorLeft = 0;
            bError = true;
        }
        Console.WriteLine();
        return result;
    }
}
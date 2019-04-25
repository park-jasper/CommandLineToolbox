using System;

namespace CommandLineTools.Helpers
{
    public static class TypeHelpers
    {
        public static int GetAsInt(object value)
        {
            switch (value)
            {
                case int ivalue:
                    return ivalue;
                case long lvalue:
                    return (int) lvalue;
                case float fvalue:
                    return (int) fvalue;
                case double dvalue:
                    return (int) dvalue;
                case decimal dvalue:
                    return (int) dvalue;
                case string svalue:
                    if (int.TryParse(svalue, out int result))
                    {
                        return result;
                    }
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to int.");
                default:
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to int.");
            }
        }

        public static double GetAsDouble(object value)
        {
            switch (value)
            {
                case int ivalue:
                    return (double) ivalue;
                case long lvalue:
                    return (double) lvalue;
                case float fvalue:
                    return (double) fvalue;
                case double dvalue:
                    return dvalue;
                case decimal dvalue:
                    return (double) dvalue;
                case string svalue:
                    if (double.TryParse(svalue, out var result))
                    {
                        return result;
                    }
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to double.");
                default:
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to double.");
            }
        }

        public static string GetAsString(object value)
        {
            switch (value)
            {
                case string svalue:
                    return svalue;
                default:
                    return value.ToString();
            }
        }
    }
}
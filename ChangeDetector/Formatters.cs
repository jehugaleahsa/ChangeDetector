using System;

namespace ChangeDetector
{
    public static class Formatters
    {
        public static string FormatString(string prop)
        {
            return prop;
        }

        public static string FormatDateTime(DateTime? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return prop.Value.ToString("G");
        }

        public static string FormatMoney(decimal? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return prop.Value.ToString("C");
        }

        public static string FormatInt32(Int32? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return prop.Value.ToString("N0");
        }

        public static string FormatId(Int32? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return prop.Value.ToString("D");
        }

        public static string FormatId(Int64? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return prop.Value.ToString("D");
        }

        public static string FormatBoolean(bool? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return FormatBoolean(prop.Value);
        }

        public static string FormatBoolean(bool prop)
        {
            return prop ? Boolean.TrueString : Boolean.FalseString;
        }

        public static string FormatPercent(decimal? prop)
        {
            if (prop == null)
            {
                return null;
            }
            decimal reduced = prop.Value / 100;
            return reduced.ToString("P");
        }

        public static string FormatGuid(Guid? prop)
        {
            if (prop == null)
            {
                return null;
            }
            return FormatGuid(prop.Value);
        }

        public static string FormatGuid(Guid prop)
        {
            return prop.ToString("D");
        }
    }
}

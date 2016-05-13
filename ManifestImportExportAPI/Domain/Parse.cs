using ManifestImportExportAPI.Models;
using System;

namespace ManifestImportExportAPI.Domain
{
    public static class Parse
    {
        public static bool? ParseBool(string boolString)
        {
            bool value;
            if (boolString.Equals("")) return new Nullable<bool>();
            var result = Boolean.TryParse(boolString, out value);
            return (bool?)(result ? value : new Nullable<bool>());
        }

        public static char? ParseChar(string charString)
        {
            char value;
            if (charString.Equals("")) return new Nullable<char>();
            var result = Char.TryParse(charString, out value);
            return (char?)(result ? value : new Nullable<char>());
        }

        public static int? ParseInt(string intString)
        {
            int value;
            if (intString.Equals("")) return new Nullable<int>();
            var result = int.TryParse(intString, out value);
            return (int?)(result ? value : new Nullable<int>());
        }

        public static decimal? ParseDecimal(string decimalString)
        {
            decimal value;
            if (decimalString.Equals("")) return new Nullable<decimal>();
            var result = decimal.TryParse(decimalString, out value);
            return (decimal?)(result ? value : new Nullable<decimal>());
        }

        public static DateTime ParseDate(string dateTime)
        {
            DateTime date;

            var success = DateTime.TryParse(dateTime, out date);
            if (success)
                return date;
            else
                return DateTime.MinValue;
        }

        public static DateTime ParseTime(string dateTime)
        {
            DateTime time;
            var success = DateTime.TryParse(dateTime, out time);
            if (success)
                return time;
            else
                return DateTime.MinValue;
        }

        public static string ParseString(string aString)
        {
            return aString.Trim();
        }

        public static APCRoleType ParseRoleType(string roletype)
        {
            APCRoleType type;
            var success = Enum.TryParse(roletype, out type);
            if (success)
                return type;
            else
                return APCRoleType.Consignor;
        }
    }
}
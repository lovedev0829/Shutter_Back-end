using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class PhoneHelper
    {
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length >= 10)
            {
                return $"{phoneNumber[^10..^7]}-{phoneNumber[^7..^4]}-{phoneNumber[^4..]}";
            }

            return phoneNumber ?? string.Empty;
        }

        public static string FormatPhoneNumberforGHL(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var match = Regex.Match(phoneNumber, @"^\+?\d{1}");
            return match.Value == "+1" ? phoneNumber : "+1" + phoneNumber.TrimStart('+');
        }
    }
}

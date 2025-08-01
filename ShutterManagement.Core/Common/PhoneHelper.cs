using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}

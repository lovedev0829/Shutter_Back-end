using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public enum MasterItemEnum
    {
        Contact,
        Phone,
        Assembly,
        Address,
    }
    public enum FileType
    {
        Image,
        Document
    }

    public enum ModuleName
    {
        [EnumMember(Value = "JOB")]
        Job,
        [EnumMember(Value = "EST")]
        Estimation,
        [EnumMember(Value = "ORD")]
        Order,
        [EnumMember(Value = "INV")]
        Invoice,
        [EnumMember(Value = "VEN")]
        Vendor
    }

    public enum Estimation
    {
        [EnumMember(Value = "New")]
        New,
        [EnumMember(Value = "Pending For Approval")]
        PendingForApproval,
        [EnumMember(Value = "Approved")]
        Approved,
    }

    public static class EnumValue
    {
        public static string GetEnumValue<T>(T enumValue) where T : Enum
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
            return ((EnumMemberAttribute)attributes[0]).Value;
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Stripe;
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

    public enum ReportFilterType
    {
        Pending,
        Paid,
        Duedate,
    }

    public enum FileType
    {
        Image,
        Document
    }

    public enum ArchiveType
    {
        Single,
        All
    }

    public enum AppliedFor
    {
        [EnumMember(Value = "All")]
        All,
        [EnumMember(Value = "Customers")]
        Customers
        
    }

    public enum PriceType
    {
        [EnumMember(Value = "Percentage")]
        Percentage,
        [EnumMember(Value = "Fixed")]
        Fixed

    }

    public enum DiscountType
    {
        [EnumMember(Value = "Percentage")]
        Percentage,
        [EnumMember(Value = "Fixed")]
        Fixed
    }

    public enum EstimateDocumentType
    {
        [EnumMember(Value = "ForEmail")]
        ForEmail,
        [EnumMember(Value = "DownloadPDF")]
        DownloadPDF,
        [EnumMember(Value = "OrderForEmail")]
        OrderForEmail,
        [EnumMember(Value = "InvoicePaymentForEmail")]
        InvoicePaymentForEmail,
        [EnumMember(Value = "OrderSheet")]
        OrderSheet
    }


    public enum ModuleName
    {
        [EnumMember(Value = "CUST")]
        Contact,
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

    // JobStatus Enum Value
    public enum JobStatus
    {
        [EnumMember(Value = "New")]
        New,
        [EnumMember(Value = "Appointment Scheduled")]
        Appointment_Scheduled,
        [EnumMember(Value = "Quote Sent")]
        Quote_Sent,
        [EnumMember(Value = "Won")]
        Won,
        [EnumMember(Value = "In-Production")]
        In_Production,
        [EnumMember(Value = "Installation Scheduled")]
        Installation_Scheduled,
        [EnumMember(Value = "Completed")]
        Completed
    }



    public enum Estimation
    {
        [EnumMember(Value = "New")]
        New,
        [EnumMember(Value = "PendingForApproval")]
        PendingForApproval,
        [EnumMember(Value = "Approved")]
        Approved,
        [EnumMember(Value = "Rejected")]
        Rejected,
        [EnumMember(Value = "GenerateOrder")]
        GenerateOrder
    }

    public enum PaymentStatus
    {
        [EnumMember(Value = "Unpaid")]
        Unpaid,
        [EnumMember(Value = "Paid")]
        Paid,
        [EnumMember(Value = "Draft")]
        Draft,
        [EnumMember(Value = "Manual")]
        Manual
    }


    public enum EmailTemplateType
    {
        [EnumMember(Value = "WelcomeEmail")]
        WelcomeEmail,
        [EnumMember(Value = "EstimatePDF")]
        EstimatePDF,
        [EnumMember(Value = "InvoicePDF")]
        InvoicePDF,
        [EnumMember(Value = "PaymentConfirmation")]
        PaymentConfirmation,
       
    }




    public enum VendorOrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "Approved")]
        Approved
    }

    public enum NotificationModule
    {
        [EnumMember(Value = "Job")]
        Job,
        [EnumMember(Value = "Contact")]
        Contact,        
        [EnumMember(Value = "Estimate")]
        Estimate,  
        [EnumMember(Value = "Tenant")]
        Tenant,
        [EnumMember(Value = "Partner")]
        SubTenant,   
        [EnumMember(Value = "User")]
        User
    }


    public enum DeleteType
    {
        [EnumMember(Value = "Stripe")]
        Stripe,
        [EnumMember(Value = "GHL")]
        GHL
    } 
    public enum OptionUnit
    {
        [EnumMember(Value = "Per Unit")]
        PerUnit,
        [EnumMember(Value = "Each Job")]
        EachJob,
        [EnumMember(Value = "Inch")]
        Inch,
        [EnumMember(Value = "CM")]
        CM,
        [EnumMember(Value = "Sqft")]
        Sqft,
        [EnumMember(Value = "lbs")]
        lbs
    }

    public enum DownloadSample
    {
        Material,
        Contact,
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core
{
    public class AppConstant
    {
        public static string DBConnectionString = string.Empty;
        public static string NotificationUsername = string.Empty;
        public static string NotificationPassword = string.Empty;
        public static string JWTSecretKey = string.Empty;
        public static string JwtIssuer = string.Empty;
        public static string JwtAudience = string.Empty;
        public static string JwtExpiryTime = string.Empty;
        public static bool IsAdminRolesShow = false;


        //Error Message//
        public static string FileVirtualPath { get; set; } = "";
        public static string FileUploadPath { get; set; } = "";
        public static string ExternalError { get; set; } = "Something went wrong. ";
        public static string ModelError { get; set; } = "Something incorrect in details. ";

        public static string SuccessMsg { get; set; } = "Success";
        public static string PasswordChangeMessage { get; set; } = "Password Change Successfully";
        public static string ComponentNotExists { get; set; } = "Component Does not exist.";
        public static string MaterialAlreadyExists { get; set; } = "Material is already exists.";

        public static string UIBaseURL { get; set; } = "";
        public static string ForgotPasswordSuccess { get; set; } = "Link has been sent on your register email.";

        public static string InsertSuccessMsg { get; set; } = "Data Added Successfully";
        public static string UpdateSuccessMsg { get; set; } = "Data Updated Successfully";
        public static string UploadSuccessMsg { get; set; } = "File Upload Successfully";
        public static string DeleteSuccessMsg { get; set; } = "Data Deleted Successfully";
        public static string NoDataFound { get; set; } = "Data not found.";
        public static string AlreadyExists { get; set; } = "Data already exists!";
        public static string CodeAlreadyExists { get; set; } = "Code already exists!";
        public static string IncorrectDetails { get; set; } = "Incorrect details";
        public static string InvalidCred { get; set; } = "Invalid Credentials";
        public static string InactivationMsg { get; set; } = "Inactivated Successfully";
        public static string FetchSuccessMsg { get; set; } = "Data fetched Successfully";
        public static string InvalidGuid { get; set; } = "Invalid Data Id";
        public static string LoginSuccessMessgae { get; set; } = "User loggedin successfully.";
        public static string NoUserFound { get; set; } = "No user found";
        public static string ExceptionError { get; set; } = "Server error. ";
        public static string InvalidExcelFile { get; set; } = "Please import only CSV or Excel files.";
        public static string ParentTenantIdNull { get; set; } = "Need parent Tenant ID to create or update Sub Tenant.. ";
        public static string TenantIdNull { get; set; } = "Need Tenant ID to update Sub Tenant.. ";
        public static string TenantNotFound { get; set; } = "Tenant not found.";
        public static string NotRelatesToTenant { get; set; } = "This data is not related to this tenant.";
        public static string AccessDenied { get; set; } = "Access Denied";
        public static Guid? LoggedInUserID { get; set; } = Guid.Empty;
        public static string LoggedInUserName { get; set; } = string.Empty;
        public static Guid? LoggedInTenantId { get; set; } = Guid.Empty;
        public static string LoggedInEmail { get; set; } = string.Empty;
        public static string LoggedInPhone { get; set; } = string.Empty;
        public static string LoggedInName { get; set; } = string.Empty;
        public static string LoggedInCompanyCode { get; set; } = string.Empty;
        public static string? LoggedInParentTenantId { get; set; }

        public const string AlphabaticalError = "Only alphabetic characters are allowed.";

        public const string ColorCodeError = "Not a valid color code";

        public const string ZipCodeError = " Invalid Zip Code Format";

        public const string NumericError = "Only Numerical values are allowed.";
        public const string SKUError = "Invlaid SKU format.";
    }
}

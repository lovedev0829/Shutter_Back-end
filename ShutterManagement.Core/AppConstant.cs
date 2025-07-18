using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core
{
    public class AppConstant
    {
        #region Database and JWT Configuration
        public static string DBConnectionString = string.Empty;
        public static string NotificationUsername = string.Empty;
        public static string NotificationPassword = string.Empty;
        public static string JWTSecretKey = string.Empty;
        public static string JwtIssuer = string.Empty;
        public static string JwtAudience = string.Empty;
        public static string JwtExpiryTime = string.Empty;
        public static bool IsAdminRolesShow = false;
        #endregion

        #region SMTP Configuration
        public static string SMTPServer { get; set; } = string.Empty;
        public static string SMTPPort { get; set; } = string.Empty;
        public static string SMTPUsername { get; set; } = string.Empty;
        public static string SMTPPassword { get; set; } = string.Empty;
        public static string UISitePath = string.Empty;
        #endregion

        #region File Upload Configuration
        public static string FileVirtualPath { get; set; } = string.Empty;
        public static string FileUploadPath { get; set; } = string.Empty;
        public static string UIBaseURL { get; set; } = string.Empty;
        #endregion

        #region Success Messages
        public static string SuccessMsg { get; set; } = "Success";
        public static string PasswordChangeMessage { get; set; } = "Password changed successfully";
        public static string ForgotPasswordSuccess { get; set; } = "Link has been sent to your registered email.";
        public static string InsertSuccessMsg { get; set; } = "Data added successfully";
        public static string UpdateSuccessMsg { get; set; } = "Data updated successfully";
        public static string UploadSuccessMsg { get; set; } = "File uploaded successfully";
        public static string DeleteSuccessMsg { get; set; } = "Data deleted successfully";
        public static string InactivationMsg { get; set; } = "Inactivated successfully";
        public static string FetchSuccessMsg { get; set; } = "Data fetched successfully";
        public static string LoginSuccessMessage { get; set; } = "User logged in successfully.";
        public static string InvoiceDownloadSuccessMsg { get; set; } = "Invoice downloaded successfully.";
        #endregion

        #region Error Messages
        public static string ExternalError { get; set; } = "Something went wrong.";
        public static string ModelError { get; set; } = "Something incorrect in details.";
        public static string ComponentNotExists { get; set; } = "Component does not exist.";
        public static string MaterialAlreadyExists { get; set; } = "Material already exists.";
        public static string NoDataFound { get; set; } = "Data not found.";
        public static string UserNotExist { get; set; } = "User does not exist.";
        public static string AlreadyExists { get; set; } = "Data already exists!";
        public static string CodeAlreadyExists { get; set; } = "Code already exists!";
        public static string IncorrectDetails { get; set; } = "Incorrect details";
        public static string InvalidCred { get; set; } = "Invalid credentials";
        public static string InvalidGuid { get; set; } = "Invalid data ID";
        public static string NoUserFound { get; set; } = "No user found";
        public static string ExceptionError { get; set; } = "Server error.";
        public static string InvalidExcelFile { get; set; } = "Please import only CSV or Excel files.";
        public static string ParentTenantIdNull { get; set; } = "Parent Tenant ID is required to create or update Sub Tenant.";
        public static string TenantIdNull { get; set; } = "Tenant ID is required to update Sub Tenant.";
        public static string TenantNotFound { get; set; } = "Tenant not found.";
        public static string NotRelatesToTenant { get; set; } = "This data is not related to this tenant.";
        public static string AccessDenied { get; set; } = "Access denied";
        #endregion

        #region Validation Error Messages
        public const string AlphabeticalError = "Only alphabetic characters are allowed.";
        public const string ColorCodeError = "Not a valid color code";
        public const string ZipCodeError = "Invalid zip code format";
        public const string NumericError = "Only numerical values are allowed.";
        public const string SKUError = "Invalid SKU format.";
        public const string AuthenticationExpiry = "Your authentication has expired. Please reconnect your email to continue sending emails.";
        #endregion

        #region Success Messages (Additional)
        public const string ExportSuccessMsg = "Export completed successfully.";
        public static string InvoicePaymentLinkandPDF { get; set; } = "Invoice Payment Link and PDF";
        #endregion

        #region Dashboard Icons (Should be moved to configuration)
        public static string InProductionImg { get; set; } = "/image/d-icon-1.svg";
        public static string NoOfContactsImg { get; set; } = "/image/d-icon-4.svg";
        public static string EstimateRevenueImg { get; set; } = "/image/d-icon-2.svg";
        public static string InProductionIcon { get; set; } = "/image/d-icon-1.svg";
        public static string AwardedIcon { get; set; } = "/image/d-icon-3.svg";
        #endregion

        #region User Session Properties
        public static Guid? LoggedInUserID { get; set; } = Guid.Empty;
        public static string LoggedInUserName { get; set; } = string.Empty;
        public static Guid? LoggedInTenantId { get; set; } = Guid.Empty;
        public static string LoggedInEmail { get; set; } = string.Empty;
        public static string LoggedInPhone { get; set; } = string.Empty;
        public static string LoggedInName { get; set; } = string.Empty;
        public static string LoggedInCompanyCode { get; set; } = string.Empty;
        public static bool IsTenant { get; set; } = false;
        public static string? LoggedInParentTenantId { get; set; }
        #endregion

        #region Email Configuration
        public static string Sub_ResetPassword = "Password reset successfully";
        public static string Sub_ForgotPassword = "Forgot Password";
        public static string Sub_WelcomeEmail = "Welcome Email";
        public static string Sub_EstimateApproval = "Estimate Approval Link and PDF";
        public static string Sub_EstimateOrderApproval = "Estimate For Approval";
        public static string Con_ResetPassword = "Your password has been reset successfully";
        #endregion

        #region Notification Messages
        public static string Create_Notification = "Created.";
        public static string UpdateStatus_Notification = "Status changed.";
        public static string Update_Notification = "Updated.";
        public static string Welcome_Notification = "Welcome";
        #endregion

        #region GHL Configuration
        public static string GHL_APIKey { get; set; } = string.Empty;
        public static string GHL_BaseAddress { get; set; } = string.Empty;
        //public static string GHL_PipelineId { get; set; } = string.Empty;
        //public static string GHL_LocationId { get; set; } = string.Empty; // Fixed typo: was "GHL_LocationId"
        //public static string GHL_RefreshToken { get; set; } = string.Empty;
        //public static string GHL_AccessToken { get; set; } = string.Empty;
        //public static string GHL_CalendarName { get; set; } = string.Empty;
        public static string GHL_ClientSecret { get; set; } = string.Empty;
        public static string GHL_ClientId { get; set; } = string.Empty;
        public static string GHL_RedirectUri { get; set; } = string.Empty;
        public static string GHL_Version { get; set; } = string.Empty;
        #endregion

        #region GHL MarketPlace Credentials
        public static string GHL_Marketplace_ClientId { get; set; } = string.Empty;
        public static string GHL_Marketplace_ClientSecret { get; set; } = string.Empty;
        public static string GHL_Marketplace_RedirectUri { get; set; } = string.Empty;
        public static string GHL_Marketplace_TokenURL { get; set; } = string.Empty;
        #endregion

        #region Microsoft365 Credentials (Fixed typo: was "Micorsoft365")
        public static string Microsoft365_ClientId { get; set; } = string.Empty;
        public static string Microsoft365_ClientSecret { get; set; } = string.Empty;
        public static string Microsoft365_RedirectUri { get; set; } = string.Empty;
        public static string Microsoft365_AuthUri { get; set; } = string.Empty;
        public static string Microsoft365_TenantId { get; set; } = string.Empty;
        public static string Microsoft365_Scopes { get; set; } = string.Empty;
        #endregion

        #region Google OAuth Configuration

        public static string Google_ClientId { get; set; } = string.Empty;
        public static string Google_ClientSecret { get; set; } = string.Empty;
        public static string Google_RedirectUri { get; set; } = string.Empty;
        public static string Google_AuthUrl { get; set; } = string.Empty;
        public static string Google_TokenUrl { get; set; } = string.Empty;

        #endregion

        #region Stripe Configuration (URLs moved to config)
        public static string Stripe_SecretKey { get; set; } = string.Empty;
        public static string Stripe_PublishableKey { get; set; } = string.Empty;
        public static string Stripe_WebhookSecret { get; set; } = string.Empty;
        public static string Stripe_CancelUrl { get; set; } = string.Empty;
        public static string Stripe_SuccessUrl { get; set; } = string.Empty;
        #endregion

        #region SMTP Host Configuration
        public static string SmtpHost_Google { get; set; } = string.Empty;
        public static string SmtpHost_MSOffice365 { get; set; } = string.Empty;
        #endregion

        #region Blob storedge config
        public static string AzureBlobStorage_ConnectionString { get; set; } = string.Empty;
        public static string AzureBlobStorage_ContainerName { get; set; } = string.Empty;

        #endregion Blob storedge config

        #region URL Shortening Configuration
        public static string BitlyApiToken { get; set; } = string.Empty;
        public static string BitlyDomain { get; set; } = string.Empty;
        #endregion URL Shortening Configuration
    }
}
using ShutterManagement.Core;

namespace ShutterManagement
{
    public static class Extension
    {
        public static void AddConstants(this IConfiguration Configuration)
        {
            // Database and JWT Configuration
            AppConstant.DBConnectionString = Configuration["ConnectionStrings:DefaultConnection"] ?? "";
            AppConstant.JWTSecretKey = Configuration["Jwt:SecretKey"] ?? "";
            AppConstant.JwtIssuer = Configuration["Jwt:Issuer"] ?? ""; 
            AppConstant.JwtAudience = Configuration["Jwt:Audience"] ?? "";
            AppConstant.JwtExpiryTime = Configuration["Jwt:ExpireTimeOut"] ?? "";
            
            // UI Configuration
            AppConstant.UIBaseURL = Configuration["UIBaseURL"] ?? "";
            
            // SMTP Configuration
            AppConstant.SMTPServer = Configuration["SMTPDetails:SMTPServer"] ?? "";
            AppConstant.SMTPPort = Configuration["SMTPDetails:SMTPPort"] ?? "";
            AppConstant.SMTPPassword = Configuration["SMTPDetails:SMTPPassword"] ?? "";
            AppConstant.SMTPUsername = Configuration["SMTPDetails:SMTPUsername"] ?? "";
            
            // SMTP Host Configuration
            AppConstant.SmtpHost_Google =  "smtp.gmail.com";
            AppConstant.SmtpHost_MSOffice365 =  "smtp.office365.com";
            
            // GHL Marketplace Configuration
            AppConstant.GHL_Marketplace_ClientId = Configuration["GoHighLevelOAuthSettings:ClientId"] ?? "";
            AppConstant.GHL_Marketplace_ClientSecret = Configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "";
            AppConstant.GHL_Marketplace_RedirectUri = Configuration["GoHighLevelOAuthSettings:RedirectUri"] ?? "";
            AppConstant.GHL_Marketplace_TokenURL = Configuration["GoHighLevelOAuthSettings:TokenUrl"] ?? "";
            AppConstant.GHL_Version = Configuration["GoHighLevelOAuthSettings:Version"] ?? "";
            
            // Microsoft365 Configuration (Fixed typo)
            AppConstant.Microsoft365_ClientId = Configuration["Microsoft365Credentials:ClientId"] ?? "";
            AppConstant.Microsoft365_ClientSecret = Configuration["Microsoft365Credentials:ClientSecret"] ?? "";
            AppConstant.Microsoft365_RedirectUri = Configuration["Microsoft365Credentials:RedirectUri"] ?? "";
            AppConstant.Microsoft365_AuthUri = Configuration["Microsoft365Credentials:AuthUri"] ?? "";
            AppConstant.Microsoft365_TenantId = Configuration["Microsoft365Credentials:tenantId"] ?? "";
            AppConstant.Microsoft365_Scopes = Configuration["Microsoft365Credentials:scopes"] ?? "";

            //Google OAuth Configuration
            AppConstant.Google_ClientId = Configuration["GoogleOAuth:ClientId"] ?? "";
            AppConstant.Google_ClientSecret = Configuration["GoogleOAuth:ClientSecret"] ?? "";
            AppConstant.Google_RedirectUri = Configuration["GoogleOAuth:RedirectUri"] ?? "";
            AppConstant.Google_AuthUrl = Configuration["GoogleOAuth:AuthUrl"] ?? "";
            AppConstant.Google_TokenUrl = Configuration["GoogleOAuth:TokenUrl"] ?? "";

            // Stripe Configuration
            AppConstant.Stripe_SecretKey = Configuration["Stripe:SecretKey"] ?? "";
            AppConstant.Stripe_PublishableKey = Configuration["Stripe:PublishableKey"] ?? "";
            AppConstant.Stripe_WebhookSecret = Configuration["Stripe:WebhookSecret"] ?? "";
            AppConstant.Stripe_CancelUrl = Configuration["Stripe:cancelUrl"] ?? "";
            AppConstant.Stripe_SuccessUrl = Configuration["Stripe:successUrl"] ?? "";
        }
    }
}

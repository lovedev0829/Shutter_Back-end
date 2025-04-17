using ShutterManagement.Core;

namespace ShutterManagement
{
    public static class Extension
    {
        public static void AddConstants(this IConfiguration Configuration)
        {
            
            AppConstant.DBConnectionString = Configuration["ConnectionStrings:DefaultConnection"] ?? "";
            AppConstant.JWTSecretKey = Configuration["Jwt:SecretKey"] ?? "";
            AppConstant.JwtIssuer = Configuration["Jwt:Issuer"] ?? ""; 
            AppConstant.JwtAudience = Configuration["Jwt:Audience"] ?? "";
            AppConstant.JwtExpiryTime = Configuration["Jwt:ExpireTimeOut"] ?? "";
            AppConstant.UIBaseURL = Configuration["UIBaseURL"] ?? "";
            AppConstant.SMTPServer = Configuration["SMTPDetails:SMTPServer"] ?? "";
            AppConstant.SMTPPort = Configuration["SMTPDetails:SMTPPort"] ?? "";
            AppConstant.SMTPPassword = Configuration["SMTPDetails:SMTPPassword"] ?? "";
            AppConstant.SMTPUsername = Configuration["SMTPDetails:SMTPUsername"] ?? "";
            AppConstant.GHL_Marketplace_ClientId = Configuration["GoHighLevelOAuthSettings:ClientId"] ?? "";
            AppConstant.GHL_Marketplace_ClientSecret = Configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "";
            AppConstant.GHL_Marketplace_RedirectUri = Configuration["GoHighLevelOAuthSettings:RedirectUri"]  ?? "";
            AppConstant.GHL_Marketplace_TokenURL = Configuration["GoHighLevelOAuthSettings:TokenUrl"] ?? "";
            AppConstant.GHL_Version= Configuration["GoHighLevelOAuthSettings:Version"] ?? "";
            //AppConstant.IsAdminRolesShow = Convert.ToBoolean(Configuration["IsAdminRolesShow"] ?? "false");
        }
    }
}

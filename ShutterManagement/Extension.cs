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
            //AppConstant.IsAdminRolesShow = Convert.ToBoolean(Configuration["IsAdminRolesShow"] ?? "false");
        }
    }
}

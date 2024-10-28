using ShutterManagement.Core;

namespace ShutterManagement
{
    public static class Extension
    {
        public static void AddConstants(this IConfiguration Configuration)
        {
            
            AppConstant.DBConnectionString = Configuration["ConnectionStrings:DefaultConnection"];
            AppConstant.JWTSecretKey = Configuration["Jwt:SecretKey"]; ;
            AppConstant.JwtIssuer = Configuration["Jwt:Issuer"]; ;
            AppConstant.JwtAudience = Configuration["Jwt:Audience"]; ;
            AppConstant.JwtExpiryTime = Configuration["Jwt:ExpireTimeOut"]; ;

            
        }
    }
}

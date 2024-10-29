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

            AppConstant.ExternalError = "Internal server error:";
            AppConstant.InsertSuccessMsg = "Data added successfully";
            AppConstant.UpdateSuccessMsg = "Updated successfully";
            AppConstant.DeletedSuccessMsg = "Deleted successfully";

            AppConstant.InvalidGuid = "Invalid ID specified.";
            AppConstant.FetchSuccessMsg = "Fetched Successfully";




        }
    }
}

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


        //Error Message//
        public static string ExternalError { get; set; } = string.Empty;
        public static string ModelError { get; set; } = string.Empty;

        public static string SuccessMsg {get; set; } = string.Empty; 
        
        public static string InsertSuccessMsg { get; set; } = string.Empty; 
        public static string UpdateSuccessMsg { get; set; }= string.Empty; 

        public static string DeletedSuccessMsg { get; set; }= string.Empty;
        public static string FetchSuccessMsg { get; set; }=string.Empty;    
        public  static string InvalidGuid { get; set; } = string.Empty; 
}
}

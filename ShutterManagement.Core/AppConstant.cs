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
        public static string ExternalError { get; set; } = "Something went wrong. ";
        public static string ModelError { get; set; } = "Something incorrect in details. ";

        public static string SuccessMsg { get; set; } = "Success";

        public static string InsertSuccessMsg { get; set; } = "Data Added Successfully";
        public static string UpdateSuccessMsg { get; set; } = "Data Updated Successfully";
        public static string DeleteSuccessMsg { get; set; } = "Data Deleted Successfully";
        public static string NotAddedMsg { get; set; } = "Data not added.";
        public static string AlreadyExists { get; set; } = "Data already exists!";
        public static string IncorrectDetails { get; set; } = "Incorrect details";

        public static string FetchSuccessMsg { get; set; } = "Data fetched Successfully";
        public static string InvalidGuid { get; set; } = "Invalid Data Id";
        public static string LoginSuccessMessgae { get; set; } = "User loggedin successfully.";
        public static string NoUserFound { get; set; } = "No user found";
        public static string ExceptionError { get; set; } = "Server error. ";
    }
}

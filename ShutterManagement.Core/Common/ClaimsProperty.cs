//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Security.Claims;
//using System.Net.Http;
//using Microsoft.AspNetCore.Http;

//namespace ShutterManagement.Core.Common
//{
//    public class ClaimsProperty
//    {
//        private readonly IHttpContextAccessor httpContext;

//        public ClaimsProperty(IHttpContextAccessor httpContext)
//        {
//            this.httpContext = httpContext;
//        }
//        public Guid GetUserId()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "UserId");
//            var value = Guid.Parse(claim?.Value ?? "");
//            AppConstant.LoggedInUserID = value;
//            return value;
//        }

//        public Guid GetTenantId()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "TenantId");
//            var value = Guid.Parse(claim?.Value ?? "");
//            AppConstant.LoggedInTenantId = value;
//            return value;
//        }

//        public string GetUsername()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "Username");
//            var value = claim?.Value ?? "";
//            AppConstant.LoggedInUserName = value;
//            return value;
//        }
//        public string GetEmail()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email);
//            var value = claim?.Value ?? "";
//            AppConstant.LoggedInEmail = value;
//            return value;
//        }
//        public string GetPhone()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "Phone");
//            var value = claim?.Value ?? "";
//            AppConstant.LoggedInPhone = value;
//            return value;
//        }
//        public string GetName()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name);
//            var value = claim?.Value ?? "";
//            AppConstant.LoggedInName = value;
//            return value;
//        }
//        public string GetCompanyCode()
//        {
//            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "CompanyCode");
//            var value = claim?.Value ?? "";
//            AppConstant.LoggedInCompanyCode = value;
//            return value;
//        }
//    }
//}

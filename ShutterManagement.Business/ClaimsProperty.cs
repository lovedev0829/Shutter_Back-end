using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ShutterManagement.Core;

namespace ShutterManagement.Business
{
    public class ClaimsProperty
    {
        private readonly IHttpContextAccessor httpContext;

        public ClaimsProperty(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
            AppConstant.LoggedInUserID = Guid.Parse(httpContext.HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
            AppConstant.LoggedInTenantId = Guid.Parse(httpContext.HttpContext.User.Claims.First(c => c.Type == "TenantId").Value);
            AppConstant.LoggedInUserName = httpContext.HttpContext.User.Claims.First(c => c.Type == "Username").Value;
            AppConstant.LoggedInCompanyCode = httpContext.HttpContext.User.Claims.First(c => c.Type == "CompanyCode").Value;
            AppConstant.LoggedInName = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            AppConstant.LoggedInPhone = httpContext.HttpContext.User.Claims.First(c => c.Type == "Phone").Value;
            AppConstant.LoggedInEmail = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        }
        public Guid GetUserId()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "UserId");
            var value = Guid.Parse(claim?.Value ?? "");
            AppConstant.LoggedInUserID = value;
            return value;
        }

        public Guid GetTenantId()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "TenantId");
            var value = Guid.Parse(claim?.Value ?? "");
            AppConstant.LoggedInTenantId = value;
            return value;
        }

        public string GetUsername()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "Username");
            var value = claim?.Value ?? "";
            AppConstant.LoggedInUserName = value;
            return value;
        }
        public string GetEmail()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email);
            var value = claim?.Value ?? "";
            AppConstant.LoggedInEmail = value;
            return value;
        }
        public string GetPhone()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "Phone");
            var value = claim?.Value ?? "";
            AppConstant.LoggedInPhone = value;
            return value;
        }
        public string GetName()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name);
            var value = claim?.Value ?? "";
            AppConstant.LoggedInName = value;
            return value;
        }
        public string GetCompanyCode()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "CompanyCode");
            var value = claim?.Value ?? "";
            AppConstant.LoggedInCompanyCode = value;
            return value;
        }
    }
}

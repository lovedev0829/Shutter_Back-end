using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ShutterManagement.Core;

namespace ShutterManagement.Business
{
    public class ClaimsProperty
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IConfiguration configuration;

        public ClaimsProperty(IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            this.httpContext = httpContext;
            this.configuration = configuration;
            if (httpContext.HttpContext.User.Identity.IsAuthenticated)
            {
                AppConstant.LoggedInUserID = Guid.Parse(httpContext.HttpContext.User.Claims.First(c => c.Type == "UserId").Value ?? "");
                AppConstant.LoggedInTenantId = Guid.Parse(httpContext.HttpContext.User.Claims.First(c => c.Type == "TenantId").Value ?? "");
                AppConstant.LoggedInUserName = httpContext.HttpContext.User.Claims.First(c => c.Type == "Username").Value;
                AppConstant.LoggedInCompanyCode = httpContext.HttpContext.User.Claims.First(c => c.Type == "CompanyCode").Value;
                AppConstant.LoggedInName = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
                AppConstant.LoggedInPhone = httpContext.HttpContext.User.Claims.First(c => c.Type == "Phone").Value;
                AppConstant.LoggedInEmail = httpContext.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                AppConstant.LoggedInParentTenantId = httpContext.HttpContext.User.Claims.First(c => c.Type == "ParentTenantId").Value;
                AppConstant.IsAdminRolesShow = Convert.ToBoolean(configuration["IsAdminRolesShow"] ?? "false");
                AppConstant.IsTenant = Convert.ToBoolean(configuration["IsTenant"] ?? "false");
            }
            
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

        public string GetParentTenantId()
        {
            var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "ParentTenantId");
            var value = claim?.Value ?? "";
            AppConstant.LoggedInParentTenantId = value.ToString();
            return value.ToString();
        }
        public bool GetIsAdminRolesShow()
        {
            var value = Convert.ToBoolean(configuration["IsAdminRolesShow"] ?? "false");
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

        //public bool IsTenant()
        //{
        //    var claim = httpContext.HttpContext.User.Claims.First(c => c.Type == "CompanyCode");
        //    var value = claim?.Value ?? "";
        //    //claimsProperty.GetCompanyCode() = value;
        //    return value;
        //    var claim = httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IsTenant");

        //    if (claim != null && bool.TryParse(claim.Value, out bool claimValue))
        //    {
        //        AppConstant.IsTenant = claimValue;
        //    }
        //    return AppConstant.IsTenant;
        //}

    }
}

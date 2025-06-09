using Azure;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

[ApiController]
[Route("api/[controller]")]
public class EmailOAuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;

    private readonly IConfiguration _config;
    private readonly IEmailConfigurationService emailConfigurationService;
    private readonly MS365Configuration mS365Configuration;

    public EmailOAuthController(GoogleAuthService authService, IConfiguration config,
        IEmailConfigurationService emailConfigurationService,MS365Configuration mS365Configuration)
    {
        _googleAuthService = authService;
        _config = config;
        this.emailConfigurationService = emailConfigurationService;
        this.mS365Configuration = mS365Configuration;
    }

    [HttpGet("LoginOauth")]
    [Authorize(Roles = "EmailConfigurationEdit")]
    public IActionResult StartGoogleAuth(string OauthType)
    {
       if(OauthType==EnumValue.GetEnumValue(EmailSendType.Google))
        {
            var Respose = _googleAuthService.GenerateGoogleOAuthUrl();
            return Ok(Respose);
        }
        else if (OauthType == EnumValue.GetEnumValue(EmailSendType.Microsoft365))
        {
            var ResponseMS  = mS365Configuration.GetAuthorizationurl().Result;
            return Ok(ResponseMS);
        }
        return Ok(new { message = "Invalid OAuth Type" });
 
    }



    [HttpGet("Callback")]
    [Authorize(Roles = "EmailConfigurationEdit")]
    public async Task<IActionResult> OAuthCallback(string OauthType, string code)
    {
        if (OauthType == string.Empty && code == string.Empty)
        {
            return Ok(new { message = "OauthType and code are required." });
        }
        if (OauthType == EnumValue.GetEnumValue(EmailSendType.Google))
        {
            var ResposeGoogle = await _googleAuthService.ExchangeCodeForTokenAsync(code);
            return Ok(ResposeGoogle);
        }
        else if (OauthType == EnumValue.GetEnumValue(EmailSendType.Microsoft365))
        {
            var ResponseMS = await mS365Configuration.ExchangeCodeForTokensAsync(code);
            return Ok(ResponseMS);
        }
        return Ok(new { message = "Invalid OAuth Type" });
    }

    //[HttpPost("send")]
    //public async Task<IActionResult> Send([FromBody] EmailRequest request)
    //{
    //    var getAccessTokenFromDb = emailConfigurationService.GetAllQuerable().Where(x=>x.From==request.FromEmail && x.TenantId==AppConstant.LoggedInTenantId).FirstOrDefault();
    //    if (getAccessTokenFromDb != null)
    //    {

    //        if (getAccessTokenFromDb.AccessExpirationtime < DateTime.Now)
    //        {
    //            getAccessTokenFromDb.AccessToken = await _googleAuthService.GoogleRefreshAccessToken(getAccessTokenFromDb.RefreshToken);
    //            getAccessTokenFromDb.AccessExpirationtime = DateTime.Now.AddHours(1);
    //        }
    //        var Request = new OAuthEmailRequest
    //        {
    //            FromEmail = request.FromEmail,
    //            ToEmail = request.ToEmail,
    //            Subject = request.Subject,
    //            Body = request.Body,
    //            UserEmail = request.FromEmail,
    //            AccessToken = getAccessTokenFromDb.AccessToken
    //        };

    //        await EmailHelper.SendEmailWithOAuth(Request);
    //    }
    //    else
    //    {
    //        return Ok(getAccessTokenFromDb);
    //    }
    //    return Ok(getAccessTokenFromDb);
    //}
}

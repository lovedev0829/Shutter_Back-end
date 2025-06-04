using MailKit.Security;
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
[Route("[controller]")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;

    private readonly IConfiguration _config;
    private readonly IEmailConfigurationService emailConfigurationService;

    public GoogleAuthController(GoogleAuthService authService, IConfiguration config,IEmailConfigurationService emailConfigurationService)
    {
        _googleAuthService = authService;
        _config = config;
        this.emailConfigurationService = emailConfigurationService;
    }

    [HttpGet("start-auth")]
    public IActionResult StartGoogleAuth()
    {
       
        var Respose = _googleAuthService.GenerateGoogleOAuthUrl();
        return  Ok(Respose);
 
    }



    [HttpGet("/api/auth/callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        var token = await _googleAuthService.ExchangeCodeForTokenAsync(code);

        return Ok(token);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] EmailRequest request)
    {
        var getAccessTokenFromDb = emailConfigurationService.GetAllQuerable().Where(x=>x.From==request.FromEmail && x.TenantId==AppConstant.LoggedInTenantId).FirstOrDefault();
        if (getAccessTokenFromDb != null)
        {

            if (getAccessTokenFromDb.AccessExpirationtime < DateTime.Now)
            {
                getAccessTokenFromDb.AccessToken = await _googleAuthService.RefreshAccessTokenAsync(getAccessTokenFromDb.RefreshToken);
                getAccessTokenFromDb.AccessExpirationtime = DateTime.Now.AddHours(1);
            }
            var Request = new GmailOAuthEmailRequest
            {
                FromEmail = request.FromEmail,
                ToEmail = request.ToEmail,
                Subject = request.Subject,
                Body = request.Body,
                UserEmail = request.FromEmail,
                AccessToken = getAccessTokenFromDb.AccessToken
            };

            await EmailHelper.SendEmailWithGoogleOAuth(Request);
        }
        else
        {
            return Ok(getAccessTokenFromDb);
        }
        return Ok(getAccessTokenFromDb);
    }
}

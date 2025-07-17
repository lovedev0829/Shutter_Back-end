using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using System.Net.Security;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Mail;
using ShutterManagement.Core.ViewModels;
using MailKit.Security;
using ShutterManagement.Core.ViewModels.Response;
using Azure;

namespace ShutterManagement.Core.Common
{
    public static class EmailHelper
    {
        public static async Task<IResponseVm<string>> SendEmail(OAuthEmailRequest request)
        {
            var responseVm = new ResponseVm<string>();

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(request.FromEmail));
                message.To.Add(MailboxAddress.Parse(request.ToEmail));
                message.Subject = request.Subject;
                message.ReplyTo.Add(MailboxAddress.Parse(request.ReplyToEmail ?? request.FromEmail));

                var builder = new BodyBuilder { HtmlBody = request.Body };

                if (request.Attachments != null && request.Attachments.Any())
                {
                    foreach (var attachment in request.Attachments)
                    {
                        builder.Attachments.Add(attachment.FileName, attachment.Stream,
                            ContentType.Parse(attachment.MediaType ?? "application/octet-stream"));
                    }
                }

                message.Body = builder.ToMessageBody();


                using var client = new MailKit.Net.Smtp.SmtpClient();
                switch (request?.EmailType?.Trim().ToLower())
                {
                    case "google":
                       
                        await client.ConnectAsync(AppConstant.SmtpHost_Google, 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(new SaslMechanismOAuth2(request.Username, request.AccessToken));
                        break;

                    case "microsoft365":
                        await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(new SaslMechanismOAuth2(request.Username, request.AccessToken));
                        break;

                    case "smtp":
                        var smtpCLient = new System.Net.Mail.SmtpClient(request.SmtpServer)
                        {
                            Port = request.Port,
                            Credentials = new NetworkCredential(request.Username, request.Password),
                            
                            EnableSsl = request.UseSsl

                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(request.FromEmail),
                            Subject = request.Subject,
                            Body = request.Body, // Use the string body directly
                            IsBodyHtml = true,
                        };
                        mailMessage.To.Add(new MailAddress(request.ToEmail));
                        mailMessage.ReplyToList.Add(new MailAddress(request.ReplyToEmail ?? request.FromEmail));
                        // Add attachments if any
                        if (request.Attachments != null && request.Attachments.Any())
                        {
                            foreach (var attachment in request.Attachments)
                            {
                                mailMessage.Attachments.Add(new System.Net.Mail.Attachment(attachment.Stream, attachment.FileName, attachment.MediaType ?? "application/octet-stream"));
                            }
                        }

                        smtpCLient.Send(mailMessage);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported email type.");
                }

               

                responseVm.IsSuccess = true;
                responseVm.Message = "Email sent successfully.";
            }
            catch (AuthenticationException authEx)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Authentication failed: {authEx.Message}";

                if (authEx.InnerException != null)
                {
                    responseVm.Message += $" | Inner Exception: {authEx.InnerException.Message}";
                }

                FileLogger.Log($"[OAuthEmail] AuthException: {authEx}");
            }
            catch (SmtpCommandException smtpEx)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"SMTP Command Error: {smtpEx.Message} (StatusCode: {smtpEx.StatusCode})";

                FileLogger.Log($"[OAuthEmail] SMTP Command Exception: {smtpEx}");
            }
            catch (SmtpProtocolException protoEx)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"SMTP Protocol Error: {protoEx.Message}";

                FileLogger.Log($"[OAuthEmail] SMTP Protocol Exception: {protoEx}");
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Unexpected error: {ex.Message}";

                if (ex.InnerException != null)
                {
                    responseVm.Message += $" | Inner: {ex.InnerException.Message}";
                }

                FileLogger.Log($"[OAuthEmail] General Exception: {ex}");
            }
            return responseVm;
        }
    }
}

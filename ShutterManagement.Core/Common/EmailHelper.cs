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
        public static IResponseVm<string> ResetPassword(string Email)
        {
            SendEmail vm = new SendEmail
            {
                ToEmail = Email,
                Subject = AppConstant.Sub_ResetPassword,
                Body = AppConstant.Con_ResetPassword,
            };
            var result = SendMail(vm);
            return result;
        }

        public static IResponseVm<string> ForgotPassword(string Email, string content)
        {
            SendEmail vm = new SendEmail
            {
                ToEmail = Email,
                Subject = AppConstant.Sub_ForgotPassword,
                Body =  content,
            };
            var result = SendMail(vm);
            return result;
        }

        public static IResponseVm<string> WelcomeEmail(string Email, string content)
        {
            SendEmail vm = new SendEmail
            {
                ToEmail = Email,
                Subject = AppConstant.Sub_WelcomeEmail,
                Body = content,
            };
            var result = SendMail(vm);
            return result;
        }
        public static string ReplacePlaceholdersForlWelcomeEmail(string template, WelcomeEmailModel model)
        {
            if (string.IsNullOrEmpty(template) || model == null)
                return template;

            return template
                .Replace("{{companyname}}", model.CompanyName ?? "")
                .Replace("{{username}}", model.UserName ?? "")
                .Replace("{{password}}", model.Password ?? "")
                .Replace("{{logo}}", model.Logo ?? "")
                .Replace("{{loginurl}}", model.loginUrl ?? "")
                .Replace("{{name}}", model.Name ?? "")
                .Replace("{{color}}", model.color ?? "");
        }

        public static bool SendEmailWithAttachment(string toEmail, string subject, string body, List<EmailAttachment> attachments = null)
        {
            try
            {
                var sendEmail = new SendEmail
                {
                    ToEmail = toEmail,
                    Subject = subject,
                    Body = body,
                    Attachments = attachments ?? new List<EmailAttachment>()
                };

                var emailResult = SendMail(sendEmail);

                return emailResult.IsSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public static IResponseVm<string> SendMail(SendEmail sendEmail)
        {
            IResponseVm<string> res = new ResponseVm<string>();
            string smtpServer = AppConstant.SMTPServer; // Replace with your domain's SMTP server (e.g., mail.yourdomain.com)
            int smtpPort = Convert.ToInt32(AppConstant.SMTPPort);  // For TLS. Use 465 for SSL.
            string smtpUser = AppConstant.SMTPUsername;  // Your full email address
            string smtpPassword = AppConstant.SMTPPassword;  // The password you created for the email account

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Shutter Management", smtpUser));
            emailMessage.To.Add(new MailboxAddress("Recipient", sendEmail.ToEmail));
            emailMessage.Subject = sendEmail.Subject;

            var multipart = new MimeKit.Multipart("mixed");

            // Add the text part
            var body = new TextPart("html")
            {
                Text = sendEmail.Body
            };
            multipart.Add(body);

            // Attach files if available
            if (sendEmail.Attachments != null && sendEmail.Attachments.Count() > 0)
            {
                foreach (var attach in sendEmail.Attachments)
                {
                    var attachment = new MimePart(attach.MediaType)
                    {
                        Content = new MimeContent(attach.Stream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attach.FileName
                    };
                    multipart.Add(attachment);
                }
            }

            // Set the email body
            emailMessage.Body = multipart;
            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    ServicePointManager.CheckCertificateRevocationList = false;
                    smtpClient.Connect(smtpServer, smtpPort, true); // Use SSL
                    smtpClient.Authenticate(smtpUser, smtpPassword);
                    smtpClient.Send(emailMessage);
                    smtpClient.Disconnect(true);
                    res.IsSuccess = true;
                    
                    res.Message = AppConstant.SuccessMsg;

                }
                catch (Exception ex)
                {
                    res.Message = AppConstant.ExceptionError + ex.Message;
                }
            }
            return res;
        }


        public static async Task<IResponseVm<string>> SendEmailWithOAuth(OAuthEmailRequest request)
        {
            var responseVm = new ResponseVm<string>();

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(request.FromEmail));
                message.To.Add(MailboxAddress.Parse(request.ToEmail));
                message.Subject = request.Subject;

                var builder = new BodyBuilder { HtmlBody = request.Body };

                if (request.Attachments?.Any() == true)
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
                        await client.ConnectAsync(AppConstant.smtpHost_Google, 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(new SaslMechanismOAuth2(request.UserEmail, request.AccessToken));
                        break;

                    case "microsoft365":
                        await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(new SaslMechanismOAuth2(request.UserEmail, request.AccessToken));
                        break;

                    case "smtp":
                        await client.ConnectAsync(request.SmtpServer,request.Port,request.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(request.Username, request.Password);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported email type.");
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                responseVm.IsSuccess = true;
                responseVm.Message = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error sending email: {ex.Message}";
            }

            return responseVm;
        }


        //public static async Task<IResponseVm<string>> SendEmailWithGoogleOAuth(GmailOAuthEmailRequest request)
        //{
        //    var responseVm = new ResponseVm<string>();

        //    try
        //    {
        //        var message = new MimeMessage();
        //        message.From.Add(MailboxAddress.Parse(request.FromEmail));
        //        message.To.Add(MailboxAddress.Parse(request.ToEmail));
        //        message.Subject = request.Subject;

        //        var builder = new BodyBuilder
        //        {
        //            HtmlBody = request.Body
        //        };

        //        // Add attachments if any
        //        if (request.Attachments != null && request.Attachments.Any())
        //        {
        //            foreach (var attachment in request.Attachments)
        //            {
        //                if (attachment.Stream != null && !string.IsNullOrWhiteSpace(attachment.FileName))
        //                {
        //                    builder.Attachments.Add(attachment.FileName, attachment.Stream, ContentType.Parse(attachment.MediaType ?? "application/octet-stream"));
        //                }
        //            }
        //        }

        //        message.Body = builder.ToMessageBody();

        //        using var client = new MailKit.Net.Smtp.SmtpClient();
        //        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

        //        var oauth2 = new SaslMechanismOAuth2(request.UserEmail, request.AccessToken);
        //        await client.AuthenticateAsync(oauth2);

        //        await client.SendAsync(message);
        //        await client.DisconnectAsync(true);

        //        responseVm.IsSuccess = true;
        //        responseVm.Message = "Email sent successfully.";
        //    }
        //    catch (AuthenticationException authEx)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Authentication failed: {authEx.Message}. " +
        //                            "Make sure access token is valid and matches user email.";
        //    }
        //    catch (SmtpCommandException smtpEx)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"SMTP error ({smtpEx.StatusCode}): {smtpEx.Message}";
        //    }
        //    catch (SmtpProtocolException protoEx)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Protocol error: {protoEx.Message}";
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Unexpected error: {ex.Message}";
        //    }

        //    return responseVm;
        //}



    }
}

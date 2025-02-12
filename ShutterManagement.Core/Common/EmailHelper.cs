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

    }
}

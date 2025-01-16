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

namespace ShutterManagement.Core.Common
{
    public class EmailHelper
    {

        public void SendMail(SendEmail sendEmail)
        {
            string smtpServer = "mail.btprojects.in"; // Replace with your domain's SMTP server (e.g., mail.yourdomain.com)
            int smtpPort = 465;  // For TLS. Use 465 for SSL.
            string smtpUser = "shutter@btprojects.in";  // Your full email address
            string smtpPassword = "ne716Bau@sllYzr7N";  // The password you created for the email account

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Shutter Management", smtpUser));
            emailMessage.To.Add(new MailboxAddress("Recipient", sendEmail.ToEmail));
            emailMessage.Subject = sendEmail.Subject;

            var body = new TextPart("plain")
            {
                Text = sendEmail.Body
            };

            emailMessage.Body = body;

            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    ServicePointManager.CheckCertificateRevocationList = false;
                    smtpClient.Connect(smtpServer, smtpPort, true); // Use SSL
                    smtpClient.Authenticate(smtpUser, smtpPassword);
                    smtpClient.Send(emailMessage);
                    smtpClient.Disconnect(true);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }

        //public void SendMail(SendEmail sendEmail)
        //    {

        //        // SMTP server settings for HostingRaja
        //        string smtpServer = "mail.btprojects.in"; // Replace with your domain's SMTP server (e.g., mail.yourdomain.com)
        //        int smtpPort = 465;  // For TLS. Use 465 for SSL.
        //        string smtpUser = "shutter@btprojects.in";  // Your full email address
        //        string smtpPassword = "ne716Bau@sllYzr7N";  // The password you created for the email account

        //        // Creating a new SmtpClient instance
        //        SmtpClient smtpClient = new SmtpClient(smtpServer)
        //        {
        //            Port = smtpPort,
        //            Credentials = new NetworkCredential(smtpUser, smtpPassword),
        //            EnableSsl = true // Use SSL/TLS
        //        };

        //        // Creating the email message
        //        MailMessage mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(smtpUser),
        //            Subject = sendEmail.Subject,
        //            Body = sendEmail.Body,
        //            IsBodyHtml = true // Set to true if you're sending HTML content
        //        };

        //        // Add recipient email address
        //        mailMessage.To.Add(sendEmail.ToEmail);  // Replace with the recipient's email address

        //        // Try to send the email
        //        try
        //        {
        //            smtpClient.Send(mailMessage);
        //            Console.WriteLine("Email sent successfully.");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Failed to send email. Error: {ex.Message}");
        //        }
        //        // SMTP Server configuration
        //    }
    }
}

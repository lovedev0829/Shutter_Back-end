using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class Templates
    {
        public static string Estimate { get; set; } = $@"
<div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px; margin: auto; background: #f9f9f9; padding: 20px; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
    <div style='background: white; padding: 20px; border-radius: 10px;'>

        <p style='font-size: 16px; margin-bottom: 10px;'>
            <strong>Dear {{CustomerName}},</strong>
        </p>

        <p style='font-size: 15px;'>
            Please review the attached <strong>estimate</strong> prepared for your consideration.
        </p>

        <p style='font-size: 15px;'>
            You can approve your estimate - <strong>{{EstimateCode}}</strong> by clicking the link below:
        </p>

        <div style='text-align: center; margin: 25px 0;'>
            <a href='{{InvoiceLink}}' 
               style='background-color: #0066cc; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold;'>
                View Estimate
            </a>
        </div>

        <p style='font-size: 15px;'>
            Once approved, an invoice will be automatically created for you to pay online or visit us to complete the payment.
        </p>

        <p style='font-size: 15px; margin-top: 30px;'>
            Thank you for your business!
        </p>

        <img src='{{Logo}}' alt='Company Logo' height='50' style='margin-top: 20px; display: block;' />

        <p style='font-size: 15px; margin-top: 20px;'>Best regards,</p>
        <p style='font-size: 15px; font-weight: bold;'>{{TenantCompanyName}}</p>
        <p style='font-size: 14px;'>Shutter Management and Estimation System</p>
    </div>
</div>";

        public static string Order { get; set; } = $@"
     <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px;'>
        <p><strong>Dear {{CustomerName}},</strong></p>

        <p>Please find your invoice attached for your reference.</p>

        <p>You can approve  your Order - {{OrderName}} by clicking the link below:</p>

        <p>
            <a href='{{InvoiceLink}}'>
               Order Approval link. 
            </a>
        </p>

        <p><strong>Thank you for your business!</strong></p>

       
        

        <p>
            <img src='{{Logo}}' alt='Company Logo' height='100px' 
                 style='margin-top: 10px; display: block;'>
        </p>
        <p>Best regards,</p>
        <p><strong>{{TenantCompanyName}}</strong></p>
        <p>Shutter Management and Estimation System</p>
    </div>";

        public static string Invoice { get; set; } = $@"
<div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0px 0px 10px rgba(0,0,0,0.1); padding: 20px; font-family: Arial, sans-serif; color: #333;'>

    <div style='background: #007bff; color: #fff; text-align: center; padding: 15px; border-radius: 8px 8px 0 0;'>
        <h2 style='margin: 0; font-size: 20px;'>Invoice Details</h2>
    </div>

    <div style='padding: 20px;'>
        <p style='font-size: 16px; margin: 10px 0;'><strong>Dear {{CustomerName}},</strong></p>
        <p style='font-size: 14px; margin: 10px 0;'>Please find your invoice attached for your reference.</p>

        <div style='background: #e9ecef; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Total Amount:</strong> {{TotalAmount}}</p>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Due Amount:</strong> {{DueAmount}}</p>
        </div>

        <p style='font-size: 14px; margin: 15px 0;'>{{paymentMessage}}</p>'

        {{PaymentLink}}                   

   
        <hr style='border: 0; height: 1px; background: #ddd; margin: 20px 0;'>

        <p style='font-size: 14px; text-align: center;'>Thank you for your business!</p>

        <div style='text-align: center; margin: 15px 0;'>
            <img src='{{Logo}}' alt='Company Logo' style='height: 50px;'>
        </div>

        <p style='text-align: center; font-size: 14px;'><strong>Best regards,</strong></p>
        <p style='text-align: center; font-size: 14px;'><strong>{{CompanyName}}</strong></p>
        <p style='text-align: center; font-size: 14px;'>Shutter Management and Estimation System</p>
    </div>
</div>";

        public static string InvoicePaymentLink { get; set; } = $@"
        <div style='text-align: center; margin: 20px 0;'>
            <a href='{{paymentLink}}' style='background: #28a745; color: #fff; padding: 12px 20px; text-decoration: none; font-size: 14px; border-radius: 5px; display: inline-block;'>Pay Now</a>
        </div>";
    }
}

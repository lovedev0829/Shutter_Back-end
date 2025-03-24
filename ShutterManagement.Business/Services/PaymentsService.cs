using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using System.Drawing;
using ShutterManagement.Stripe;
using Stripe;
using System.Xml.Linq;
using Azure;

namespace ShutterManagement.Business.Services
{

    public class PaymentsService : BaseRepository<ShutterDBContext, TblPayment>, IPaymentsService
    {
        private readonly ITenantService tenantService;
        private readonly IInvoiceService invoiceService;
        private readonly StripeService stripeService;
        private readonly IOrderService orderService;
        public Pagination filter = new Pagination();

        public PaymentsService(ITenantService tenantService ,IInvoiceService invoiceService 
            ,StripeService stripeService ,IOrderService orderService)
        {
            this.tenantService = tenantService;
            this.invoiceService = invoiceService;
            this.stripeService = stripeService;
            this.orderService = orderService;
        }

        #region Active_InActive_Payment
        public async Task<IResponseVm<string>> ActiveInActive(Guid PaymentId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var Payment = GetAllQuerable()
                        .Where(c => c.PaymentId == PaymentId
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    if (Payment != null)
                    {
                        Payment.IsActive = !Payment.IsActive;
                        Payment.UpdatedAt = DateTime.Now;
                        Payment.ModifiedBy = AppConstant.LoggedInUserID;

                        var result = await Update(Payment);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;

                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        #endregion Active_InActive_Payment

        #region CreatePayment
        public async Task<IResponseVm<string>> CreatePayment(CreatePayments model)
      {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var isTransactionExists = await AnyAsync(x =>
                    x.TransactionId == model.TransactionId && x.IsDelete == false && x.IsActive==true && x.TenantId == AppConstant.LoggedInTenantId);

                if (isTransactionExists)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "TransactionId Already Exists.";
                    return responseVm;
                }
                var invoice = invoiceService.GetAllQuerable()
                    .FirstOrDefault(i => i.InvoiceId == model.InvoiceId && i.IsActive == true);

                if (model.Amount > invoice.TotalAmount)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Payment cannot be greater than the total Amount";
                    return responseVm;
                }

                if (invoice.BalanceDue == 0)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Payment Already Paid.";
                    return responseVm;
                }

                if (model.Amount > invoice.BalanceDue)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Please enter an amount equal to or less than balance due.";
                    return responseVm;
                }

                // ✅ Check if Payment is Already Completed (Stripe Webhook Might Have Updated It)
                var existingPayment =  GetAllQuerable().Where(x => x.TransactionId == model.TransactionId && x.IsActive == true).FirstOrDefault();
                if (existingPayment != null && existingPayment.Status == "Paid")
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Payment already completed.";
                    return responseVm;
                }
                TblPayment tblPayments = new TblPayment
                {
                    PaymentId = Guid.NewGuid(),
                    InvoiceId = model.InvoiceId,
                    PaymentDate = model.PaymentDate,
                   // PaymentDueDate = model.PaymentDueDate,
                    Amount = model?.Amount,
                    PaymentMethod = model?.PaymentMethod,
                    TransactionId = model?.TransactionId?.Trim(),
                    Notes = model?.Notes,
                    Status = EnumValue.GetEnumValue(model.PaymentStatus),
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false
                };

                await Add(tblPayments);
                if (invoice == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invoice not found.";
                    await transaction.RollbackAsync();
                    return responseVm;
                }
                if (model.PaymentStatus == PaymentStatus.Unpaid)
                {
                    var invoiceDetail = dbContext.TblInvoices.Where(x => x.InvoiceId == model.InvoiceId).FirstOrDefault();
                    var job = dbContext.TblJobs.Where(x => x.JobId == invoiceDetail.JobId).FirstOrDefault();
                    var contactDetails = dbContext.TblContacts.Where(x => x.ContactId == job.ContactId).FirstOrDefault();

                    var paymentRequest = new PaymentRequest
                    {
                        CustomerEmail = contactDetails?.Email??"",
                        ProductName = $"Invoice Payment - {invoiceDetail?.InvoiceNumber}",
                        Amount = (long)(model.Amount * 100), // Convert to cents for Stripe
                        Quantity = 1,
                        InvoiceId = invoiceDetail?.InvoiceId.ToString(),
                        PaymentId = tblPayments?.PaymentId.ToString(),
                    };

                    string paymentLink = stripeService.CreateCheckoutSession(paymentRequest);

                    var emailBody = $@"
<div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; 
            box-shadow: 0px 0px 10px rgba(0,0,0,0.1); padding: 20px; 
            font-family: Arial, sans-serif; color: #333;'>

    <div style='background: #dc3545; color: #fff; text-align: center; padding: 15px; 
                border-radius: 8px 8px 0 0;'>
        <h2 style='margin: 0; font-size: 20px;'>Payment Request</h2>
    </div>

    <div style='padding: 20px;'>
        <p style='font-size: 16px; margin: 10px 0;'><strong>Dear {contactDetails?.FirstName} {contactDetails?.LastName},</strong></p>
        <p style='font-size: 14px; margin: 10px 0;'>We are requesting a payment for your invoice <strong>{invoiceDetail?.InvoiceNumber}</strong>.</p>

        <!-- Invoice Details -->
        <div style='background: #e9ecef; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Total Invoice Amount:</strong> {invoiceDetail?.TotalAmount:C}</p>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Due Amount:</strong> {invoiceDetail?.BalanceDue:C}</p>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Amount Requested for Payment:</strong> {model.Amount:C}</p>
        </div>

        <p style='font-size: 14px; margin: 15px 0;'>Please click the button below to complete your payment:</p>

        <div style='text-align: center; margin: 20px 0;'>
            <a href='{paymentLink}' style='background: #007bff; color: #fff; padding: 12px 20px; 
               text-decoration: none; font-size: 14px; border-radius: 5px; display: inline-block;'>Pay Now</a>
        </div>

        <hr style='border: 0; height: 1px; background: #ddd; margin: 20px 0;'>

        <p style='font-size: 14px; text-align: center;'>Thank you for your prompt attention to this matter.</p>
    </div>
</div>";


                    // ✅ Send Payment Email
                    EmailHelper.SendEmailWithAttachment(
                        contactDetails?.Email,
                        "Invoice Payment Reminder",
                        emailBody
                    );

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Payment link email sent successfully.";
                }

                if (model.PaymentStatus == PaymentStatus.Paid)
                {
                    invoice.PaidAmount += model.Amount ?? 0;
                    invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
                    invoice.Status = EnumValue.GetEnumValue(PaymentStatus.Paid);
                    invoice.UpdatedAt = DateTime.Now;
                    invoice.ModifiedBy = AppConstant.LoggedInUserID;
                    await invoiceService.Update(invoice);
                    var invoivedetail = dbContext.TblInvoices.Where(x => x.InvoiceId == model.InvoiceId).FirstOrDefault();
                    var job = dbContext.TblJobs.Where(x => x.JobId == invoivedetail.JobId).FirstOrDefault();
                    var customerDetails = dbContext.TblContacts.Where(x => x.ContactId == job.ContactId).FirstOrDefault();


                    SendPaymentConfirmationEmail(
                        customerDetails?.Email,
                        customerDetails?.FirstName ?? "",
                        customerDetails?.LastName ?? "",
                        invoice?.InvoiceNumber ?? "",
                        model.Amount ?? 0,
                        invoice.BalanceDue
                        );


                }
                await transaction.CommitAsync();
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        #endregion CreatePayment

        #region DeletePayment
        public async Task<IResponseVm<string>> DeletePayment(Guid PaymentId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var Payment = GetAllQuerable()
                        .Where(c => c.PaymentId == PaymentId
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                        .FirstOrDefault();


                    if (Payment != null && Payment.Status == EnumValue.GetEnumValue(PaymentStatus.Paid))
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = $"Payment Paid Already {AppConstant.AccessDenied}";
                        return responseVm;
                    }

                    if (Payment != null)
                    {
                        Payment.IsDelete = !Payment.IsDelete;
                        Payment.UpdatedAt = DateTime.Now;
                        Payment.ModifiedBy = AppConstant.LoggedInUserID;

                        var result = await Update(Payment);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.DeleteSuccessMsg;
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        #endregion DeletePayment

        #region Get_All_Payments
        public async Task<IResponseVm<List<PaymentsVM>>> GetAllPayment(Pagination filter)
        {
            IResponseVm<List<PaymentsVM>> responseVm = new ResponseVm<List<PaymentsVM>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var PaymentsQuery = GetAllQuerable().Where(x=>x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x=>x.CreatedAt).
                                    Select(i=>  new PaymentsVM
                                    {
                                        PaymentId = i.PaymentId,
                                        InvoiceId = i.InvoiceId,
                                        PaymentDate = i.PaymentDate,
                                        //PaymentDueDate = i.PaymentDueDate,
                                        Amount = i.Amount,
                                        PaymentMethod = i.PaymentMethod,
                                        TransactionId = i.TransactionId,
                                        Notes = i.Notes,
                                        PaymentStatus = i.Status == EnumValue.GetEnumValue(PaymentStatus.Unpaid) ? 0 : 1,
                                        Status = i.Status,
                                        IsActive = i.IsActive,
                                    }).ToList();
                filter.SearchValue = filter?.SearchValue.Trim().ToLower();
                var filteredPayments = PaymentsQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.TransactionId.Contains(filter.SearchValue.Trim())) ||
                            (x.PaymentId.ToString() == filter.SearchValue.Trim()) ||
                            (x.InvoiceId.ToString() == filter.SearchValue.Trim()))).ToList();

                

                var paginatedInvoices = filteredPayments
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedInvoices;
                responseVm.TotalRecords = filteredPayments.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        #endregion Get_All_Payments

        #region UpdatePayment
        public async Task<IResponseVm<string>> UpdatePayment(UpdatePayments updatePayment)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var payment = GetAllQuerable()
                    .FirstOrDefault(c => c.PaymentId == updatePayment.PaymentId
                                         && c.TenantId == AppConstant.LoggedInTenantId
                                         && c.IsDelete == false);

                if (payment == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                var invoice = invoiceService.GetAllQuerable()
                    .FirstOrDefault(i => i.InvoiceId == payment.InvoiceId && i.IsActive == true);

                if (invoice == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invoice not found.";
                    await transaction.RollbackAsync();
                    return responseVm;
                }

                if (updatePayment.Amount > invoice.BalanceDue)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Please enter an amount equal to or less than balance due.";
                    await transaction.RollbackAsync();
                    return responseVm;
                }

                payment.TransactionId = updatePayment.TransactionId?.Trim();
                payment.PaymentDate = updatePayment.PaymentDate;
                //payment.PaymentDueDate = updatePayment.PaymentDueDate;
                payment.PaymentMethod = updatePayment.PaymentMethod;
                payment.Amount = updatePayment.Amount;
                payment.Status = EnumValue.GetEnumValue(updatePayment.PaymentStatus);
                payment.Notes = updatePayment.Notes;
                payment.UpdatedAt = DateTime.Now;
                payment.ModifiedBy = AppConstant.LoggedInUserID;


                await Update(payment);

                if (updatePayment.PaymentStatus == PaymentStatus.Paid)
                {
                    
                    invoice.PaidAmount += updatePayment.Amount ?? 0;
                    invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
                    invoice.Status = EnumValue.GetEnumValue(PaymentStatus.Paid);
                    invoice.UpdatedAt = DateTime.Now;
                    invoice.ModifiedBy = AppConstant.LoggedInUserID;
                    await invoiceService.Update(invoice);

                    var invoivedetail = invoiceService.GetAllQuerable().Where(x => x.InvoiceId == updatePayment.InvoiceId).FirstOrDefault();
                    var job = dbContext.TblJobs.Where(x => x.JobId == invoivedetail.JobId).FirstOrDefault();
                    var customerDetails = dbContext.TblContacts.Where(x => x.ContactId == job.ContactId).FirstOrDefault();

                    var orderdetails = orderService.GetAllQuerable().Where(x => x.EstimateId == invoivedetail.EstimateId).FirstOrDefault();
                    if (orderdetails != null)
                    {
                        orderdetails.PaidAmount += updatePayment.Amount ?? 0;
                        orderdetails.BalanceDue = orderdetails.GrandTotal - orderdetails.PaidAmount;
                        orderdetails.UpdatedAt = DateTime.Now;
                        orderdetails.ModifiedBy = AppConstant.LoggedInUserID;
                        await orderService.Update(orderdetails);
                    }

                    SendPaymentConfirmationEmail(
                        customerDetails?.Email,
                        customerDetails?.FirstName ?? "",
                        customerDetails?.LastName ?? "",
                        invoice?.InvoiceNumber ?? "",
                        updatePayment.Amount ?? 0,
                        invoice.BalanceDue
                        );
                }
                await transaction.CommitAsync();

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        #endregion UpdatePayment

        #region Get_Payment_Details
        public async Task<IResponseVm<StripePaymentVm>> GetPaymentDetails(string Tranjectionid, string PaymentId)
        {
            IResponseVm<StripePaymentVm> responseVm = new ResponseVm<StripePaymentVm>();

            try
            {
                var payment =  dbContext.TblStripePayments
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
                    .Where(x => string.IsNullOrEmpty(Tranjectionid) || x.StripeTransactionId == Tranjectionid)
                    .Where(x => string.IsNullOrEmpty(PaymentId) || x.PaymentId.ToString() == PaymentId)
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(i => new StripePaymentVm
                    {
                        Id = i.Id,
                        PaymentId = i.PaymentId,
                        InvoiceId = i.InvoiceId,
                        PaymentDate = i.PaymentDate,
                        PaymentAmount = i.PaymentAmount,
                        PaymentMethod = i.PaymentMethod,
                        PaymentStatus = i.PaymentStatus,
                        TenantId = i.TenantId,
                        CreatedDate = i.CreatedDate,
                        ModifiedAt = i.ModeifiedAt,
                        WebhookResponse = i.WebhookResponse,
                        StripeTranjectionId=i.StripeTransactionId,
                        StripeEventType = i.StripeEventType,
                        CustomerEmail=i.CustomerEmail??"",
                        CustomerName=i.CustomerName??"",
                        PaymentMode=i.PaymentMode??"",
                        Status=i.Status??""
                        
                    })
                    .FirstOrDefault(); 
                if (payment != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Data = payment; 
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No payment record found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
        #endregion Get_Payment_Details

        #region EstimateAmount
        public async Task<IResponseVm<EstimateAmountVm>> GetEstimateAmmount(Guid? EstimateId)
        {
            IResponseVm<EstimateAmountVm> responseVm = new ResponseVm<EstimateAmountVm>();

            try
            {
                if (EstimateId == null || EstimateId == Guid.Empty)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invalid Estimate ID.";
                    return responseVm;
                }

                var estimate = dbContext.TblOrders
                    .Where(e => e.EstimateId == EstimateId && e.IsDelete == false && e.Status == EnumValue.GetEnumValue(Estimation.Approved))
                    .OrderByDescending(e => e.Version)
                    .ThenByDescending(e => e.CreatedAt)
                    .FirstOrDefault();

                if (estimate == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No approved estimate found.";
                    return responseVm;
                }

                decimal totalOrderAmount = estimate.GrandTotal ?? 0;
                decimal halfAmount = totalOrderAmount / 2;

                var invoice = dbContext.TblInvoices
                    .Where(x => x.EstimateId == EstimateId && x.IsActive == true && x.IsDelete == false)
                    .FirstOrDefault();

                decimal totalInvoiceAmount = 0;
                decimal totalPayments = 0;
                decimal dueBalance = 0;

                if (invoice != null)
                {
                    totalInvoiceAmount = invoice.TotalAmount ?? 0;

                    totalPayments = dbContext.TblPayments
                        .Where(x => x.InvoiceId == invoice.InvoiceId && x.IsActive == true && x.IsDelete == false)
                        .Sum(x => x.Amount ?? 0);

                    dueBalance = totalInvoiceAmount - totalPayments;
                }

                var invoices = await invoiceService.GetAllInvoice(filter);
                var filteredInvoices = invoices.Data?
                    .Where(x =>x.EstimateId == EstimateId)
                    .ToList() ?? new List<InvoiceVM>();

                var payments = await GetAllPayment(filter);

                var invoiceIds = filteredInvoices.Select(i => i.InvoiceId).ToList();

                var filteredPayments = payments.Data?
                .Where(x => x.InvoiceId.HasValue && invoiceIds.Contains(x.InvoiceId.Value))
                .ToList() ?? new List<PaymentsVM>();

                var estimateAmountVm = new EstimateAmountVm
                {
                    TotalOrderAmount = totalOrderAmount,
                    TotalInvoiceAmount = totalInvoiceAmount,
                    TotalPaidAmount = totalPayments,
                    DueBalance = totalOrderAmount - totalPayments,
                    HalfAmount = halfAmount,
                    Invoices = filteredInvoices.Select(x=> new InvoicePayment_VM
                    {
                        InvoiceId = x.InvoiceId ,
                        BalanceDue = x.BalanceDue,
                        ContactFirstName = x.ContactFirstName,
                        ContactLastName = x.ContactLastName,
                        DueDate = x.DueDate,
                        EstimateId = x.EstimateId,
                        EstimateName = x.EstimateName,
                        InvoiceNumber = x.InvoiceNumber,
                        IssueDate = x.IssueDate,
                        PaidAmount = x.PaidAmount,
                        Status = x.Status,
                        TotalAmount = x.TotalAmount,
                        
                        Payments = filteredPayments.Where(y=>y.InvoiceId==x.InvoiceId).ToList(),
                    }).ToList(),
                };
                

                responseVm.IsSuccess = true;
                responseVm.Data = estimateAmountVm;
                responseVm.Message = $"Total Order: {totalOrderAmount:C}, Invoice Total: {totalInvoiceAmount:C}, Payments Made: {totalPayments:C}, Due Balance: {totalOrderAmount - totalPayments:C}";

                FileLogger.Log($"Returning Total Order: {totalOrderAmount:C}, Invoice Total: {totalInvoiceAmount:C}, Payments Made: {totalPayments:C}, Due Balance: {totalOrderAmount - totalPayments:C}");
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "Error fetching estimate amount: " + ex.Message;
            }

            return responseVm;
        }

        #endregion EstimateAmount

        #region SendPaymentConfirmationEmail
        private void SendPaymentConfirmationEmail(string customerEmail, string firstName, string lastName, string invoiceNumber, decimal paidAmount, decimal? remainingBalance)
        {
            try
            {
                var tenantDetails = tenantService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true);

                var tenantInfoDetails = dbContext.TblTenantBrandingInfos
                    .FirstOrDefault(x => x.Id == tenantDetails.BrandingInfo);

                FileLogger.Log($"Sending payment confirmation email. Tenant Details: {tenantDetails}");

                Guid templateId = Guid.Parse("11B8D318-52A5-4060-BEAD-A69800B904CA");
                var invoiceTemplateValue = dbContext.TblTemplates
                    .FirstOrDefault(c => c.TemplateId == templateId);

                if (invoiceTemplateValue == null)
                {
                    FileLogger.Log("Error: Email template not found.");
                    return;
                }

                var bodyContent = invoiceTemplateValue.TemplateValue;
                var invoiceDataModel = new PaymentConfirmationModel
                {
                    CustomerEmail = customerEmail,
                    FirstName = firstName,
                    LastName = lastName,
                    InvoiceNumber = invoiceNumber,
                    PaidAmount = paidAmount,
                    RemainingBalance = remainingBalance,
                    CompanyName = tenantInfoDetails?.CompanyName ?? "",
                    TenantEmail = tenantInfoDetails?.Email ?? "",
                    CompanyLogo = tenantInfoDetails?.Logo ?? ""
                };

                var emailBody = ReplacePaymentConfirmationPlaceholders(bodyContent, invoiceDataModel);
                EmailHelper.SendEmailWithAttachment(customerEmail, "Payment Confirmation", emailBody);
                FileLogger.Log("Payment confirmation email sent successfully.");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error sending payment confirmation email: {ex.Message}");
            }
        }

        #endregion SendPaymentConfirmationEmail

        #region ReplacePaymentConfirmationPlaceholders
        private string ReplacePaymentConfirmationPlaceholders(string template, PaymentConfirmationModel data)
        {
            return template
                .Replace("{firstName}", data.FirstName ?? "")
                .Replace("{lastName}", data.LastName ?? "")
                .Replace("{invoiceNumber}", data.InvoiceNumber ?? "")
                .Replace("{paidAmount:C}", data.PaidAmount.ToString())
                .Replace("{remainingBalance:C}", data.RemainingBalance?.ToString() ?? "$0.00")
                .Replace("{tenantInfoDetails?.CompanyName}", data.CompanyName ?? "")
                .Replace("{tenantInfoDetails?.Email}", data.TenantEmail ?? "")
                .Replace("{tenantInfoDetails?.Logo}", data.CompanyLogo ?? "");
        }
        #endregion ReplacePaymentConfirmationPlaceholders

    }

}

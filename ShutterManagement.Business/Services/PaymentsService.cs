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
using Microsoft.EntityFrameworkCore.Storage;

namespace ShutterManagement.Business.Services
{

    public class PaymentsService : BaseRepository<ShutterDBContext, TblPayment>, IPaymentsService
    {
        #region Private Fields
        private readonly ITenantService tenantService;
        private readonly IInvoiceService invoiceService;
        private readonly ClaimsProperty claimsProperty;
        private readonly StripeService stripeService;
        private readonly IOrderService orderService;
        private readonly EmailSendHelperService emailSendHelperService;
        public Pagination filter = new Pagination();
        #endregion Private Fields
        public PaymentsService(ITenantService tenantService ,IInvoiceService invoiceService ,ClaimsProperty claimsProperty
            ,StripeService stripeService ,IOrderService orderService,EmailSendHelperService emailSendHelperService)
        {
            this.tenantService = tenantService;
            this.invoiceService = invoiceService;
            this.claimsProperty = claimsProperty;
            this.stripeService = stripeService;
            this.orderService = orderService;
            this.emailSendHelperService = emailSendHelperService;
        }

        #region Active_InActive_Payment
        public async Task<IResponseVm<string>> ActiveInActive(Guid PaymentId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var Payment = GetAllQuerable()
                        .Where(c => c.PaymentId == PaymentId
                                   && c.TenantId == claimsProperty.GetTenantId()
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
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var isTransactionExists = await AnyAsync(x =>
                    x.TransactionId == model.TransactionId && x.IsDelete == false && x.IsActive==true && x.TenantId == claimsProperty.GetTenantId());

                if (isTransactionExists)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "TransactionId Already Exists.";
                    return responseVm;
                }
                var invoice = invoiceService.GetAllQuerable()
                    .FirstOrDefault(i => i.InvoiceId == model.InvoiceId && i.IsActive == true && i.TenantId== claimsProperty.GetTenantId());

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
                var existingPayment =  GetAllQuerable().Where(x => x.TransactionId == model.TransactionId && x.IsActive == true && x.TenantId==claimsProperty.GetTenantId()).FirstOrDefault();
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
                    PaymentTypeStatus = EnumValue.GetEnumValue(PaymentStatus.Manual),
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
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
                bool isValidStripe = IsStripeApiKeyValid();
                List<EmailAttachment> emailAttachments = new List<EmailAttachment>();
                if (isValidStripe)
                {
                    if (model.PaymentStatus == PaymentStatus.Unpaid)
                    {
                        var invoiceDetail = dbContext.TblInvoices.Where(x => x.InvoiceId == model.InvoiceId).FirstOrDefault();
                        var job = dbContext.TblJobs.Where(x => x.JobId == invoiceDetail.JobId).FirstOrDefault();
                        var contactDetails = dbContext.TblContacts.Where(x => x.ContactId == job.ContactId).FirstOrDefault();

                        var paymentRequest = new PaymentRequest
                        {
                            CustomerEmail = contactDetails?.Email ?? "",
                            ProductName = $"Invoice Payment - {invoiceDetail?.InvoiceNumber}",
                            Amount = (long)(model.Amount * 100), // Convert to cents for Stripe
                            Quantity = 1,
                            InvoiceId = invoiceDetail?.InvoiceId.ToString(),
                            PaymentId = tblPayments?.PaymentId.ToString(),
                        };

                        string paymentLink = stripeService.CreateCheckoutSession(paymentRequest);

                        var emailBody = Templates.PaymentLink;

                       var replacedDataEmailBody = emailSendHelperService.PlaceholderReplace(
                           emailBody,
                            new
                            {
                                CustomerName = contactDetails?.FirstName  + " " + contactDetails?.LastName,
                                invoiceNumber = invoiceDetail?.InvoiceNumber ?? "",
                                PaymentLink = paymentLink,
                                TotalAmount = invoiceDetail?.TotalAmount ?? 0,
                                BalanceDue = invoiceDetail?.BalanceDue ?? 0,
                                amount = model.Amount ?? 0
                            }
                        );
                        var res = emailSendHelperService.GetEmailConfigAndSendEmail(
                            contactDetails?.Email,
                            replacedDataEmailBody,
                            "Payment Reminder",
                            contactDetails?.Email
                        );

                        responseVm.IsSuccess = true;
                        responseVm.Message = "Payment link email sent successfully.";
                    }
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
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var Payment = GetAllQuerable()
                        .Where(c => c.PaymentId == PaymentId
                                   && c.TenantId == claimsProperty.GetTenantId()
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
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var PaymentsQuery = GetAllQuerable().Where(x=>x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).OrderByDescending(x=>x.CreatedAt).
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



                var queryable = filteredPayments.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);
                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords;
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

        #region UpdatePayment details
        public async Task<IResponseVm<string>> UpdatePayment(UpdatePayments updatePayment)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                if (!await IsTenantValid())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var payment = GetPaymentById(updatePayment.PaymentId);
                if (payment == null)
                    return  await GetErrorResponse(responseVm, AppConstant.NoDataFound);

                var invoice = GetInvoiceById(payment.InvoiceId);
                if (invoice == null)
                    return await GetErrorResponse(responseVm, "Invoice not found.", transaction);

                if (updatePayment.Amount > invoice.BalanceDue)
                    return await GetErrorResponse(responseVm, "Please enter an amount equal to or less than balance due.", transaction);


                UpdatePaymentDetails(payment, updatePayment);
                await Update(payment);

                if (updatePayment.PaymentStatus == PaymentStatus.Paid)
                {
                    await UpdateInvoicePayment(invoice, updatePayment);
                   // await UpdateOrderPayment(invoice, updatePayment);

                    var customerDetails = GetCustomerDetails(invoice.InvoiceId);
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
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }
     


        private async Task<bool> IsTenantValid()
        {
            return await tenantService.AnyAsync(x =>
                x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true && x.IsDelete==false);
        }


        private TblPayment GetPaymentById(Guid? paymentId)
        {
            return GetAllQuerable().FirstOrDefault(c =>
                c.PaymentId == paymentId &&
                c.TenantId == claimsProperty.GetTenantId() &&
                c.IsDelete==false);
        }

        private TblInvoice GetInvoiceById(Guid? invoiceId)
        {
            return invoiceService.GetAllQuerable().FirstOrDefault(i => i.InvoiceId == invoiceId && i.IsActive==true);
        }

        private void UpdatePaymentDetails(TblPayment payment, UpdatePayments updatePayment)
        {
            payment.TransactionId = updatePayment.TransactionId?.Trim();
            payment.PaymentDate = updatePayment.PaymentDate;
            payment.PaymentMethod = updatePayment.PaymentMethod;
            payment.Amount = updatePayment.Amount;
            payment.Status = EnumValue.GetEnumValue(updatePayment.PaymentStatus);
            payment.Notes = updatePayment.Notes;
            payment.UpdatedAt = DateTime.Now;
            payment.ModifiedBy = AppConstant.LoggedInUserID;
        }

 
        private async Task UpdateInvoicePayment(TblInvoice invoice, UpdatePayments updatePayment)
        {
            invoice.PaidAmount += updatePayment.Amount ?? 0;
            invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
            invoice.Status = EnumValue.GetEnumValue(PaymentStatus.Paid);
            invoice.UpdatedAt = DateTime.Now;
            invoice.ModifiedBy = AppConstant.LoggedInUserID;

            await invoiceService.Update(invoice);
        }

        //private async Task UpdateOrderPayment(TblInvoice invoice, UpdatePayments updatePayment)
        //{
        //    var order = orderService.GetAllQuerable()
        //        .FirstOrDefault(x => x.EstimateId == invoice.EstimateId);

        //    if (order == null) return;

        //    if (order.BalanceDue == 0)
        //    {
        //        order.ExtraBalance += updatePayment.Amount ?? 0;

        //    }
        //    else
        //    {
        //        order.PaidAmount += updatePayment.Amount ?? 0;
        //        order.BalanceDue = Math.Round(order.GrandTotal ?? 0, 2) - Math.Round(order.PaidAmount ?? 0, 2);
        //    }

        //    order.UpdatedAt = DateTime.Now;
        //    order.ModifiedBy = AppConstant.LoggedInUserID;

        //    await orderService.Update(order);
        //}

        private TblContact GetCustomerDetails(Guid invoiceId)
        {
            var invoiceDetail = invoiceService.GetAllQuerable().FirstOrDefault(x => x.InvoiceId == invoiceId);
            var job = dbContext.TblJobs.FirstOrDefault(x => x.JobId == invoiceDetail.JobId);
            return dbContext.TblContacts.FirstOrDefault(x => x.ContactId == job.ContactId);
        }

        /// <summary>
        /// Returns an error response and rolls back the transaction if provided.
        /// </summary>
        private async Task<IResponseVm<string>> GetErrorResponse(IResponseVm<string> responseVm, string message, IDbContextTransaction transaction = null)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = message;

            if (transaction != null)
                await transaction.RollbackAsync();

            return responseVm;
        }

        #endregion UpdatePayment details

        #region Get_Payment_Details
        public async Task<IResponseVm<StripePaymentVm>> GetPaymentDetails(string Tranjectionid, string PaymentId)
        {
            IResponseVm<StripePaymentVm> responseVm = new ResponseVm<StripePaymentVm>();

            try
            {
                var payment =  dbContext.TblStripePayments
                    .Where(x => x.TenantId == claimsProperty.GetTenantId())
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
                    return GenerateErrorResponse("Invalid Estimate ID.");
                }

                var LatestOrder = dbContext.TblOrders
                    .Where(e => e.EstimateId == EstimateId && e.IsDelete == false &&
                                e.Status == EnumValue.GetEnumValue(Estimation.Approved))
                    .OrderByDescending(e => e.Version)
                    .ThenByDescending(e => e.CreatedAt)
                    .FirstOrDefault();

                if (LatestOrder == null)
                {
                    return GenerateErrorResponse("No approved estimate found.");
                }

                var invoices = await GetInvoicesByEstimateId(EstimateId);
                var payments = await GetPaymentsByInvoices(invoices);

                decimal? invoiceSum = invoices.Sum(x => x.TotalAmount);
                decimal totalOrderAmount = LatestOrder.GrandTotal ?? 0;
                decimal totalPaidAmount = payments.Sum(p => p.Amount ?? 0);

                bool isHalfPaymentMade = totalPaidAmount >= Math.Round(totalOrderAmount / 2, 2);


                decimal dueBalance = isHalfPaymentMade
                    ? totalOrderAmount - totalPaidAmount  
                    : Math.Round(totalOrderAmount / 2, 2);

                dueBalance = Math.Max(dueBalance, 0);

               // decimal dueBalance = invoiceSum > 0 ? totalOrderAmount - totalPaidAmount : Math.Round(totalOrderAmount/2,2);
                //decimal halfAmount = ? Math.Round(totalOrderAmount / 2, 2) : 0;
                //decimal extraBalance = Math.Max(0, totalPaidAmount - totalOrderAmount);

                var estimateAmountVm = CreateEstimateAmountVm(
                    totalOrderAmount,
                    totalPaidAmount,
                    dueBalance, 
                    //halfAmount,
                    //extraBalance,
                    invoices,
                    payments
                );

                responseVm.IsSuccess = true;
                responseVm.Data = estimateAmountVm;
                responseVm.Message = (dueBalance == 0)
                    ? $"Payment has already been completed for Order {LatestOrder.OrderName}."
                    : $"Total Order: {totalOrderAmount:C}, Payments Paid: {totalPaidAmount:C}, Due Balance: {dueBalance:C}";

                FileLogger.Log(responseVm.Message);
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "Error fetching estimate amount: " + ex.Message;
            }

            return responseVm;
        }


        private async Task<List<InvoiceVM>> GetInvoicesByEstimateId(Guid? estimateId)
        {
            var invoices = await invoiceService.GetAllInvoice(filter);
            return invoices.Data?
                .Where(x => x.EstimateId == estimateId)
                .ToList() ?? new List<InvoiceVM>();
        }

        private async Task<List<PaymentsVM>> GetPaymentsByInvoices(List<InvoiceVM> invoices)
        {
            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();
            var payments = await GetAllPayment(filter);
            return payments.Data?
                .Where(x => x.InvoiceId.HasValue && invoiceIds.Contains(x.InvoiceId.Value))
                .ToList() ?? new List<PaymentsVM>();
        }

        private EstimateAmountVm CreateEstimateAmountVm(decimal totalOrderAmount, decimal totalPaidAmount, decimal dueBalance
                                                        /*decimal halfAmount, decimal ExtraBalance*/, List<InvoiceVM> invoices, List<PaymentsVM> payments)
        {
            return new EstimateAmountVm
            {
                TotalOrderAmount = totalOrderAmount,
                TotalInvoiceAmount = invoices.Sum(x => x.TotalAmount ?? 0),
                TotalPaidAmount = totalPaidAmount,
                DueBalance = dueBalance,
                //HalfAmount = halfAmount,
                //ExtraBalance= ExtraBalance,
                Invoices = invoices.Select(x => new InvoicePayment_VM
                {
                    InvoiceId = x.InvoiceId,
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
                    Payments = payments.Where(y => y.InvoiceId == x.InvoiceId).ToList(),
                }).ToList(),
            };
        }

        private IResponseVm<EstimateAmountVm> GenerateErrorResponse(string message)
        {
            return new ResponseVm<EstimateAmountVm>
            {
                IsSuccess = false,
                Message = message
            };
        }

        #endregion EstimateAmount

        #region SendPaymentConfirmationEmail
        private async Task SendPaymentConfirmationEmail(string customerEmail, string firstName, string lastName, string invoiceNumber, decimal paidAmount, decimal? remainingBalance)
        {
            try
            {
                var tenantDetails = tenantService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true);

                var tenantInfoDetails = dbContext.TblTenantBrandingInfos
                    .FirstOrDefault(x => x.Id == tenantDetails.BrandingInfo);

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

                var res = await emailSendHelperService.SendEmailWithReplacedContent(EmailTemplateType.PaymentConfirmation, invoiceDataModel);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error sending payment confirmation email: {ex.Message}");
            }
        }

        #endregion SendPaymentConfirmationEmail

        #region Validate Stripe API Key 
        public static bool IsStripeApiKeyValid()
        {
            try
            {
                if (string.IsNullOrEmpty(AppConstant.Stripe_SecretKey))
                {
                    FileLogger.Log("Stripe API Key is missing.");
                    return false;
                }

                StripeConfiguration.ApiKey = AppConstant.Stripe_SecretKey;

                var service = new BalanceService();
                var balance = service.Get();

                FileLogger.Log("Stripe API Key validation successful.");
                return true;
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.StripeError != null)
                {
                    FileLogger.Log($"Stripe API Key Validation Failed: {stripeEx.StripeError.Message} (Code: {stripeEx.StripeError.Code})");
                }
                else
                {
                    FileLogger.Log($"Stripe API Key Validation Failed: {stripeEx.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Unexpected Error During Stripe API Key Validation: {ex.Message}");
                return false;
            }
        }

        #endregion Validate Stripe API Key 

    }

}

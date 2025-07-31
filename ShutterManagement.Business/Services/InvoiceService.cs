using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Microsoft.Extensions.Hosting;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using CsvHelper.TypeConversion;
using DinkToPdf;
using DinkToPdf.Contracts;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using ShutterManagement.Core.Common;
using Microsoft.Extensions.Primitives;
using MailKit.Search;
using Microsoft.Extensions.Options;
using ShutterManagement.Stripe;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Linq.Dynamic.Core;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core;
using QRCoder;
using System.IO;
using System.Drawing;

namespace ShutterManagement.Business.Services
{
    public class InvoiceService : BaseRepository<ShutterDBContext, TblInvoice>, IInvoiceService
    {
        private readonly ITenantService tenantService;
        private readonly IInvoicePaymentService invoicePaymentService;
        private readonly IConverter converter;
        private readonly IContactService contactService;
        private readonly IEstimateService estimateService;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly IJobService jobService;
        private readonly IOrderService orderService;
        public Pagination filter = new Pagination();
        private readonly StripeService _stripeService;
        private readonly IStripeConfiguration stripeConfiguration;
        private readonly JobStatusAndStageMapping statusAndStageMapping;
        private readonly ClaimsProperty claimsProperty;
        private readonly EmailSendHelperService emailSendHelperService;
        private readonly IUrlShortenerService urlShortenerService;
        private readonly StripeConfig _stripeConfig;
        public InvoiceService(ITenantService tenantService, IInvoicePaymentService invoicePaymentService,
           IConverter converter, IContactService contactService
            , IEstimateService estimateService, ITenantBrandingService tenantBrandingService
            , IJobService jobService, IOrderService orderService, StripeService stripeService,
            IStripeConfiguration stripeConfiguration, JobStatusAndStageMapping statusAndStageMapping, ClaimsProperty claimsProperty,
          EmailSendHelperService emailSendHelperService, IUrlShortenerService urlShortenerService)
        {
            this.tenantService = tenantService;
            this.invoicePaymentService = invoicePaymentService;
            this.converter = converter;
            this.contactService = contactService;
            this.estimateService = estimateService;
            this.tenantBrandingService = tenantBrandingService;
            this.jobService = jobService;
            this.orderService = orderService;
            _stripeService = stripeService;
            this.stripeConfiguration = stripeConfiguration;
            this.statusAndStageMapping = statusAndStageMapping;
            this.claimsProperty = claimsProperty;
            this.emailSendHelperService = emailSendHelperService;
            this.urlShortenerService = urlShortenerService;
            _stripeConfig = stripeConfiguration.GetStripeConfigurationAsync(claimsProperty.GetTenantId()).Result;
        }

        #region Create  update ActiveInActive Delete Getall
        public async Task<IResponseVm<string>> ActiveInActive(Guid InvoiceId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var invoice = GetAllQuerable()
                        .Where(c => c.InvoiceId == InvoiceId
                                   && c.TenantId == claimsProperty.GetTenantId()
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    if (invoice != null)
                    {
                        invoice.IsActive = !invoice.IsActive;
                        invoice.UpdatedAt = DateTime.Now;
                        invoice.ModifiedBy = AppConstant.LoggedInUserID;

                        var result = await Update(invoice);

                        if (result != null)
                        {
                            var invoicePayments = invoicePaymentService.GetAllQuerable()
                                .Where(x => x.InvoiceId == InvoiceId && x.IsDelete == false)
                                .ToList();

                            foreach (var payment in invoicePayments)
                            {
                                payment.UpdatedAt = DateTime.Now;
                                payment.IsActive = invoice.IsActive;
                                payment.ModifiedBy = AppConstant.LoggedInUserID;
                                await invoicePaymentService.Update(payment);
                            }

                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UpdateSuccessMsg;
                        }
                        else
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = "Failed to update the invoice status.";
                        }
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

        public async Task<IResponseVm<string>> CreateInvoice(CreateInvoice model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

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

                int Version = GetAllQuerable().Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
               .Select(p => (p.InvoiceNumber.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.InvoiceNumber.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                Version += 1;
                var InvoiceNumberUniquNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Invoice, claimsProperty.GetCompanyCode(), Version);     //   var InvoiceNumberUniquNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Invoice, claimsProperty.GetCompanyCode());
                TblInvoice tblInvoice = new TblInvoice
                {
                    InvoiceId = Guid.NewGuid(),
                    JobId = model.JobId,
                    InvoiceNumber = InvoiceNumberUniquNumber,
                    EstimateId = model.EstimateId,
                    IssueDate = model?.IssueDate,
                    DueDate = model?.DueDate,
                    TotalAmount = model.TotalAmount,
                    BalanceDue = model.TotalAmount,
                    PaidAmount = 0,
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                    Status = EnumValue.GetEnumValue(PaymentStatus.Unpaid),
                    IsActive = true,
                    IsDelete = false
                };

                var res = await Add(tblInvoice);
                if (res == null || res.InvoiceId == Guid.Empty)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Insert failed.";
                    return responseVm;
                }

                foreach (var invoicePayment in model.InvoicePayment)
                {
                    var tblInvoicePayment = new TblInvoicePayment
                    {
                        InvoicePaymentId = Guid.NewGuid(),
                        InvoiceId = tblInvoice.InvoiceId,
                        Notes = invoicePayment.Notes,
                        InvoiceAmount = invoicePayment.InvoiceAmount,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false
                    };



                    var paymentResult = await invoicePaymentService.Add(tblInvoicePayment);
                    if (paymentResult == null || paymentResult.InvoicePaymentId == Guid.Empty)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Insert Failed";
                        return responseVm;
                    }
                }
                TblPayment tblPayments = new TblPayment
                {
                    PaymentId = Guid.NewGuid(),
                    InvoiceId = tblInvoice.InvoiceId,
                    PaymentDate = DateTime.Now,
                    Amount = tblInvoice.BalanceDue,
                    PaymentMethod = "",
                    TransactionId = "",
                    Notes = "",
                    Status = EnumValue.GetEnumValue(PaymentStatus.Unpaid),
                    PaymentTypeStatus = EnumValue.GetEnumValue(PaymentStatus.Draft),
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false
                };

                await dbContext.TblPayments.AddAsync(tblPayments);
                await dbContext.SaveChangesAsync();
                FileLogger.Log($"Payment Record Created: {tblPayments.PaymentId}");

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;

                if (responseVm.IsSuccess)
                {
                    var jobStatusName = EnumValue.GetEnumValue(JobStatus.In_Production);
                    var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(tblInvoice?.JobId ?? Guid.Empty, jobStatusName);

                    if (!updateResponse.IsSuccess)
                    {
                        FileLogger.Log(updateResponse.Message);
                        //responseVm.IsSuccess = false;
                        //responseVm.Message = updateResponse.Message;
                        //return responseVm;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteInvoice(Guid InvoiceId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var invoice = GetAllQuerable()
                        .Where(c => c.InvoiceId == InvoiceId
                                   && c.TenantId == claimsProperty.GetTenantId()
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    //var CheckPaymentExist = paymentsService.GetAllQuerable()
                    //   .Where(c => c.InvoiceId == InvoiceId
                    //              && c.TenantId == claimsProperty.GetTenantId()
                    //              && c.IsDelete == false)
                    //   .FirstOrDefault();

                    //if(CheckPaymentExist!=null)
                    //{
                    //    responseVm.IsSuccess = false;
                    //    responseVm.Message = "Payment Exit in This Invoice";
                    //    return responseVm;
                    //}

                    if (invoice != null)
                    {
                        invoice.IsDelete = !invoice.IsDelete;
                        invoice.UpdatedAt = DateTime.Now;
                        invoice.ModifiedBy = AppConstant.LoggedInUserID;

                        var result = await Update(invoice);

                        if (result != null)
                        {
                            var invoicePayments = invoicePaymentService.GetAllQuerable()
                                .Where(x => x.InvoiceId == InvoiceId && x.IsDelete == false)
                                .ToList();

                            foreach (var payment in invoicePayments)
                            {
                                payment.UpdatedAt = DateTime.Now;
                                payment.IsDelete = invoice.IsDelete;
                                payment.ModifiedBy = AppConstant.LoggedInUserID;
                                await invoicePaymentService.Update(payment);
                            }

                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UpdateSuccessMsg;
                        }
                        else
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = "Failed to update the invoice status.";
                        }
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

        public async Task<IResponseVm<List<InvoiceVM>>> GetAllInvoice(Pagination filter)
        {
            IResponseVm<List<InvoiceVM>> responseVm = new ResponseVm<List<InvoiceVM>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var invoicesQuery = (from i in GetAllQuerable().OrderByDescending(x => x.CreatedAt)
                                     join j in dbContext.TblJobs on i.JobId equals j.JobId
                                     join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                                     join d in dbContext.TblEstimates on i.EstimateId equals d.EstimateId
                                     let hasActiveOrder = dbContext.TblOrders.Any(o => o.EstimateId == i.EstimateId && o.IsActive == true)
                                     where i.IsDelete == false && hasActiveOrder && j.IsActive == true
                                     orderby i.CreatedAt descending
                                     select new InvoiceVM
                                     {
                                         InvoiceId = i.InvoiceId,
                                         InvoiceNumber = i.InvoiceNumber,
                                         JobId = i.JobId,
                                         EstimateId = i.EstimateId,
                                         IssueDate = i.IssueDate,
                                         TotalAmount = i.TotalAmount,
                                         PaidAmount = i.PaidAmount,
                                         BalanceDue = i.BalanceDue,
                                         DueDate = i.DueDate,
                                         Status = i.Status ?? "",
                                         IsActive = i.IsActive,
                                         ContactFirstName = c.FirstName,
                                         ContactLastName = c.LastName,
                                         EstimateName = d.EstimateName,
                                     }).ToList();



                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var filteredInvoices = invoicesQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        ((string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.InvoiceNumber.ToString().ToLower().Contains(filter.SearchValue.Trim()) ||
                            (x.ContactFirstName?.ToString().ToLower() == filter.SearchValue.Trim())) ||
                            (x.ContactLastName?.ToString().ToLower() == filter.SearchValue.Trim()) ||
                            ((x.ContactFirstName + x.ContactLastName)?.ToString().ToLower() == filter.SearchValue.Trim()) ||
                            (x.Status.ToLower().Contains(filter.SearchValue.Trim())) ||
                            (x.InvoiceId.ToString() == filter.SearchValue.Trim())) ||
                            (x.JobId.ToString() == filter.SearchValue.Trim()))).ToList();

                foreach (var invoice in filteredInvoices)
                {
                    invoice.InvoicePayment = dbContext.TblInvoicePayments
                        .Where(p => p.InvoiceId == invoice.InvoiceId && p.IsDelete == false)
                        .Select(p => new InvoicePaymentVM
                        {
                            InvoicePaymentId = p.InvoicePaymentId,
                            InvoiceId = p.InvoiceId,
                            InvoiceAmount = p.InvoiceAmount,
                            Notes = p.Notes,
                            PaymentDate = p.CreatedAt,
                            IsActive = p.IsActive
                        })
                        .OrderBy(p => p.PaymentDate)
                        .ToList();
                }

                var paginatedInvoices = filteredInvoices
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedInvoices;
                responseVm.TotalRecords = filteredInvoices.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateInvoice(UpdateInvoice updateInvoice)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

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

                var invoice = GetAllQuerable()
                    .Where(c => c.InvoiceId == updateInvoice.InvoiceId
                                && c.TenantId == claimsProperty.GetTenantId()
                                && c.IsDelete == false)
                    .FirstOrDefault();

                if (invoice == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                invoice.InvoiceNumber = updateInvoice.InvoiceNumber?.Trim();
                invoice.IssueDate = updateInvoice.IssueDate;
                invoice.DueDate = updateInvoice.DueDate;
                invoice.TotalAmount = updateInvoice.TotalAmount;
                invoice.EstimateId = updateInvoice.EstimateId;
                invoice.BalanceDue = (updateInvoice.TotalAmount - invoice.PaidAmount) ?? 0.0m;
                invoice.UpdatedAt = DateTime.Now;
                invoice.ModifiedBy = AppConstant.LoggedInUserID;

                var result = await Update(invoice);
                if (result == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to update the invoice.";
                    return responseVm;
                }

                var existingPayments = invoicePaymentService.GetAllQuerable()
                    .Where(x => x.InvoiceId == updateInvoice.InvoiceId && x.IsDelete == false)
                    .ToList();

                foreach (var payment in existingPayments)
                {
                    payment.IsDelete = true;
                    payment.IsActive = false;
                    payment.UpdatedAt = DateTime.Now;
                    payment.ModifiedBy = AppConstant.LoggedInUserID;
                    await invoicePaymentService.Update(payment);
                }

                foreach (var newPayment in updateInvoice.InvoicePayment)
                {
                    var tblPayment = new TblInvoicePayment
                    {
                        InvoicePaymentId = Guid.NewGuid(),
                        InvoiceId = updateInvoice.InvoiceId,
                        InvoiceAmount = newPayment.InvoiceAmount,
                        Notes = newPayment.Notes?.Trim(),
                        CreatedAt = DateTime.Now,
                        ModifiedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false
                    };

                    await invoicePaymentService.Add(tblPayment);
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
        #endregion 

        #region Invoice_Generate_Methods

        public async Task<IResponseVm<string>> SendInvoiceAttachment(GenerateInvoiceModel model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                //FileLogger.Log($"Stripe Configuration{claimsProperty.GetTenantId().ToString()}");
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }

               // await stripeConfiguration.GetStripeConfiguration(claimsProperty.GetTenantId().ToString());

                filter.RecordPerPage = 1000;
                filter.CustomerId = model.ContactId.ToString();
                var customerDetails = (await contactService.GetAll_Contact(filter, false, false))
                    .Data.FirstOrDefault(x => x.ContactId == model.ContactId && x.IsActive == true);


                var tenantDetails = (await tenantService.GetAll()).Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true).FirstOrDefault();
                if (tenantDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Tenant " + AppConstant.NoDataFound;
                    return responseVm;
                }

                var tenantInfoDetails = tenantBrandingService.GetAllQuerable().Where(x => x.Id == tenantDetails.BrandingInfo && x.IsActive == true).FirstOrDefault();



                var estimationDetails = dbContext.TblEstimates.FirstOrDefault(c =>
                    c.EstimateId == model.EstimationId &&
                    c.TenantId == claimsProperty.GetTenantId() &&
                    c.IsDelete == false);
                if (estimationDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"EstimationDetails " + AppConstant.NoDataFound;
                    return responseVm;
                }

                var userDetails = dbContext.TblUsers.FirstOrDefault(c =>
                   c.Id == estimationDetails.CreatedBy &&
                   c.TenantId == claimsProperty.GetTenantId() &&
                   c.IsDelete == false);

                var orderDetails = dbContext.TblOrders.FirstOrDefault(c =>
                   c.OrderId == model.OrderId &&
                   c.TenantId == claimsProperty.GetTenantId() &&
                   c.IsDelete == false);

                var estimationRecord = await estimateService.ViewEachEstimation(model.EstimationId);
                var productList = estimationRecord.Data
                    .FirstOrDefault(x => x.IsActive == true)
                    ?.EstimateProductList ?? new List<EstimateProductList>();

                string customerState = "";
                if (!string.IsNullOrEmpty(customerDetails?.billingAddressVm?.StateId.ToString()))
                {
                    customerState = dbContext.MstStates.Where(x => x.Id == customerDetails.billingAddressVm.StateId).Select(y => y.StateName ?? "").FirstOrDefault();
                }

                var powderCoatColor = estimationRecord.Data
                     .FirstOrDefault(x => x.IsActive == true).jobDetails.PowderCoatColor;

                var PreparedBy = dbContext.TblUsers
                    .Where(s => s.Id == (estimationDetails.PreparedBy == Guid.Empty || estimationDetails.PreparedBy == null ? estimationDetails.CreatedBy : estimationDetails.PreparedBy))
                    .Select(n => (n.FirstName ?? "") + " " + (n.LastName ?? ""))
                    .FirstOrDefault() ?? "";
        
                var invoiceData = new InvoiceDataModel
                {
                    CustomerName = (customerDetails?.FirstName ??"")+ " " + (customerDetails?.LastName ??""),
                    PreparedBy = PreparedBy ?? "",
                    CustomerId = customerDetails?.ContactUniqueId ?? "",
                    CustomerCity = customerDetails?.billingAddressVm?.City ?? "",
                    CustomerState = customerState ?? "",
                    CustomerZipCode = customerDetails?.billingAddressVm?.ZipCode ?? "",
                    HomePhone = PhoneHelper.FormatPhoneNumber(customerDetails?.billingAddressVm?.PhoneNumber??"")??"",
                    CompanyStreetAddress = tenantInfoDetails?.StreetAddress ?? "",
                    CompanyCity = tenantInfoDetails?.City ?? "",
                    CompanyCountry = tenantInfoDetails?.Country ?? "",
                    CompanyZipCode = tenantInfoDetails?.ZipCode ?? "",
                    CustomerStreetAddress = customerDetails?.billingAddressVm?.StreetAddress ?? "",
                    StreetAddress = customerDetails?.billingAddressVm?.StreetAddress ?? "",
                    MobilePhone = PhoneHelper.FormatPhoneNumber(customerDetails?.billingAddressVm?.PhoneNumber ?? "") ?? "" ?? "",
                    TenantCompanyName = tenantInfoDetails?.CompanyName ?? "",
                    Logo = tenantInfoDetails?.Logo ?? "",
                    EstimationDate = Convert.ToDateTime(estimationDetails?.CreatedAt).ToString("MM-dd-yyyy") ?? "",
                    CreatedBy = dbContext.TblUsers.Where(s => s.Id == estimationDetails.PreparedBy).Select(n => (n.FirstName ?? "") + " " + (n.LastName ?? "")).FirstOrDefault() ?? "",
                    EstimationSubtotal = estimationDetails?.TotalPrice.ToString() ?? "",
                    EstimationGrandTotal = estimationDetails?.GrandTotal.ToString() ?? "",
                    EstimationId = estimationDetails?.EstimateCode?.ToString() ?? "",
                    EstimationCode = estimationDetails?.EstimateCode ?? "",
                    EmailAddress = customerDetails?.Email ?? "",
                    Terms = estimationDetails?.TermsCondition ?? "",
                    Note = estimationDetails?.Notes ?? "",
                    OrderName = (!string.IsNullOrEmpty(orderDetails?.OrderName)
    ? $@"  <p style='font-size: 16px;margin: 0;margin-top: 17px;margin-bottom: 0;'> <span
                                    style='font-weight: 700; color: #ff0000;'>Order ID : {orderDetails?.OrderName}</span> </p>"
    : ""),
                    
                    CompanyEmail = tenantInfoDetails?.Email ?? "",
                    CompanyPhone = PhoneHelper.FormatPhoneNumber(tenantInfoDetails?.PhoneNumber ?? "") ?? "",
                    Taxes = (estimationDetails?.TotaltaxAmount > 0
    ? $@"<tr>
<td style=""padding: 5px 5px; font-weight: 700; font-size: 14px;"">Taxes :</td>
<td style=""text-align: end; padding: 5px 20px; font-weight: 700; font-size: 14px;"">${estimationDetails?.TotaltaxAmount}</td>
</tr>"
    : ""),
                    Discount = (estimationDetails.DiscountAmount > 0
    ? $@"<tr>
<td style=""padding: 5px 5px; font-weight: 700; font-size: 14px;"">Discount :</td>
<td style=""text-align: end; padding: 5px 20px; font-weight: 700; font-size: 14px;"">${estimationDetails.DiscountAmount}</td>
</tr>"
    : ""),
                    Additionalcost = estimationDetails?.AdditionalCost.ToString() ?? "",
                    ProductList = GenerateProductListHtml(productList),
                    PowderCoatColor = (!string.IsNullOrEmpty(powderCoatColor)
    ? $@" <p style='font-size: 14px;margin: 0;margin-top: 10px;margin-bottom: 10px;'> <span
                                    style='font-weight: 700;display: inline-block;'>Powder Coat Color : </span> <span
                                    style='font-weight: 700;display: inline-block;'>{powderCoatColor}</span> </p>"
    : ""),
                    
                };

                var originalEstimateUrltoQR = $"{AppConstant.UIBaseURL}contacts/estimate/preview/{estimationDetails?.EstimateId}";
                invoiceData.EstimateQRCode = GenerateQRCodeBase64(originalEstimateUrltoQR) ?? "";
                // Generate the Estimate HTML
                var htmlContent = await emailSendHelperService.GetTemplateContent(EmailTemplateType.EstimatePDF, invoiceData);

                //Generate the Estimate Invoice PDF
                var pdfBytes = GeneratePdf(htmlContent.Data);

                if (!pdfBytes.IsSuccess)
                {
                    responseVm.Message = pdfBytes.Message;
                    return responseVm;
                }

                var rootPath = Path.Combine(AppConstant.FileUploadPath, claimsProperty.GetTenantId().ToString());
                //FileLogger.Log($"Invoice File Path for payment {rootPath}");

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                var fileName = $"Invoice_{Guid.NewGuid()}.pdf";
                var savePath = Path.Combine(rootPath, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.WriteAsync(pdfBytes.Data, 0, pdfBytes.Data.Length);
                }


                var pdfBase64 = Convert.ToBase64String(pdfBytes.Data);


                var PreparedByEmail = dbContext.TblUsers.Where(u => u.Id == estimationDetails.PreparedBy).Select(s => s.Email).FirstOrDefault() ?? "";
                string emailBody = string.Empty;
                List<EmailAttachments> emailAttachments = new List<EmailAttachments>();
                IResponseVm<string> EmailSendResult = new ResponseVm<string>();
                var LoggedInUser = claimsProperty.GetEmail() ?? string.Empty;
                switch (model.DocumentType)
                {

                    case EstimateDocumentType.ForEmail:
                        var IsEstimateExists = dbContext.TblEstimates
                        .Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId() && x.EstimateId == model.EstimationId)
                        .OrderByDescending(x => x.CreatedAt)
                        .FirstOrDefault();

                        if (IsEstimateExists == null)
                        {
                            responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                            return responseVm;
                        }
                        if (model.IsResend == null || !(bool)model.IsResend)
                        {
                            IsEstimateExists.Status = EnumValue.GetEnumValue(Estimation.PendingForApproval);
                            IsEstimateExists.UpdatedAt = DateTime.Now;
                            IsEstimateExists.ModifiedBy = AppConstant.LoggedInUserID;
                            await estimateService.Update(IsEstimateExists);
                        }
                        // Generate the original estimate URL
                        var originalEstimateUrl = $"{AppConstant.UIBaseURL}contacts/estimate/preview/{IsEstimateExists?.EstimateId}";
                        
                        // Attempt to shorten the URL using Bitly API
                        var shortenResult = await urlShortenerService.ShortenUrlAsync(originalEstimateUrl);
                        
                        if (shortenResult.IsSuccess && !string.IsNullOrWhiteSpace(shortenResult.Data))
                        {
                            invoiceData.InvoiceLink = shortenResult.Data;
                            FileLogger.Log($"EstimateService: Successfully shortened estimate URL from {originalEstimateUrl} to {shortenResult.Data}");
                        }
                        else
                        {
                            // Fallback to original URL if shortening fails
                            invoiceData.InvoiceLink = originalEstimateUrl;
                            FileLogger.Log($"EstimateService: URL shortening failed ({shortenResult.Message}), using original URL: {originalEstimateUrl}");
                        }

                        if (model.IsResend == null || !(bool)model.IsResend)
                        {
                            var jobStatusName = EnumValue.GetEnumValue(JobStatus.Quote_Sent);
                            var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(IsEstimateExists?.JobId ?? Guid.Empty, jobStatusName);

                            if (!updateResponse.IsSuccess)
                            {
                                responseVm.IsSuccess = false;
                                responseVm.Message = updateResponse.Message;
                                return responseVm;
                            }


                            var jobDetails = await jobService.GetById(IsEstimateExists.JobId ?? Guid.Empty);

                            if (jobDetails != null)
                            {
                                var ghlUrlResponse = await jobService.UpdateEstimateUrlInGhlOnly(jobDetails, invoiceData.InvoiceLink, IsEstimateExists.GrandTotal ?? 0.0m);

                                if (!ghlUrlResponse.IsSuccess)
                                {
                                    // Optional: log or return a warning, depending on your use case
                                    FileLogger.Log($"GHL URL Update failed: {ghlUrlResponse.Message}");
                                }
                            }
                        }

                        emailBody = emailSendHelperService.PlaceholderReplace(Templates.Estimate,invoiceData);
                        emailAttachments.Add(new EmailAttachments
                        {
                            FileName = $"{invoiceData.EstimationCode}.pdf",
                            Stream = new MemoryStream(pdfBytes.Data),
                            MediaType = "application/pdf"
                        });
                        EmailSendResult = await emailSendHelperService.GetEmailConfigAndSendEmail(customerDetails?.Email, emailBody, AppConstant.Sub_EstimateApproval, PreparedByEmail, emailAttachments,string.Empty,LoggedInUser);

                        responseVm.IsSuccess = true;
                        responseVm.Message = EmailSendResult.Message;
                        System.IO.File.Delete(savePath);
                        break;

                    case EstimateDocumentType.OrderForEmail:

                        var IsOrderExists = (await orderService.GetAll()).Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId() &&
                    x.OrderId == model.OrderId && x.Status != EnumValue.GetEnumValue(Estimation.Approved)).
                    OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                        if (IsOrderExists == null)
                        {
                            responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                            return responseVm;
                        }

                        IsOrderExists.Status = EnumValue.GetEnumValue(Estimation.PendingForApproval);
                        IsOrderExists.UpdatedAt = DateTime.Now;
                        IsOrderExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await orderService.Update(IsOrderExists);

                        invoiceData.InvoiceLink = $"{AppConstant.UIBaseURL}contacts/order/preview/{IsOrderExists?.OrderId}";

                        emailBody = Templates.Order;
                        string orderEmailContent = emailSendHelperService.PlaceholderReplace(emailBody, invoiceData);

                        emailAttachments.Add(new EmailAttachments
                        {
                            FileName = $"{invoiceData.EstimationCode}.pdf",
                            Stream = new MemoryStream(pdfBytes.Data),
                            MediaType = "application/pdf"
                        });
                        
                        EmailSendResult = await emailSendHelperService.GetEmailConfigAndSendEmail(customerDetails?.Email, orderEmailContent, AppConstant.Sub_EstimateOrderApproval, PreparedByEmail, emailAttachments,string.Empty, LoggedInUser);
                        responseVm.IsSuccess = true;
                        responseVm.Message = EmailSendResult.Message;
                        System.IO.File.Delete(savePath);

                        break;

                    case EstimateDocumentType.InvoicePaymentForEmail:
                        var paymentDetails = dbContext.TblInvoicePayments.FirstOrDefault(x => x.InvoiceId == model.InvoiceId && x.IsDelete == false);

                        if (paymentDetails == null)
                        {
                            responseVm.Message = "Payment " + AppConstant.NoDataFound;
                            return responseVm;
                        }

                        var invoiceDetails = dbContext.TblInvoices
                            .FirstOrDefault(x => x.InvoiceId == model.InvoiceId && x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false);

                        if (invoiceDetails == null)
                        {
                            responseVm.Message = "Invoice " + AppConstant.NoDataFound;
                            return responseVm;
                        }

                        var paymentList = dbContext.TblInvoicePayments
                            .Where(x => x.InvoiceId == model.InvoiceId && x.IsActive == true)
                            .Select(p => new Core.ViewModels.Request.InvoicePayment
                            {
                                Notes = p.Notes,
                                InvoiceAmount = p.InvoiceAmount,

                            })
                            .ToList();


                        invoiceData.TotalAmount = invoiceDetails?.TotalAmount.ToString() ?? "";
                        invoiceData.DueAmount = invoiceDetails?.BalanceDue.ToString() ?? "";
                        invoiceData.Balancedue = invoiceDetails?.BalanceDue.ToString() ?? "";
                        invoiceData.InvoiceNo = invoiceDetails?.InvoiceNumber?.ToString() ?? "";
                        invoiceData.IssueDate = Convert.ToDateTime(invoiceDetails?.IssueDate).ToString("MM-dd-yyyy") ?? "";
                        invoiceData.PaymentList = GenerateInvoicetListHtml(paymentList);

                        //placeholders in Invoice Template
                        var invoiceEmailContent = await emailSendHelperService.GetTemplateContent(EmailTemplateType.InvoicePDF, invoiceData);

                        // Generate Invoice PDF
                        var pdfBytesInvoice = GeneratePdf(invoiceEmailContent.Data);

                        // Save the invoice PDF
                        var rootPathInvoice = Path.Combine(AppConstant.FileUploadPath, claimsProperty.GetTenantId().ToString());
                        if (!Directory.Exists(rootPathInvoice))
                        {
                            Directory.CreateDirectory(rootPathInvoice);
                        }

                        var InvoicefileName = $"Invoice_{Guid.NewGuid()}.pdf";
                        var InvicesavePath = Path.Combine(rootPathInvoice, InvoicefileName);

                        await using (var stream1 = new FileStream(InvicesavePath, FileMode.Create))
                        {
                            await stream1.WriteAsync(pdfBytesInvoice.Data, 0, pdfBytesInvoice.Data.Length);
                        }

                        var InvoicepdfBase64 = Convert.ToBase64String(pdfBytesInvoice.Data);

                        string paymentLink = string.Empty;
                        bool isStripeConfigured = !string.IsNullOrEmpty(_stripeConfig?.StripeSecretKey??"") && IsStripeApiKeyValid();
                        bool isPaymentLinkGenerated = false;

                        if (invoiceDetails?.BalanceDue > 0 && IsStripeApiKeyValid())
                        {

                            var Payment = dbContext.TblPayments.Where(x => x.InvoiceId == model.InvoiceId && x.PaymentTypeStatus == EnumValue.GetEnumValue(PaymentStatus.Draft) && x.Status == EnumValue.GetEnumValue(PaymentStatus.Unpaid)).FirstOrDefault();

                            if (Payment != null)
                            {
                                if (isStripeConfigured)
                                {
                                    var paymentRequest = new PaymentRequest
                                    {
                                        CustomerEmail = customerDetails?.Email ?? "",
                                        ProductName = $"Invoice Payment - {invoiceDetails?.InvoiceNumber}",
                                        Amount = (long)(invoiceDetails?.BalanceDue * 100),
                                        Quantity = 1,
                                        InvoiceId = invoiceDetails?.InvoiceId.ToString(),
                                        PaymentId = Payment?.PaymentId.ToString(),
                                    };

                                    paymentLink = _stripeService.CreateCheckoutSession(paymentRequest);
                                    isPaymentLinkGenerated = !string.IsNullOrEmpty(paymentLink);
                                    FileLogger.Log($"Payment Link Generated: {paymentLink}");
                                }
                                else
                                {
                                    FileLogger.Log("Stripe credentials are missing. Payment link cannot be generated.");
                                }
                            }
                            else
                            {
                                FileLogger.Log($"Payment Not Found Those payment created time on Invoice.{Payment}");
                            }


                        }

                        invoiceData.Message = isPaymentLinkGenerated
                            ? $"You can make the payment for your invoice <strong>{invoiceDetails?.InvoiceNumber}</strong> by clicking the link below:"
                            : $"<strong>Note:</strong> Online payment is currently unavailable . Please contact support {tenantInfoDetails?.Email ?? ""}";

                        invoiceData.PaymentLink = isPaymentLinkGenerated ? emailSendHelperService.PlaceholderReplace(Templates.InvoicePaymentLink,new { paymentLink }) : string.Empty;

                        emailBody = emailSendHelperService.PlaceholderReplace(Templates.Invoice, invoiceData);
                        emailAttachments.Add(new EmailAttachments
                        {
                            FileName = $"{invoiceData.InvoiceNo}.pdf",
                            Stream = new MemoryStream(pdfBytesInvoice.Data),
                            MediaType = "application/pdf"
                        });
                        EmailSendResult = await emailSendHelperService.GetEmailConfigAndSendEmail(customerDetails?.Email, emailBody, AppConstant.InvoicePaymentLinkandPDF, PreparedByEmail, emailAttachments,string.Empty, LoggedInUser);

                        responseVm.IsSuccess = true;
                        responseVm.Message = isStripeConfigured && EmailSendResult.IsSuccess
                            ? "Email sent successfully."
                            : EmailSendResult.Message;

                        System.IO.File.Delete(InvicesavePath);


                        break;
                    case EstimateDocumentType.DownloadPDF:

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InvoiceDownloadSuccessMsg;
                        responseVm.Data = pdfBase64;

                        System.IO.File.Delete(savePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError}: {ex.Message} :{ex.StackTrace}";
            }

            return responseVm;
        }

       

        public string GenerateQRCodeBase64(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            try
            {
                var generator = new QRCodeGenerator();
                QRCodeData qrCodeData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(pixelsPerModule: 20);
                return "data:image/png;base64," + Convert.ToBase64String(qrBytes);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error generating QR code: {ex.Message}");
                return string.Empty;
            }
        }
        private string GenerateProductListHtml(List<EstimateProductList> productList)
        {
            var productListHtml = new StringBuilder();
            int i = 0;
            foreach (var product in productList)
            {
                //decimal? unitprice = product.TotalPrice / product.WindowQty;

                decimal? effectiveTotalPrice = product.ProductPrice > 0 ? product.ProductPrice.Value : product.ProductPrice;
                decimal? optionprice = (product.additionalCost + product.OptionMarkup);
                decimal? unitprice = (product.ProductPrice + product.ComponentMarkup + product.ProductMarkup + optionprice) / product.WindowQty;
                decimal? subtotal = (product.ProductPrice + product.ComponentMarkup + product.ProductMarkup + optionprice);

                var hidePreviews = product.hidePreviews.ToList(); // 0 Coating-a, 1: installation-standard....
                var options = product.Options.ToList(); // 0: true, 1: false....
                List<string> optionStr = new List<string>(); // Initialize as a List

                int j = 0;

                foreach(var item in options)
                {
                    if(hidePreviews[j] && hidePreviews[j] == true)
                    {
                        j++;
                        continue;
                    } else
                    {
                        optionStr.Add(item); // Use Add instead of Append
                    }

                    j++;
                }

                productListHtml.Append($@"
                            <tr style='background-color: #efefef;'>
                                <td style='text-align: center; padding: 5px 5px;'>{++i}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{product.ProductTypeName ?? ""}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{product.ProductName ?? ""}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{string.Join(", ", optionStr)}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{product.WindowQty ?? 0}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{product.Width ?? 0}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{product.Height ?? 0}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{unitprice ?? 0.0m:F2}</td>
                                <td style='text-align: center; padding: 5px 5px;'>{subtotal ?? 0.0m:F2}</td>
                            </tr>");
            }

            return $"<tbody>{productListHtml}</tbody>";
        }
        ////<td style='text-align: center; padding: 5px 5px;'>{optionprice ?? 0.0m:F2}</td>
        private string GenerateInvoicetListHtml(List<Core.ViewModels.Request.InvoicePayment> payments)
        {
            var paymentsListHtml = new StringBuilder();
            int i = 0;
            foreach (var payment in payments)
            {
                paymentsListHtml.Append($@"
    <tr style='background-color: #efefef;'>
        <td style='text-align: center; padding: 5px 5px;'>{++i}</td>
        <td style='text-align: center; padding: 5px 5px;'>{payment.Notes}</td>
        <td style='text-align: center; padding: 5px 5px;'>{payment.InvoiceAmount}</td>
       
    </tr>");
            }

            return $"<tbody>{paymentsListHtml}</tbody>";
        }

        public ResponseVm<byte[]> GeneratePdf(string htmlContent)
        {
            var response = new ResponseVm<byte[]>();

            try
            {
                var pdfDocument = new HtmlToPdfDocument
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 } // Remove all margins
                    },
                    Objects = {
                        new ObjectSettings {
                            HtmlContent = htmlContent,
                            WebSettings = new WebSettings { DefaultEncoding = "utf-8" },
                        }
                    }
                };

                byte[] pdfBytes = converter.Convert(pdfDocument);

                response.IsSuccess = true;
                response.Message = "PDF generated successfully.";
                response.Data = pdfBytes;
            }
            catch (BadImageFormatException)
            {
                response.IsSuccess = false;
                response.Message = "PDF generation failed: platform mismatch. Please ensure application and native libraries match (e.g., x64).";
            }
            catch (DllNotFoundException)
            {
                response.IsSuccess = false;
                response.Message = "PDF generation failed: required native library not found. Please ensure wkhtmltox or SwapToPdf native DLL is present.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An unexpected error occurred while generating the PDF. {ex.StackTrace}";
            }

            return response;
        }

        public async Task<IResponseVm<List<InvoiceVM>>> InvoiceReport(Pagination filter, ReportFilterType type)
        {
            IResponseVm<List<InvoiceVM>> responseVm = new ResponseVm<List<InvoiceVM>>();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }
                var invoiceResponse = GetFilteredInvoices(filter);
                if (invoiceResponse == null || !invoiceResponse.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No invoices found";
                    return responseVm;
                }
                var filterLogic = new Dictionary<ReportFilterType, Func<InvoiceVM, bool>>
                {
                { ReportFilterType.Duedate, invoice => invoice.DueDate < DateTime.Now  && invoice.BalanceDue>0},
                { ReportFilterType.Pending, invoice => invoice.BalanceDue > 0 },
                { ReportFilterType.Paid, invoice => invoice.BalanceDue == 0 }
                };
                if (!filterLogic.TryGetValue(type, out var filterCondition))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invalid filter type specified.";
                    return responseVm;
                }
                var filteredInvoices = invoiceResponse.Where(filterCondition).ToList();

                if (filter.ToDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(invoice => invoice.DueDate <= filter.ToDate.Value).ToList();
                }

                if (filter.FormDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(invoice => invoice.DueDate >= filter.FormDate.Value).ToList();
                }

                if (filter.ToDate.HasValue && filter.FormDate.HasValue)
                {
                    filteredInvoices = filteredInvoices
                        .Where(invoice => invoice.DueDate >= filter.FormDate.Value && invoice.DueDate <= filter.ToDate.Value)
                        .ToList();
                }


                if (!filteredInvoices.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No invoices match the specified filter criteria.";
                    return responseVm;
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchValue))
                {
                    string searchTerm = filter.SearchValue.Trim().ToLower();
                    filteredInvoices = filteredInvoices.Where(invoice =>
                        (!string.IsNullOrEmpty(invoice.InvoiceNumber) && invoice.InvoiceNumber.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(invoice.Status) && invoice.Status.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(invoice.ContactFirstName) && invoice.ContactFirstName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(invoice.ContactLastName) && invoice.ContactLastName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(invoice.EstimateName) && invoice.EstimateName.ToLower().Contains(searchTerm)) ||
                        ((!string.IsNullOrEmpty(invoice.ContactFirstName) || !string.IsNullOrEmpty(invoice.ContactLastName)) &&
                         ($"{invoice.ContactFirstName} {invoice.ContactLastName}".Trim().ToLower().Contains(searchTerm)))
                    ).ToList();
                }




                var paginatedInvoices = filteredInvoices
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedInvoices;
                responseVm.TotalRecords = filteredInvoices.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"An error occurred while generating the report: {ex.Message}";
            }

            return responseVm;
        }

        public List<InvoiceVM> GetFilteredInvoices(Pagination filter)
        {
            var invoicesQuery = (from i in GetAllQuerable()
                                 join j in dbContext.TblJobs on i.JobId equals j.JobId
                                 join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                                 where i.IsDelete == false && i.TenantId == claimsProperty.GetTenantId()
                                 orderby i.CreatedAt descending
                                 select new InvoiceVM
                                 {
                                     InvoiceId = i.InvoiceId,
                                     InvoiceNumber = i.InvoiceNumber,
                                     JobId = i.JobId,
                                     EstimateId = i.EstimateId,
                                     IssueDate = i.IssueDate,
                                     TotalAmount = i.TotalAmount,
                                     PaidAmount = i.PaidAmount,
                                     BalanceDue = i.BalanceDue,
                                     DueDate = i.DueDate,
                                     Status = i.Status ?? "",
                                     IsActive = i.IsActive,
                                     ContactFirstName = c.FirstName,
                                     ContactLastName = c.LastName,
                                     Suffix = c.Suffix ?? "",
                                     TenantId = i.TenantId
                                 }).ToList();

            filter.SearchValue = filter.SearchValue?.Trim().ToLower();

            var filteredInvoices = invoicesQuery
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (
                        string.IsNullOrEmpty(filter.SearchValue) ||
                        x.InvoiceNumber.ToString().ToLower().Contains(filter.SearchValue) ||
                        (x.ContactFirstName?.ToLower() == filter.SearchValue) ||
                        x.ContactLastName?.ToLower() == filter.SearchValue ||
                        x.Status.ToLower().Contains(filter.SearchValue) ||
                        x.InvoiceId.ToString() == filter.SearchValue ||
                        x.JobId.ToString() == filter.SearchValue
                    )
                ).ToList();

            foreach (var invoice in filteredInvoices)
            {
                invoice.InvoicePayment = dbContext.TblInvoicePayments
                    .Where(p => p.InvoiceId == invoice.InvoiceId && p.IsDelete == false)
                    .Select(p => new InvoicePaymentVM
                    {
                        InvoicePaymentId = p.InvoicePaymentId,
                        InvoiceId = p.InvoiceId,
                        InvoiceAmount = p.InvoiceAmount,
                        Notes = p.Notes,
                        PaymentDate = p.CreatedAt,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PaymentDate)
                    .ToList();
            }

            return filteredInvoices;
        }


        #endregion Invoice_Generate_Methods

        #region Validate Stripe API Key 
        public bool IsStripeApiKeyValid()
        {
            try
            {
                if (string.IsNullOrEmpty(_stripeConfig?.StripeSecretKey??""))
                {
                    FileLogger.Log("Stripe API Key is missing.");
                    return false;
                }

                StripeConfiguration.ApiKey = _stripeConfig?.StripeSecretKey??"";

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
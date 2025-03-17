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

namespace ShutterManagement.Business.Services
{
    public class InvoiceService : BaseRepository<ShutterDBContext, TblInvoice>, IInvoiceService
    {
        private readonly ITenantService tenantService;
        private readonly IInvoicePaymentService invoicePaymentService;
        private readonly IHostEnvironment environment;
        private readonly IConverter converter;
        private readonly IContactService contactService;
        private readonly IEstimateService estimateService;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly IUserService userService;
        private readonly IJobService jobService;
        private readonly IOrderService orderService;
        public Pagination filter = new Pagination();
        private readonly StripeService _stripeService;

        public InvoiceService(ITenantService tenantService, IInvoicePaymentService invoicePaymentService,
            IHostEnvironment environment, IConverter converter ,IContactService contactService 
            ,IEstimateService estimateService, ITenantBrandingService tenantBrandingService,IUserService userService 
            ,IJobService jobService,IOrderService orderService,StripeService stripeService)
        {
            this.tenantService = tenantService;
            this.invoicePaymentService = invoicePaymentService;
            this.environment = environment;
            this.converter = converter;
            this.contactService = contactService;
            this.estimateService = estimateService;
            this.tenantBrandingService = tenantBrandingService;
            this.userService = userService;
            this.jobService = jobService;
            this.orderService = orderService;
            _stripeService = stripeService;
        }

        #region Create  update ActiveInActive Delete Getall
        public async Task<IResponseVm<string>> ActiveInActive(Guid InvoiceId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var invoice = GetAllQuerable()
                        .Where(c => c.InvoiceId == InvoiceId
                                   && c.TenantId == AppConstant.LoggedInTenantId
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                //var isInvoiceNumberExists = await AnyAsync(x =>
                //  x.InvoiceNumber == model.InvoiceNumber && x.IsDelete == false && x.IsActive==true && x.TenantId == AppConstant.LoggedInTenantId);

                //if (isInvoiceNumberExists)
                //{
                //    responseVm.IsSuccess = false;
                //    responseVm.Message = "Invoice Number Already Exists.";
                //    return responseVm;
                //}

                int Version = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
               .Select(p => (p.InvoiceNumber.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.InvoiceNumber.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                Version += 1;
                var InvoiceNumberUniquNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Invoice, AppConstant.LoggedInCompanyCode, Version);     //   var InvoiceNumberUniquNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Invoice, AppConstant.LoggedInCompanyCode);
                TblInvoice tblInvoice = new TblInvoice
                {
                    InvoiceId = Guid.NewGuid(),
                    JobId = model.JobId,
                    InvoiceNumber = InvoiceNumberUniquNumber,
                    EstimateId=model.EstimateId,
                    IssueDate = model?.IssueDate,
                    DueDate = model?.DueDate,
                    TotalAmount = model.TotalAmount,
                    BalanceDue = model.TotalAmount,
                    PaidAmount = 0,
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedBy = AppConstant.LoggedInUserID,
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

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;

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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var invoice = GetAllQuerable()
                        .Where(c => c.InvoiceId == InvoiceId
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    //var CheckPaymentExist = paymentsService.GetAllQuerable()
                    //   .Where(c => c.InvoiceId == InvoiceId
                    //              && c.TenantId == AppConstant.LoggedInTenantId
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var invoicesQuery = (from i in GetAllQuerable()
                                     join j in dbContext.TblJobs on i.JobId equals j.JobId
                                     join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                                     join d in dbContext.TblEstimates on i.EstimateId equals d.EstimateId
                                     where i.IsDelete == false
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
                                         ContactFirstName = c.FirstName ,
                                         ContactLastName = c.LastName,
                                         EstimateName=d.EstimateName,
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var invoice = GetAllQuerable()
                    .Where(c => c.InvoiceId == updateInvoice.InvoiceId
                                && c.TenantId == AppConstant.LoggedInTenantId
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
                invoice.BalanceDue = updateInvoice.TotalAmount - invoice.PaidAmount;
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
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }
                Guid? templateId = Guid.Parse("3A49D48B-CA1D-4E0E-9E1F-A22E7529EF47");

                var invoiceTemplate = dbContext.TblTemplates.FirstOrDefault(c =>
                    c.TemplateId == templateId  &&
                    c.IsDelete == false);

                if (invoiceTemplate == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
               

                var customerDetails = (await contactService.GetAll_Contact(filter,false))
                    .Data.FirstOrDefault(x => x.ContactId == model.ContactId && x.IsActive == true);

                var tenantDetails = tenantService.GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true).FirstOrDefault();
                if (tenantDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Tenant " + AppConstant.NoDataFound;
                    return responseVm;
                }
                var tenantInfoDetails = tenantBrandingService.GetAllQuerable().Where(x => x.Id == tenantDetails.BrandingInfo && x.IsActive == true).FirstOrDefault();



                var estimationDetails = dbContext.TblEstimates.FirstOrDefault(c =>
                    c.EstimateId == model.EstimationId &&
                    c.TenantId == AppConstant.LoggedInTenantId &&
                    c.IsDelete == false);
                if (estimationDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"EstimationDetails " + AppConstant.NoDataFound;
                    return responseVm;
                }



                var userDetails = dbContext.TblUsers.FirstOrDefault(c =>
                   c.Id == estimationDetails.CreatedBy &&
                   c.TenantId == AppConstant.LoggedInTenantId &&
                   c.IsDelete == false);

                var orderDetails = dbContext.TblOrders.FirstOrDefault(c =>
                   c.OrderId == model.OrderId &&
                   c.TenantId == AppConstant.LoggedInTenantId &&
                   c.IsDelete == false);

                var estimationRecord = await estimateService.ViewEachEstimation(model.EstimationId);

                var productList = estimationRecord.Data
                    .FirstOrDefault(x => x.IsActive == true)
                    ?.EstimateProductList ?? new List<EstimateProductList>();

                var invoiceData = new InvoiceDataModel
                {
                    CustomerName = customerDetails?.FirstName ?? "" + customerDetails?.LastName ?? "",
                    PreparedBy = userDetails?.FirstName + " " + userDetails?.LastName,
                    CustomerId = customerDetails?.ContactUniqueId ?? "",
                    CustomerCity = customerDetails?.billingAddressVm?.City ?? "",
                    CustomerState = customerDetails?.billingAddressVm?.State ?? "",
                    CustomerZipCode = customerDetails?.billingAddressVm?.ZipCode ?? "",
                    HomePhone = customerDetails?.billingAddressVm?.PhoneNumber ?? "",
                    CompanyStreetAddress = tenantInfoDetails?.StreetAddress ?? "",
                    CompanyCity = tenantInfoDetails?.City ?? "",
                    CompanyCountry = tenantInfoDetails?.Country ?? "",
                    CompanyZipCode = tenantInfoDetails?.ZipCode ?? "",
                    CustomerStreetAddress = customerDetails?.billingAddressVm?.StreetAddress ?? "",
                    MobilePhone = customerDetails?.PhoneNumber ?? "",
                    TenantCompanyName = tenantInfoDetails?.CompanyName ?? "",
                    Logo = tenantInfoDetails?.Logo ?? "",
                    EstimationDate = Convert.ToDateTime(estimationDetails?.CreatedAt).ToString("MM-dd-yyyy") ?? "",
                    CreatedBy = estimationDetails?.CreatedBy.ToString() ?? "",
                    EstimationSubtotal = estimationDetails?.TotalPrice.ToString() ?? "",
                    EstimationGrandTotal = estimationDetails?.GrandTotal.ToString() ?? "",
                    EstimationId = estimationDetails?.EstimateCode.ToString() ?? "",
                    EmailAddress = customerDetails?.Email ?? "",
                    Terms = estimationDetails?.TermsCondition ?? "",
                    Note = estimationDetails?.Notes ?? "",
                    OrderId = orderDetails?.OrderName ?? "",
                    Taxes = estimationDetails?.TaxAmount.ToString() ?? "",
                    Discount = estimationDetails?.DiscountAmount.ToString() ?? "",
                    Additionalcost = estimationDetails?.AdditionalCost.ToString() ?? "",
                    ProductListHtml = GenerateProductListHtml(productList)
                };

                var htmlContent = ReplaceTemplatePlaceholders(invoiceTemplate.TemplateValue, invoiceData);

                var pdfBytes = GenerateInvoicePdf(htmlContent);

                var tenantId = AppConstant.LoggedInTenantId.ToString();
                var rootPath = Path.Combine(AppConstant.FileUploadPath, tenantId);
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                var fileName = $"Invoice_{Guid.NewGuid()}.pdf";
                var savePath = Path.Combine(rootPath, fileName);

                await using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
                var pdfBase64 = Convert.ToBase64String(pdfBytes);


                if (model.DocumentType == EstimateDocumentType.ForEmail)
                {
                    var IsEstimateExists = dbContext.TblEstimates
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.EstimateId == model.EstimationId)
                        .OrderByDescending(x => x.CreatedAt)
                        .FirstOrDefault();

                    if (IsEstimateExists == null)
                    {
                        responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                        return responseVm;
                    }

                    IsEstimateExists.Status = EnumValue.GetEnumValue(Estimation.PendingForApproval);
                    IsEstimateExists.UpdatedAt = DateTime.Now;
                    IsEstimateExists.ModifiedBy = AppConstant.LoggedInUserID;
                    await estimateService.Update(IsEstimateExists);

                    string approvalEmailLink = $"{AppConstant.UIBaseURL}contacts/estimate/preview/{IsEstimateExists?.EstimateId}";

                    var emailBody = $@"
     <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px;'>
        <p><strong>Dear {customerDetails?.FirstName+" "+ customerDetails?.LastName},</strong></p>

        <p>Please find your invoice attached for your reference.</p>

        <p><strong>You can approve  your estimate - {estimationDetails?.EstimateCode}by clicking the link below:</strong></p>

        <p>
            <a href='{approvalEmailLink}'>
                Approval Estimate
            </a>
        </p>

        <p><strong>Thank you for your business!</strong></p>

        
        

        <p>
            <img src='{tenantInfoDetails?.Logo}' alt='Company Logo' height='50px' 
                 style='margin-top: 10px; display: block;'>
        </p>
<p>Best regards,</p>
<p><strong>{tenantInfoDetails?.CompanyName}</strong></p>
<p>Shutter Management and Estimation System</p>
    </div>";


                    var emailAttachment = new EmailAttachment
                    {
                        FileName = $"{estimationDetails.EstimateCode}.pdf",
                        Stream = new MemoryStream(pdfBytes),
                        MediaType = "application/pdf"
                    };

                    var emailSent = EmailHelper.SendEmailWithAttachment(
                        customerDetails.Email,
                        "Estimate Approval Link and PDF",
                        emailBody,
                        new List<EmailAttachment> { emailAttachment }
                    );

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Email Send SuccessFully.";
                    File.Delete(savePath);

                }
                else if(model.DocumentType==EstimateDocumentType.OrderForEmail)
                {
                    var IsOrderExists =  (await orderService.GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId &&
                    x.OrderId== model.OrderId && x.Status != EnumValue.GetEnumValue(Estimation.Approved)).
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

                   // string approvalOrderEmailLink = $"{AppConstant.UIBaseURL}contacts/order/preview/{IsOrderExists?.OrderId}";

                    var emailBody = $@"
     <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px;'>
        <p><strong>Dear {customerDetails?.FirstName + " " + customerDetails?.LastName},</strong></p>

        <p>Please find your invoice attached for your reference.</p>

        <p>You can approve  your Order - {IsOrderExists?.OrderName} by clicking the link below:</p>

        <p>
            <a href='{AppConstant.UIBaseURL}contacts/order/preview/{IsOrderExists?.OrderId}'>
               Order Approval link. 
            </a>
        </p>

        <p><strong>Thank you for your business!</strong></p>

       
        

        <p>
            <img src='{tenantInfoDetails?.Logo}' alt='Company Logo' height='100px' 
                 style='margin-top: 10px; display: block;'>
        </p>
        <p>Best regards,</p>
        <p><strong>{tenantInfoDetails?.CompanyName}</strong></p>
        <p>Shutter Management and Estimation System</p>
    </div>";


                    var emailAttachment = new EmailAttachment
                    {
                        FileName = $"{IsOrderExists?.OrderName}.pdf",
                        Stream = new MemoryStream(pdfBytes),
                        MediaType = "application/pdf"
                    };

                    var emailSent = EmailHelper.SendEmailWithAttachment(
                        customerDetails.Email,
                        "Order Approval Link and PDF",
                        emailBody,
                        new List<EmailAttachment> { emailAttachment }
                    );

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Email Send SuccessFully.";
                    File.Delete(savePath);



                }

                else if (model.DocumentType == EstimateDocumentType.InvoicePaymentForEmail)
                {
                    Guid? invoiceTemplateId = Guid.Parse("E9E279C4-1F54-476B-BCDF-778BD1634894");

                    var invoiceForEmailTemplate = dbContext.TblTemplates.FirstOrDefault(c =>
                        c.TemplateId == invoiceTemplateId &&
                        c.IsDelete == false);

                   

                    if (invoiceForEmailTemplate == null)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Invoice Template " + AppConstant.NoDataFound;
                        return responseVm;
                    }

                    var paymentDetails = dbContext.TblInvoicePayments
                        .FirstOrDefault(x => x.InvoiceId == model.InvoiceId &&
                                             x.IsDelete == false);

                    if (paymentDetails == null)
                    {
                        responseVm.Message = "Payment " + AppConstant.NoDataFound;
                        return responseVm;
                    }

                    var invoiceDetails = dbContext.TblInvoices
                        .FirstOrDefault(x => x.InvoiceId == model.InvoiceId &&
                                             x.TenantId == AppConstant.LoggedInTenantId &&
                                             x.IsDelete == false);

                   

                    if (invoiceDetails == null)
                    {
                        responseVm.Message = "Invoice " + AppConstant.NoDataFound;
                        return responseVm;
                    }
                    
                    var paymentList = dbContext.TblInvoicePayments
                        .Where(x => x.InvoiceId == model.InvoiceId && x.IsActive == true)
                        .Select(p => new InvoicePayment
                        {
                            Notes = p.Notes,
                            InvoiceAmount = p.InvoiceAmount,

                        })
                        .ToList();

                    var estimationDetail = dbContext.TblEstimates.FirstOrDefault(c =>
                   c.EstimateId == model.EstimationId &&
                   c.TenantId == AppConstant.LoggedInTenantId &&
                   c.IsDelete == false);
                    if (estimationDetail == null)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = $"EstimationDetails " + AppConstant.NoDataFound;
                        return responseVm;
                    }

                    var invoiceDataModel = new InvoiceDataModel
                    {
                        CustomerName = customerDetails?.FirstName + " " + customerDetails?.LastName,
                        PreparedBy = userDetails?.FirstName + " " + userDetails?.LastName,
                        CustomerId = customerDetails?.ContactUniqueId ?? "",
                        CustomerCity = customerDetails?.billingAddressVm?.City ?? "",
                        CustomerState = customerDetails?.billingAddressVm?.State ?? "",
                        CustomerZipCode = customerDetails?.billingAddressVm?.ZipCode ?? "",
                        HomePhone = customerDetails?.billingAddressVm?.PhoneNumber ?? "",
                        CompanyStreetAddress = tenantInfoDetails?.StreetAddress ?? "",
                        CompanyCity = tenantInfoDetails?.City ?? "",
                        CompanyCountry = tenantInfoDetails?.Country ?? "",
                        CompanyZipCode = tenantInfoDetails?.ZipCode ?? "",
                        CustomerStreetAddress = customerDetails?.billingAddressVm?.StreetAddress ?? "",
                        MobilePhone = customerDetails?.PhoneNumber ?? "",
                        TenantCompanyName = tenantInfoDetails?.CompanyName ?? "",
                        Logo = tenantInfoDetails?.Logo ?? "",
                        EstimationDate = Convert.ToDateTime(invoiceDetails?.CreatedAt).ToString("MM-dd-yyyy") ?? "",
                        CreatedBy = invoiceDetails?.CreatedBy.ToString() ?? "",
                        EstimationSubtotal = invoiceDetails?.TotalAmount.ToString() ?? "", 
                        EstimationGrandTotal = invoiceDetails?.BalanceDue.ToString() ?? "", 
                        EstimationId = estimationDetail?.EstimateCode?.ToString() ?? "",
                        EmailAddress = customerDetails?.Email ?? "",
                        Terms = estimationDetails?.TermsCondition ?? "",
                        Note = estimationDetails?.Notes ?? "",
                        OrderId = orderDetails?.OrderName ?? "",
                        TotalAmount = invoiceDetails?.TotalAmount.ToString() ?? "",
                        DueAmount = invoiceDetails?.BalanceDue.ToString() ?? "",
                        InvoiceNo = invoiceDetails?.InvoiceNumber?.ToString() ?? "",
                        IssueDate = Convert.ToDateTime(invoiceDetails?.IssueDate).ToString("MM-dd-yyyy") ?? "",
                        PaymentList = GenerateInvoicetListHtml(paymentList) 
                    };

                    // Replace placeholders in Invoice Template
                    var htmlContentforInvoice = ReplaceTemplatePlaceholders(invoiceForEmailTemplate.TemplateValue, invoiceDataModel);

                    // Generate Invoice PDF
                    var pdfBytesInvoice = GenerateInvoicePdf(htmlContentforInvoice);

                    // Save the invoice PDF
                    var tenantIdInvoice = AppConstant.LoggedInTenantId.ToString();
                    var rootPathInvoice = Path.Combine(AppConstant.FileUploadPath, tenantIdInvoice);
                    if (!Directory.Exists(rootPathInvoice))
                    {
                        Directory.CreateDirectory(rootPathInvoice);
                    }

                    var InvoicefileName = $"Invoice_{Guid.NewGuid()}.pdf";
                    var InvicesavePath = Path.Combine(rootPathInvoice, InvoicefileName);

                    await using (var stream = new FileStream(InvicesavePath, FileMode.Create))
                    {
                        await stream.WriteAsync(pdfBytesInvoice, 0, pdfBytesInvoice.Length);
                    }

                    var InvoicepdfBase64 = Convert.ToBase64String(pdfBytesInvoice);

                    // Generate Invoice Payment Link
                    // string paymentLink = $"{AppConstant.UIBaseURL}contacts/invoice/payment/{invoiceDetails?.InvoiceId}";

                    // Construct Invoice Email Body

                    // Generate Stripe Payment Link

                    // Generate Stripe Payment Link
                    var paymentRequest = new PaymentRequest
                    {
                        CustomerEmail = customerDetails.Email,
                        ProductName = $"Invoice Payment - {invoiceDetails?.InvoiceNumber}",
                        Amount = (long)(invoiceDetails?.BalanceDue * 100), // Convert to cents
                        Quantity = 1,
                    };

                    string paymentLink = _stripeService.CreateCheckoutSession(paymentRequest);


                    var emailBody = $@"
<div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0px 0px 10px rgba(0,0,0,0.1); padding: 20px; font-family: Arial, sans-serif; color: #333;'>

    <!-- Header -->
    <div style='background: #007bff; color: #fff; text-align: center; padding: 15px; border-radius: 8px 8px 0 0;'>
        <h2 style='margin: 0; font-size: 20px;'>Invoice Details</h2>
    </div>

    <!-- Content -->
    <div style='padding: 20px;'>
        <p style='font-size: 16px; margin: 10px 0;'><strong>Dear {customerDetails?.FirstName + " " + customerDetails?.LastName},</strong></p>
        <p style='font-size: 14px; margin: 10px 0;'>Please find your invoice attached for your reference.</p>

        <!-- Invoice Details -->
        <div style='background: #e9ecef; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Total Amount:</strong> {invoiceDetails?.TotalAmount}</p>
            <p style='margin: 5px 0; font-size: 14px;'><strong>Due Amount:</strong> {invoiceDetails?.BalanceDue}</p>
        </div>

        <!-- Payment Link -->
        <p style='font-size: 14px; margin: 15px 0;'>You can make the payment for your invoice <strong>{invoiceDetails?.InvoiceNumber}</strong> by clicking the link below:</p>
        <div style='text-align: center; margin: 20px 0;'>
            <a href='{paymentLink}' style='background: #28a745; color: #fff; padding: 12px 20px; text-decoration: none; font-size: 14px; border-radius: 5px; display: inline-block;'>Pay Now</a>
        </div>

        <hr style='border: 0; height: 1px; background: #ddd; margin: 20px 0;'>

        <!-- Footer -->
        <p style='font-size: 14px; text-align: center;'>Thank you for your business!</p>

        <div style='text-align: center; margin: 15px 0;'>
            <img src='{tenantInfoDetails?.Logo}' alt='Company Logo' style='height: 50px;'>
        </div>

        <p style='text-align: center; font-size: 14px;'><strong>Best regards,</strong></p>
        <p style='text-align: center; font-size: 14px;'><strong>{tenantInfoDetails?.CompanyName}</strong></p>
        <p style='text-align: center; font-size: 14px;'>Shutter Management and Estimation System</p>
    </div>
</div>";


                    var emailAttachment = new EmailAttachment
                    {
                        FileName = $"{invoiceDetails?.InvoiceNumber}.pdf",
                        Stream = new MemoryStream(pdfBytesInvoice),
                        MediaType = "application/pdf"
                    };

                    var emailSent = EmailHelper.SendEmailWithAttachment(
                        customerDetails.Email,
                        "Invoice Payment Link and PDF",
                        emailBody,
                        new List<EmailAttachment> { emailAttachment }
                    );

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Invoice email sent successfully.";
                    FileLogger.Log($"Invoice File Path for payment {InvicesavePath}");
                   // File.Delete(InvicesavePath);
                }

                else if (model.DocumentType == EstimateDocumentType.DownloadPDF)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InvoiceDownloadSuccessMsg;
                    responseVm.Data = pdfBase64;

                    System.IO.File.Delete(savePath);
                }

                }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError}: {ex.Message}";
            }

            return responseVm;
        }

        private string GenerateProductListHtml(List<EstimateProductList> productList)
        {
            var productListHtml = new StringBuilder();
            int i = 0;
            foreach (var product in productList)
            {
                decimal? unitprice = product.TotalPrice / product.WindowQty;
                productListHtml.Append($@"
    <tr style='background-color: #efefef;'>
        <td style='text-align: center; padding: 5px 5px;'>{++i}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.ProductTypeName}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.ProductName}</td>
        <td style='text-align: center; padding: 5px 5px;'>{string.Join(", ", product.Options)}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.WindowQty}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.Width}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.Height}</td>
        <td style='text-align: center; padding: 5px 5px;'>{unitprice}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.TotalPrice}</td>
    </tr>");
            }

            return $"<tbody>{productListHtml}</tbody>";
        }


        private string GenerateInvoicetListHtml(List<InvoicePayment> payments)
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

         

        private string ReplaceTemplatePlaceholders(string template, InvoiceDataModel data) 
        {
            return template
                .Replace("{{customerName}}", data.CustomerName ?? "")
                .Replace("{{customerId}}", data.CustomerId ?? "")
                .Replace("{{customercity}}", data.CustomerCity ?? "")
                .Replace("{{customerstate}}", data.CustomerState ?? "")
                .Replace("{{customerZipcode}}", data.CustomerZipCode ?? "")
                .Replace("{{streetaddress}}", data.CustomerStreetAddress ?? "")
                .Replace("{{homephone}}", data.HomePhone ?? "")
                .Replace("{{companystreetaddress}}", data.CompanyStreetAddress ?? "")
                .Replace("{{companycity}}", data.CompanyCity ?? "")
                .Replace("{{companycountry}}", data.CompanyCountry ?? "")
                .Replace("{{companyzipcode}}", data.CompanyZipCode ?? "")
                .Replace("{{mobilephone}}", data.MobilePhone ?? "")
                .Replace("{{tenantCompanyName}}", data.TenantCompanyName ?? "")
                .Replace("{{logo}}", data.Logo ?? "")
                .Replace("{{estimationdate}}", data.EstimationDate ?? "")
                .Replace("{{createdby}}", data.PreparedBy ?? "")
                .Replace("{{estimationsubtotal}}", data.EstimationSubtotal ?? "")
                .Replace("{{estimationgrandtotal}}", data.EstimationGrandTotal ?? "")
                .Replace("{{estimationId}}", data.EstimationId ?? "")
                .Replace("{{emailaddress}}", data.EmailAddress ?? "")
                .Replace("{{productlist}}", data.ProductListHtml ?? "")
                .Replace("{{paymentlist}}", data.PaymentList ?? "")
                .Replace("{{terms}}", data.Terms ?? "")
                .Replace("{{note}}", data.Note ?? "")
                .Replace("{{orderid}}", string.IsNullOrEmpty(data.OrderId) ? "" : $"Order ID: {data.OrderId}")
                .Replace("{{issuedate}}", data.IssueDate ?? "")
                .Replace("{{totalamount}}", data.TotalAmount ?? "")
                .Replace("{{balancedue}}", data.DueAmount ?? "")
                .Replace("{{invoiceno}}", data.InvoiceNo ?? "")
                .Replace("{{additionalcost}}", data.Additionalcost ?? "")
                .Replace("{{discount}}", data.Discount ?? "")
                .Replace("{{taxes}}", data.Taxes ?? "");
        }

        public byte[] GenerateInvoicePdf(string htmlContent)
        {
            var pdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = DinkToPdf.ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4
                },
                Objects = { new ObjectSettings { HtmlContent = htmlContent } }
            };

            return converter.Convert(pdfDocument);
        }



        public async Task<IResponseVm<List<InvoiceVM>>> InvoiceReport(Pagination filter, ReportFilterType type)
        {
            IResponseVm<List<InvoiceVM>> responseVm = new ResponseVm<List<InvoiceVM>>();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
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
                                 where i.IsDelete == false && i.TenantId == AppConstant.LoggedInTenantId
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
                                     TenantId=i.TenantId
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
    }
}

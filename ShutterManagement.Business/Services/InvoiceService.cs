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

namespace ShutterManagement.Business.Services
{
    public class InvoiceService : BaseRepository<BtprojecShuttermanagementDevContext, TblInvoice>, IInvoiceService
    {
        private readonly ITenantService tenantService;
        private readonly IInvoicePaymentService invoicePaymentService;

        public InvoiceService(ITenantService tenantService, IInvoicePaymentService invoicePaymentService )
        {
            this.tenantService = tenantService;
            this.invoicePaymentService = invoicePaymentService;
        }

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
                var isInvoiceNumberExists = await AnyAsync(x =>
                  x.InvoiceNumber == model.InvoiceNumber && x.IsDelete == false && x.IsActive==true && x.TenantId == AppConstant.LoggedInTenantId);

                if (isInvoiceNumberExists)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invoice Number Already Exists.";
                    return responseVm;
                }

                TblInvoice tblInvoice = new TblInvoice
                {
                    InvoiceId = Guid.NewGuid(),
                    JobId = model.JobId,
                    InvoiceNumber = model.InvoiceNumber,
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

                var invoicesQuery = GetAllQuerable().Where(x => x.IsDelete == false).OrderByDescending(x => x.CreatedAt)
                                    .Select(i => new InvoiceVM
                                    {
                                        InvoiceId = i.InvoiceId,
                                        InvoiceNumber = i.InvoiceNumber,
                                        JobId = i.JobId,
                                        IssueDate = i.IssueDate,
                                        TotalAmount = i.TotalAmount,
                                        PaidAmount = i.PaidAmount,
                                        BalanceDue = i.BalanceDue,
                                        DueDate = i.DueDate,
                                        Status = i.Status??"",
                                        IsActive = i.IsActive,
                                    }).ToList();
                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var filteredInvoices = invoicesQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        ((string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.InvoiceNumber.Contains(filter.SearchValue.Trim())) ||
                            (x.Status.Contains(filter.SearchValue.Trim())) ||
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


    }
}

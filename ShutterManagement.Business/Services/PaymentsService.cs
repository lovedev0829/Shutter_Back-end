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

namespace ShutterManagement.Business.Services
{

    public class PaymentsService : BaseRepository<BtprojecShuttermanagementDevContext, TblPayments>, IPaymentsService
    {
        private readonly ITenantService tenantService;
        private readonly IInvoiceService invoiceService;

        public PaymentsService(ITenantService tenantService ,IInvoiceService invoiceService)
        {
            this.tenantService = tenantService;
            this.invoiceService = invoiceService;
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
                TblPayments tblPayments = new TblPayments
                {
                    PaymentId = Guid.NewGuid(),
                    InvoiceId = model.InvoiceId,
                    PaymentDate = model.PaymentDate,
                    PaymentDueDate = model.PaymentDueDate,
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
                
                if (model.PaymentStatus == PaymentStatus.Paid)
                {
                    invoice.PaidAmount += model.Amount ?? 0;
                    invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
                    invoice.Status = EnumValue.GetEnumValue(PaymentStatus.Paid);
                    invoice.UpdatedAt = DateTime.Now;
                    invoice.ModifiedBy = AppConstant.LoggedInUserID;
                    await invoiceService.Update(invoice);
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
                                        PaymentDueDate = i.PaymentDueDate,
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
                payment.PaymentDueDate = updatePayment.PaymentDueDate;
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





    }

}

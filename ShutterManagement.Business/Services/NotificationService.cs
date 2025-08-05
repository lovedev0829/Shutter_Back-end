using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class NotificationService : BaseRepository<ShutterDBContext, TblNotification>, INotificationService
    {
        private readonly ClaimsProperty claimsProperty;
        private readonly EmailSendHelperService emailSendHelperService;

        public NotificationService(ClaimsProperty claimsProperty,EmailSendHelperService emailSendHelperService)
        {
            this.claimsProperty = claimsProperty;
            this.emailSendHelperService = emailSendHelperService;
        }

        public async Task<IResponseVm<string>> CreateNotification(CreateNotification model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                TblNotification notification = new TblNotification()
                {
                    Id = Guid.NewGuid(),
                    UserId = model.UserId,
                    Subject = model.Subject,
                    Body = model.Body,
                    Createdate = DateTime.Now,
                    IsReaded = model.IsReaded,
                    Type = model.Type,
                    TypeId = model.TypeId,
                };
                var res = await Add(notification);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateNotification(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var logdinTenantId = claimsProperty.GetTenantId();

                var subTenantIds = new List<Guid>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    subTenantIds = dbContext.TblTenants.Where(t => t.ParentTenantId == logdinTenantId || t.TenantId == logdinTenantId).Select(t => t.TenantId).ToList();
                }

                var notification = GetAllQuerable().Where(x => x.Id == Id && (x.UserId == logdinTenantId || subTenantIds.Contains(x.UserId ?? Guid.Empty))).FirstOrDefault();
                if (notification != null)
                {
                    notification.IsReaded = true;
                    var res = await Update(notification);
                    responseVm.IsSuccess = true;
                    responseVm.Message += AppConstant.UpdateSuccessMsg;
                }
            }
            catch(Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllNotification>>> GetAllNotifications()
        {
            IResponseVm<List<GetAllNotification>> responseVm = new ResponseVm<List<GetAllNotification>>(); 
            try
            {
                var logdinTenantId = claimsProperty.GetTenantId();

                var subTenantIds = new List<Guid>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    subTenantIds = dbContext.TblTenants.Where(t => t.ParentTenantId == logdinTenantId || t.TenantId == logdinTenantId).Select(t => t.TenantId).ToList();
                }

                var notification = GetAllQuerable().Where(x => (x.UserId == logdinTenantId || subTenantIds.Contains(x.UserId ?? Guid.Empty))).OrderByDescending(pt=> pt.Createdate).Select(pt => new GetAllNotification
                {
                    Id = pt.Id,
                    Subject = pt.Subject,
                    Body = pt.Body,
                    CreatedDate = pt.Createdate,
                    IsReaded = pt.IsReaded,
                    Type = pt.Type,
                    TypeId = pt.TypeId.ToString(),
                    UserId = pt.UserId,
                }).ToList();
               
                responseVm.IsSuccess = true;
                responseVm.Data = notification;
                responseVm.TotalRecords = notification.Count;

            }
            catch(Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }


        public async Task SendInAppNotification(EstimateOrOrderBaseVm vm, string type, Guid? TypeId, NotificationModule notificationModule = NotificationModule.Default)
        {
            var notificationTemplate = dbContext.TblNotificationTemplates
                .FirstOrDefault(t => t.Type == type);

            if (notificationTemplate == null) return;

            var replacedContent = emailSendHelperService.PlaceholderReplace(notificationTemplate.Body, vm);

            var notificationVm = new CreateNotification
            {
                UserId = vm.TenantId,
                Subject = notificationTemplate.Subject ?? "",
                Body = replacedContent ?? "",
                CreatedDate = DateTime.Now,
                Type = nameof(notificationModule),
                TypeId = TypeId
            };

            await CreateNotification(notificationVm);
        }
    }

}

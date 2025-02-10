using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class NotificationService : BaseRepository<ShutterDBContext, TblNotification>, INotificationService
    {
        public NotificationService()
        {
            
        }

        public async Task<IResponseVm<string>> CreateNotification(CreateNotification model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                TblNotification notification = new TblNotification()
                {
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
                var notification = GetAllQuerable().Where(x => x.Id == Id && x.UserId == AppConstant.LoggedInUserID).FirstOrDefault();
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
                var  notification = GetAllQuerable().Where(x => x.UserId == AppConstant.LoggedInUserID).OrderByDescending(pt=> pt.Createdate).Select(pt => new GetAllNotification
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
    }

}

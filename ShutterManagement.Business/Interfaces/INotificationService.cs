using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Interfaces
{
    public interface INotificationService : IGenericRepository<TblNotification>

    {
        Task<IResponseVm<List<GetAllNotification>>> GetAllNotifications();
        Task<IResponseVm<string>> UpdateNotification(Guid Id);
        Task<IResponseVm<string>> CreateNotification(CreateNotification model);

        Task SendInAppNotification(EstimateOrOrderBaseVm vm, string type, Guid? TypeId ,NotificationModule notificationModule = NotificationModule.Default);

    }
}

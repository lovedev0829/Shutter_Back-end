using Microsoft.Win32;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class DashboardService : BaseRepository<ShutterDBContext, TblJob>, IDashboard
    {
        private readonly ShutterDBContext context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;

        public DashboardService(ShutterDBContext context, ClaimsProperty claimsProperty, IAddress address,
            ITenantService tenantService) {
            this.context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
        }
        /// <summary>
        /// Retrieves all dashboard data for the logged-in tenant, including job status counts.
        /// </summary>
        /// <returns>A response object containing the dashboard data or an error message.</returns>
        public async Task<IResponseVm<DashBoardVm>> GetallDashBoardData()
        {

            IResponseVm<DashBoardVm> responseVm = new ResponseVm<DashBoardVm>();

            try
            {
                bool isTenant = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&  
                    x.IsDelete==false &&                               
                    x.IsActive==true);   
                if (!isTenant)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound; 
                    return responseVm;
                }

                var statusCounts =  (from status in dbContext.MstJobStatuses 
                                          join job in dbContext.TblJobs 
                                          on status.JobStatusId  equals job.JobStatus into jobGroup 
                                          select new StatusCountVm
                                          {
                                              StatusName = status.JobStatusName,     
                                              TotalAwardedJobs = jobGroup.Count(j => j.TenantId == AppConstant.LoggedInTenantId)    
                                          }).ToList();

                var contactsCount =  (from c in dbContext.TblContacts
                                           where c.IsDelete ==false && c.TenantId == AppConstant.LoggedInTenantId 
                                           select c).Count();

                var totalEstimateRevenue =  (from est in dbContext.TblEstimates
                                                  where est.Status == "GenerateOrder" && est.IsDelete==false &&  est.TenantId == AppConstant.LoggedInTenantId
                                             select est.GrandTotal).Sum();

                responseVm.Data = new DashBoardVm
                {
                    StatusCounts = statusCounts,
                     ContactsCount = contactsCount,
                     TotalEstimateRevenue = totalEstimateRevenue
                };

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg; 
            }
            catch (Exception ex)
            {

                responseVm.IsSuccess = false;
                responseVm.Message = $"An error occurred: {AppConstant.ExceptionError}"; 
            }
            return responseVm;
        }

       
    }
}

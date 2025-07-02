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
using System.Globalization;
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
        public async Task<IResponseVm<List<StatusCountVm>>> GetallDashBoardData()
        {

            IResponseVm<List<StatusCountVm>> responseVm = new ResponseVm<List<StatusCountVm>>();

            try
            {
                bool isTenant = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() &&  
                    x.IsDelete==false &&                               
                    x.IsActive==true);   
                if (!isTenant)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound; 
                    return responseVm;
                }
                var Data = new List<StatusCountVm>();

                var statusCounts = dbContext.MstJobStatuses
                            .GroupJoin(dbContext.TblJobs.Where(j => j.TenantId == claimsProperty.GetTenantId() && j.IsDelete==false),
                                m => m.JobStatusId,
                                j => j.JobStatus,
                                (m, jobGroup) => new
                                {
                                    m.JobStatusName,
                                    m.JobStatusId,
                                    Count = jobGroup.Count(),
                                    Icon = m.Icon,
                                    TenantId = jobGroup.Select(x=>x.TenantId).ToArray()
                                })
                            .ToList();

                Data = statusCounts.Where(x=>x.JobStatusName == "In-Production" || x.JobStatusName == "Installation Scheduled").Select(x => new StatusCountVm
                {
                    Label = x.JobStatusName.Replace("In-Production", "In Production").Replace("Installation Scheduled", "Installation Scheduled"),
                    Image = x.Icon,
                    Count = x.TenantId.Where(x => x == claimsProperty.GetTenantId()).Count().ToString(),
                    Url=$"/job/{x.JobStatusId}"
                }).ToList();

                
                var contactsCount =  (from c in dbContext.TblContacts
                                           where c.IsDelete ==false && c.TenantId == claimsProperty.GetTenantId() 
                                           select c).Count();
                StatusCountVm contactVm = new StatusCountVm
                {
                    Label = "No. Of Contacts",
                    Count = contactsCount.ToString(),
                    Image = AppConstant.NoOfContactsImg,
                    Url= "/contacts"
                };
                Data.Add(contactVm);

                decimal? totalEstimateRevenue =  (from est in dbContext.TblEstimates
                                                  where est.Status == "GenerateOrder" && est.IsDelete==false &&  est.TenantId == claimsProperty.GetTenantId()
                                             select est.GrandTotal).Sum();

                StatusCountVm estVm = new StatusCountVm
                {
                    Label = "Estimate Revenue",
                    Count = "$"+totalEstimateRevenue,
                    Image = AppConstant.EstimateRevenueImg,
                    Url= "/order"
                };
                Data.Add(estVm);
                responseVm.Data = Data;
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

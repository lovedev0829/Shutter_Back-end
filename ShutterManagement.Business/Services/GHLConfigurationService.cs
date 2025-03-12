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
    public class GHLConfigurationService : BaseRepository<ShutterDBContext, TblGhlconfiguration> , IGHLConfiguration
    {
        private readonly ClaimsProperty claimsProperty;

        public GHLConfigurationService(ClaimsProperty claimsProperty)
        {
            this.claimsProperty = claimsProperty;
        }

        public async Task GetGHLConfiguration(string Id)
        {

            var details = GetAllQuerable()
                   .Where(x => x.LocationId == Id || x.TenantId.ToString().ToLower() == Id.ToLower())
                   .FirstOrDefault();
            FileLogger.Log("details GHl: ", details);
            if (details != null)
            {
                AppConstant.GHL_BaseAddress = details.BaseAddress;
                AppConstant.GHL_APIKey = details.Apikey;
                AppConstant.GHL_PipelineId = details.PipelineId;
                AppConstant.GHL_Locationd = details.LocationId;
                AppConstant.LoggedInTenantId = details.TenantId;
            }
        }

        public async Task<IResponseVm<string>> GHLConfiguration(GHLConfiguration gHLConfiguration)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
               var IsGHLConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if(IsGHLConfigurationTenantExist != null)
                {
                    IsGHLConfigurationTenantExist.Apikey = gHLConfiguration.APIkey;
                    IsGHLConfigurationTenantExist.LocationId = gHLConfiguration.LocationId;
                    IsGHLConfigurationTenantExist.PipelineId = gHLConfiguration.PipelineId;
                    IsGHLConfigurationTenantExist.BaseAddress = gHLConfiguration.BaseAddress;

                    var res = await Update(IsGHLConfigurationTenantExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    TblGhlconfiguration tblGhlconfiguration = new TblGhlconfiguration()
                    {
                        PipelineId = gHLConfiguration.PipelineId,
                        BaseAddress = gHLConfiguration.BaseAddress,
                        LocationId = gHLConfiguration.LocationId,
                        Apikey = gHLConfiguration.APIkey,
                        TenantId = AppConstant.LoggedInTenantId,
                        LastSync = DateTime.Now,
                    };
                    var res1 = await Add(tblGhlconfiguration);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message += ex.Message;
            }
            return responseVm;
        }

        public async  Task<IResponseVm<GetGHLConfiguration>> GetGHLConfiguration()
        {
            IResponseVm<GetGHLConfiguration> responseVm = new ResponseVm<GetGHLConfiguration>();
            try
            {
                var gHLConfiguration = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                responseVm.Data = new GetGHLConfiguration
                {
                   APIkey = gHLConfiguration?.Apikey??"",
                   BaseAddress = gHLConfiguration?.BaseAddress??"",
                   LocationId = gHLConfiguration?.LocationId??"",
                   PipelineId = gHLConfiguration?.PipelineId ?? "",
                   LastSync = gHLConfiguration?.LastSync ?? DateTime.Now,
                };
                responseVm.IsSuccess = true;
                responseVm.Message= AppConstant.FetchSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message += ex.Message;
            }
            return responseVm;

        }
    }
}

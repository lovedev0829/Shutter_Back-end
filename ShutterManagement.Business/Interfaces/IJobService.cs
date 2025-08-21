using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagment.GHL.ViewModel.Response;
using ShutterManagment.GHL.ViewModel.Request;

namespace ShutterManagement.Business.Interfaces
{
    public interface IJobService : IGenericRepository<TblJob>
    {
        Task<IResponseVm<string>> Create(CreateJob model, Guid? LoggedInTenantId = null);
        Task<IResponseVm<string>> UpdateJob(UpdateJob model, Guid? LoggedInTenantId = null);
        Task<IResponseVm<string>> DeleteJob(string Id);
        Task<IResponseVm<string>> ActiveInactive(string Id);
        Task<IResponseVm<string>> UpdateJobStatus(UpdateJobStatus model);

        Task<IResponseVm<List<GetAllJobsdetails>>> GetAll(Pagination filter);

        Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetAllJobsTenant_SubTenant(Pagination filter);

        Task<IResponseVm<List<Opportunity>>> SyncOpportunitiesFromGhl(GetGHLConfiguration getGHLConfiguration, Guid? LoggedInTenantId = null);
        Task<IResponseVm<string>> UpdateOpportunityFromGHL(GHLOpportunityTrigger opportunitydata);
        Task<IResponseVm<string>> OpportunityFromGHL(GHLOpportunityTrigger opportunitydata);

        //Task<IResponseVm<string>> GetAndSaveContactsAsync();

        Task<IResponseVm<string>> UpdateEstimateUrlInGhlOnly(TblJob job, string newEstimateUrl, decimal Grandtotal);
        Task<IResponseVm<string>> UpdateJobPowderCoatColor(Guid? JobId, Guid? powderCoatColorId);

        Task<IResponseVm<GHLJobAssignedtoPartner>> AddOrUpdateOpportunityInGHL(CreateJob model, bool IsforUpdate = false);
    }
}

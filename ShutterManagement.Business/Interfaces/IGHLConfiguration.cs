using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Interfaces
{
    public interface IGHLConfiguration : IGenericRepository<TblGhlconfiguration>
    {
        Task<IResponseVm<string>> GHLConfiguration(GHLConfiguration gHLConfiguration);

        Task<IResponseVm<GetGHLConfiguration>> GetGHLConfigurations(string? code);

        Task GetGHLConfiguration(string Id);

        Task<IResponseVm<string>> CheckGHLConfiguration();
        Task<IResponseVm<string>> CheckStageMapping();

        Task<IResponseVm<string>> GetOauthGHL();

        Task<IResponseVm<string>> MapGHLStagesStages(StageMappingRequest request);


        Task<IResponseVm<List<StageMapping>>> GetGHLStagesMappings();

        Task<IResponseVm<string>> DeleteGHLConfiguration(Guid? configurationid);
    }
}

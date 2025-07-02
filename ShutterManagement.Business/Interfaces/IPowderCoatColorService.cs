using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IPowderCoatColorService
    {
        Task<IResponseVm<string>> Create_PowderCoatColor(CreatePowderCoatColor model);
        Task<IResponseVm<string>> Update_PowderCoatColor(UpdatePowderCoatColor model);
        Task<IResponseVm<string>> ActiveInActive(Guid? powderCoatColorId);
        Task<IResponseVm<string>> DeletePowderCoatColor(Guid? powderCoatColorId);
        Task<IResponseVm<List<PowderCoatColorResponse>>> GetAllPowderCoatColor(Pagination filter);
        Task<IResponseVm<List<PowderCoatColorResponse>>> GetPowderCoatColorsForDropdown();
    }
}
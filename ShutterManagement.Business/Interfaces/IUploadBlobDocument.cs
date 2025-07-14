using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Interfaces
{
    public interface IUploadBlobDocument : IGenericRepository<TblBlobdocument>
    {

        Task<IResponseVm<string>> UploadDocument(CreateBlobDocument model);
        Task<IResponseVm<string>> UpdateDocument(UpdateBlobDocument model);
        Task<IResponseVm<string>> DeleteDocument(Guid? DocumentId);
        Task<IResponseVm<string>> DownloadDocument(Guid? DocumentId);
        Task<IResponseVm<List<GetAllBlobDocument>>> GetAllDocument(Pagination filter);

    }
}

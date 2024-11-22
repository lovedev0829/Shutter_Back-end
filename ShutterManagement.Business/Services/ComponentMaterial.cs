using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ComponentMaterial : BaseRepository<ShutterDbContext, TblComponentMaterial>, IComponentMaterial
    {
        private readonly ShutterDbContext dbContext;

        public ComponentMaterial(ShutterDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        //public Task<IResponseVm<string>> AddComponentMaterial(ComponentMaterialModel model)
        
        //            {
        //                TblComponentMaterial newComponentMaterial = new TblComponentMaterial
        //                {
        //                    ComponentId = IsComponentExists.ComponentId,
        //                    RawMaterialId = material.MaterialId,
        //                    Quantity = material.Qty,
        //                    WasteFactor = material.WasteFactor,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = AppConstant.LoggedInUserID,
        //                    IsActive = true,
        //                    IsDelete = false,
        //                    TenantId = AppConstant.LoggedInTenantId
        //                };

        //                _dbContext.TblComponentMaterials.Add(newComponentMaterial);
        //                await _dbContext.SaveChangesAsync();
                 
        //}

        //public Task<IResponseVm<string>> Delete(Guid Id)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<IResponseVm<dynamic>> UpdateComponentMaterial(UpdateComponent model)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

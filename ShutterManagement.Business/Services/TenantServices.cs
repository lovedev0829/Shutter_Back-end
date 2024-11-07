using Newtonsoft.Json;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Business.Services
{
    public class TenantServices : ITenantService
    {
        private readonly DataServices dataServices;

        public TenantServices(DataServices dataServices)
        {
            this.dataServices = dataServices;
        }

        public async Task TenantCreate(CreateTenantRequest model)
        {
          

            SqlParameter[] sqlParameters = new SqlParameter[]
            {
            new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
            new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
            new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
            new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code },
            new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
            new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email },
            new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
            new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
            new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
            new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
            new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = model.Password },
            new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Create" },
            };

            await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);
        }

        public async Task TenantUpdate(UpdateTenant model)
        {
            var tenantid = new Guid(model.TenantId);
            var Parenttenantid = new Guid(model.ParentTenantId);

            SqlParameter[] sqlParameters = new SqlParameter[]
            {
            new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = tenantid},
            new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = Parenttenantid },
            new SqlParameter("@FirstName", SqlDbType.VarChar) { SqlValue = model.FirstName },
            new SqlParameter("@MiddleName", SqlDbType.VarChar) { SqlValue = model.MiddleName },
            new SqlParameter("@LastName", SqlDbType.VarChar) { SqlValue = model.LastName },
            new SqlParameter("@Code", SqlDbType.VarChar) { SqlValue = model.Code },
            new SqlParameter("@URL", SqlDbType.VarChar) { SqlValue = model.URL },
            new SqlParameter("@Email", SqlDbType.VarChar) { SqlValue = model.Email },
            new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { SqlValue = model.PhoneNumber },
            new SqlParameter("@Logo", SqlDbType.VarChar) { SqlValue = model.Logo },
            new SqlParameter("@PrimaryColor", SqlDbType.VarChar) { SqlValue = model.PrimaryColor },
            new SqlParameter("@SecondaryColor", SqlDbType.VarChar) { SqlValue = model.SecondaryColor },
            new SqlParameter("@Password", SqlDbType.VarChar) { SqlValue = model.Password },           
            new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Update" },

            };

            await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);
        }

        //public async Task<List<Sp_Tenant>> GetAll()
        //{
        //    var tenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_GetAllTenant", reader =>
        //    {
        //        return new Sp_Tenant
        //        {
        //            TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
        //            ParentTenantId = reader.IsDBNull(reader.GetOrdinal("ParentTenantId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
        //            TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
        //            Brandinginfo = reader.IsDBNull(reader.GetOrdinal("Brandinginfo")) ? null : reader.GetString(reader.GetOrdinal("Brandinginfo"))
        //        };
        //    });
        //    return  tenants ;
        //}

        //public async Task<Sp_Tenant> GetById(Guid id)
        //{
        //    var tenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_GetAllTenantById", reader =>
        //    {
        //        return new Sp_Tenant
        //        {

        //            TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
        //            ParentTenantId = reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
        //            TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
        //            Brandinginfo = reader.GetString(reader.GetOrdinal("Brandinginfo"))
        //        };
        //    }, new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = id });

        //    return tenants.FirstOrDefault();
        //}

        //public async Task DeleteTenant(Guid id)
        //{
           
        //        await dataServices.CallSP("sp_TenantDelete",null, new SqlParameter("@TenantId", id));
            
            
        //}
    }
}

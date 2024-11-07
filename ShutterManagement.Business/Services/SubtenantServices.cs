//using Newtonsoft.Json;
//using ShutterManagement.Core.Common;
//using ShutterManagement.Core.SPModel;
//using ShutterManagement.Core.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Data.SqlClient;
//using ShutterManagement.Business.Interfaces;

//namespace ShutterManagement.Business.Services
//{
//    public class SubtenantServices : ISubtenantService
//    {
//        private readonly DataServices dataServices;

//        public SubtenantServices(DataServices dataServices)
//        {
//            this.dataServices = dataServices;
//        }

//        public async Task CreateSubtenant(Sp_Tenant model)
//        {
//            SqlParameter[] sqlParameters = new SqlParameter[]
//            {
//                new SqlParameter("@TenantName", SqlDbType.NVarChar) { SqlValue = model.TenantName },
//                new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = model.ParentTenantId },
//                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "INSERT" }
//            };

//            await dataServices.CallSP("sp_SubTenantCRUD", null, sqlParameters);
//        }

//        public async Task UpdateSubtenant(Sp_Tenant model)
//        {
//            SqlParameter[] sqlParameters = new SqlParameter[]
//            {
//                new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = model.TenantId },
//                new SqlParameter("@TenantName", SqlDbType.NVarChar) { SqlValue = model.TenantName },
//                new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = model.ParentTenantId },
//                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "UPDATE" }
//            };

//            await dataServices.CallSP("sp_SubTenantCRUD", null, sqlParameters);
//        }

//        public async Task<List<Sp_Tenant>> GetAllSubtenants()
//        {
//            var subtenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_SubTenantCRUD", reader =>
//            {
//                return new Sp_Tenant
//                {
//                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
//                    TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
//                    ParentTenantId = reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
//                };
//            }, new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SELECT" });

//            return subtenants;
//        }

//        public async Task<Sp_Tenant> GetSubtenantById(Guid id)
//        {
//            var subtenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_SubTenantCRUD", reader =>
//            {
//                return new Sp_Tenant
//                {
//                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
//                    TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
//                    ParentTenantId = reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
//                };
//            },
//            new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = id },
//            new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SELECT" });

//            return subtenants.FirstOrDefault();
//        }

//        public async Task DeleteSubtenant(Guid id)
//        {
//            await dataServices.CallSP("sp_SubTenantCRUD",
//                new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = id },
//                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "DELETE" });
//        }
//    }
//}

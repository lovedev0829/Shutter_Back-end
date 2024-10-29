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

namespace ShutterManagement.Business.Services
{
    public class TenantServices : ITenantService
    {
        private readonly DataServices dataServices;

        public TenantServices(DataServices dataServices)
        {
            this.dataServices = dataServices;
        }

        public async Task TenantCreate(Sp_Tenant model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Tenant model cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(model.TenantName))
            {
                throw new ArgumentException("TenantName is required.", nameof(model.TenantName));
            }

            SqlParameter[] sqlParameters = new SqlParameter[]
            {
        new SqlParameter("@TenantName", SqlDbType.VarChar) { SqlValue = model.TenantName },
        new SqlParameter("@Brandinginfo", SqlDbType.VarChar) { SqlValue = model.Brandinginfo ?? (object)DBNull.Value },
            };

            await dataServices.CallSP("sp_CreateTenant", null, sqlParameters);
        }

        public async Task TenantUpdate(Sp_Tenant model)
        {
            if (model == null || model.TenantId == Guid.Empty)
            {
                throw new ArgumentException("Valid Tenant model with TenantId is required.");
            }

            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = model.TenantId },
                new SqlParameter("@TenantName", SqlDbType.VarChar) { SqlValue = model.TenantName },
                new SqlParameter("@Brandinginfo", SqlDbType.VarChar) { SqlValue = model.Brandinginfo }
            };

            await dataServices.CallSP("sp_UpdateTenant", null, sqlParameters);
        }

        public async Task<List<Sp_Tenant>> GetAll()
        {
            var tenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_GetAllTenant", reader =>
            {
                return new Sp_Tenant
                {
                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
                    ParentTenantId = reader.IsDBNull(reader.GetOrdinal("ParentTenantId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
                    TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
                    Brandinginfo = reader.IsDBNull(reader.GetOrdinal("Brandinginfo")) ? null : reader.GetString(reader.GetOrdinal("Brandinginfo"))
                };
            });

            return  tenants ;
        }

        public async Task<Sp_Tenant> GetById(Guid id)
        {
            var tenants = await dataServices.CallSPtoRead<Sp_Tenant>("sp_GetAllTenantById", reader =>
            {
                return new Sp_Tenant
                {

                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
                    ParentTenantId = reader.GetGuid(reader.GetOrdinal("ParentTenantId")),
                    TenantName = reader.GetString(reader.GetOrdinal("TenantName")),
                    Brandinginfo = reader.GetString(reader.GetOrdinal("Brandinginfo"))
                };
            }, new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = id });

            return tenants.FirstOrDefault();
        }

        public async Task DeleteTenant(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID specified.");
            }

            try
            {
                await dataServices.CallSP("sp_TenantDelete", new SqlParameter("@TenantId", id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tenant: {ex.Message}");
            }
        }
    }
}

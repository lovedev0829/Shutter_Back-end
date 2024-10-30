using Microsoft.Data.SqlClient;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ContactServices : IContactService
    {
        private readonly DataServices dataServices;

        public ContactServices(DataServices dataServices)
        {
            this.dataServices = dataServices;
        }

        public async Task Create_Contact(Sp_Contact model)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@ContactType", SqlDbType.VarChar) { SqlValue = model.ContactType ?? (object)DBNull.Value },
                new SqlParameter("@Name", SqlDbType.VarChar) { SqlValue = model.Name ?? (object)DBNull.Value },
                new SqlParameter("@Phone", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber ?? (object)DBNull.Value },
                new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email ?? (object)DBNull.Value },
                new SqlParameter("@Website", SqlDbType.NVarChar) { SqlValue = model.Website ?? (object)DBNull.Value },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { SqlValue = model.Notes ?? (object)DBNull.Value },
                new SqlParameter("@IsBillingAddressSameAsAddress", SqlDbType.Bit) { SqlValue = model.IsBillingAddressSameAsAddress },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Insert" }
            };

            await dataServices.CallSP("sp_ContactCRUD", null, sqlParameters);
        }

        public async Task Delete_Contact(Guid id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@ContactId", SqlDbType.UniqueIdentifier) { SqlValue = id },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Delete" }
            };

            await dataServices.CallSP("sp_ContactCRUD", null, sqlParameters);
        }

        public async Task<List<Sp_Contact>> GetAll_Contact()
        {
            var contacts = await dataServices.CallSPtoRead<Sp_Contact>("Sp_ContactCRUD", reader =>
            {
                return new Sp_Contact
                {
                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Website = reader.GetString(reader.GetOrdinal("Website")),
                    Notes = reader.GetString(reader.GetOrdinal("Notes")),
                    IsBillingAddressSameAsAddress = reader.GetBoolean(reader.GetOrdinal("IsBillingAddressSameAsAddress"))
                };
            }, new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SELECTAll" });

            return contacts;
        }

        public async Task<Sp_Contact> GetById_Contact(Guid id)
        {

            var contacts = await dataServices.CallSPtoRead<Sp_Contact>("Sp_ContactCRUD", reader =>
            {
                return new Sp_Contact
                {
                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Website = reader.GetString(reader.GetOrdinal("Website")),
                    Notes = reader.GetString(reader.GetOrdinal("Notes")),
                    IsBillingAddressSameAsAddress = reader.GetBoolean(reader.GetOrdinal("IsBillingAddressSameAsAddress"))
                };
            },
             new SqlParameter("@ContactId", SqlDbType.UniqueIdentifier) { SqlValue = id },
             new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SelectById" });

            return contacts.FirstOrDefault();
        }


        public async Task Update_Contact(Sp_Contact model)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@ContactId", SqlDbType.UniqueIdentifier) { SqlValue = model.ContactId },
                new SqlParameter("@ContactType", SqlDbType.VarChar) { SqlValue = model.ContactType ?? (object)DBNull.Value },
                new SqlParameter("@Name", SqlDbType.VarChar) { SqlValue = model.Name ?? (object)DBNull.Value },
                new SqlParameter("@Phone", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber ?? (object)DBNull.Value },
                new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email ?? (object)DBNull.Value },
                new SqlParameter("@Website", SqlDbType.NVarChar) { SqlValue = model.Website ?? (object)DBNull.Value },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { SqlValue = model.Notes ?? (object)DBNull.Value },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Update" }
            };

            await dataServices.CallSP("sp_ContactCRUD", null, sqlParameters);
        }
    }
}

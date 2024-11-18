using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Business.Services
{
    public class MasterCreate : IMasterCreate
    {
        private readonly DataServices dataServices;

        public MasterCreate(DataServices dataServices)
        {
            this.dataServices = dataServices;
        }

        public async Task Create(CreateServiceVm model)
        {
            switch (model.Type)
            {
                case MasterItemEnum.Contact:
                    var contactData = JsonConvert.DeserializeObject<Sp_Contact>(model.Data.ToString());

                    if (contactData != null)
                    {
                        SqlParameter[] sqlParameters = new SqlParameter[]
                        {
                             new SqlParameter("@ContactType", SqlDbType.VarChar) { SqlValue = contactData.ContactType },
                             new SqlParameter("@Name", SqlDbType.VarChar) { SqlValue = contactData.Name },
                             new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { SqlValue = contactData.PhoneNumber },
                             new SqlParameter("@Email", SqlDbType.VarChar) { SqlValue = contactData.Email },
                             new SqlParameter("@Website", SqlDbType.VarChar) { SqlValue = contactData.Website },
                             new SqlParameter("@Notes", SqlDbType.VarChar) { SqlValue = contactData.Notes }
                         };

                        await dataServices.CallSP("sp_CreateContact", dataServices.MapClass<Sp_Contact>, sqlParameters);
                    }
                    else
                    {
                        throw new ArgumentException("Failed to deserialize model.Data to ContactData.");
                    }
                    break;

                case MasterItemEnum.Phone:
                    break;
                case MasterItemEnum.Assembly:
                    break;
                case MasterItemEnum.Address:
                    break;
                default:
                    break;
            }
        }

        public async Task Update(CreateServiceVm model)
        {
            switch (model.Type)
            {
                case MasterItemEnum.Contact:
                    var contactData = JsonConvert.DeserializeObject<UpdateContact>(model.Data.ToString());

                    if (contactData != null)
                    {
                        if (contactData.ContactId == Guid.Empty)
                        {
                            throw new ArgumentException("ContactId must be provided.");
                        }

                        SqlParameter[] sqlParameters = new SqlParameter[]
                        {
                    new SqlParameter("@ContactType", SqlDbType.VarChar) { SqlValue = contactData.ContactType },
                    new SqlParameter("@Name", SqlDbType.VarChar) { SqlValue = contactData.Name },
                    new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { SqlValue = contactData.PhoneNumber },
                    new SqlParameter("@Email", SqlDbType.VarChar) { SqlValue = contactData.Email },
                    new SqlParameter("@Website", SqlDbType.VarChar) { SqlValue = contactData.Website },
                    new SqlParameter("@Notes", SqlDbType.VarChar) { SqlValue = contactData.Notes },
                    new SqlParameter("@ContactId", SqlDbType.UniqueIdentifier) { SqlValue = contactData.ContactId } // Add ContactId parameter
                        };

                        await dataServices.CallSP("sp_UpdateContact", dataServices.MapClass<Sp_Contact>, sqlParameters);
                    }
                    else
                    {
                        throw new ArgumentException("Failed to deserialize model.Data to ContactData.");
                    }
                    break;

                case MasterItemEnum.Phone:
                    break;

                case MasterItemEnum.Assembly:
                    break;

                case MasterItemEnum.Address:
                    break;

                default:
                    break;
            }
        }

        public async Task<GetContactResponse> GetAll()
        {
            
            var contacts = await dataServices.CallSPtoRead<UpdateContact>("sp_GetAll", reader =>
            {
                return new UpdateContact
                {
                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Website = reader.IsDBNull(reader.GetOrdinal("Website")) ? null : reader.GetString(reader.GetOrdinal("Website")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                };
            });

            return new GetContactResponse
            {
                Contacts = contacts, 
            };
        }

        public async Task<Sp_Contact> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID specified.");
            }

            var contacts = await dataServices.CallSPtoRead<UpdateContact>("sp_GetById", reader =>
            {
                return new UpdateContact
                {
                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Website = reader.IsDBNull(reader.GetOrdinal("Website")) ? null : reader.GetString(reader.GetOrdinal("Website")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                };
            }, new SqlParameter("@ContactId", SqlDbType.UniqueIdentifier) { SqlValue = id });

            return contacts.FirstOrDefault(); 
        }

        public async Task DeleteContact(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID specified.");
            }

            try
            {
                await dataServices.CallSP("sp_ContactDelete", null, new SqlParameter("@ContactId", id));
            }
            catch (Exception ex)
            {
                return;
            }
        }


    }
}

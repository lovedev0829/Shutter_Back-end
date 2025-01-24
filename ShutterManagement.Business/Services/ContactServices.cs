using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class ContactServices : BaseRepository<BtprojecShuttermanagementDevContext, TblContact>, IContactService
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly IContactBillingService billingService;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly ClaimsProperty claimsProperty;

        public ContactServices(BtprojecShuttermanagementDevContext context, IContactBillingService billingService,
            ITenantService tenantService, ITaxExemption taxExemptionService, ClaimsProperty claimsProperty)
        {
            _context = context;
            this.billingService = billingService;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> GetContactByEmailOrPhone(string email, string phone)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var IsEmailExists = GetAllQuerable()
                        .Any(c => c.Email == email && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false);

                    var IsPhoneExists = GetAllQuerable()
                        .Any(c => c.PhoneNumber == phone && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false);

                    if (IsEmailExists && IsPhoneExists)
                    {
                        responseVm.Message = $"Email {email} and Phone {phone} {AppConstant.AlreadyExists}";
                    }
                    else if (IsEmailExists)
                    {
                        responseVm.Message = $"Email {email} {AppConstant.AlreadyExists}";
                    }
                    else if (IsPhoneExists)
                    {
                        responseVm.Message = $"Phone {phone} {AppConstant.AlreadyExists}";
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Contact(Sp_Contact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false).ToList();
                    if (IsContactExists.Count > 0)
                    {
                        responseVm.Message = $"Email {model.Email} or Phone {model.PhoneNumber} " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
                    {
                        responseVm.Message = "Billing details required.";
                        return responseVm;
                    }
                    if (model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                    {
                        responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                        return responseVm;
                    }
                    int Version = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(p => p.CreatedAt)
                        .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                    Version += 1;
                    var contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, AppConstant.LoggedInCompanyCode, Version,false);


                    TblContact CreateContact = new TblContact
                    {
                        ContactId = Guid.NewGuid(),
                        ContactType = model.ContactType,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        // Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Website = model.Website,
                        Notes = model.Notes,
                        Referral = model.Referral,
                        ContactUniqueId = contactUniqueId,
                        IsTaxExemption = model.IsTaxExemption,
                        IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsArchive = false,
                        IsDelete = false
                    };
                    var res = await Add(CreateContact);
                    if (res.ContactId != Guid.Empty)
                    {
                        if (!model.IsBillingAddressSameAsAddress)
                        {

                            //TblContactBillingAddress CreateContactBillingAddress = new TblContactBillingAddress
                            //{
                            //    ContactId = res.ContactId,
                            //    FirstName = model.BillingAddressVm?.FirstName,
                            //    LastName = model.BillingAddressVm?.LastName,
                            //    PhoneNumber = model.BillingAddressVm?.PhoneNumber,
                            //    Email = model.BillingAddressVm?.Email,
                            //    City = model.BillingAddressVm?.City,
                            //    State = model.BillingAddressVm?.State,
                            //    ZipCode = model.BillingAddressVm?.ZipCode,
                            //    StreetAddress = model.BillingAddressVm?.StreetAddress,
                            //    CreatedAt = DateTime.Now,
                            //    IsActive = true,
                            //    IsDelete = false
                            //};
                            //await billingService.Add(CreateContactBillingAddress);

                            TblAddress address = new TblAddress
                            {
                                AddressId = Guid.NewGuid(),
                                ContactId = res.ContactId,
                                Label = "Primary Address",
                                FirstName = model.BillingAddressVm?.FirstName,
                                LastName = model.BillingAddressVm?.LastName,
                                Phone = model.BillingAddressVm?.PhoneNumber,
                                Email = model.BillingAddressVm?.Email,
                                City = model.BillingAddressVm?.City,
                                State = model.BillingAddressVm?.State,
                                ZipCode = model.BillingAddressVm?.ZipCode,
                                StreetAddress = model.BillingAddressVm?.StreetAddress,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false,
                                TenantId = AppConstant.LoggedInTenantId,
                                CreatedBy = AppConstant.LoggedInUserID
                            };
                            await dbContext.TblAddresses.AddAsync(address);
                            dbContext.SaveChanges();
                            res.PrimaryAddress = address.AddressId;
                            await Update(res);


                        }
                        if (!model.IsTaxExemption)
                        {
                            string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);
                            TblTaxExemption taxExemption1 = new TblTaxExemption
                            {
                                ContactId = res.ContactId,
                                ExemptionCertificate = arrayString,
                                EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
                                ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false,
                            };
                            await taxExemptionService.Add(taxExemption1);
                        }
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> Update_Contact(UpdateContact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var contact = await GetAllQuerable()
                        .FirstOrDefaultAsync(c => c.ContactId == model.ContactId && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false);
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false && c.ContactId != model.ContactId).ToList();
                    if (IsContactExists.Count > 0)
                    {
                        responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                    if (model.TaxExemptionVm != null)
                    {
                        if (model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                        {
                            responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                            return responseVm;
                        }
                    }
                    if (contact != null)
                    {
                        contact.ContactType = model.ContactType;
                        contact.FirstName = model.FirstName;
                        contact.LastName = model.LastName;
                        contact.PhoneNumber = model.PhoneNumber;
                        contact.Email = model.Email;
                        contact.Website = model.Website;
                        contact.Notes = model.Notes;
                        contact.Referral = model.Referral;
                        contact.IsTaxExemption = model.IsTaxExemption;
                        contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                        contact.ModifiedBy = AppConstant.LoggedInUserID;
                        contact.UpdatedAt = DateTime.Now;

                        await Update(contact);
                        if (!model.IsBillingAddressSameAsAddress)
                        {
                            var contactBillingAddress = dbContext.TblAddresses
                                .Where(x => x.ContactId == contact.ContactId && x.AddressId == contact.PrimaryAddress && x.IsDelete == false).FirstOrDefault();
                            if (contactBillingAddress != null)
                            {
                                contactBillingAddress.FirstName = model.BillingAddressVm?.FirstName;
                                contactBillingAddress.LastName = model.BillingAddressVm?.LastName;
                                contactBillingAddress.Phone = model.BillingAddressVm?.PhoneNumber;
                                contactBillingAddress.Email = model.BillingAddressVm?.Email;
                                contactBillingAddress.City = model.BillingAddressVm?.City;
                                contactBillingAddress.State = model.BillingAddressVm?.State;
                                contactBillingAddress.ZipCode = model.BillingAddressVm?.ZipCode;
                                contactBillingAddress.StreetAddress = model.BillingAddressVm?.StreetAddress;
                                contactBillingAddress.UpdatedAt = DateTime.Now;

                                dbContext.TblAddresses.Update(contactBillingAddress);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                TblAddress CreateContactBillingAddress = new TblAddress
                                {
                                    AddressId = Guid.NewGuid(),
                                    ContactId = contact.ContactId,
                                    FirstName = model.BillingAddressVm?.FirstName,
                                    LastName = model.BillingAddressVm?.LastName,
                                    Phone = model.BillingAddressVm?.PhoneNumber,
                                    Email = model.BillingAddressVm?.Email,
                                    City = model.BillingAddressVm?.City,
                                    State = model.BillingAddressVm?.State,
                                    ZipCode = model.BillingAddressVm?.ZipCode,
                                    StreetAddress = model.BillingAddressVm?.StreetAddress,
                                    CreatedAt = DateTime.Now,
                                    IsActive = true,
                                    IsDelete = false
                                };
                                dbContext.TblAddresses.Add(CreateContactBillingAddress);
                                dbContext.SaveChanges();    
                                contact.PrimaryAddress = CreateContactBillingAddress.AddressId;
                                await Update(contact);
                            }
                        }

                        if (!model.IsTaxExemption)
                        {
                            var contactTaxExemption = taxExemptionService.GetAllQuerable()
                                .Where(x => x.ContactId == contact.ContactId && x.IsDelete == false).FirstOrDefault();
                            string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);

                            if (contactTaxExemption != null)
                            {
                                contactTaxExemption.ExemptionCertificate = arrayString;
                                contactTaxExemption.EffectiveDate = model.TaxExemptionVm?.EffectiveDate;
                                contactTaxExemption.ExpiryDate = model.TaxExemptionVm?.ExpiryDate;
                                contactTaxExemption.UpdatedAt = DateTime.Now;
                                contactTaxExemption.IsActive = true;
                                contactTaxExemption.IsDelete = false;
                                await taxExemptionService.Update(contactTaxExemption);
                            }
                            else
                            {
                                TblTaxExemption taxExemption1 = new TblTaxExemption
                                {
                                    ContactId = contact.ContactId,
                                    ExemptionCertificate = arrayString,
                                    EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
                                    ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
                                    CreatedAt = DateTime.Now,
                                    IsActive = true,
                                    IsDelete = false

                                };
                                await taxExemptionService.Add(taxExemption1);
                            }
                        }
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> ActiveInActiveContact(Guid ContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == ContactId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                contact.IsActive = !contact.IsActive;
                contact.UpdatedAt = DateTime.Now;
                contact.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(contact);

                var billingaddress =  dbContext.TblAddresses
                   .Where(c => c.ContactId == ContactId && contact.PrimaryAddress == c.AddressId && c.IsDelete == false).FirstOrDefault();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = !billingaddress.IsActive;
                    billingaddress.UpdatedAt = DateTime.Now;
                    dbContext.TblAddresses.Update(billingaddress);
                    dbContext.SaveChanges();
                }
                var taxExemption = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == ContactId && c.IsDelete == false)
                    .FirstOrDefault();

                if (taxExemption != null)
                {
                    taxExemption.IsActive = !taxExemption.IsActive;
                    taxExemption.UpdatedAt = DateTime.Now;
                    await taxExemptionService.Update(taxExemption);
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> DeleteContact(Guid ContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == ContactId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                contact.IsActive = false;
                contact.IsDelete = true;
                contact.UpdatedAt = DateTime.Now;
                contact.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(contact);

                var billingaddress = dbContext.TblAddresses
                   .Where(c => c.ContactId == ContactId && c.AddressId == contact.PrimaryAddress && c.IsDelete == false).FirstOrDefault();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = false;
                    billingaddress.IsDelete = true;
                    billingaddress.UpdatedAt = DateTime.Now;
                    dbContext.TblAddresses.Update(billingaddress);
                    dbContext.SaveChanges();
                }
                var taxExemption = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == ContactId && c.IsDelete == false)
                    .FirstOrDefault();

                if (taxExemption != null)
                {
                    taxExemption.IsActive = false;
                    taxExemption.IsDelete = true;
                    taxExemption.UpdatedAt = DateTime.Now;
                    await taxExemptionService.Update(taxExemption);
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.DeleteSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        //public async Task<IResponseVm<string>> GetAllArchiveContacts()
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        ////var contact = GetAllQuerable()
        //        ////    .Where(c =>
        //        ////    (archiveModel.Status  ? c.IsActive == true : c.IsActive == false) &&
        //        ////    //(archiveModel.LastOrderDate != null ? c)
        //        ////     c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false ).ToList();
        //        //var contact = GetAllQuerable()
        //        //    .Where(c =>c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).ToList();


        //        //if (contact == null)
        //        //{
        //        //    responseVm.Message = AppConstant.NoDataFound;
        //        //    return responseVm;
        //        //}

        //        ////contact.IsArchive = true;
        //        ////contact.UpdatedAt = DateTime.Now;
        //        ////contact.ModifiedBy = AppConstant.LoggedInUserID;
        //        ////await Update(contact);
        //        //responseVm.IsSuccess = true;
        //        //responseVm.Message = AppConstant.FetchSuccessMsg;
        //        //responseVm.Data = contact;

        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}

        public async Task<IResponseVm<string>> ArchiveContacts(ArchiveUser archiveModel)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var query = GetAllQuerable().Where(c => c.IsActive == false && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false);
                if (query == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Contact " + AppConstant.NoDataFound;
                    return responseVm;
                }

                if (archiveModel.ArchiveType == 0)
                {
                    query = query.Where(c => c.ContactId == archiveModel.ContactId);
                }

                var contactList = query.ToList();

                if (!contactList.Any())
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                foreach (var contact in contactList)
                {
                    contact.IsArchive = true;
                    await Update(contact);
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError}: {ex.Message}";
            }

            return responseVm;
        }


        public async Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var Contacts = new List<TblContact>();
                List<ContactsList> ContactList = new List<ContactsList>();
                if (filter.TenantId != "")
                {
                    var IsRealtedTenant = tenantService.GetAllQuerable().Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                    if (IsRealtedTenant == null)
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        return responseVm;
                    }
                    else
                    {
                        Contacts = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x => x.CreatedAt).ToList();
                    }
                }
                else
                {
                    Contacts = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                }


                foreach (var contact in Contacts)
                {
                    ContactsList contacts = new ContactsList
                    {
                        ContactId = contact.ContactId,
                        ContactType = contact.ContactType ?? "",
                        Email = contact.Email,
                        PhoneNumber = contact.PhoneNumber,
                        FirstName = contact.FirstName,
                        LastName = contact.LastName,
                        Website = contact.Website,
                        Notes = contact.Notes,
                        Referral = contact.Referral,
                        ContactUniqueId = contact.ContactUniqueId,
                        PrimaryAddress = contact.PrimaryAddress,
                        IsTaxExemption = contact.IsTaxExemption ?? false,
                        IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                        IsActive = contact.IsActive,
                        IsArchive = contact.IsArchive,
                        CreatedAt = contact.CreatedAt
                    };
                    contacts.billingAddressVm = dbContext.TblAddresses
                        .Where(x => x.AddressId == contact.PrimaryAddress
                        && x.IsDelete == false).Select(x => new BillingAddressVm
                        {
                            City = x.City,
                            Email = x.Email,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            PhoneNumber = x.Phone,
                            State = x.State,
                            StreetAddress = x.StreetAddress,
                            ZipCode = x.ZipCode,
                        }).FirstOrDefault();
                    contacts.taxExemptionVm = taxExemptionService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId
                        && x.IsDelete == false).Select(x => new TaxExemption
                        {
                            EffectiveDate = x.EffectiveDate,
                            ExemptionCertificate = x.ExemptionCertificate.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                            ExpiryDate = x.ExpiryDate
                        }).FirstOrDefault();
                    ContactList.Add(contacts);
                }
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = ContactList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter.SearchValue)
                                    || (x.ContactId.ToString().ToLower()).Contains(filter.SearchValue)
                                    || (x.Email.ToLower() ?? "").Contains(filter.SearchValue)
                                    || (x.Website.ToLower() ?? "").Contains(filter.SearchValue)
                                    || (x.PhoneNumber ?? "").Contains(filter.SearchValue)
                                    || ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter.SearchValue)
                                    || (x.FirstName.ToLower() ?? "").Contains(filter.SearchValue)
                                    || (x.LastName.ToLower() ?? "").Contains(filter.SearchValue)
                                    ) : true)).ToList();

                var TakenRecords = Paginate.Pagination(FilterRecords, filter);
                responseVm.IsSuccess = true;

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.ToList();
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

    }
}

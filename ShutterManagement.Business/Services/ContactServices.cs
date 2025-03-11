//using System.Data.Entity;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL;
using ShutterManagement.GHL.Services;
using ShutterManagement.Services;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;

namespace ShutterManagement.Business.Services
{
    public class ContactServices : BaseRepository<ShutterDBContext, TblContact>, IContactService
    {
        private readonly ShutterDBContext _context;
        private readonly IContactBillingService billingService;
        private readonly IAuthService authService;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly ClaimsProperty claimsProperty;
        private readonly GhlService ghlService;

        public ContactServices(ShutterDBContext context, IContactBillingService billingService, IAuthService authService,
            ITenantService tenantService, ITaxExemption taxExemptionService,
            ClaimsProperty claimsProperty, GhlService ghlService)
        {
            _context = context;
            this.billingService = billingService;
            this.authService = authService;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.claimsProperty = claimsProperty;
            this.ghlService = ghlService;
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
                var TenantId = Guid.Empty;
                var IsTenantExists = false;
                if (model.PhoneNumber != null)
                {
                    var match = Regex.Match(model.PhoneNumber, @"^\+?\d{1}");
                    model.PhoneNumber = match.Value == "+1"
                                        ? model.PhoneNumber
                                        : "+1" + model.PhoneNumber.TrimStart('+');
                }
                if (model.locationId != null)
                {
                    authService.GetGHLConfiguration(model.locationId);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;

                    if (TenantId != null)
                    {
                        IsTenantExists = true;
                        model.IsTaxExemption = true;
                    }
                }
                else
                {
                    IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;
                }

                if (IsTenantExists)
                {
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == TenantId
                        && c.IsDelete == false).ToList();

                    if (IsContactExists.Count > 0)
                    {
                        if (model.GHLContactId != null)
                        {
                            var ghlContactAdd = new UpdateContact
                            {
                                //Id = contact1.GhlcontactId.ToString() ?? string.Empty,
                                FirstName = model.FirstName ?? string.Empty,
                                LastName = model.LastName ?? string.Empty,
                                Email = model.Email ?? string.Empty,
                                PhoneNumber = model.PhoneNumber ?? string.Empty,
                                Notes = model.Notes ?? string.Empty,
                                ContactType = model.ContactType ?? string.Empty,
                                Website = model.Website ?? string.Empty,
                                Referral = model.Referral ?? string.Empty,
                                GHLContactId = model.GHLContactId ?? string.Empty,
                                locationId = model.locationId ?? string.Empty,
                            };
                            var result = Update_Contact(ghlContactAdd);
                        }
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
                    int Version = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                        .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                    Version += 1;
                    var contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, AppConstant.LoggedInCompanyCode, Version);


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
                        TenantId = TenantId,
                        IsActive = true,
                        IsArchive = false,
                        IsDelete = false
                    };
                    var res = await Add(CreateContact);
                    if (model.lastAsync != null && model.GHLContactId != null)
                    {
                        res.LastSync = model.lastAsync;
                        res.GhlcontactId = model.GHLContactId;
                        await Update(res);
                    }

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
                                TenantId = TenantId,
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
                        if (model.GHLContactId == null)
                        {
                            authService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                            var ghlResponse = await SyncWithGHL(res, model);
                            if (!ghlResponse.IsSuccess)
                            {
                                responseVm.Message = "Contact sync with GHL failed: " + ghlResponse.Message;
                            }
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
                var TenantId = Guid.Empty;
                var IsTenantExists = false;
                if (model.PhoneNumber != null)
                {
                    var match = Regex.Match(model.PhoneNumber, @"^\+?\d{1}");
                    model.PhoneNumber = match.Value == "+1"
                                        ? model.PhoneNumber
                                        : "+1" + model.PhoneNumber.TrimStart('+');
                }
                if (model.locationId != null)
                {
                    authService.GetGHLConfiguration(model.locationId);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;

                    if (TenantId != Guid.Empty)
                    {
                        IsTenantExists = true;
                        model.IsTaxExemption = true;
                    }
                }
                else
                {
                    IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;
                }

                if (IsTenantExists)
                {
                    if (model.GHLContactId != null)
                    {
                        var contact = (await GetAll()).Where
                            (c => c.GhlcontactId == model.GHLContactId && c.TenantId == TenantId && c.IsDelete == false).FirstOrDefault();

                        var IsContactExists = GetAllQuerable()
                            .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                                        && c.TenantId == TenantId && c.IsDelete == false)
                            .ToList();

                        //if (IsContactExists.Count > 0)
                        //{
                        //    responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
                        //    return responseVm;
                        //}

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
                            contact.ModifiedBy = TenantId;
                            contact.UpdatedAt = DateTime.Now;

                            await Update(contact); responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UpdateSuccessMsg;
                        }
                        else
                        {
                            var contactCreate = new Sp_Contact
                            {
                                ContactType = model.ContactType,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                Email = model.Email,
                                PhoneNumber = model.PhoneNumber,
                                lastAsync = model.lastAsync,
                                GHLContactId = model.GHLContactId,
                                locationId = model.locationId
                            };

                            var res = await Create_Contact(contactCreate);
                        }
                    }
                    else
                    {
                        var contact1 = (await GetAll()).Where
                            (c => c.ContactId == model.ContactId && c.TenantId == TenantId && c.IsDelete == false).FirstOrDefault();

                        var IsContactExists1 = GetAllQuerable()
                            .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                                        && c.TenantId == TenantId && c.IsDelete == false && c.ContactId != model.ContactId)
                            .ToList();

                        if (IsContactExists1.Count > 0)
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
                        if (contact1 != null)
                        {
                            contact1.ContactType = model.ContactType;
                            contact1.FirstName = model.FirstName;
                            contact1.LastName = model.LastName;
                            contact1.PhoneNumber = model.PhoneNumber;
                            contact1.Email = model.Email;
                            contact1.Website = model.Website;
                            contact1.Notes = model.Notes;
                            contact1.Referral = model.Referral;
                            contact1.IsTaxExemption = model.IsTaxExemption;
                            contact1.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                            contact1.ModifiedBy = TenantId;
                            contact1.UpdatedAt = DateTime.Now;

                            await Update(contact1);

                            if (!model.IsBillingAddressSameAsAddress)
                            {
                                var contactBillingAddress = dbContext.TblAddresses
                                    .Where(x => x.ContactId == contact1.ContactId && x.AddressId == contact1.PrimaryAddress && x.IsDelete == false)
                                    .FirstOrDefault();

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
                                        ContactId = contact1.ContactId,
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
                                        TenantId = TenantId,
                                    };

                                    dbContext.TblAddresses.Add(CreateContactBillingAddress);
                                    dbContext.SaveChanges();

                                    contact1.PrimaryAddress = CreateContactBillingAddress.AddressId;
                                    await Update(contact1);
                                }
                            }

                            if (!model.IsTaxExemption)
                            {
                                var contactTaxExemption = taxExemptionService.GetAllQuerable()
                                    .Where(x => x.ContactId == contact1.ContactId && x.IsDelete == false).FirstOrDefault();

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
                                        ContactId = contact1.ContactId,
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
                        }
                        var ghlContactAdd = new Sp_Contact
                        {
                            //Id = contact1.GhlcontactId.ToString() ?? string.Empty,
                            FirstName = model.FirstName ?? string.Empty,
                            LastName = model.LastName ?? string.Empty,
                            Email = model.Email ?? string.Empty,
                            PhoneNumber = model.PhoneNumber ?? string.Empty,
                            Notes = model.Notes ?? string.Empty,
                            ContactType = model.ContactType ?? string.Empty,
                            Website = model.Website ?? string.Empty,
                            Referral = model.Referral ?? string.Empty,
                            GHLContactId = model.GHLContactId ?? string.Empty,
                        };
                        authService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                        var ghlResponse = await SyncWithGHL(contact1, ghlContactAdd);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
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

                var billingaddress = dbContext.TblAddresses
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

                var jobExist = dbContext.TblJobs.Where(x => x.ContactId == ContactId && x.IsDelete == false).ToList();
                if (jobExist != null)
                {
                    foreach (var item in jobExist)
                    {
                        item.IsActive = false;
                        item.IsDelete = true;
                        item.UpdatedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;
                        dbContext.TblJobs.Update(item);
                        dbContext.SaveChanges();
                    }
                }
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


        public async Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter, bool Role)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var IsSubtenants = false;
                var Contacts = new List<TblContact>();
                List<ContactsList> ContactList = new List<ContactsList>();
                if (filter.TenantId != "" && Role)
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
                        IsSubtenants = true;
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
                        Email = contact.Email ?? "",
                        PhoneNumber = contact.PhoneNumber ?? "",
                        FirstName = contact.FirstName ?? "",
                        LastName = contact.LastName ?? "",
                        Website = contact.Website ?? "",
                        Notes = contact.Notes ?? "",
                        Referral = contact.Referral ?? "",
                        ContactUniqueId = contact.ContactUniqueId ?? "",
                        PrimaryAddress = contact.PrimaryAddress,
                        IsTaxExemption = contact.IsTaxExemption ?? false,
                        IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                        IsActive = contact.IsActive,
                        IsArchive = contact.IsArchive,
                        CreatedAt = contact.CreatedAt,
                        IsSubtenant = IsSubtenants,
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

        public async Task<IResponseVm<string>> SyncWithGHL(TblContact contact, Sp_Contact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var ghlResponse = new GhlContactSingleResponse();
                var ghlContactAdd = new GhlContactAdd
                {
                    Id = contact.ContactId.ToString() ?? string.Empty,
                    FirstName = model.FirstName ?? string.Empty,
                    LastName = model.LastName ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    Phone = model.PhoneNumber ?? string.Empty,
                    Notes = model.Notes ?? string.Empty,
                    ContactType = model.ContactType ?? string.Empty,
                    Website = model.Website ?? string.Empty,
                    Referral = model.Referral ?? string.Empty
                };

                if (contact.GhlcontactId == null)
                {
                    authService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    ghlResponse = await ghlService.CreateContactAsync(ghlContactAdd);
                    if (ghlResponse != null)
                    {
                        var contactExist = (await GetById(contact.ContactId));

                        if (contactExist != null && contactExist.IsDelete == false)
                        {
                            contactExist.GhlcontactId = ghlResponse.Contact.id;
                            contactExist.LastSync = DateTime.Now;
                        }
                        var res = await Update(contactExist);

                    }

                }
                else
                {
                    authService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    ghlResponse = await ghlService.UpdateContactAsync(contact.GhlcontactId, ghlContactAdd);
                }
                if (ghlResponse != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Contact synced successfully with GHL.";
                }
                else
                {
                    responseVm.Message = "Contact sync with GHL failed.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = "Error syncing with GHL: " + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> ContactFromGHL(GHLContactTrigger contactData)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try
                {
                    var contact = new Sp_Contact
                    {
                        ContactType = contactData.contact_type,
                        FirstName = contactData.first_name,
                        LastName = contactData.last_name,
                        Email = contactData.email,
                        PhoneNumber = contactData.phone,
                        locationId = contactData.location.id,
                        GHLContactId = contactData.contact_id,
                        lastAsync = DateTime.Now,
                    };
                    if (contactData.location != null)
                    {
                        contact.BillingAddressVm = new BillingAddressVm
                        {
                            BillingFirstName = contactData.first_name,
                            BillingLastName = contactData.last_name,
                            FirstName = contactData.first_name,
                            LastName = contactData.last_name,
                            Email = contactData.email,
                            PhoneNumber = contactData.phone,
                            City = contactData.location.city,
                            State = contactData.location.state,
                            ZipCode = contactData.location.postalCode,
                            StreetAddress = contactData.location.address
                        };
                    }
                    authService.GetGHLConfiguration(contact.locationId);
                    var res = await Create_Contact(contact);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }
        public async Task<IResponseVm<string>> UpdateContactFromGHL(GHLContactTrigger contactData)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try
                {
                    var contact = new UpdateContact
                    {
                        ContactType = contactData.contact_type,
                        FirstName = contactData.first_name,
                        LastName = contactData.last_name,
                        Email = contactData.email,
                        PhoneNumber = contactData.phone,
                        locationId = contactData.location.id,
                        GHLContactId = contactData.contact_id,
                        lastAsync = DateTime.Now,
                    };
                    if (contactData.location != null)
                    {
                        contact.BillingAddressVm = new BillingAddressVm
                        {
                            BillingFirstName = contactData.first_name,
                            BillingLastName = contactData.last_name,
                            FirstName = contactData.first_name,
                            LastName = contactData.last_name,
                            Email = contactData.email,
                            PhoneNumber = contactData.phone,
                            City = contactData.location.city,
                            State = contactData.location.state,
                            ZipCode = contactData.location.postalCode,
                            StreetAddress = contactData.location.address
                        };
                    }
                    authService.GetGHLConfiguration(contact.locationId);
                    var res = await Update_Contact(contact);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }

        }
    }
}

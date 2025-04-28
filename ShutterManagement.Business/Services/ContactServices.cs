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
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IGHLConfiguration gHLConfigurationService;
        private readonly ITenantBrandingService tenantBranding;
        private readonly GHLService_v2 gHLService_V2;

        public ContactServices(ShutterDBContext context, IContactBillingService billingService,
            ITenantService tenantService, ITaxExemption taxExemptionService,
            ClaimsProperty claimsProperty,IGHLConfiguration gHLConfigurationService,
            ITenantBrandingService tenantBranding,GHLService_v2 gHLService_V2)
        {
            _context = context;
            this.billingService = billingService;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.claimsProperty = claimsProperty;
            this.gHLConfigurationService = gHLConfigurationService;
            this.tenantBranding = tenantBranding;
            this.gHLService_V2 = gHLService_V2;
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


        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var match = Regex.Match(phoneNumber, @"^\+?\d{1}");
            return match.Value == "+1" ? phoneNumber : "+1" + phoneNumber.TrimStart('+');
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
                    model.PhoneNumber = FormatPhoneNumber(model.PhoneNumber);
                }
                FileLogger.Log($"Location id fetched {model.locationId}");
                if (model.locationId != null)
                {
                    await gHLConfigurationService.GetGHLConfiguration(model.locationId);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;
                    FileLogger.Log($"Location id fetched {model.locationId}");
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
                    FileLogger.Log($"Tenant Id id fetched {TenantId}");
                    model.IsTaxExemption = true;
                }
                if(!model.IsTaxExemption)
                {
                    if(model.TaxExemptionVm.EffectiveDate == null && model.TaxExemptionVm.ExpiryDate == null)
                    {
                        responseVm.Message = "Please Fill TaxExemption Details.";
                        return responseVm;
                    }
                }

                if (IsTenantExists)
                {
                    //await gHLConfigurationService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == TenantId
                        && c.IsDelete == false).ToList();

                    if (IsContactExists.Count > 0)
                    {
                        //If Contact is already exists in Shutter DB then update the GHL Contact if GHLContactId is not null
                        if (model.GHLContactId != null)
                        {
                            var ghlContactAdd = new UpdateContact
                            {
                                //Id = contact1.GhlcontactId.ToString() ?? string.Empty,
                                FirstName = model.FirstName ?? string.Empty,
                                LastName = model.LastName ?? string.Empty,
                                Suffix = model.Suffix ?? string.Empty,
                                Email = model.Email ?? string.Empty,
                                PhoneNumber = model.PhoneNumber ?? string.Empty,
                                Notes = model.Notes ?? string.Empty,
                                ContactType = model.ContactType ?? string.Empty,
                                Website = model.Website ?? string.Empty,
                                Referral = model.Referral ?? string.Empty,
                                GHLContactId = model.GHLContactId ?? string.Empty,
                                locationId = model.locationId ?? string.Empty,
                            };
                            var result = await Update_Contact(ghlContactAdd);
                        }
                        else
                        {
                            responseVm.Message = $"Email {model.Email} or Phone {model.PhoneNumber} " + AppConstant.AlreadyExists;
                            return responseVm;
                        }
                    }
                    //for Billing address
                    if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
                    {
                        responseVm.Message = "Billing details required.";
                        return responseVm;
                    }
                    //for Tax exemption
                    if (model.TaxExemptionVm != null && model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                    {
                        responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                        return responseVm;
                    }

                    //generate unique version
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
                        Suffix = model.Suffix,
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
                        // Add Billing Address
                        if (!model.IsBillingAddressSameAsAddress)
                        {

                            Guid? stateId = null;

                            // Validate and assign StateId only if it exists in MstState
                            if (model.BillingAddressVm?.StateId != null &&
                                model.BillingAddressVm.StateId != Guid.Empty)
                            {
                                var isValidState = await dbContext.MstStates
                                    .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

                                if (isValidState)
                                {
                                    stateId = model.BillingAddressVm.StateId;
                                }
                            }
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
                                State = stateId,
                                ZipCode = model.BillingAddressVm?.ZipCode,
                                StreetAddress = model.BillingAddressVm?.StreetAddress,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false,
                                TenantId = TenantId,
                                CreatedBy = AppConstant.LoggedInUserID
                            };
                             dbContext.TblAddresses.Add(address);
                            await dbContext.SaveChangesAsync();
                            res.PrimaryAddress = address.AddressId;
                            await Update(res);
                        }

                        //Add Tax Exemption
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
                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.InsertSuccessMsg;
                        }
                        if (model.GHLContactId == null)
                        {
                            var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration();
                            if (Checkghlresponse.IsSuccess)
                            {
                                var ghlResponse = await SyncWithGHL(res, model);
                                if (ghlResponse.IsSuccess)
                                {
                                    responseVm.IsSuccess = true;
                                    responseVm.Message = AppConstant.InsertSuccessMsg + ghlResponse.Message;
                                }
                                else
                                {
                                    responseVm.IsSuccess = true;
                                    responseVm.Message = AppConstant.InsertSuccessMsg + ghlResponse.Message;
                                }
                            }
                            if (!Checkghlresponse.IsSuccess)
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = AppConstant.InsertSuccessMsg;
                                FileLogger.Log(Checkghlresponse.Message);
                                //responseVm.IsSuccess = true;
                                //responseVm.Message = Checkghlresponse.Message;
                            }

                        }
                        
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
                    model.PhoneNumber = FormatPhoneNumber(model.PhoneNumber);
                }
                if (model.locationId != null)
                {
                    await gHLConfigurationService.GetGHLConfiguration(model.locationId);
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
                    await gHLConfigurationService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
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
                            contact.Suffix = model.Suffix;
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
                                Suffix=model.Suffix,
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
                        var contact1 = GetAllQuerable().Where
                            (c => c.ContactId == model.ContactId && c.TenantId == TenantId && c.IsDelete == false).FirstOrDefault();

                        var IsContactExists1 = (await GetAll())
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
                            contact1.Suffix = model.Suffix;
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
                                    .Where(x => x.ContactId == contact1.ContactId && x.AddressId == contact1.PrimaryAddress && x.IsDelete == false && x.TenantId==AppConstant.LoggedInTenantId)
                                    .FirstOrDefault();

                                if (contactBillingAddress != null)
                                {
                                    Guid? stateId = null;

                                    // Validate and assign StateId only if it exists in MstState
                                    if (model.BillingAddressVm?.StateId != null &&
                                        model.BillingAddressVm.StateId != Guid.Empty)
                                    {
                                        var isValidState = await dbContext.MstStates
                                            .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

                                        if (isValidState)
                                        {
                                            stateId = model.BillingAddressVm.StateId;
                                        }
                                    }


                                    contactBillingAddress.FirstName = model.BillingAddressVm?.FirstName;
                                    contactBillingAddress.LastName = model.BillingAddressVm?.LastName;
                                    contactBillingAddress.Phone = model.BillingAddressVm?.PhoneNumber;
                                    contactBillingAddress.Email = model.BillingAddressVm?.Email;
                                    contactBillingAddress.City = model.BillingAddressVm?.City;
                                    contactBillingAddress.State = stateId;
                                    contactBillingAddress.ZipCode = model.BillingAddressVm?.ZipCode;
                                    contactBillingAddress.StreetAddress = model.BillingAddressVm?.StreetAddress;
                                    contactBillingAddress.UpdatedAt = DateTime.Now;

                                    dbContext.TblAddresses.Update(contactBillingAddress);
                                    await dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    Guid? stateId = null;

                                    // Validate and assign StateId only if it exists in MstState
                                    if (model.BillingAddressVm?.StateId != null &&
                                        model.BillingAddressVm.StateId != Guid.Empty)
                                    {
                                        var isValidState = await dbContext.MstStates
                                            .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

                                        if (isValidState)
                                        {
                                            stateId = model.BillingAddressVm.StateId;
                                        }
                                    }
                                    TblAddress CreateContactBillingAddress = new TblAddress
                                    {
                                        AddressId = Guid.NewGuid(),
                                        ContactId = contact1.ContactId,
                                        FirstName = model.BillingAddressVm?.FirstName,
                                        LastName = model.BillingAddressVm?.LastName,
                                        Phone = model.BillingAddressVm?.PhoneNumber,
                                        Email = model.BillingAddressVm?.Email,
                                        City = model.BillingAddressVm?.City,
                                        State = stateId,
                                        ZipCode = model.BillingAddressVm?.ZipCode,
                                        StreetAddress = model.BillingAddressVm?.StreetAddress,
                                        CreatedAt = DateTime.Now,
                                        IsActive = true,
                                        IsDelete = false,
                                        TenantId = TenantId,
                                    };

                                    dbContext.TblAddresses.Add(CreateContactBillingAddress);
                                    await dbContext.SaveChangesAsync();

                                    contact1.PrimaryAddress = CreateContactBillingAddress.AddressId;
                                    await Update(contact1);
                                }
                            }

                            if (!model.IsTaxExemption)
                            {
                                var contactTaxExemption = taxExemptionService.GetAllQuerable()
                                    .Where(x => x.ContactId == contact1.ContactId && x.IsDelete == false ).FirstOrDefault();

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
                            Suffix = model.Suffix ?? string.Empty,
                            Email = model.Email ?? string.Empty,
                            PhoneNumber = model.PhoneNumber ?? string.Empty,
                            Notes = model.Notes ?? string.Empty,
                            ContactType = model.ContactType ?? string.Empty,
                            Website = model.Website ?? string.Empty,
                            Referral = model.Referral ?? string.Empty,
                            GHLContactId = model.GHLContactId ?? string.Empty,
                        };
                        var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration();
                        if (Checkghlresponse.IsSuccess)
                        {
                            var ghlResponse = await SyncWithGHL(contact1, ghlContactAdd);
                            responseVm.IsSuccess = true;
                            responseVm.Message = Checkghlresponse.Message;
                        }
                        else
                         {
                            responseVm.IsSuccess = true;
                            FileLogger.Log(Checkghlresponse.Message);
                          //  responseVm.Message = Checkghlresponse.Message;
                         }
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

                var billingaddress =  await dbContext.TblAddresses
                   .Where(c => c.ContactId == ContactId && contact.PrimaryAddress == c.AddressId && c.IsDelete == false).FirstOrDefaultAsync();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = !billingaddress.IsActive;
                    billingaddress.UpdatedAt = DateTime.Now;
                    dbContext.TblAddresses.Update(billingaddress);
                    await dbContext.SaveChangesAsync();
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

                var billingaddress = await dbContext.TblAddresses
                   .Where(c => c.ContactId == ContactId && c.AddressId == contact.PrimaryAddress && c.IsDelete == false).FirstOrDefaultAsync();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = false;
                    billingaddress.IsDelete = true;
                    billingaddress.UpdatedAt = DateTime.Now;
                    dbContext.TblAddresses.Update(billingaddress);
                    await dbContext.SaveChangesAsync();
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
                        await dbContext.SaveChangesAsync();
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
                if (filter?.TenantId != "" && Role)
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
                        Suffix = contact.Suffix ?? "",
                        Website = contact.Website ?? "",
                        Notes = contact.Notes ?? "",
                        Referral = contact.Referral ?? "",
                        ContactUniqueId = contact.ContactUniqueId ?? "",
                        PrimaryAddress = contact.PrimaryAddress ?? null,
                        IsTaxExemption = contact.IsTaxExemption ?? false,
                        IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                        IsActive = contact.IsActive,
                        IsArchive = contact.IsArchive,
                        CreatedAt = contact.CreatedAt,
                        IsSubtenant = IsSubtenants,
                    };
                    contacts.billingAddressVm = dbContext.TblAddresses.Include(x => x.StateNavigation)
                        .Where(x => x.AddressId == contact.PrimaryAddress
                        && x.IsDelete == false).Select(x => new BillingAddressVm
                        {

                            City = x.City ?? "",
                            Email = x.Email ?? "",
                            FirstName = x.FirstName ?? "",
                            LastName = x.LastName ?? "",
                            PhoneNumber = x.Phone ?? "",
                            StateName = dbContext.MstStates.Where(s => s.Id == x.State).Select(y => y.StateName ?? "").FirstOrDefault() ?? "",
                            StateId = x.State ?? null,
                            StreetAddress = x.StreetAddress ?? "",
                            ZipCode = x.ZipCode ?? "",
                        }).FirstOrDefault();
                    var taxExemption = taxExemptionService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId
                        && x.IsDelete == false).FirstOrDefault();
                    if (taxExemption != null) { 
                    contacts.taxExemptionVm = new TaxExemption
                    {
                        EffectiveDate = taxExemption.EffectiveDate ?? null,
                        ExemptionCertificate = taxExemption.ExemptionCertificate.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray() ?? null,
                        ExpiryDate = taxExemption.ExpiryDate ?? null
                    };
                }
                    ContactList.Add(contacts);
                }
                filter.SearchValue = filter?.SearchValue?.Trim().ToLower();
                var FilterRecords = ContactList
                                    .Where(x =>
                                    (filter?.Status == 0 ? x.IsActive == false : (filter?.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter?.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter?.SearchValue)
                                    || (x.ContactId.ToString().ToLower()).Contains(filter?.SearchValue)
                                    || (x.Email.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.Website.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.PhoneNumber ?? "").Contains(filter?.SearchValue)
                                    || ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter?.SearchValue)
                                    || (x.FirstName.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.LastName.ToLower() ?? "").Contains(filter?.SearchValue)
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

                IResponseVm<GhlContactSingleResponse> ghlResponse = new ResponseVm<GhlContactSingleResponse>();
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
                    await gHLConfigurationService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    ghlResponse = await gHLService_V2.CreateContactAsync(ghlContactAdd);
                    if (ghlResponse.IsSuccess)
                    {
                        var contactExist = (await GetById(contact.ContactId));

                        if (contactExist != null && contactExist.IsDelete == false)
                        {
                            contactExist.GhlcontactId = ghlResponse?.Data?.Contact?.id;
                            contactExist.LastSync = DateTime.Now;
                        }
                        var res = await Update(contactExist);
                        responseVm.IsSuccess = true;
                        responseVm.Message = "Contact synced successfully with GHL.";
                    }
                    //responseVm.IsSuccess = true;
                    //responseVm.Message = AppConstant.InsertSuccessMsg;
                    return responseVm;
                }
                else
                {
                    await gHLConfigurationService.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    ghlResponse = await gHLService_V2.UpdateContactAsync(contact.GhlcontactId, ghlContactAdd);
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
                            //State = contactData.location.state,
                            ZipCode = contactData.location.postalCode,
                            StreetAddress = contactData.location.address
                        };
                    }
                    await gHLConfigurationService.GetGHLConfiguration(contact.locationId);
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
                            StateName = contactData.location.state,
                            ZipCode = contactData.location.postalCode,
                            StreetAddress = contactData.location.address
                        };
                    }
                    await gHLConfigurationService.GetGHLConfiguration(contact.locationId);
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

        public async Task<IResponseVm<string>> ContactSync()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration();
                if (Checkghlresponse.IsSuccess)
                {
                    var getGHLContacts = await gHLService_V2.GetContactsAsync();
                    if (getGHLContacts != null)
                    {
                        FileLogger.Log("GHL", getGHLContacts);
                    }
                    else
                    {
                        FileLogger.Log("Exception occurred GHLContact is null.");
                    }
                    var getAppContacts = (await GetAll()).Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).ToList();

                    List<Sp_Contact> contactsToInsert = new List<Sp_Contact>();
                    List<GhlContactAdd> contactsToAddToGHL = new List<GhlContactAdd>();
                    List<GhlContactAdd> contactsUpdateToGHL = new List<GhlContactAdd>();
                    foreach (var ghlcontact in getGHLContacts)
                    {
                        var existingContact = getAppContacts.FirstOrDefault(x => x.GhlcontactId == ghlcontact.id);

                        if (existingContact != null)
                        {
                            if (ghlcontact.dateUpdated > existingContact.UpdatedAt)
                            {
                                existingContact.FirstName = ghlcontact.firstName;
                                existingContact.Email = ghlcontact.email;
                                existingContact.PhoneNumber = ghlcontact.phone;
                                existingContact.UpdatedAt = DateTime.Now;
                                existingContact.ModifiedBy = AppConstant.LoggedInUserID;
                                existingContact.LastSync = DateTime.Now;
                                existingContact.ContactType = ghlcontact.type;
                                FileLogger.Log("Updating Contact in application(Application ContactId).", existingContact.ContactId);
                                FileLogger.Log("Updating Contact in application(GhlContactId).", existingContact.GhlcontactId);
                                var res = await Update(existingContact);
                                if (res == null) { FileLogger.Log("Exception occurred Updating Contact in application.", res); }
                            }
                            else
                            {
                                var ghlUpdatecontact = new GhlContactAdd
                                {
                                    FirstName = existingContact.FirstName ?? "",
                                    LastName = existingContact.LastName ?? "",
                                    Phone = existingContact.PhoneNumber ?? "",
                                    Email = existingContact.Email ?? "",
                                    ContactType = existingContact.ContactType ?? "",
                                    GHLContactId = existingContact.GhlcontactId ?? ""
                                };
                                FileLogger.Log("Updating Contact in application(GhlContactId).", existingContact.GhlcontactId);
                                var res = await gHLService_V2.UpdateContactAsync(ghlUpdatecontact.GHLContactId, ghlUpdatecontact);
                                if (res == null)
                                {
                                    FileLogger.Log("Exception occurred: Response object is null.");
                                }
                                else if (!res.IsSuccess)
                                {
                                    FileLogger.Log("Exception occurred: Contact is null.", res);
                                }
                            }
                        }
                        else if (existingContact == null)
                        {
                            var tenant = tenantService.GetAllQuerable()
                                .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false)
                                .Select(x => x.BrandingInfo)
                                .FirstOrDefault();

                            var brandingInfo = tenantBranding.GetAllQuerable()
                                .Where(x => x.Id == tenant && x.IsDelete == false)
                                .FirstOrDefault();

                            var newContact = new Sp_Contact
                            {
                                ContactType = ghlcontact.type,
                                FirstName = ghlcontact.firstName,
                                LastName = ghlcontact.lastName,
                                Email = ghlcontact.email,
                                PhoneNumber = ghlcontact.phone,
                                //locationId = ghlcontact.locationId,
                                GHLContactId = ghlcontact.id,
                                lastAsync = DateTime.Now,
                            };

                            if (brandingInfo != null)
                            {
                                newContact.BillingAddressVm = new BillingAddressVm
                                {
                                    BillingFirstName = newContact.FirstName,
                                    BillingLastName = newContact.LastName,
                                    FirstName = newContact.FirstName,
                                    LastName = newContact.LastName,
                                    Email = newContact.Email,
                                    PhoneNumber = newContact.PhoneNumber,
                                    City = brandingInfo.City,
                                    //State = brandingInfo.StreetAddress,
                                    ZipCode = brandingInfo.ZipCode,
                                    StreetAddress = brandingInfo.StreetAddress
                                };
                            }
                            contactsToInsert.Add(newContact);
                        }
                    }

                    foreach (var appContact in getAppContacts)
                    {
                        if (string.IsNullOrEmpty(appContact.GhlcontactId))
                        {
                            var newContactToAddToGHL = new GhlContactAdd
                            {
                                FirstName = appContact.FirstName,
                                LastName = appContact.LastName,
                                Email = appContact.Email,
                                Phone = appContact.PhoneNumber,
                                ContactType = appContact.ContactType,
                            };
                            contactsToAddToGHL.Add(newContactToAddToGHL);
                        }
                        else
                        {
                            var existingGHLContact = getGHLContacts.FirstOrDefault(x => x.id == appContact.GhlcontactId);
                            if (existingGHLContact != null)
                            {
                                if (existingGHLContact.dateUpdated < appContact.UpdatedAt)
                                {
                                    var newContactToAddToGHL = new GhlContactAdd
                                    {
                                        FirstName = existingGHLContact.firstName,
                                        LastName = existingGHLContact.lastName,
                                        Email = existingGHLContact.email,
                                        Phone = existingGHLContact.phone,
                                        ContactType = existingGHLContact.type,
                                        GHLContactId = existingGHLContact.id
                                    };
                                    contactsUpdateToGHL.Add(newContactToAddToGHL);
                                }
                                else
                                {
                                    var updatecontact = new UpdateContact
                                    {
                                        FirstName = existingGHLContact.firstName,
                                        LastName = existingGHLContact.lastName,
                                        Email = existingGHLContact.email,
                                        PhoneNumber = existingGHLContact.phone,
                                        ContactType = existingGHLContact.type,
                                        GHLContactId = existingGHLContact.id,
                                        lastAsync = DateTime.Now
                                    };
                                    FileLogger.Log("Updating Contact in application(Application ContactId).", updatecontact.GHLContactId);
                                    var res = await Update_Contact(updatecontact);
                                    if (res.IsSuccess = false)
                                    { FileLogger.Log("Exception occurred Updating Contact in application.", res.Message); }
                                }
                            }
                            else
                            { 
                                FileLogger.Log("this contact deleted on GHL but in shutter ", appContact.ContactId);
                               // var res = await DeleteContact(appContact.ContactId);
                                //if (!res.IsSuccess)
                                //{ FileLogger.Log("Exception occurred while Deleting Contact in application.", res.Message); }
                            }
                        }
                    }
                    if (contactsToInsert.Any())
                    {
                        foreach (var contact in contactsToInsert)
                        {
                            //if (string.IsNullOrEmpty(contact.GHLContactId))
                            //{
                                var res = await Create_Contact(contact);
                                if (!res.IsSuccess) { FileLogger.Log("Exception occurred Creating Contact in application.", res.Message); }
                            //}
                            //else
                            //{
                            //    var ghlContactAdd = new UpdateContact
                            //    {
                            //        FirstName = contact.FirstName ?? string.Empty,
                            //        LastName = contact.LastName ?? string.Empty,
                            //        Email = contact.Email ?? string.Empty,
                            //        PhoneNumber = contact.PhoneNumber ?? string.Empty,
                            //        Notes = contact.Notes ?? string.Empty,
                            //        ContactType = contact.ContactType ?? string.Empty,
                            //        Website = contact.Website ?? string.Empty,
                            //        Referral = contact.Referral ?? string.Empty,
                            //        GHLContactId = contact.GHLContactId ?? string.Empty,
                            //        locationId = contact.locationId ?? string.Empty,
                            //    };
                            //    var res = await Update_Contact(ghlContactAdd);
                            //    if (!res.IsSuccess) { FileLogger.Log("Exception occurred Updating Contact in application.", res.Message); }
                            //}
                        }
                    }
                    if (contactsToAddToGHL.Any())
                    {
                        foreach (var contact in contactsToAddToGHL)
                        {
                            if (contact?.GHLContactId == null)
                            {

                                FileLogger.Log("Creating bulk Contact in GHL(GhlContactId).", contact);
                                var res = await gHLService_V2.CreateContactAsync(contact);
                                if (res.IsSuccess)
                                {
                                    var contactExist = (await GetAll()).Where(x => x.Email == contact.Email && x.TenantId==AppConstant.LoggedInTenantId).FirstOrDefault();
                                    if (contactExist != null && res?.Data?.Contact?.id!=null)
                                    {
                                        contactExist.GhlcontactId = res?.Data?.Contact?.id??"";
                                        contactExist.LastSync = DateTime.Now;
                                    }
                                    await Update(contactExist);

                                }
                                if (!res.IsSuccess)
                                { FileLogger.Log("Exception occurred Creating bulk Contact in GHL.", res); }
                            }
                            else
                            {
                                FileLogger.Log("Updating Contact in GHL.", contact.GHLContactId);
                                var res = await gHLService_V2.UpdateContactAsync(contact.GHLContactId, contact);
                                if (!res.IsSuccess)
                                { FileLogger.Log("Updating Contact in GHL.", contact.GHLContactId); }

                            }
                        }
                    }
                    var isUpdated = await gHLService_V2.UpdateLastSyncConfigurationAsync();

                    if (!isUpdated)
                    {
                        FileLogger.Log("Updating Contact in GHL.", isUpdated);
                        responseVm.Message = "Error: Configuration not found or update failed.";
                        return responseVm;
                    }

                    responseVm.IsSuccess = true;
                    responseVm.Message = "All contacts synced successfully.";
                    responseVm.Data = "Synced successful.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }

            return responseVm;
        }



    }
}

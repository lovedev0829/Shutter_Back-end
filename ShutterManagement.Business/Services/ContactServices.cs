//using System.Data.Entity;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true);

                if (IsTenantExists)
                {
                    var IsEmailExists = GetAllQuerable()
                        .Any(c => c.Email == email && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false);

                    var IsPhoneExists = GetAllQuerable()
                        .Any(c => c.PhoneNumber == phone && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false);

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

        //public async Task<IResponseVm<string>> Create_Contact(Sp_Contact model, Guid? LoggedInTenantId = null)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
        //        var IsTenantExists = false;

        //        //Change the format of phonenumber added with +1 
        //        if (!string.IsNullOrEmpty(model.PhoneNumber))
        //        {
        //            model.PhoneNumber = FormatPhoneNumber(model.PhoneNumber);
        //        }
        //        FileLogger.Log($"Location id fetched {model.locationId}");
        //        //if (model.locationId != null)
        //        //{
        //        //    // await gHLConfigurationService.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
        //        //    TenantId = (Guid)LoggedInTenantId;
        //        //    FileLogger.Log($"Tenant id fetched {TenantId}");
        //        //    if (TenantId != null)
        //        //    {
        //        //        IsTenantExists = true;
        //        //        model.IsTaxExemption = true;
        //        //    }
        //        //}
        //        //else
        //        //{
        //            IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == LoggedInTenantId && x.IsActive == true);
        //            FileLogger.Log($"Tenant Id id fetched {LoggedInTenantId}");
        //            //model.IsTaxExemption = true;
        //        //}
        //        if (!model.IsTaxExemption)
        //        {
        //            if (model.TaxExemptionVm.EffectiveDate == null && model.TaxExemptionVm.ExpiryDate == null)
        //            {
        //                responseVm.Message = "Please Fill TaxExemption Details.";
        //                return responseVm;
        //            }
        //        }

        //        if (IsTenantExists)
        //        {
        //            //await gHLConfigurationService.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
        //            var IsContactExists = GetAllQuerable()
        //                .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
        //                //.Where(c => (c.Email == model.Email && c.PhoneNumber == model.PhoneNumber)
        //                && c.TenantId == LoggedInTenantId
        //                && c.IsDelete == false).ToList();

        //            if (IsContactExists.Count > 0)
        //            {
        //                //If Contact is already exists in Shutter DB then update the GHL Contact if GHLContactId is not null
        //                if (!string.IsNullOrEmpty(model.GHLContactId))
        //                {
        //                    var ghlContactAdd = new UpdateContact
        //                    {
        //                        FirstName = model.FirstName ?? string.Empty,
        //                        LastName = model.LastName ?? string.Empty,
        //                        Suffix = model.Suffix ?? string.Empty,
        //                        Email = model.Email ?? string.Empty,
        //                        PhoneNumber = model.PhoneNumber ?? string.Empty,
        //                        Notes = model.Notes ?? string.Empty,
        //                        ContactType = model.ContactType ?? string.Empty,
        //                        Website = model.Website ?? string.Empty,
        //                        Referral = model.Referral ?? string.Empty,
        //                        GHLContactId = model.GHLContactId ?? string.Empty,

        //                    };
        //                    if (!model.IsBillingAddressSameAsAddress)
        //                    {
        //                        var stateId = dbContext.MstStates.Where(x => x.StateCode.ToLower() == model.BillingAddressVm.StateCode.ToLower().Trim() ||
        //                        x.StateName == model.BillingAddressVm.StateCode.ToLower().Trim()).Select(y => y.Id).FirstOrDefault();
        //                        ghlContactAdd.BillingAddressVm = new BillingAddressVm
        //                        {
        //                            BillingFirstName = model.BillingAddressVm.FirstName ?? "",
        //                            BillingLastName = model.BillingAddressVm.LastName ?? "",
        //                            FirstName = model.BillingAddressVm.FirstName ?? "",
        //                            LastName = model.BillingAddressVm.LastName ?? "",
        //                            Email = model.BillingAddressVm.Email ?? "",
        //                            PhoneNumber = model.BillingAddressVm.PhoneNumber ?? "",
        //                            City = model.BillingAddressVm.City ?? "",
        //                            StateId = stateId != Guid.Empty ? stateId : null,
        //                            ZipCode = model.BillingAddressVm.ZipCode,
        //                            StreetAddress = model.BillingAddressVm.StreetAddress ?? ""
        //                        };
        //                    }
        //                    var result = await Update_Contact(ghlContactAdd, LoggedInTenantId);
        //                    return result;
        //                }
        //                else
        //                {
        //                    responseVm.Message = $"Email {model.Email} or Phone {model.PhoneNumber} " + AppConstant.AlreadyExists;
        //                    return responseVm;
        //                }
        //            }
        //                //for Billing address
        //                if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
        //                {
        //                    responseVm.Message = "Billing details required.";
        //                    return responseVm;
        //                }
        //                //for Tax exemption
        //                if (model.TaxExemptionVm != null && model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
        //                {
        //                    responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
        //                    return responseVm;
        //                }

        //                //generate unique version
        //                int Version = GetAllQuerable().Where(x => x.TenantId == LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
        //                    .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
        //                Version += 1;
        //                var Companycode = tenantService.GetAllQuerable().Where(x => x.TenantId == LoggedInTenantId && x.IsActive == true && x.IsDelete == false).Select(x => x.Code).FirstOrDefault();
        //                var contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, Companycode, Version);

        //                TblContact CreateContact = new TblContact
        //                {
        //                    ContactId = Guid.NewGuid(),
        //                    ContactType = model.ContactType ?? "",
        //                    FirstName = model.FirstName ?? "",
        //                    LastName = model.LastName ?? "",
        //                    Suffix = model.Suffix ?? "",
        //                    // Name = model.Name,
        //                    PhoneNumber = model.PhoneNumber ?? "",
        //                    Email = model.Email ?? "",
        //                    Website = model.Website ?? "",
        //                    Notes = model.Notes ?? "",
        //                    Referral = model.Referral ?? "",
        //                    ContactUniqueId = contactUniqueId ?? "",
        //                    IsTaxExemption = model.IsTaxExemption,
        //                    IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
        //                    CreatedBy = LoggedInTenantId,
        //                    CreatedAt = DateTime.Now,
        //                    TenantId = LoggedInTenantId,
        //                    IsActive = true,
        //                    IsArchive = false,
        //                    IsDelete = false
        //                };
        //                var res = await Add(CreateContact);
        //                if (model.lastAsync != null && !string.IsNullOrEmpty(model.GHLContactId))
        //                {
        //                    res.LastSync = model.lastAsync;
        //                    res.GhlcontactId = model.GHLContactId ?? "";
        //                    await Update(res);
        //                }
        //                 // To add address of contact
        //                if (res.ContactId != Guid.Empty)
        //                {
        //                    // Add Billing Address
        //                    if (!model.IsBillingAddressSameAsAddress)
        //                    {
        //                        Guid? stateId = null;
        //                        // Validate and assign StateId only if it exists in MstState
        //                        if (model.BillingAddressVm?.StateId != null &&
        //                            model.BillingAddressVm.StateId != Guid.Empty)
        //                        {
        //                            var isValidState = await dbContext.MstStates
        //                                .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

        //                            if (isValidState)
        //                            {
        //                                stateId = model.BillingAddressVm.StateId;
        //                            }
        //                        }
        //                        TblAddress address = new TblAddress
        //                        {
        //                            AddressId = Guid.NewGuid(),
        //                            ContactId = res.ContactId,
        //                            Label = "Primary Address",
        //                            FirstName = model.BillingAddressVm?.FirstName ?? "",
        //                            LastName = model.BillingAddressVm?.LastName ?? "",
        //                            Phone = model.BillingAddressVm?.PhoneNumber ?? "",
        //                            Email = model.BillingAddressVm?.Email ?? "",
        //                            City = model.BillingAddressVm?.City ?? "",
        //                            State = stateId ?? null,
        //                            ZipCode = model.BillingAddressVm?.ZipCode ?? "",
        //                            StreetAddress = model.BillingAddressVm?.StreetAddress ?? "",
        //                            CreatedAt = DateTime.Now,
        //                            IsActive = true,
        //                            IsDelete = false,
        //                            TenantId = LoggedInTenantId,
        //                            CreatedBy = AppConstant.LoggedInUserID
        //                        };
        //                        dbContext.TblAddresses.Add(address);
        //                        await dbContext.SaveChangesAsync();

        //                    // Update Primary Address in Contact
        //                    res.PrimaryAddress = address.AddressId;
        //                        await Update(res);
        //                    }

        //                    //Add Tax Exemption
        //                    if (!model.IsTaxExemption)
        //                    {
        //                        string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);
        //                        TblTaxExemption taxExemption1 = new TblTaxExemption
        //                        {
        //                            ContactId = res.ContactId,
        //                            ExemptionCertificate = arrayString ?? "",
        //                            EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
        //                            ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
        //                            CreatedAt = DateTime.Now,
        //                            IsActive = true,
        //                            IsDelete = false,
        //                        };
        //                        await taxExemptionService.Add(taxExemption1);
        //                        responseVm.IsSuccess = true;
        //                        responseVm.Message = AppConstant.InsertSuccessMsg;
        //                    }

        //                    //Add contct in GHL if contact added from Shutter software
        //                    if (string.IsNullOrEmpty(model.GHLContactId))
        //                    {
        //                        var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration(LoggedInTenantId);
        //                        if (Checkghlresponse.IsSuccess)
        //                        {
        //                            var ghlResponse = await SyncWithGHL(res, model);
        //                            if (ghlResponse.IsSuccess)
        //                            {
        //                                responseVm.IsSuccess = true;
        //                                responseVm.Message = AppConstant.InsertSuccessMsg + ghlResponse.Message;
        //                            }
        //                            else
        //                            {
        //                                responseVm.IsSuccess = true;
        //                                responseVm.Message = AppConstant.InsertSuccessMsg + ghlResponse.Message;
        //                            }
        //                        }
        //                        if (!Checkghlresponse.IsSuccess)
        //                        {
        //                            responseVm.IsSuccess = true;
        //                            responseVm.Message = AppConstant.InsertSuccessMsg;
        //                            FileLogger.Log(Checkghlresponse.Message);
        //                            //responseVm.IsSuccess = true;
        //                            //responseVm.Message = Checkghlresponse.Message;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    responseVm.Message = AppConstant.NoDataFound;
        //                }
        //            }
        //            else
        //            {
        //                responseVm.Message = AppConstant.TenantNotFound;
        //            }
        //        }


        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}
        //public async Task<IResponseVm<string>> Update_Contact(UpdateContact model, Guid? LoggedInTenantId = null)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

        //        var IsTenantExists = false;
        //        if (model.PhoneNumber != null)
        //        {
        //            model.PhoneNumber = FormatPhoneNumber(model.PhoneNumber);
        //        }
        //        //if (model.locationId != null)
        //        //{
        //        //   // await gHLConfigurationService.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
        //        //    TenantId = (Guid)LoggedInTenantId;

        //        //    if (TenantId != Guid.Empty)
        //        //    {
        //        //        IsTenantExists = true;
        //        //        model.IsTaxExemption = true;
        //        //    }
        //        //}
        //        //else
        //        //{
        //        IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == LoggedInTenantId && x.IsActive == true);
        //        //}

        //        if (IsTenantExists)
        //        {
        //            await gHLConfigurationService.GetGHLConfiguration(LoggedInTenantId.ToString());
        //            if (!string.IsNullOrEmpty(model.GHLContactId))
        //            {
        //                var contact = (await GetAll()).Where
        //                    (c => c.GhlcontactId == model.GHLContactId && c.TenantId == LoggedInTenantId && c.IsDelete == false).FirstOrDefault();

        //                var IsContactExists = GetAllQuerable()
        //                    .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
        //                                && c.TenantId == LoggedInTenantId && c.IsDelete == false).OrderByDescending(x=>x.CreatedAt)
        //                    .ToList();

        //                if (contact == null && IsContactExists.Count > 0)
        //                {
        //                    contact = IsContactExists.FirstOrDefault();

        //                    //responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
        //                    //return responseVm;
        //                }

        //                if (contact != null)
        //                {
        //                    contact.ContactType = model.ContactType??"";
        //                    contact.FirstName = model.FirstName??"";
        //                    contact.LastName = model.LastName??"";
        //                    contact.Suffix = model.Suffix ??"";
        //                    contact.PhoneNumber = model.PhoneNumber ??"";
        //                    contact.Email = model.Email ??"";
        //                    contact.Website = model.Website ??"";
        //                    contact.Notes = model.Notes ??"";
        //                    contact.Referral = model.Referral ?? "";
        //                    contact.IsTaxExemption = model.IsTaxExemption ;
        //                    contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
        //                    contact.ModifiedBy = LoggedInTenantId;
        //                    contact.UpdatedAt = DateTime.Now;

        //                    await Update(contact); responseVm.IsSuccess = true;
        //                    responseVm.Message = AppConstant.UpdateSuccessMsg;
        //                    if (!model.IsBillingAddressSameAsAddress)
        //                    {
        //                        var contactBillingAddress = dbContext.TblAddresses
        //                        .Where(x => x.ContactId == contact.ContactId && x.AddressId == contact.PrimaryAddress && x.IsDelete == false && x.TenantId == LoggedInTenantId)
        //                        .FirstOrDefault();

        //                        if (contactBillingAddress != null)
        //                        {
        //                            Guid? stateId = null;

        //                            // Validate and assign StateId only if it exists in MstState
        //                            if (model.BillingAddressVm?.StateId != null &&
        //                                model.BillingAddressVm.StateId != Guid.Empty)
        //                            {
        //                                var isValidState = await dbContext.MstStates
        //                                    .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

        //                                if (isValidState)
        //                                {
        //                                    stateId = model.BillingAddressVm.StateId;
        //                                }
        //                            }


        //                            contactBillingAddress.FirstName = model.BillingAddressVm?.FirstName;
        //                            contactBillingAddress.LastName = model.BillingAddressVm?.LastName;
        //                            contactBillingAddress.Phone = model.BillingAddressVm?.PhoneNumber;
        //                            contactBillingAddress.Email = model.BillingAddressVm?.Email;
        //                            contactBillingAddress.City = model.BillingAddressVm?.City;
        //                            contactBillingAddress.State = stateId;
        //                            contactBillingAddress.ZipCode = model.BillingAddressVm?.ZipCode;
        //                            contactBillingAddress.StreetAddress = model.BillingAddressVm?.StreetAddress;
        //                            contactBillingAddress.UpdatedAt = DateTime.Now;

        //                            dbContext.TblAddresses.Update(contactBillingAddress);
        //                            await dbContext.SaveChangesAsync();
        //                        }
        //                        else
        //                        {
        //                            Guid? stateId = null;

        //                            // Validate and assign StateId only if it exists in MstState
        //                            if (model.BillingAddressVm?.StateId != null &&
        //                                model.BillingAddressVm.StateId != Guid.Empty)
        //                            {
        //                                var isValidState = await dbContext.MstStates
        //                                    .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

        //                                if (isValidState)
        //                                {
        //                                    stateId = model.BillingAddressVm.StateId;
        //                                }
        //                            }
        //                            TblAddress CreateContactBillingAddress = new TblAddress
        //                            {
        //                                AddressId = Guid.NewGuid(),
        //                                ContactId = contact.ContactId,
        //                                FirstName = model.BillingAddressVm?.FirstName,
        //                                LastName = model.BillingAddressVm?.LastName,
        //                                Phone = model.BillingAddressVm?.PhoneNumber,
        //                                Email = model.BillingAddressVm?.Email,
        //                                City = model.BillingAddressVm?.City,
        //                                State = stateId,
        //                                ZipCode = model.BillingAddressVm?.ZipCode,
        //                                StreetAddress = model.BillingAddressVm?.StreetAddress,
        //                                CreatedAt = DateTime.Now,
        //                                IsActive = true,
        //                                IsDelete = false,
        //                                TenantId = LoggedInTenantId,
        //                            };

        //                            dbContext.TblAddresses.Add(CreateContactBillingAddress);
        //                            await dbContext.SaveChangesAsync();

        //                            contact.PrimaryAddress = CreateContactBillingAddress.AddressId;
        //                            await Update(contact);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var contactCreate = new Sp_Contact
        //                    {
        //                        ContactType = model.ContactType ?? "",
        //                        FirstName = model.FirstName??"",
        //                        LastName = model.LastName??"",
        //                        Suffix=model.Suffix??"",
        //                        Email = model.Email??"",
        //                        PhoneNumber = model.PhoneNumber ?? "",
        //                        lastAsync = model.lastAsync,
        //                        GHLContactId = model.GHLContactId,
        //                    };
        //                    if (!model.IsBillingAddressSameAsAddress)
        //                    {
        //                       var stateId = dbContext.MstStates.Where(x => x.StateCode.ToLower() == model.BillingAddressVm.StateCode.ToLower().Trim() 
        //                       || x.StateName.ToLower() == model.BillingAddressVm.StateCode.ToLower().Trim()).Select(y => y.Id).FirstOrDefault();
        //                        model.BillingAddressVm = new BillingAddressVm
        //                        {
        //                            BillingFirstName = model.BillingAddressVm.FirstName,
        //                            BillingLastName = model.BillingAddressVm.LastName,
        //                            FirstName = model.BillingAddressVm.FirstName,
        //                            LastName = model.BillingAddressVm.LastName,
        //                            Email = model.BillingAddressVm.Email,
        //                            PhoneNumber = model.BillingAddressVm.PhoneNumber,
        //                            City = model.BillingAddressVm.City,
        //                            StateId = stateId != Guid.Empty ? stateId : null,
        //                            ZipCode = model.BillingAddressVm.ZipCode,
        //                            StreetAddress = model.BillingAddressVm.StreetAddress
        //                        };

        //                    }

        //                    var res = await Create_Contact(contactCreate,LoggedInTenantId);
        //                }
        //            }
        //            else
        //            {
        //                var contact1 = GetAllQuerable().Where
        //                    (c => c.ContactId == model.ContactId && c.TenantId == LoggedInTenantId && c.IsDelete == false).FirstOrDefault();

        //                var IsContactExists1 = (await GetAll())
        //                    .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
        //                                && c.TenantId == LoggedInTenantId && c.IsDelete == false && c.ContactId != model.ContactId)
        //                    .ToList();

        //                if (IsContactExists1.Count > 0)
        //                {
        //                    responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
        //                    return responseVm;
        //                }
        //                if (model.TaxExemptionVm != null)
        //                {
        //                    if (model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
        //                    {
        //                        responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
        //                        return responseVm;
        //                    }
        //                }
        //                if (contact1 != null)
        //                {
        //                    contact1.ContactType = model.ContactType;
        //                    contact1.FirstName = model.FirstName;
        //                    contact1.LastName = model.LastName;
        //                    contact1.Suffix = model.Suffix;
        //                    contact1.PhoneNumber = model.PhoneNumber;
        //                    contact1.Email = model.Email;
        //                    contact1.Website = model.Website;
        //                    contact1.Notes = model.Notes;
        //                    contact1.Referral = model.Referral;
        //                    contact1.IsTaxExemption = model.IsTaxExemption;
        //                    contact1.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
        //                    contact1.ModifiedBy = LoggedInTenantId;
        //                    contact1.UpdatedAt = DateTime.Now;

        //                    await Update(contact1);
        //                    if(contact1.GhlcontactId !=null)
        //                    {
        //                        model.IsBillingAddressSameAsAddress = false;
        //                    }
        //                    if (!model.IsBillingAddressSameAsAddress)
        //                    {
        //                        var contactBillingAddress = dbContext.TblAddresses
        //                            .Where(x => x.ContactId == contact1.ContactId && x.AddressId == contact1.PrimaryAddress && x.IsDelete == false && x.TenantId==LoggedInTenantId)
        //                            .FirstOrDefault();

        //                        if (contactBillingAddress != null)
        //                        {
        //                            Guid? stateId = null;

        //                            // Validate and assign StateId only if it exists in MstState
        //                            if (model.BillingAddressVm?.StateId != null &&
        //                                model.BillingAddressVm.StateId != Guid.Empty)
        //                            {
        //                                var isValidState = await dbContext.MstStates
        //                                    .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

        //                                if (isValidState)
        //                                {
        //                                    stateId = model.BillingAddressVm.StateId;
        //                                }
        //                            }


        //                            contactBillingAddress.FirstName = model.BillingAddressVm?.FirstName;
        //                            contactBillingAddress.LastName = model.BillingAddressVm?.LastName;
        //                            contactBillingAddress.Phone = model.BillingAddressVm?.PhoneNumber;
        //                            contactBillingAddress.Email = model.BillingAddressVm?.Email;
        //                            contactBillingAddress.City = model.BillingAddressVm?.City;
        //                            contactBillingAddress.State = stateId;
        //                            contactBillingAddress.ZipCode = model.BillingAddressVm?.ZipCode;
        //                            contactBillingAddress.StreetAddress = model.BillingAddressVm?.StreetAddress;
        //                            contactBillingAddress.UpdatedAt = DateTime.Now;

        //                            dbContext.TblAddresses.Update(contactBillingAddress);
        //                            await dbContext.SaveChangesAsync();
        //                        }
        //                        else
        //                        {
        //                            Guid? stateId = null;

        //                            // Validate and assign StateId only if it exists in MstState
        //                            if (model.BillingAddressVm?.StateId != null &&
        //                                model.BillingAddressVm.StateId != Guid.Empty)
        //                            {
        //                                var isValidState = await dbContext.MstStates
        //                                    .AnyAsync(s => s.Id == model.BillingAddressVm.StateId);

        //                                if (isValidState)
        //                                {
        //                                    stateId = model.BillingAddressVm.StateId;
        //                                }
        //                            }
        //                            TblAddress CreateContactBillingAddress = new TblAddress
        //                            {
        //                                AddressId = Guid.NewGuid(),
        //                                ContactId = contact1.ContactId,
        //                                FirstName = model.BillingAddressVm?.FirstName,
        //                                LastName = model.BillingAddressVm?.LastName,
        //                                Phone = model.BillingAddressVm?.PhoneNumber,
        //                                Email = model.BillingAddressVm?.Email,
        //                                City = model.BillingAddressVm?.City,
        //                                State = stateId,
        //                                ZipCode = model.BillingAddressVm?.ZipCode,
        //                                StreetAddress = model.BillingAddressVm?.StreetAddress,
        //                                CreatedAt = DateTime.Now,
        //                                IsActive = true,
        //                                IsDelete = false,
        //                                TenantId = LoggedInTenantId,
        //                            };

        //                            dbContext.TblAddresses.Add(CreateContactBillingAddress);
        //                            await dbContext.SaveChangesAsync();

        //                            contact1.PrimaryAddress = CreateContactBillingAddress.AddressId;
        //                            await Update(contact1);
        //                        }
        //                    }

        //                    if (!model.IsTaxExemption)
        //                    {
        //                        var contactTaxExemption = taxExemptionService.GetAllQuerable()
        //                            .Where(x => x.ContactId == contact1.ContactId && x.IsDelete == false ).FirstOrDefault();

        //                        string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);

        //                        if (contactTaxExemption != null)
        //                        {
        //                            contactTaxExemption.ExemptionCertificate = arrayString;
        //                            contactTaxExemption.EffectiveDate = model.TaxExemptionVm?.EffectiveDate;
        //                            contactTaxExemption.ExpiryDate = model.TaxExemptionVm?.ExpiryDate;
        //                            contactTaxExemption.UpdatedAt = DateTime.Now;
        //                            contactTaxExemption.IsActive = true;
        //                            contactTaxExemption.IsDelete = false;
        //                            await taxExemptionService.Update(contactTaxExemption);
        //                        }
        //                        else
        //                        {
        //                            TblTaxExemption taxExemption1 = new TblTaxExemption
        //                            {
        //                                ContactId = contact1.ContactId,
        //                                ExemptionCertificate = arrayString,
        //                                EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
        //                                ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
        //                                CreatedAt = DateTime.Now,
        //                                IsActive = true,
        //                                IsDelete = false
        //                            };
        //                            await taxExemptionService.Add(taxExemption1);
        //                        }
        //                    }
        //                }
        //                var ghlContactAdd = new Sp_Contact
        //                {
        //                    //Id = contact1.GhlcontactId.ToString() ?? string.Empty,
        //                    FirstName = model.FirstName ?? string.Empty,
        //                    LastName = model.LastName ?? string.Empty,
        //                    Suffix = model.Suffix ?? string.Empty,
        //                    Email = model.Email ?? string.Empty,
        //                    PhoneNumber = model.PhoneNumber ?? string.Empty,
        //                    Notes = model.Notes ?? string.Empty,
        //                    ContactType = model.ContactType ?? string.Empty,
        //                    Website = model.Website ?? string.Empty,
        //                    Referral = model.Referral ?? string.Empty,
        //                    GHLContactId = model.GHLContactId ?? string.Empty,
        //                };
        //                if(!model.IsBillingAddressSameAsAddress)
        //                {
        //                    ghlContactAdd.BillingAddressVm = model.BillingAddressVm;
        //                }
        //                var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration(LoggedInTenantId);
        //                if (Checkghlresponse.IsSuccess)
        //                {
        //                    var ghlResponse = await SyncWithGHL(contact1, ghlContactAdd);
        //                    responseVm.IsSuccess = true;
        //                    responseVm.Message = Checkghlresponse.Message;
        //                }
        //                else
        //                 {
        //                    responseVm.IsSuccess = true;
        //                    FileLogger.Log(Checkghlresponse.Message);
        //                  //  responseVm.Message = Checkghlresponse.Message;
        //                 }
        //                responseVm.IsSuccess = true;
        //                responseVm.Message = AppConstant.UpdateSuccessMsg;
        //            }
        //        }
        //        else
        //        {
        //            responseVm.Message = AppConstant.TenantNotFound;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}
        public async Task<IResponseVm<string>> Create_Contact(Sp_Contact model, Guid? LoggedInTenantId = null,bool? IsGHLContact = false)
        {
            var responseVm = new ResponseVm<string>();
            try
            {
                var tenantId = (LoggedInTenantId != null && LoggedInTenantId != Guid.Empty)
                    ? LoggedInTenantId.Value
                    : claimsProperty.GetTenantId();

                if (!await tenantService.AnyAsync(x => x.TenantId == tenantId && x.IsActive == true))
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var phoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
                if (phoneNumber == "+1")
                {
                    model.PhoneNumber = null; // Treat "+1" alone as no valid phone number
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    model.PhoneNumber = PhoneHelper.FormatPhoneNumberforGHL(model.PhoneNumber);

                if (!model.IsTaxExemption)
                {
                    if
                    (model.TaxExemptionVm?.EffectiveDate == null || model.TaxExemptionVm?.ExpiryDate == null)
                    {
                        responseVm.Message = "Please Fill TaxExemption Details.";
                        return responseVm;
                    }
                }

                if (!string.IsNullOrEmpty(model.GHLContactId))
                {
                    if ((bool)IsGHLContact)
                    {
                        var updateModel = PrepareUpdateModelFromSpContact(model);
                        return await Update_Contact(updateModel, tenantId, IsGHLContact);
                    }
                }

                var existingContacts = GetAllQuerable().Where(c =>
                 //(c.Email == model.Email || c.PhoneNumber==model.PhoneNumber) && model.GHLContactId != null
                c.TenantId == tenantId && c.IsDelete == false).ToList();

              

                // Validate at least one contact field
                if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.PhoneNumber))
                {
                    responseVm.Message = "Email or Phone Number — please fill at least one";
                    return responseVm;
                }

                // Validate duplicates based on available fields
                if (!string.IsNullOrEmpty(model.Email) && existingContacts.Any(c => c.Email == model.Email))
                {
                    responseVm.Message = $"Email {model.Email} {AppConstant.AlreadyExists}";
                    return responseVm;
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber) && existingContacts.Any(c => c.PhoneNumber == model.PhoneNumber))
                {
                    responseVm.Message = $"Phone {model.PhoneNumber} {AppConstant.AlreadyExists}";
                    return responseVm;
                }

                if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
                {
                    responseVm.Message = "Billing details required.";
                    return responseVm;
                }

                if (model.TaxExemptionVm != null && model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                {
                    responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                    return responseVm;
                }

                var contact = await CreateNewContactRecord(model, tenantId);

                if (contact == null || contact.ContactId == Guid.Empty)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                if (model.lastAsync != null && !string.IsNullOrEmpty(model.GHLContactId))
                {
                    contact.LastSync = model.lastAsync;
                    contact.GhlcontactId = model.GHLContactId;
                    await Update(contact);
                }

                if (!model.IsBillingAddressSameAsAddress)
                    await AddBillingAddress(model.BillingAddressVm, contact, tenantId);

                if (!model.IsTaxExemption)
                    await AddTaxExemption(model.TaxExemptionVm, contact);

                if (string.IsNullOrEmpty(model.GHLContactId))
                {
                    var ghlConfig = await gHLConfigurationService.CheckGHLConfiguration(tenantId);
                    if (ghlConfig.IsSuccess)
                    {
                        var ghlResponse = await SyncWithGHL(contact, model);
                        responseVm.Message = AppConstant.InsertSuccessMsg + "."+ ghlResponse.Message;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                        FileLogger.Log(ghlConfig.Message);
                    }
                    responseVm.IsSuccess = true;
                }
                else
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        private UpdateContact PrepareUpdateModelFromSpContact(Sp_Contact model)
        {
            var updateModel = new UpdateContact
            {
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
                IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                BillingAddressVm = model.BillingAddressVm,
                IsTaxExemption = model.IsTaxExemption
            };
            return updateModel;
        }

        private async Task<TblContact> CreateNewContactRecord(Sp_Contact model, Guid tenantId)
        {
            
            int Version = GetAllQuerable().Where(x => x.TenantId == tenantId).OrderByDescending(p => p.CreatedAt)
                         .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
            Version++;


            var companyCode = tenantService.GetAllQuerable()
                .Where(x => x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false)
                .Select(x => x.Code).FirstOrDefault();

            var uniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, companyCode, Version);

            var contact = new TblContact
            {
                ContactId = Guid.NewGuid(),
                ContactType = model.ContactType ?? string.Empty,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                Suffix = model.Suffix ?? string.Empty,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Email = model.Email ?? string.Empty,
                Website = model.Website ?? string.Empty,
                Notes = model.Notes ?? string.Empty,
                Referral = model.Referral ?? string.Empty,
                ContactUniqueId = uniqueId ?? string.Empty,
                IsTaxExemption = model.IsTaxExemption,
                IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                CreatedBy = tenantId,
                CreatedAt = DateTime.Now,
                TenantId = tenantId,
                IsActive = true,
                IsArchive = false,
                IsDelete = false
            };

            return await Add(contact);
        }

        private async Task AddBillingAddress(BillingAddressVm vm, TblContact contact, Guid tenantId)
        {
            Guid? stateId = null;
            if (vm?.StateId != null && vm.StateId != Guid.Empty)
            {
                var isValid = await dbContext.MstStates.AnyAsync(s => s.Id == vm.StateId);
                stateId = isValid ? vm.StateId : null;
            }

            var address = new TblAddress
            {
                AddressId = Guid.NewGuid(),
                ContactId = contact.ContactId,
                Label = "Primary Address",
                FirstName = vm?.FirstName ?? string.Empty,
                LastName = vm?.LastName ?? string.Empty,
                Phone = vm?.PhoneNumber ?? string.Empty,
                Email = vm?.Email ?? string.Empty,
                City = vm?.City ?? string.Empty,
                State = stateId,
                ZipCode = vm?.ZipCode ?? string.Empty,
                StreetAddress = vm?.StreetAddress ?? string.Empty,
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsDelete = false,
                TenantId = tenantId,
                CreatedBy = AppConstant.LoggedInUserID
            };

            dbContext.TblAddresses.Add(address);
            await dbContext.SaveChangesAsync();

            contact.PrimaryAddress = address.AddressId;
            await Update(contact);
        }

        private async Task AddTaxExemption(TaxExemption vm, TblContact contact)
        {
            var taxExemption = new TblTaxExemption
            {
                ContactId = contact.ContactId,
                ExemptionCertificate = string.Join(", ", vm?.ExemptionCertificate) ?? string.Empty,
                EffectiveDate = vm?.EffectiveDate,
                ExpiryDate = vm?.ExpiryDate,
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };
            await taxExemptionService.Add(taxExemption);
        }

        // Update_Contact method
        public async Task<IResponseVm<string>> Update_Contact(UpdateContact model, Guid? LoggedInTenantId = null,bool? IsGHLContact= false)
        {
            var responseVm = new ResponseVm<string>();
            try
            {
                var tenantId = (LoggedInTenantId != null && LoggedInTenantId != Guid.Empty)
                    ? LoggedInTenantId.Value
                    : claimsProperty.GetTenantId();
               // model.IsBillingAddressSameAsAddress = false;
                if (!await tenantService.AnyAsync(x => x.TenantId == tenantId && x.IsActive == true))
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var phoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
                if (phoneNumber == "+1")
                {
                    model.PhoneNumber = null; // Treat "+1" alone as no valid phone number
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    model.PhoneNumber = PhoneHelper.FormatPhoneNumberforGHL(model.PhoneNumber);
                //await gHLConfigurationService.GetGHLConfiguration(tenantId.ToString());

                if (!string.IsNullOrEmpty(model.GHLContactId))
                {
                    if(string.IsNullOrEmpty(model.PhoneNumber) && string.IsNullOrEmpty(model.Email))
                    {
                        responseVm.Message = "Email or Phone Number — please fill at least one";
                        return responseVm;
                    }

                    var contact = (await GetAll())
                        .FirstOrDefault(c => c.GhlcontactId == model.GHLContactId && c.TenantId == tenantId && c.IsDelete == false);

                   
                    if (contact == null && (bool)IsGHLContact)
                    {
                        if (!string.IsNullOrEmpty(model.Email))
                        {
                            contact = (await GetAll())
                            .FirstOrDefault(c => c.Email == model.Email && c.TenantId == tenantId && c.IsDelete == false);
                        }
                    }
                    if (contact == null && (bool)IsGHLContact)
                    {
                        if (!string.IsNullOrEmpty(model.PhoneNumber))
                        {
                        contact = (await GetAll())
                            .FirstOrDefault(c => c.PhoneNumber == model.PhoneNumber && c.TenantId == tenantId && c.IsDelete == false);
                    }
                    }
                    //contact = (await GetAll())
                    //    .FirstOrDefault(c => c.GhlcontactId == model.GHLContactId && c.TenantId == tenantId && c.IsDelete == false);

                    //var existingContacts = GetAllQuerable()
                    //    .Where(c => (c.Email == model.Email)
                    //                && c.TenantId == tenantId && c.IsDelete == false)
                    //    .OrderByDescending(x => x.CreatedAt)
                    //    .ToList();


                    // contact ??= existingContacts.FirstOrDefault();

                    if (contact != null)
                    {

                        await UpdateExistingContact(model, contact, tenantId,true);
                    }
                    else
                    {
                        await CreateNewContact(model, tenantId);
                    }
                }
                else
                {
                    var contact = GetAllQuerable()
                        .FirstOrDefault(c => c.ContactId == model.ContactId && c.TenantId == tenantId && c.IsDelete == false);

                    var duplicateContacts = (await GetAll())
                        .Where(c => c.TenantId == tenantId && c.IsDelete == false && c.ContactId != model.ContactId)
                        .ToList();
  

                    // Validate at least one contact field
                    if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.PhoneNumber))
                    {
                        responseVm.Message = "Email or Phone Number — please fill at least one";
                        return responseVm;
                    }

                    // Validate duplicates based on available fields
                    if (!string.IsNullOrEmpty(model.Email) && duplicateContacts.Any(c => c.Email == model.Email))
                    {
                        responseVm.Message = $"Email {model.Email} {AppConstant.AlreadyExists}";
                        return responseVm;
                    }

                    if (!string.IsNullOrEmpty(model.PhoneNumber) && duplicateContacts.Any(c => c.PhoneNumber == model.PhoneNumber))
                    {
                        responseVm.Message = $"Phone {model.PhoneNumber} {AppConstant.AlreadyExists}";
                        return responseVm;
                    }

                    //if (duplicateContacts.Any())
                    //{
                    //    responseVm.Message = "Email" + AppConstant.AlreadyExists;
                    //    return responseVm;
                    //}

                    if (!model.IsTaxExemption && (model.TaxExemptionVm?.EffectiveDate == null || model.TaxExemptionVm?.ExpiryDate == null))
                    {
                        responseVm.Message = "Please Fill TaxExemption Details.";
                        return responseVm;
                    }

                    if (model.TaxExemptionVm != null && model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                    {
                        responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                        return responseVm;
                    }

                    if (contact != null)
                    {
                        await UpdateExistingContact(model, contact, tenantId,IsGHLContact);
                        if (!model.IsTaxExemption)
                        {
                            await HandleTaxExemption(model, contact);
                        }
                        await SyncWithGHLIfConfigured(model, contact, tenantId);
                    }
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

        private async Task UpdateExistingContact(UpdateContact model, TblContact contact, Guid tenantId, bool? IsGHLContact)
        {
            if ((bool)!IsGHLContact)
            {
                contact.ContactType = model.ContactType ?? string.Empty;
                contact.FirstName = model.FirstName ?? string.Empty;
                contact.LastName = model.LastName ?? string.Empty;
                contact.Suffix = model.Suffix ?? string.Empty;
                contact.PhoneNumber = model.PhoneNumber ?? string.Empty;
                contact.Email = model.Email ?? string.Empty;
                contact.Website = model.Website ?? string.Empty;
                contact.Notes = model.Notes ?? string.Empty;
                contact.Referral = model.Referral ?? string.Empty;
                contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                contact.ModifiedBy = tenantId;
                contact.UpdatedAt = DateTime.Now;

                await Update(contact);
            }
            else
            {
                contact.ContactType = string.IsNullOrEmpty(contact.ContactType) ? (model.ContactType ?? string.Empty) : contact.ContactType;
                contact.FirstName = model.FirstName ?? string.Empty;
                contact.LastName = model.LastName ?? string.Empty;
                contact.Suffix = string.IsNullOrEmpty(contact.Suffix) ? (model.Suffix ?? string.Empty) : contact.Suffix;
                contact.PhoneNumber = model.PhoneNumber ?? string.Empty;
                contact.Email = model.Email ?? string.Empty;
                contact.Website = string.IsNullOrEmpty(contact.Website) ? (model.Website ?? string.Empty) : contact.Website;
                contact.Notes = string.IsNullOrEmpty(contact.Notes) ? (model.Notes ?? string.Empty) : contact.Notes;
                contact.Referral = string.IsNullOrEmpty(contact.Referral) ? (model.Referral ?? string.Empty) : contact.Referral;
                contact.IsTaxExemption = model.IsTaxExemption;
                contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                contact.ModifiedBy = tenantId;
                contact.UpdatedAt = DateTime.Now;
                contact.LastSync = DateTime.Now;
                contact.GhlcontactId = model.GHLContactId ?? string.Empty;

                await Update(contact);
            }

            if (!model.IsBillingAddressSameAsAddress)
                await UpdateOrCreateBillingAddress(model.BillingAddressVm, contact, tenantId, IsGHLContact);
        }

        private async Task UpdateOrCreateBillingAddress(BillingAddressVm billingVm, TblContact contact, Guid tenantId,bool? IsGHLContact)
        {
            var stateId = await GetValidStateId(billingVm);

            var existingAddress = dbContext.TblAddresses.FirstOrDefault(x =>
                x.ContactId == contact.ContactId &&
                x.AddressId == contact.PrimaryAddress &&
                x.IsDelete == false &&
                x.TenantId == tenantId);

            if ((bool)!IsGHLContact)
            {
                if (existingAddress != null)
                {
                    existingAddress.FirstName = billingVm?.FirstName;
                    existingAddress.LastName =  billingVm?.LastName;
                    existingAddress.Phone =  billingVm?.PhoneNumber;
                    existingAddress.Email = billingVm?.Email;
                    existingAddress.City = billingVm?.City;
                    existingAddress.State = stateId;
                    existingAddress.ZipCode = billingVm?.ZipCode;
                    existingAddress.StreetAddress = billingVm?.StreetAddress;
                    existingAddress.UpdatedAt = DateTime.Now;

                    dbContext.TblAddresses.Update(existingAddress);
                }
                else
                {
                    var newAddress = new TblAddress
                    {
                        AddressId = Guid.NewGuid(),
                        ContactId = contact.ContactId,
                        FirstName = billingVm?.FirstName,
                        LastName = billingVm?.LastName,
                        Phone = billingVm?.PhoneNumber,
                        Email = billingVm?.Email,
                        City = billingVm?.City,
                        State = stateId,
                        ZipCode = billingVm?.ZipCode,
                        StreetAddress = billingVm?.StreetAddress,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = tenantId,
                    };

                    dbContext.TblAddresses.Add(newAddress);
                    contact.PrimaryAddress = newAddress.AddressId;
                    await Update(contact);
                }
            }
            else
            {
                if (existingAddress != null)
                {
                    existingAddress.FirstName = string.IsNullOrEmpty(existingAddress.FirstName) ? billingVm?.FirstName : existingAddress.FirstName;
                    existingAddress.LastName = string.IsNullOrEmpty(existingAddress.LastName) ? billingVm?.LastName : existingAddress.LastName;
                    existingAddress.Phone = string.IsNullOrEmpty(existingAddress.Phone) ? billingVm?.PhoneNumber : existingAddress.Phone;
                    existingAddress.Email = string.IsNullOrEmpty(existingAddress.Email) ? billingVm?.Email : existingAddress.Email;
                    existingAddress.City = billingVm?.City;
                    existingAddress.State = stateId;
                    existingAddress.ZipCode = billingVm?.ZipCode;
                    existingAddress.StreetAddress = billingVm?.StreetAddress;
                    existingAddress.UpdatedAt = DateTime.Now;

                    dbContext.TblAddresses.Update(existingAddress);
                }
                else
                {
                    var newAddress = new TblAddress
                    {
                        AddressId = Guid.NewGuid(),
                        ContactId = contact.ContactId,
                        FirstName = billingVm?.FirstName,
                        LastName = billingVm?.LastName,
                        Phone = billingVm?.PhoneNumber,
                        Email = billingVm?.Email,
                        City = billingVm?.City,
                        State = stateId,
                        ZipCode = billingVm?.ZipCode,
                        StreetAddress = billingVm?.StreetAddress,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = tenantId,
                    };

                    dbContext.TblAddresses.Add(newAddress);
                    contact.PrimaryAddress = newAddress.AddressId;
                    await Update(contact);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        private async Task<Guid?> GetValidStateId(BillingAddressVm billingVm)
        {
            if (billingVm?.StateId != null && billingVm.StateId != Guid.Empty)
            {
                var isValid = await dbContext.MstStates.AnyAsync(s => s.Id == billingVm.StateId);
                return isValid ? billingVm.StateId : null;
            }
            return null;
        }

        private async Task HandleTaxExemption(UpdateContact model, TblContact contact)
        {
            if (model.IsTaxExemption || model.TaxExemptionVm == null)
                return;

            var taxExemption = taxExemptionService.GetAllQuerable()
                .FirstOrDefault(x => x.ContactId == contact.ContactId && x.IsDelete == false);

            var certificateString = string.Join(", ", model.TaxExemptionVm.ExemptionCertificate);

            if (taxExemption != null)
            {
                taxExemption.ExemptionCertificate = certificateString;
                taxExemption.EffectiveDate = model.TaxExemptionVm.EffectiveDate;
                taxExemption.ExpiryDate = model.TaxExemptionVm.ExpiryDate;
                taxExemption.UpdatedAt = DateTime.Now;
                taxExemption.IsActive = true;
                taxExemption.IsDelete = false;
                await taxExemptionService.Update(taxExemption);
            }
            else
            {
                var newTaxExemption = new TblTaxExemption
                {
                    ContactId = contact.ContactId,
                    ExemptionCertificate = certificateString,
                    EffectiveDate = model.TaxExemptionVm.EffectiveDate,
                    ExpiryDate = model.TaxExemptionVm.ExpiryDate,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    IsDelete = false
                };
                await taxExemptionService.Add(newTaxExemption);
            }
        }

        private async Task SyncWithGHLIfConfigured(UpdateContact model, TblContact contact, Guid tenantId)
        {
            var ghlContact = new Sp_Contact
            {
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
                BillingAddressVm = !model.IsBillingAddressSameAsAddress ? model.BillingAddressVm : null
            };

            var ghlCheck = await gHLConfigurationService.CheckGHLConfiguration(tenantId);
            if (ghlCheck.IsSuccess)
            {
                await SyncWithGHL(contact, ghlContact);
            }
            else
            {
                FileLogger.Log(ghlCheck.Message);
            }
        }

        private async Task CreateNewContact(UpdateContact model, Guid tenantId)
        {
            var stateId = Guid.Empty;
            if (model.BillingAddressVm != null)
            {
                stateId = dbContext.MstStates
                 .Where(x => x.StateCode.ToLower() == model.BillingAddressVm.StateCode.ToLower().Trim()
                          || x.StateName.ToLower() == model.BillingAddressVm.StateCode.ToLower().Trim())
                 .Select(y => y.Id)
                 .FirstOrDefault();

                // If stateId is not found, it will be Guid.Empty
                model.BillingAddressVm.StateId = stateId != Guid.Empty ? stateId : Guid.Empty;

            }

           
            var newContact = new Sp_Contact
            {
                ContactType = model.ContactType ?? string.Empty,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                Suffix = model.Suffix ?? string.Empty,
                Email = model.Email ?? string.Empty,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                lastAsync = DateTime.Now,
                GHLContactId = model.GHLContactId,
                IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                BillingAddressVm = !model.IsBillingAddressSameAsAddress ? model.BillingAddressVm : null,
                IsTaxExemption = model.IsTaxExemption,
             //   TaxExemptionVm = !model.IsTaxExemption ? model.TaxExemptionVm : null,
            };

            await Create_Contact(newContact, tenantId);
        }

        public async Task<IResponseVm<string>> ActiveInActiveContact(Guid ContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId.ToString().ToLower() == ContactId.ToString().ToLower()
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false).FirstOrDefault();
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
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false).FirstOrDefault();
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
        //        ////     c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false ).ToList();
        //        //var contact = GetAllQuerable()
        //        //    .Where(c =>c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false).ToList();


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

                var query = GetAllQuerable().Where(c => c.IsActive == false && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false);
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

        public async Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter, bool Role, bool? IsArchive)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var IsSubtenants = false;
                var Contacts = new List<TblContact>();
                List<ContactsList> ContactList = new List<ContactsList>();
                if (filter?.TenantId != "" && Role)
                {
                    
                    var IsRealtedTenant = tenantService.GetAllQuerable().Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                    if (IsRealtedTenant == null)
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        return responseVm;
                    }
                    else
                    {
                        Contacts = (await GetAll()).Where(x => ( x.IsDelete == false && (IsArchive == true ?  x.IsArchive == true || x.IsActive==false : x.IsArchive == false) ) && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x => x.CreatedAt).ToList();
                        IsSubtenants = true;
                    }
                }
                else
                {
                    Contacts = (await GetAll()).Where(x => ( x.IsDelete == false && (IsArchive == true ?  x.IsArchive == true || x.IsActive==false : x.IsArchive == false) ) && x.TenantId == claimsProperty.GetTenantId()).OrderByDescending(x => x.CreatedAt).ToList();
                }

                if (!string.IsNullOrEmpty(filter?.CustomerId))
                {
                    Contacts = Contacts.Where(x => x.ContactId.ToString() == filter.CustomerId).ToList();
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
                var filteredRecords = ContactList
                                    .Where(x =>
                                    (filter?.Status == 0 ? x.IsActive == false : (filter?.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter?.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter?.SearchValue)
                                    || (x.ContactId.ToString().ToLower()).Contains(filter?.SearchValue)
                                    || (x.Email.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.Website.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.PhoneNumber ?? "").ToLower().Contains(filter?.SearchValue)
                                    || (x.ContactType ?? "").ToLower().Contains(filter?.SearchValue)
                                    || (x.ContactUniqueId ?? "").ToLower().Contains(filter?.SearchValue)
                                    || ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter?.SearchValue)
                                    || (x.FirstName.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.LastName.ToLower() ?? "").Contains(filter?.SearchValue)
                                    ) : true)).AsQueryable();





                filteredRecords = Paginate.ApplyColumnFilters(filteredRecords, filter.ColumnFilters);
                filteredRecords = Paginate.ApplySorting(filteredRecords, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(filteredRecords, filter);
                

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Data = TakenRecords.ToList();
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = filteredRecords.Count();
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

        public async Task<IResponseVm<string>> SyncWithGHL(TblContact contact, Sp_Contact model, Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

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
                    Referral = model.Referral ?? string.Empty,
                    StreetAddress = model?.BillingAddressVm?.StreetAddress ?? string.Empty,
                    City = model?.BillingAddressVm?.City ?? string.Empty,
                    State = dbContext.MstStates.Where(x=>x.Id== model.BillingAddressVm.StateId).Select(s=>s.StateName).FirstOrDefault() ,
                    ZipCode = model?.BillingAddressVm?.ZipCode ?? string.Empty,
                };

                if (string.IsNullOrEmpty(contact.GhlcontactId))
                {
                    var ghlconfigdetails =  await gHLConfigurationService.GetGHLConfiguration(LoggedInTenantId.ToString());
                    ghlResponse = await gHLService_V2.CreateContactAsync(ghlconfigdetails?.Data,ghlContactAdd);
                   
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Contact synced successfully with GHL.";
                }
                else
                {
                    var ghlconfigdetails =await gHLConfigurationService.GetGHLConfiguration(LoggedInTenantId.ToString());
                    ghlResponse = await gHLService_V2.UpdateContactAsync(ghlconfigdetails?.Data,contact.GhlcontactId, ghlContactAdd);
                }
                if (ghlResponse.IsSuccess)
                {
                    //var contactExist = (await GetById(contact.ContactId));
                    var contactExist = contact;

                    if (contactExist != null && contactExist.IsDelete == false)
                    {
                        contactExist.GhlcontactId = ghlResponse?.Data?.Contact?.id;
                        contactExist.LastSync = DateTime.Now;
                    }
                    var res = await Update(contactExist);
                //    responseVm.IsSuccess = true;
                //    responseVm.Message = "Contact synced successfully with GHL.";
                //}
                //if (ghlResponse != null)
                //{
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
                    bool IsbillingAddressSameAsAddress = true;
                    if (!string.IsNullOrEmpty(contactData.full_address))
                    {
                        IsbillingAddressSameAsAddress = false;
                    }
                    var contact = new Sp_Contact
                    {
                        ContactType = contactData.contact_type ?? "",
                        FirstName = contactData.first_name ?? "",
                        LastName = contactData.last_name ?? "",
                        Email = contactData.email ?? "",
                        PhoneNumber = contactData.phone ?? "",
                      //  locationId = contactData?.location?.id ?? "",
                        GHLContactId = contactData?.contact_id ?? "",
                        lastAsync = DateTime.Now,
                        IsBillingAddressSameAsAddress = IsbillingAddressSameAsAddress,
                        IsTaxExemption = true,
                    };
                    if (!string.IsNullOrEmpty(contactData.full_address))
                    {
                        var stateId = dbContext.MstStates.Where(x => x.StateCode == contactData.state).FirstOrDefault();
                        contact.BillingAddressVm = new BillingAddressVm
                        {
                            BillingFirstName = contactData.first_name,
                            BillingLastName = contactData.last_name,
                            FirstName = contactData.first_name,
                            LastName = contactData.last_name,
                            Email = contactData.email,
                            PhoneNumber = contactData.phone,
                            City = contactData.city,
                            StateCode = contactData.state ?? "",
                            ZipCode = contactData.postal_code,
                            StreetAddress = contactData.StreetAddress == null ? contactData.address1 : contactData.StreetAddress
                        };

                    }
                       await gHLConfigurationService.GetGHLConfiguration(contactData.WebhookTenantId);
                        var res = await Create_Contact(contact, Guid.Parse(contactData.WebhookTenantId),true);
                        responseVm.IsSuccess = res.IsSuccess;
                        responseVm.Message = res.Message;
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
                    bool IsbillingAddressSameAsAddress = true;
                    if (!string.IsNullOrEmpty(contactData.full_address))
                    {
                        IsbillingAddressSameAsAddress = false;
                    }
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
                        IsBillingAddressSameAsAddress = IsbillingAddressSameAsAddress,
                        IsTaxExemption = true,
                    };
                    if (!string.IsNullOrEmpty(contactData.full_address))
                    {
                        var stateId = dbContext.MstStates.Where(x => x.StateCode == contactData.state).FirstOrDefault();
                        contact.BillingAddressVm = new BillingAddressVm
                        {
                            BillingFirstName = contactData.first_name,
                            BillingLastName = contactData.last_name,
                            FirstName = contactData.first_name,
                            LastName = contactData.last_name,
                            Email = contactData.email,
                            PhoneNumber = contactData.phone,
                            City = contactData.city,
                            StateCode = contactData.state ?? "",
                            ZipCode = contactData.postal_code,
                            StreetAddress = contactData.StreetAddress == null ? contactData.address1 : contactData.StreetAddress
                        };
                    }
                        //await gHLConfigurationService.GetGHLConfiguration(contactData.WebhookTenantId);
                    var res = await Update_Contact(contact, Guid.Parse(contactData.WebhookTenantId),true);
                    //var AllSubetenants = tenantService.GetAllQuerable().Where(x => x.ParentTenantId.ToString() ==
                    //contactData.WebhookTenantId && x.IsActive == true && x.IsDelete == false && x.IsGhlconfigSameAsTenant == true)
                    //    .ToList();
                    //if (AllSubetenants != null && AllSubetenants.Count > 0)
                    //{
                    //    foreach (var partner in AllSubetenants)
                    //    {
                    //        if (!string.IsNullOrEmpty(partner.GhluserId) && partner.GhluserId.ToLower() == ghlUser.GhlUserId.ToLower())
                    //        {
                    //            await gHLConfigurationService.GetGHLConfiguration(contactData.WebhookTenantId);
                    //            var res1 = await Update_Contact(contact, Guid.Parse(contactData.WebhookTenantId));
                    //        }
                    //    }
                    //}
                  
                        responseVm.IsSuccess = res.IsSuccess;
                        responseVm.Message = res.Message;
                        return responseVm;
                    
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }

        private string ToTitleCaseSafe(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        }

        public async Task<IResponseVm<string>> ContactSync(Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                TextInfo textInfo;
                var configDetails = await gHLConfigurationService.GetGHLConfiguration(LoggedInTenantId.ToString());
                var Checkghlresponse = await gHLConfigurationService.CheckGHLConfiguration(LoggedInTenantId);
                if (Checkghlresponse.IsSuccess)
                {
                    var getGHLContacts = await gHLService_V2.GetContactsAsync(configDetails.Data);
                    if (getGHLContacts != null)
                    {
                        FileLogger.Log("GHL", getGHLContacts.Count());
                    }
                    else
                    {
                        FileLogger.Log("Exception occurred GHLContact is null.");
                    }
                    //Before TanentId is here
                    var getAppContacts = (await GetAll()).Where(x => x.TenantId == LoggedInTenantId && x.IsDelete == false && x.IsArchive ==false).ToList();

                    List<Sp_Contact> contactsToInsert = new List<Sp_Contact>();
                    List<GhlContactAdd> contactsToAddToGHL = new List<GhlContactAdd>();
                    List<GhlContactAdd> contactsUpdateToGHL = new List<GhlContactAdd>();
                    foreach (var ghlcontact in getGHLContacts)
                    {
                        var existingContact = getAppContacts.FirstOrDefault(x => x.GhlcontactId == ghlcontact?.id &&
                        x.TenantId == LoggedInTenantId && x.IsArchive == false && x.IsActive == true
                        && x.IsDelete == false
                      );
                        var IsContactExists = GetAllQuerable()
                          .Where(c => (c.Email == ghlcontact.email || c.PhoneNumber == ghlcontact.phone)
                                      && c.TenantId == LoggedInTenantId && c.IsDelete == false).OrderByDescending(x => x.CreatedAt)
                          .FirstOrDefault();

                        existingContact = IsContactExists != null ? IsContactExists : null;

                        if (existingContact != null)
                        {
                            if (ghlcontact.dateUpdated.ToLocalTime() > existingContact.UpdatedAt)
                            {
                                

                                existingContact.FirstName = ToTitleCaseSafe(ghlcontact.firstNameLowerCase);

                                existingContact.LastName = ToTitleCaseSafe(ghlcontact.firstNameLowerCase);

                                existingContact.Email = ghlcontact.email??"";
                                existingContact.PhoneNumber = ghlcontact.phone??"";
                                existingContact.UpdatedAt = DateTime.Now;
                                existingContact.ModifiedBy = AppConstant.LoggedInUserID;
                                existingContact.LastSync = DateTime.Now;
                                existingContact.ContactType = ghlcontact.type ?? "";
                                existingContact.GhlcontactId = ghlcontact.id??"";
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
                                var res = await gHLService_V2.UpdateContactAsync(configDetails?.Data,ghlUpdatecontact.GHLContactId, ghlUpdatecontact);
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
                                .Where(x => x.TenantId == LoggedInTenantId && x.IsDelete == false)
                                .Select(x => x.BrandingInfo)
                                .FirstOrDefault();

                            var brandingInfo = tenantBranding.GetAllQuerable()
                                .Where(x => x.Id == tenant && x.IsDelete == false)
                                .FirstOrDefault();

                            var newContact = new Sp_Contact
                            {
                                ContactType = ghlcontact?.type??"",
                                FirstName = ToTitleCaseSafe(ghlcontact?.firstNameLowerCase??"")??"",
                                LastName = ToTitleCaseSafe(ghlcontact?.lastNameLowerCase ?? "")??"",
                                Email = ghlcontact?.email??"",
                                PhoneNumber = ghlcontact?.phone??"",
                                //locationId = ghlcontact.locationId,
                                GHLContactId = ghlcontact?.id??"",
                                lastAsync = DateTime.Now,
                            };

                            
                            newContact.IsBillingAddressSameAsAddress = true;
                            newContact.IsTaxExemption = true;
                           var contactaddRespnse= await Create_Contact(newContact,LoggedInTenantId);
                            if(contactaddRespnse.IsSuccess)
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = contactaddRespnse.Message;
                            }
                            else
                            {
                                responseVm.Message = contactaddRespnse.Message;

                            }
                           
                            //contactsToInsert.Add(newContact);
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
                                if (existingGHLContact.dateUpdated.ToLocalTime() < appContact.UpdatedAt)
                                {
                                    var newContactToAddToGHL = new GhlContactAdd
                                    {
                                        FirstName = ToTitleCaseSafe(existingGHLContact.firstNameLowerCase),
                                        LastName = ToTitleCaseSafe(existingGHLContact.lastNameLowerCase),
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
                                        FirstName = ToTitleCaseSafe(existingGHLContact.firstNameLowerCase),
                                        LastName = ToTitleCaseSafe(existingGHLContact.lastNameLowerCase),
                                        Email = existingGHLContact.email,
                                        PhoneNumber = existingGHLContact.phone,
                                        ContactType = existingGHLContact.type,
                                        GHLContactId = existingGHLContact.id,
                                        lastAsync = DateTime.Now
                                    };
                                    FileLogger.Log("Updating Contact in application(Application ContactId).", updatecontact.GHLContactId);
                                    var res = await Update_Contact(updatecontact,LoggedInTenantId);
                                    if (!res.IsSuccess )
                                    { FileLogger.Log("Exception occurred Updating Contact in application.", res.Message); }
                                }
                            }
                            else
                            { 
                                FileLogger.Log("this contact deleted on GHL but in shutter ", appContact.ContactId);
                            }
                        }
                    }
                   
                    if (contactsToAddToGHL.Any())
                    {
                        foreach (var contact in contactsToAddToGHL)
                        {
                            if (string.IsNullOrEmpty(contact?.GHLContactId))
                            {

                                FileLogger.Log("Creating bulk Contact in GHL(GhlContactId).", contact);
                                var res = await gHLService_V2.CreateContactAsync(configDetails?.Data,contact);
                                if (res.IsSuccess)
                                {
                                    var contactExist = (await GetAll()).Where(x => x.Email == contact.Email && x.TenantId==LoggedInTenantId).FirstOrDefault();
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
                                var res = await gHLService_V2.UpdateContactAsync(configDetails?.Data,contact.GHLContactId, contact);
                                if (!res.IsSuccess)
                                { FileLogger.Log("Updating Contact in GHL.", contact.GHLContactId); }

                            }
                        }
                    }
                    var isUpdated = await gHLService_V2.UpdateLastSyncConfigurationAsync(LoggedInTenantId);

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

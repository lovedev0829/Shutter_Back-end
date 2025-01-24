using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core.SPModel.SP_Responses;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Azure;
using ShutterManagement.Core.ViewModels.Response;
using System.Data.Entity;
using System.Diagnostics.Metrics;
using ShutterManagement.Core.Common;
using System.ComponentModel.DataAnnotations;
using AutoMapper.Internal;

namespace ShutterManagement.Business.Services
{

    public class ImportandExportCSV
    {
        private readonly BtprojecShuttermanagementDevContext dbContext;
        private readonly IVendorService vendorService;
        private readonly IMaterial material;
        private readonly IContactService contactService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IUnitofMeasurementService unitofMeasurement;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly IContactBillingService billingService;
        private readonly IEstimateService estimateService;
        private readonly IEstimationBreakdownService estimationBreakdownService;
        private readonly IMstProductType productType;

        public ImportandExportCSV(BtprojecShuttermanagementDevContext context, IVendorService vendorService,
            IMaterial material, IContactService contactService, ClaimsProperty claimsProperty,
            IUnitofMeasurementService unitofMeasurement, ITenantService tenantService,
            ITaxExemption taxExemptionService, IContactBillingService billingService,
            BtprojecShuttermanagementDevContext btprojecShuttermanagementDevContext , 
            IEstimateService estimateService,IEstimationBreakdownService estimationBreakdownService)
        {
            dbContext = context;
            this.vendorService = vendorService;
            this.material = material;
            this.contactService = contactService;
            this.claimsProperty = claimsProperty;
            this.unitofMeasurement = unitofMeasurement;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.billingService = billingService;
            this.estimateService = estimateService;
            this.estimationBreakdownService = estimationBreakdownService;
            this.productType = productType;
        }

        public async Task<IResponseVm<List<string>>> ImportCsvToDatabase(Stream csvStream)
        {
            IResponseVm<List<string>> responseVm = new ResponseVm<List<string>>();
            List<string> validationErrors = new List<string>();
            List<CreateMaterial> validMaterials = new List<CreateMaterial>();
            int successfulImports = 0;
            int totalRecords = 0;

            var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
            if (!isTenantExists)
            {
                responseVm.Message = AppConstant.TenantNotFound;
                return responseVm;
            }

            try
            {
                using (var reader = new StreamReader(csvStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    //var records = csv.GetRecords<ImportMaterial>().ToList();
                    var expectedHeaders = new[]
                   {
                   "MaterialName", "UnitCost", "UnitOfMeasurement", "StockQuantity", "ReorderLevel",
                   "VendorCode", "Notes", "ProductType", "SKU", "IsCutSheet"
                    };

                    csv.Read();
                    csv.ReadHeader();
                    var actualHeaders = csv.HeaderRecord;

                    if (!expectedHeaders.SequenceEqual(actualHeaders, StringComparer.InvariantCultureIgnoreCase))
                    {
                        responseVm.Message = "The CSV file header format is  incorrect. Please export format";
                        return responseVm;
                    }
                    var records = csv.GetRecords<ImportMaterial>().ToList();

                    totalRecords = records.Count;
                    if (totalRecords == 0)
                    {
                        responseVm.Message = "No records found in the CSV.";
                        return responseVm;
                    }

                    int rowNumber = 1;
                    bool hasValidationError = false;

                    foreach (var record in records)
                    {
                        if (string.IsNullOrEmpty(record.VendorCode))
                        {
                            validationErrors.Add($"Error in row {rowNumber}: VendorCode is required.");
                            hasValidationError = true;
                            continue;
                        }

                        var vendor = vendorService.GetAllQuerable()
                            .Where(v => v.VendorCode == record.VendorCode && v.IsDelete == false)
                            .FirstOrDefault();

                        if (vendor == null)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: Vendor  Code Required");
                            hasValidationError = true;
                            continue;
                        }

                        var productType = dbContext.MstProductTypes
                            .Where(p => p.ProductName == record.ProductType && p.IsDelete == false)
                            .FirstOrDefault();

                        if (productType == null)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: ProductType '{record.ProductType}' is required.");
                            hasValidationError = true;
                            continue;
                        }

                        var unitOfMeasurement = unitofMeasurement.GetAllQuerable()
                            .Where(u => u.IsDelete == false)
                            .ToList();

                        bool unitFound = false;

                        foreach (var unitOfMeasurementExist in unitOfMeasurement)
                        {
                            if (unitOfMeasurementExist.UnitofMeasurementName == record.UnitOfMeasurement?.Trim())
                            {
                                unitFound = true;
                                break;
                            }
                        }

                        if (!unitFound)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: UnitOfMeasurement '{record.UnitOfMeasurement}' is Not Valid.");
                            hasValidationError = true;
                            continue;

                        }

                        var materialModel = new CreateMaterial
                        {
                            SKU = record.SKU?.Trim(),
                            MaterialName = record.MaterialName?.Trim(),
                            UnitCost = record.UnitCost,
                            UnitOfMeasurement = record.UnitOfMeasurement,
                            StockQuantity = record.StockQuantity,
                            ReorderLevel = record.ReorderLevel,
                            IsCutSheet = record.IsCutSheet,
                            VendorId = vendor.VendorId,
                            Notes = record.Notes,
                            ProductType = productType.ProductId.ToString(),
                        };

                        var existingMaterialdata = await material.Validate_MaterialExcell(record);
                        if (existingMaterialdata.Message != null)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: {existingMaterialdata.Message}");
                            hasValidationError = true;
                        }
                        else
                        {
                            validMaterials.Add(materialModel);
                        }

                        rowNumber++;
                    }

                    if (hasValidationError)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Validation errors occurred during the import.";
                        responseVm.Data = validationErrors;
                        responseVm.TotalRecords = validMaterials.Count;
                    }
                    else
                    {
                        int successfulInserts = 0;

                        foreach (var validMaterial in validMaterials)
                        {
                            var createMaterialResponse = await material.Create_Material(validMaterial);
                            if (createMaterialResponse.IsSuccess)
                            {
                                successfulInserts++;
                            }
                            else
                            {
                                validationErrors.Add($"Error inserting material: {createMaterialResponse.Message}");
                            }
                        }

                        if (validationErrors.Any())
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = "Some materials failed to be inserted.";
                            responseVm.Data = validationErrors;
                            responseVm.TotalRecords = successfulInserts;
                        }
                        else
                        {
                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UploadSuccessMsg;
                            responseVm.TotalRecords = successfulInserts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }


        public async Task<IResponseVm<List<string>>> ImportCsvForContact(Stream csvStream)
        {
            IResponseVm<List<string>> responseVm = new ResponseVm<List<string>>();
            List<string> validationErrors = new List<string>();
            List<Sp_Contact> validContacts = new List<Sp_Contact>();
            int successfulImports = 0;
            int totalRecords = 0;

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                using (var reader = new StreamReader(csvStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var expectedHeaders = new[]
                    {
                   "ContactType", "FirstName","LastName", "PhoneNumber", "Email", "Website",
                   "Notes", "Referral", "BillingFirstName","BillingLastName", "PhoneNumber", "Email","City","State","ZipCode","StreetAddress","EffectiveDate","ExpiryDate"
                    };

                    csv.Read();
                    csv.ReadHeader();
                    var actualHeaders = csv.HeaderRecord;

                    if (!expectedHeaders.SequenceEqual(actualHeaders, StringComparer.InvariantCultureIgnoreCase))
                    {
                        responseVm.Message = "The CSV file header format is  incorrect. Please export format";
                        return responseVm;
                    }

                    var records = csv.GetRecords<ImportContact>().ToList();
                    totalRecords = records.Count;
                    int rowNumber = 1;
                    bool hasValidationError = false;

                    foreach (var record in records)
                    {
                        var contactModel = MapToSpContact(record);

                        var validationError = await ValidateContactAsync(contactModel, rowNumber);
                        if (validationError != null)
                        {
                            validationErrors.Add(validationError);
                            hasValidationError = true;
                        }
                        else
                        {
                            validContacts.Add(contactModel);
                        }

                        rowNumber++;
                    }

                    if (hasValidationError)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Validation errors occurred during the import.";
                        responseVm.Data = validationErrors;
                        responseVm.TotalRecords = validContacts.Count;
                    }
                    else
                    {
                        successfulImports = await InsertValidContacts(validContacts, validationErrors);

                        if (validationErrors.Any())
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = "Some contacts failed to be inserted.";
                            responseVm.Data = validationErrors;
                            responseVm.TotalRecords = successfulImports;
                        }
                        else
                        {
                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UploadSuccessMsg;
                            responseVm.TotalRecords = successfulImports;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        private Sp_Contact MapToSpContact(ImportContact record)
        {
            var contactModel = new Sp_Contact
            {
                ContactType = record.ContactType?.Trim(),
                FirstName = record.FirstName?.Trim(),
                LastName = record.LastName?.Trim(),
                PhoneNumber = record.PhoneNumber.Trim(),
                Email = record.Email?.Trim(),
                Website = record.Website.Trim(),
                Notes = record.Notes,
                Referral = record.Referral.Trim(),
                IsBillingAddressSameAsAddress = false,
            };

            if (record.BillingAddressVm != null)
            {
                contactModel.BillingAddressVm = new BillingAddressVm
                {
                    FirstName = record.BillingAddressVm.BillingFirstName?.Trim(),
                    LastName = record.BillingAddressVm.BillingLastName?.Trim(),
                    PhoneNumber = record.BillingAddressVm.PhoneNumber?.Trim(),
                    Email = record.BillingAddressVm.Email?.Trim(),
                    StreetAddress = record.BillingAddressVm.StreetAddress?.Trim(),
                    ZipCode = record.BillingAddressVm.ZipCode?.Trim(),
                    City = record.BillingAddressVm.City?.Trim(),
                    State = record.BillingAddressVm.State?.Trim()
                };
            }

            if (record.TaxExemptionVm != null)
            {
                contactModel.TaxExemptionVm = new TaxExemption
                {
                    ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate,
                    EffectiveDate = record.TaxExemptionVm.EffectiveDate,
                    ExpiryDate = record.TaxExemptionVm?.ExpiryDate
                };
            }

            return contactModel;
        }

        private async Task<string> ValidateContactAsync(Sp_Contact contactModel, int rowNumber)
        {
            var existingContact = await contactService.GetContactByEmailOrPhone(contactModel.Email, contactModel.PhoneNumber);
            if (existingContact.Message != null)
            {
                return $"Error in row {rowNumber}: {existingContact.Message}";
            }
            return null;
        }

        private async Task<int> InsertValidContacts(List<Sp_Contact> validContacts, List<string> validationErrors)
        {
            int successfulImports = 0;

            foreach (var contactToInsert in validContacts)
            {
                var createContactResponse = await contactService.Create_Contact(contactToInsert);

                if (createContactResponse.IsSuccess)
                {
                    successfulImports++;
                }
                else
                {
                    validationErrors.Add($"Error with contact: {createContactResponse.Message}");
                }
            }

            return successfulImports;
        }



        //public async Task<IResponseVm<dynamic>> ExportContactsToCsvAsync(string filePath)
        //{
        //    IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
        //    try
        //    {
        //        var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
        //        if (IsTenantExists)
        //        {
        //            var Contacts = new List<TblContact>();
        //            List<ContactsList> ContactList = new List<ContactsList>();

        //            Contacts = (await contactService.GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();

        //            foreach (var contact in Contacts)
        //            {
        //                ContactsList contacts = new ContactsList
        //                {
        //                    ContactId = contact.ContactId,
        //                    ContactType = contact.ContactType ?? "",
        //                    Email = contact.Email,
        //                    PhoneNumber = contact.PhoneNumber,
        //                    Name = contact.Name,
        //                    Website = contact.Website,
        //                    Notes = contact.Notes,
        //                    Referral = contact.Referral,
        //                    IsTaxExemption = contact.IsTaxExemption ?? false,
        //                    IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
        //                    IsActive = contact.IsActive
        //                };

        //                contacts.billingAddressVm = billingService.GetAllQuerable()
        //                    .Where(x => x.ContactId == contact.ContactId
        //                    && x.IsDelete == false).Select(x => new BillingAddressVm
        //                    {
        //                        City = x.City,
        //                        Email = x.Email,
        //                        FullName = x.FullName,
        //                        PhoneNumber = x.PhoneNumber,
        //                        State = x.State,
        //                        StreetAddress = x.StreetAddress,
        //                        ZipCode = x.ZipCode,
        //                    }).FirstOrDefault();
        //                contacts.taxExemptionVm = taxExemptionService.GetAllQuerable()
        //                    .Where(x => x.ContactId == contact.ContactId
        //                    && x.IsDelete == false).Select(x => new TaxExemption
        //                    {
        //                        EffectiveDate = x.EffectiveDate,
        //                        ExemptionCertificate = x.ExemptionCertificate.Split(new[] {","},StringSplitOptions.None),
        //                        ExpiryDate = x.ExpiryDate
        //                    }).FirstOrDefault();
        //                ContactList.Add(contacts);
        //            }
        //            var csvBuilder = new StringBuilder();
        //            csvBuilder.AppendLine("ContactType,Name,PhoneNumber,Email,Website,Notes,Referral,FullName,PhoneNumber,Email,City,State,ZipCode,StreetAddress,ExemptionCertificate,EffectiveDate,ExpiryDate");


        //            foreach (var contact in ContactList)
        //            {
        //                var billingAddress = contact.billingAddressVm ?? new BillingAddressVm();


        //                var line = $"{contact.ContactType},{contact.Name},{contact.PhoneNumber},{contact.Email},{contact.Website},{contact.Notes},{contact.Referral}," +
        //                           $"{billingAddress.FullName},{billingAddress.PhoneNumber},{billingAddress.Email},{billingAddress.City},{billingAddress.State}," +
        //                           $"{billingAddress.ZipCode},{billingAddress.StreetAddress},{contact.taxExemptionVm?.ExemptionCertificate},{contact.taxExemptionVm?.EffectiveDate},{contact.taxExemptionVm?.ExpiryDate}";

        //                csvBuilder.AppendLine(line);
        //            }

        //            try
        //            {
        //                await File.WriteAllTextAsync(filePath, csvBuilder.ToString());
        //                responseVm.IsSuccess = true;
        //                responseVm.Message = AppConstant.ExportSuccessMsg; 
        //            }
        //            catch (Exception ex)
        //            {
        //                responseVm.IsSuccess = false;
        //                responseVm.Message = AppConstant.ExceptionError + ex.Message; 
        //            }

        //            return responseVm;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message; 
        //        return responseVm;
        //    }
        //    return responseVm;
        //}

        #region Start Export ExportContactsToCsv

        public async Task<IResponseVm<string>> ExportContactsToCsvAsync(Pagination filter, string filePath)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

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
                        Contacts = (await contactService.GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x => x.CreatedAt).ToList();
                    }
                }
                else
                {
                    Contacts = (await contactService.GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                }

                if (filter.Status == 0)
                {
                    Contacts = Contacts.Where(x => x.IsActive == false).ToList();
                }
                else if (filter.Status == 1)
                {
                    Contacts = Contacts.Where(x => x.IsActive == true).ToList();
                }

                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    var searchValue = filter.SearchValue.Trim().ToLower();
                    Contacts = Contacts.Where(x =>
                        (x.Notes ?? "").ToLower().Contains(searchValue) ||
                        x.ContactId.ToString().ToLower().Contains(searchValue) ||
                        (x.Email ?? "").ToLower().Contains(searchValue) ||
                        (x.Website ?? "").ToLower().Contains(searchValue) ||
                        (x.PhoneNumber ?? "").ToLower().Contains(searchValue) ||
                        (x.Name ?? "").ToLower().Contains(searchValue)).ToList();
                }

                var contactsList = Contacts
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(contact => new
                    {
                        contact.ContactId,
                        contact.ContactType,
                        contact.Email,
                        contact.PhoneNumber,
                        contact.FirstName,
                        contact.LastName,
                        contact.Website,
                        contact.Notes,
                        contact.Referral,
                        contact.IsTaxExemption,
                        contact.IsBillingAddressSameAsAddress,
                        contact.IsActive
                    }).ToList();

                var detailedContactList = new List<ContactsList>();
                foreach (var contact in contactsList)
                {
                    var billingAddress = billingService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId && !x.IsDelete)
                        .Select(x => new BillingAddressVm
                        {
                            City = x.City,
                            Email = x.Email,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            PhoneNumber = x.PhoneNumber,
                            State = x.State,
                            StreetAddress = x.StreetAddress,
                            ZipCode = x.ZipCode,
                        }).FirstOrDefault();

                    var taxExemption = taxExemptionService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId && x.IsDelete == false)
                        .Select(x => new TaxExemption
                        {
                            EffectiveDate = x.EffectiveDate,
                            ExemptionCertificate = string.IsNullOrEmpty(x.ExemptionCertificate)
                                ? null
                                : x.ExemptionCertificate.Split(new[] { ',' }, StringSplitOptions.None),
                            ExpiryDate = x.ExpiryDate
                        }).FirstOrDefault();

                    detailedContactList.Add(new ContactsList
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
                        IsTaxExemption = contact.IsTaxExemption ?? false,
                        IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                        IsActive = contact.IsActive,
                        billingAddressVm = billingAddress,
                        taxExemptionVm = taxExemption
                    });
                }

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("ContactType,FirstName,LastName,PhoneNumber,Email,Website,Notes,Referral,BillingFirstName,BillingLastName,PhoneNumber,Email,City,State,ZipCode,StreetAddress,EffectiveDate,ExpiryDate");

                foreach (var contact in detailedContactList)
                {
                    var billingAddress = contact.billingAddressVm ?? new BillingAddressVm();
                    var exemptionCertificates = contact.taxExemptionVm?.ExemptionCertificate != null
                        ? string.Join(";", contact.taxExemptionVm.ExemptionCertificate)
                        : "";

                    var line = $"{contact.ContactType},{contact.FirstName},{contact.LastName},{contact.PhoneNumber},{contact.Email},{contact.Website},{contact.Notes},{contact.Referral}," +
                               $"{billingAddress.FirstName},{billingAddress.LastName},{billingAddress.PhoneNumber},{billingAddress.Email},{billingAddress.City},{billingAddress.State}," +
                               $"{billingAddress.ZipCode},{billingAddress.StreetAddress},{contact.taxExemptionVm?.EffectiveDate},{contact.taxExemptionVm?.ExpiryDate}";

                    csvBuilder.AppendLine(line);
                }

                var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var base64Csv = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.ExportSuccessMsg;
                responseVm.Data = base64Csv;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        #endregion Start Export ExportContactsToCsv

        #region Start Export ExportMaterialsToCsv
        public async Task<IResponseVm<string>> ExportMaterialsToCsvAsync(Pagination filter, string filePath)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var materialQuery = dbContext.TblRawMaterials
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);

                if (filter.Status == 0)
                {
                    materialQuery = materialQuery.Where(x => x.IsActive == false);
                }
                else if (filter.Status == 1)
                {
                    materialQuery = materialQuery.Where(x => x.IsActive == true);
                }

                if (!string.IsNullOrEmpty(filter.Vendor))
                {
                    materialQuery = materialQuery.Where(x => x.VendorId.ToString() == filter.Vendor);
                }

                if (!string.IsNullOrEmpty(filter.ProductType))
                {
                    materialQuery = materialQuery.Where(x => x.ProductType.ToString() == filter.ProductType);
                }

                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    var searchValue = filter.SearchValue.Trim().ToLower();
                    materialQuery = materialQuery.Where(x =>
                        x.RawMaterialId.ToString() == searchValue ||
                        x.ProductType.ToString() == searchValue ||
                        (x.MaterialName ?? "").ToLower().Contains(searchValue) ||
                        (x.UnitCost.ToString() ?? "").ToLower().Contains(searchValue) ||
                        (x.ReorderLevel.ToString() ?? "").ToLower().Contains(searchValue) ||
                        (x.StockQuantity.ToString() ?? "").ToLower().Contains(searchValue) ||
                        (x.Notes ?? "").ToLower().Contains(searchValue) ||
                        (x.UnitOfMeasurement ?? "").ToLower().Contains(searchValue));
                }

                var materialList = (from t2 in materialQuery
                                    join t1 in dbContext.TblVendors on t2.VendorId equals t1.VendorId
                                    join t3 in dbContext.MstProductTypes on t2.ProductType equals t3.ProductId
                                    select new ExportMaterialsCsv
                                    {
                                        IsActive = t2.IsActive,
                                        MaterialId = t2.RawMaterialId,
                                        MaterialName = t2.MaterialName,
                                        Notes = t2.Notes,
                                        ReorderLevel = t2.ReorderLevel,
                                        SKU = t2.Sku,
                                        StockQuantity = t2.StockQuantity,
                                        UnitCost = t2.UnitCost,
                                        ProductTypeName = t3.ProductName,
                                        UnitOfMeasurement = t2.UnitOfMeasurement,
                                        VendorId = t2.VendorId,
                                        VendorName = t1.VendorName,
                                        VendorCode = t1.VendorCode,
                                        IsCutSheet = t2.IsCutSheet,
                                    }).ToList();

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("MaterialName,UnitCost,UnitOfMeasurement,StockQuantity,ReorderLevel,VendorCode,Notes,ProductType,SKU,IsCutSheet");

                foreach (var material in materialList)
                {
                    var line = $"{material.MaterialName},{material.UnitCost},{material.UnitOfMeasurement},{material.StockQuantity}," +
                               $"{material.ReorderLevel},{material.VendorCode},{material.Notes},{material.ProductTypeName},{material.SKU},{material.IsCutSheet}";

                    csvBuilder.AppendLine(line);
                }

                var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var base64Csv = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.ExportSuccessMsg;
                responseVm.Data = base64Csv;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }
        #endregion Start Export ExportMaterialsToCsvAsync

        #region Start Export ExportJobsToCsv
        public async Task<IResponseVm<string>> ExportJobsToCsv(Pagination filter, string filePath)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var subTenantlist = (await tenantService.GetAll())
                    .Where(x => x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false)
                    .Select(pt => pt.TenantId)
                    .ToList();

                var jobsQuery = dbContext.TblJobs
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId || subTenantlist.Contains((Guid)x.TenantId));

                if (filter.Status == 0)
                {
                    jobsQuery = jobsQuery.Where(x => x.IsActive == false);
                }
                else if (filter.Status == 1)
                {
                    jobsQuery = jobsQuery.Where(x => x.IsActive == true);
                }

                if (!string.IsNullOrEmpty(filter.JobStatus))
                {
                    jobsQuery = jobsQuery.Where(x => x.JobStatus.ToString() == filter.JobStatus);
                }

                if (!string.IsNullOrEmpty(filter.Address))
                {
                    jobsQuery = jobsQuery.Where(x => x.AddressId.ToString() == filter.Address);
                }

                if (!string.IsNullOrEmpty(filter.TenantId))
                {
                    Guid tenantId = Guid.Parse(filter.TenantId);
                    jobsQuery = jobsQuery.Where(x => x.TenantId == tenantId);
                }

                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    string searchValue = filter.SearchValue.Trim().ToLower();
                    jobsQuery = jobsQuery.Where(x =>
                        x.JobId.ToString() == searchValue ||
                        x.ContactId.ToString() == searchValue ||
                        x.JobStatus.ToString() == searchValue ||
                        x.AddressId.ToString() == searchValue ||
                        (x.JobName ?? "").ToLower().Contains(searchValue) ||
                        (x.JobType.ToString() ?? "").ToLower().Contains(searchValue) ||
                        (x.AssignedPersonnel.ToString() ?? "").ToLower().Contains(searchValue) ||
                        (x.Notes ?? "").ToLower().Contains(searchValue));
                }

                var jobList = (from t2 in jobsQuery
                               join t1 in dbContext.TblAddresses on t2.AddressId equals t1.AddressId
                               join t3 in dbContext.MstJobTypes on t2.JobType equals t3.Id
                               join t4 in dbContext.TblUsers on t2.AssignedPersonnel equals t4.Id
                               join p1 in dbContext.TblUsers on t2.CreatedBy equals p1.Id
                               join p2 in dbContext.TblTenants on t2.TenantId equals p2.TenantId
                               join t5 in dbContext.MstJobStatuses on t2.JobStatus equals t5.JobStatusId
                               join t6 in dbContext.TblContacts on t2.ContactId equals t6.ContactId
                               select new ExportJobsVm
                               {
                                   ContactFirstName = t6.FirstName,
                                   ContactLastName = t6.LastName,
                                   ContactEmail = t6.Email,
                                   ContactPhone = t6.PhoneNumber,
                                   SubtenantName = p2.FirstName + " " + p2.LastName,
                                   CreatedbyUsername = p1.FirstName + " " + p1.LastName,
                                   JobName = t2.JobName,
                                   JobStatusName = t5.JobStatusName,
                                   StreetAddress = t1.StreetAddress,
                                   City = t1.City,
                                   State = t1.State,
                                   ZipCode = t1.ZipCode
                               }).ToList();

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("ContactFirstName,ContactFirstName,ContactEmail,ContactPhone,Partner/Tenant,CreatedBy,JobName,Status,Address,City,State,ZipCode");

                foreach (var job in jobList)
                {
                    var line = $"{job.ContactFirstName},{job.ContactLastName},{job.ContactEmail},{job.ContactPhone}," +
                               $"{job.SubtenantName},{job.CreatedbyUsername},{job.JobName}," +
                               $"{job.JobStatusName},{job.StreetAddress},{job.City}," +
                               $"{job.State},{job.ZipCode}";

                    csvBuilder.AppendLine(line);
                }

                var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var base64Csv = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.ExportSuccessMsg;
                responseVm.Data = base64Csv;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }
        #endregion Start Export ExportJobsToCsv

        #region Start Export Breakdowns

        public async Task<IResponseVm<string>> ExportDynamicBreakdown(Pagination filter, string estimationId, string windowProductId, string filePath)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true);
                if (!isTenantExists)
                {
                    return CreateErrorResponse(AppConstant.TenantNotFound);
                }

                Guid[] ids = string.IsNullOrEmpty(windowProductId) ? null : new Guid[] { Guid.Parse(windowProductId) };
                var estimateResponse = !string.IsNullOrEmpty(estimationId)
                    ? await estimationBreakdownService.GetProductBreakdown(Guid.Parse(estimationId), null)
                    : await estimateService.BuildEstimate(ids, true);

                if (estimateResponse.Data == null || !estimateResponse.Data.Any())
                {
                    return CreateErrorResponse("No data available to export.");
                }

                var commonHeaders = new List<string>
                {
                "WindowLabel",
                "ProductTypeName",
                "ProductName",
                "Quantity",
                "Width",
                "Height"
                };


                var dynamicHeaders = GetDynamicHeaders(estimateResponse.Data, "ComponentName");

                var headers = commonHeaders.Concat(dynamicHeaders).ToList();

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine(string.Join(",", headers));

                foreach (var breakdownGroup in estimateResponse.Data.GroupBy(d => d.GetType().GetProperty("WindowId")?.GetValue(d)))
                {
                    var firstEntry = breakdownGroup.First();
                    var rowValues = new List<string>();

                    rowValues.AddRange(GetHeaderValues(firstEntry, commonHeaders));

                    var componentPrices = GetComponentPrices(breakdownGroup, "ComponentName", "Price");

                    rowValues.AddRange(dynamicHeaders.Select(header => componentPrices.GetValueOrDefault(header, "0")));

                    csvBuilder.AppendLine(string.Join(",", rowValues));
                }

                var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var base64Csv = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.ExportSuccessMsg;
                responseVm.Data = base64Csv;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        private List<string> GetDynamicHeaders(IEnumerable<object> data, string propertyName)
        {
            return data
                .Where(d => d.GetType().GetProperty(propertyName) != null)
                .Select(d => d.GetType().GetProperty(propertyName)?.GetValue(d)?.ToString())
                .Where(header => !string.IsNullOrEmpty(header))
                .Distinct()
                .ToList();
        }

        private List<string> GetHeaderValues(object entry, List<string> headers)
        {
            var values = new List<string>();

            foreach (var header in headers)
            {
                var property = entry.GetType().GetProperty(header);

                if (header == "WindowLabel")
                {
                    var labelValue = entry.GetType().GetProperty("Label")?.GetValue(entry)?.ToString();
                    values.Add(labelValue ?? "");
                }
                else
                {
                    values.Add(property?.GetValue(entry)?.ToString() ?? "");
                }
            }

            return values;
        }

        private Dictionary<string, string> GetComponentPrices(IEnumerable<object> breakdownGroup, string componentNameProperty, string priceProperty)
        {
            var componentPrices = new Dictionary<string, string>();
            foreach (var breakdown in breakdownGroup)
            {
                var componentName = breakdown.GetType().GetProperty(componentNameProperty)?.GetValue(breakdown)?.ToString();
                var componentPrice = breakdown.GetType().GetProperty(priceProperty)?.GetValue(breakdown)?.ToString();

                if (!string.IsNullOrEmpty(componentName))
                {
                    componentPrices[componentName] = componentPrice ?? "0";
                }
            }
            return componentPrices;
        }

        private IResponseVm<string> CreateErrorResponse(string message)
        {
            return new ResponseVm<string>
            {
                IsSuccess = false,
                Message = message
            };
        }

        #endregion End Export Breakdowns





    }


}



   




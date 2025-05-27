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
using Stripe;

namespace ShutterManagement.Business.Services
{

    public class ImportandExportCSV
    {
        private readonly ShutterDBContext dbContext;
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
        private readonly IInvoiceService invoiceService;
        private readonly IReportServices reportServices;
        private readonly IMstProductType productType;
        public Pagination filter = new Pagination();
        public ImportandExportCSV(ShutterDBContext context, IVendorService vendorService,
            IMaterial material, IContactService contactService, ClaimsProperty claimsProperty,
            IUnitofMeasurementService unitofMeasurement, ITenantService tenantService,
            ITaxExemption taxExemptionService, IContactBillingService billingService, 
            IEstimateService estimateService,IEstimationBreakdownService estimationBreakdownService
            , IInvoiceService invoiceService ,IReportServices reportServices)
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
            this.invoiceService = invoiceService;
            this.reportServices = reportServices;
            this.productType = productType;
            filter.PageNo = 1;
            filter.RecordPerPage = 1000;
            filter.SearchValue = "";
            filter.TenantId = "";
            filter.Status = 1;
            filter.Vendor = "";
            filter.JobStatus = "";
            filter.ProductType = "";
            filter.Region = "";
            filter.Address = "";
        }

        #region Start Export import Contact and material


        public async Task<IResponseVm<string>> DownloadCsvSampleMaterial()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var rootPath = Directory.GetCurrentDirectory();
                var tenantId = AppConstant.LoggedInTenantId;
                var relativePath = Path.Combine("Files", tenantId.ToString(), "Samples");
                var fullDirectoryPath = Path.Combine(rootPath, relativePath);

                if (!Directory.Exists(fullDirectoryPath))
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }

                var fileName = "MaterialSample.csv";
                var filePathWithName = Path.Combine(fullDirectoryPath, fileName);

                // CSV headers with * for required fields
                if (!System.IO.File.Exists(filePathWithName))
                {
                    using (var writer = new StreamWriter(filePathWithName))
                    {
                        await writer.WriteLineAsync("MaterialName*,UnitCost*,UnitOfMeasurement*,Lenght,Width,StockQuantity,ReorderLevel,Vendor*,Notes,ProductType*,SKU,IsCutSheet*");
                    }
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePathWithName);
                string base64Data = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = "Material Sample Downloaded successfully.";
                responseVm.Data = base64Data;

                // Clean up temporary file
                System.IO.File.Delete(filePathWithName);
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DownloadCsvSampleContact()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var rootPath = Directory.GetCurrentDirectory();

                var tenantId = AppConstant.LoggedInTenantId;
                var relativePath = Path.Combine("Files", AppConstant.LoggedInTenantId.ToString(), "Samples");
                var fullDirectoryPath = Path.Combine(rootPath, relativePath);

                if (!Directory.Exists(fullDirectoryPath))
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }

                var fileName = "MaterialSample.csv";
                var filePathWithName = Path.Combine(fullDirectoryPath, fileName);

                if (!System.IO.File.Exists(filePathWithName))
                {
                    using (var writer = new StreamWriter(filePathWithName))
                    {
                      
                        await writer.WriteLineAsync("ContactType*,FirstName*,LastName*,Suffix*,PhoneNumber*,Email*,Website,Notes,Referral,BillingFirstName*,BillingLastName*,PhoneNumber*,Email*,City*,StateName*,ZipCode*,StreetAddress*,EffectiveDate,ExpiryDate");
                    }
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePathWithName);
                string base64Data = Convert.ToBase64String(fileBytes);

                responseVm.IsSuccess = true;
                responseVm.Message = "Material Sample Downloaded successfully..";
                responseVm.Data = base64Data;
                System.IO.File.Delete(filePathWithName);
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }



        public async Task<IResponseVm<List<string>>> ImportMaterialDatabase(Stream csvStream)
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
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null 
                }))
                {
                    //var records = csv.GetRecords<ImportMaterial>().ToList();
                    var expectedHeaders = new[]
                    {
                   "MaterialName", "UnitCost", "UnitOfMeasurement","Lenght","Width", "StockQuantity", "ReorderLevel",
                   "Vendor", "Notes", "ProductType", "SKU", "IsCutSheet"
                    };

                    

                    csv.Read();
                    csv.ReadHeader();
                    var actualHeaders = csv.HeaderRecord.Select(h => h.Trim().TrimEnd('*').ToLower()) .ToArray();

                    var normalizedExpectedHeaders = expectedHeaders
                        .Select(h => h.Trim().ToLower()) 
                        .ToArray();

                    if (!normalizedExpectedHeaders.SequenceEqual(actualHeaders))
                    {
                        responseVm.Message = "The CSV file header format is incorrect. Please download the sample format.";
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
                        if (string.IsNullOrEmpty(record.Vendor))
                        {
                            validationErrors.Add($"Error in row {rowNumber}: VendorCode is required.");
                            hasValidationError = true;
                            continue;
                        }

                        var vendor = vendorService.GetAllQuerable()
                            .Where(v => (v.VendorCode == record.Vendor|| v.CompanyName==record.Vendor) && v.IsDelete == false && v.TenantId==AppConstant.LoggedInTenantId)
                            .FirstOrDefault();

                        if (vendor == null)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: Vendor  Required");
                            hasValidationError = true;
                            continue;
                        }

                        var productType = dbContext.MstProductTypes
                            .Where(p => p.ProductName == record.ProductType && p.IsDelete == false && p.TenantId==AppConstant.LoggedInTenantId)
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
                            if (unitOfMeasurementExist?.UnitofMeasurementName?.ToLower() == record?.UnitOfMeasurement?.ToLower()?.Trim())
                            {
                                unitFound = true;
                                break;
                            }
                        }

                        if (!unitFound)
                        {
                            validationErrors.Add($"Error in row {rowNumber}: UnitOfMeasurement '{record?.UnitOfMeasurement}' is Not Valid.");
                            hasValidationError = true;
                            continue;

                        }

                        var materialModel = new CreateMaterial
                        {
                            SKU = record.SKU?.Trim(),
                            MaterialName = record.MaterialName?.Trim(),
                            Lenght = record.Lenght??null,
                            Width = record.Width??null,
                            UnitCost = record.UnitCost,
                            //NonRatedUnitCost=record.NonRatedUnitCost,
                           // NonRatedUnitCost=record.UnitCost,
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
            int successfulUpdates = 0;
            int successfulInserts = 0;
            int totalRecords = 0;

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                using (var reader = new StreamReader(csvStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null
                }))
                {
                    var expectedHeaders = new[]
                    {
                "ContactType", "FirstName", "LastName","Suffix", "PhoneNumber", "Email", "Website",
                "Notes", "Referral", "BillingFirstName", "BillingLastName", "PhoneNumber",
                "Email", "City", "StateName", "ZipCode", "StreetAddress", "EffectiveDate", "ExpiryDate"
            };

                    csv.Read();
                    csv.ReadHeader();
                    var actualHeaders = csv.HeaderRecord.Select(h => h.Trim().TrimEnd('*').ToLower()).ToArray();

                    var normalizedExpectedHeaders = expectedHeaders
                        .Select(h => h.Trim().ToLower())
                        .ToArray();

                    if (!normalizedExpectedHeaders.SequenceEqual(actualHeaders))
                    {
                        responseVm.Message = "The CSV file header format is incorrect. Please download the sample format.";
                        return responseVm;
                    }

                    var records = csv.GetRecords<ImportContact>().ToList();
                    totalRecords = records.Count;
                    int rowNumber = 1;

                    foreach (var record in records)
                    {
                        var filter = new Pagination { SearchValue = record.Email };
                        bool role = true;

                        var existingContactResponse = await contactService.GetAll_Contact(filter, role);
                        var existingContact = existingContactResponse.Data?.FirstOrDefault();

                        bool isUpdate =  false;
                        if(existingContact!=null)
                        {
                            isUpdate = true;
                        }

                        var contactModel =  await MapToSpContact(record, isUpdate, existingContact?.ContactId);

                        if (existingContact != null)
                        {
                            contactModel.ContactId = existingContact.ContactId;

                            var updateResponse = await contactService.Update_Contact(contactModel);
                            if (updateResponse.IsSuccess)
                            {
                                successfulUpdates++;
                            }
                            else
                            {
                                validationErrors.Add($"Error updating contact in row {rowNumber}: {updateResponse.Message}");
                            }
                        }
                        else
                        {
                            validContacts.Add(contactModel);
                        }
                        rowNumber++;
                    }

                    if (validContacts.Any())
                    {
                        successfulInserts = await SaveOrUpdateContacts(validContacts, validationErrors);
                    }

                    responseVm.IsSuccess = !validationErrors.Any();
                    responseVm.Message = $"Processed {successfulUpdates + successfulInserts} contacts ({successfulUpdates} updated, {successfulInserts} inserted).";
                    responseVm.TotalRecords = successfulUpdates + successfulInserts;
                    if (validationErrors.Any())
                    {
                        responseVm.Data = validationErrors;
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

        private async Task<int> SaveOrUpdateContacts(List<Sp_Contact> contacts, List<string> validationErrors)
        {
            int successfulInserts = 0;

            foreach (var contact in contacts)
            {
                var createContactResponse = await contactService.Create_Contact(contact);

                if (createContactResponse.IsSuccess)
                {
                    successfulInserts++;
                }
                else
                {
                    validationErrors.Add($"Error creating contact {contact.Email}: {createContactResponse.Message}");
                }
            }

            return successfulInserts;
        }

        private async Task<dynamic> MapToSpContact(ImportContact record, bool isUpdate, Guid? contactId)
        {
            var stateId =  GetStateIdByCode(record.BillingAddressVm?.StateName);
            if (isUpdate)
            {
                return new UpdateContact
                {
                    ContactId = contactId,
                    ContactType=record.ContactType?.Trim(),
                    FirstName = record.FirstName?.Trim(),
                    LastName = record.LastName?.Trim(),
                    Suffix = record.Suffix?.Trim(),
                    PhoneNumber = record.PhoneNumber?.Trim(),
                    Email = record.Email?.Trim(),
                    Website = record.Website?.Trim(),
                    Notes = record.Notes?.Trim(),
                    Referral = record.Referral?.Trim(),
                    BillingAddressVm = record.BillingAddressVm != null ? new BillingAddressVm
                    {
                        FirstName = record.BillingAddressVm.BillingFirstName?.Trim(),
                        LastName = record.BillingAddressVm.BillingLastName?.Trim(),
                        PhoneNumber = record.BillingAddressVm.PhoneNumber?.Trim(),
                        Email = record.BillingAddressVm.Email?.Trim(),
                        StreetAddress = record.BillingAddressVm.StreetAddress?.Trim(),
                        ZipCode = record.BillingAddressVm.ZipCode?.Trim(),
                        City = record.BillingAddressVm.City?.Trim(),
                        StateId= stateId
                    } : null,
                    TaxExemptionVm = record.TaxExemptionVm != null ? new TaxExemption
                    {
                        ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate,
                        EffectiveDate = record.TaxExemptionVm.EffectiveDate,
                        ExpiryDate = record.TaxExemptionVm.ExpiryDate
                    } : null
                };
            }

            return new Sp_Contact
            {
                ContactType = record.ContactType?.Trim(),
                FirstName = record.FirstName?.Trim(),
                LastName = record.LastName?.Trim(),
                Suffix=record.Suffix?.Trim(),
                PhoneNumber = record.PhoneNumber?.Trim(),
                Email = record.Email?.Trim(),
                Website = record.Website?.Trim(),
                Notes = record.Notes?.Trim(),
                Referral = record.Referral?.Trim(),
                IsBillingAddressSameAsAddress = false,
                BillingAddressVm = record.BillingAddressVm != null ? new BillingAddressVm
                {
                    FirstName = record.BillingAddressVm.BillingFirstName?.Trim(),
                    LastName = record.BillingAddressVm.BillingLastName?.Trim(),
                    PhoneNumber = record.BillingAddressVm.PhoneNumber?.Trim(),
                    Email = record.BillingAddressVm.Email?.Trim(),
                    StreetAddress = record.BillingAddressVm.StreetAddress?.Trim(),
                    ZipCode = record.BillingAddressVm.ZipCode?.Trim(),
                    City = record.BillingAddressVm.City?.Trim(),
                    StateName = record.BillingAddressVm.StateName?.Trim(),
                    StateId=stateId
                } : null,
                TaxExemptionVm = record.TaxExemptionVm != null ? new TaxExemption
                {
                    ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate,
                    EffectiveDate = record.TaxExemptionVm.EffectiveDate,
                    ExpiryDate = record.TaxExemptionVm.ExpiryDate
                } : null
            };
        }


        private Guid GetStateIdByCode(string stateCode)
        {
            if (string.IsNullOrWhiteSpace(stateCode))
                return Guid.Empty;

            var state = dbContext.MstStates
                .FirstOrDefault(s =>
                    s.StateCode.Trim().ToLower() == stateCode.Trim().ToLower() ||
                    s.StateName.Trim().ToLower() == stateCode.Trim().ToLower());

            return state?.Id ?? Guid.Empty;
        }


        //private async Task<int> SaveOrUpdateContacts(List<Sp_Contact> contacts, List<string> validationErrors)
        //{
        //    int successfulInserts = 0;

        //    foreach (var contact in contacts)
        //    {
        //        var createContactResponse = await contactService.Create_Contact(contact);

        //        if (createContactResponse.IsSuccess)
        //        {
        //            successfulInserts++;
        //        }
        //        else
        //        {
        //            validationErrors.Add($"Error creating contact {contact.Email}: {createContactResponse.Message}");
        //        }
        //    }

        //    return successfulInserts;
        //}



        //private Sp_Contact MapToSpContact(ImportContact record , bool IsUpdate)
        //{
        //    if(IsUpdate)
        //    {
        //        var contact = new UpdateContact();

        //        {

        //        }
        //    }
        //    var contactModel = new Sp_Contact
        //    {
        //        ContactType = record.ContactType?.Trim(),
        //        ContactId= record?.ContactId.Trim(),
        //        FirstName = record.FirstName?.Trim(),
        //        LastName = record.LastName?.Trim(),
        //        PhoneNumber = record.PhoneNumber.Trim(),
        //        Email = record.Email?.Trim(),
        //        Website = record.Website.Trim(),
        //        Notes = record.Notes,
        //        Referral = record.Referral.Trim(),
        //        IsBillingAddressSameAsAddress = false,
        //    };

        //    if (record.BillingAddressVm != null)
        //    {
        //        contactModel.BillingAddressVm = new BillingAddressVm
        //        {
        //            FirstName = record.BillingAddressVm.BillingFirstName?.Trim(),
        //            LastName = record.BillingAddressVm.BillingLastName?.Trim(),
        //            PhoneNumber = record.BillingAddressVm.PhoneNumber?.Trim(),
        //            Email = record.BillingAddressVm.Email?.Trim(),
        //            StreetAddress = record.BillingAddressVm.StreetAddress?.Trim(),
        //            ZipCode = record.BillingAddressVm.ZipCode?.Trim(),
        //            City = record.BillingAddressVm.City?.Trim(),
        //            State = record.BillingAddressVm.State?.Trim()
        //        };
        //    }

        //    if (record.TaxExemptionVm != null)
        //    {
        //        contactModel.TaxExemptionVm = new TaxExemption
        //        {
        //            ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate,
        //            EffectiveDate = record.TaxExemptionVm.EffectiveDate,
        //            ExpiryDate = record.TaxExemptionVm?.ExpiryDate
        //        };
        //    }

        //    return contactModel;
        //}

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

        #endregion Start Export import Contact and material

        

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
                        contact.Suffix,
                        contact.Website,
                        contact.Notes,
                        contact.Referral,
                        contact.IsTaxExemption,
                        contact.IsBillingAddressSameAsAddress,
                        contact.PrimaryAddress,
                        contact.IsActive
                    }).ToList();

                var detailedContactList = new List<ContactsList>();
                foreach (var contact in contactsList)
                {
                    var billingAddress = dbContext.TblAddresses
                        .Where(x => x.AddressId == contact.PrimaryAddress
                        && x.IsDelete == false)
                        .Select(x => new BillingAddressVm
                        {
                            City = x.City,
                            Email = x.Email,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            PhoneNumber = x.Phone,
                            StateName =  dbContext.MstStates.Where(s=>s.Id==x.State).Select(y=>y.StateName).FirstOrDefault(),
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
                        Suffix=contact.Suffix,
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
                csvBuilder.AppendLine("ContactType*,FirstName*,LastName*,Suffix*,PhoneNumber*,Email*,Website,Notes,Referral,BillingFirstName*,BillingLastName*,PhoneNumber*,Email*,City*,StateName*,ZipCode*,StreetAddress*,EffectiveDate,ExpiryDate");

                foreach (var contact in detailedContactList)
                {
                    var billingAddress = contact.billingAddressVm ?? new BillingAddressVm();
                    var exemptionCertificates = contact.taxExemptionVm?.ExemptionCertificate != null
                        ? string.Join(";", contact.taxExemptionVm.ExemptionCertificate)
                        : "";

                    var line = $"{contact.ContactType},{contact.FirstName},{contact.LastName},{contact.Suffix},{contact.PhoneNumber},{contact.Email},{contact.Website},{contact.Notes},{contact.Referral}," +
                               $"{billingAddress.FirstName},{billingAddress.LastName},{billingAddress.PhoneNumber},{billingAddress.Email},{billingAddress.City},{billingAddress.StateName}," +
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
                                        Lenght=t2.Length??null,
                                        Width=t2.Width??null,
                                        StockQuantity = t2.StockQuantity,
                                        UnitCost = t2.UnitCost,
                                        ProductTypeName = t3.ProductName,
                                        UnitOfMeasurement = t2.UnitOfMeasurement,
                                       // Non_Rated_UnitCost = t2.NonRatedUnitCost,
                                        VendorId = t2.VendorId,
                                        FirstName = t1.FirstName,
                                        LastName = t1.LastName,
                                        VendorCode = t1.VendorCode,
                                        IsCutSheet = t2.IsCutSheet,
                                    }).ToList();

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("MaterialName*,UnitCost*,UnitOfMeasurement*,Lenght,Width,StockQuantity,ReorderLevel,Vendor*,Notes,ProductType*,SKU,IsCutSheet*");

                foreach (var material in materialList)
                {
                    var line = $"{material.MaterialName},{material.UnitCost},{material.UnitOfMeasurement},{material.Lenght},{material.Width},{material.StockQuantity}," +
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
                                   State = dbContext.MstStates.Where(s => s.Id == t1.State).Select(y => y.StateName ?? "").FirstOrDefault(),
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

                Guid[] ids = windowProductId?.Split(',')
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => Guid.Parse(id))
                 .ToArray();

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

                var csvBuilder = new StringBuilder();
                var dynamicHeaders = GetDynamicHeaders(estimateResponse.Data, "ComponentName");

                var optionHeaders = GetOptionNameTypeHeaders(estimateResponse.Data);

                var finalHeaders = commonHeaders.Concat(dynamicHeaders).Concat(optionHeaders).Concat(new[] { "Sub Total", "Total" }).ToList();

                csvBuilder.AppendLine(string.Join(",", finalHeaders));



                foreach (var breakdownGroup in estimateResponse.Data.GroupBy(d => d.GetType().GetProperty("WindowId")?.GetValue(d)))
                {
                    var firstEntry = breakdownGroup.First();
                    var rowValues = new List<string>();

                    rowValues.AddRange(GetHeaderValues(firstEntry, commonHeaders));

                    var componentPrices = GetComponentPrices(breakdownGroup, "ComponentName", "Price");

                    rowValues.AddRange(dynamicHeaders.Select(header => componentPrices.GetValueOrDefault(header, "0")));

                    var optionValues = GetOptionPriceValues(firstEntry);
                    rowValues.AddRange(optionHeaders.Select(header => optionValues.GetValueOrDefault(header, "0")));

                    // Add Sub Total and Total
                    decimal subTotal = GetDynamicValue(firstEntry, "totalPrice");  
                    decimal additionalCost = GetDynamicValue(firstEntry, "additionalCost");

                    decimal total = subTotal + additionalCost;

                    rowValues.Add($"${subTotal.ToString("F2")}");
                    rowValues.Add($"${total.ToString("F2")}");

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

        private decimal GetDynamicValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return 0;

            var type = obj.GetType();

            // Find property case-insensitive
            var property = type.GetProperties()
                               .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                return 0;

            var value = property.GetValue(obj);

            if (value == null)
                return 0;

            // Try parse decimal from value
            if (value is decimal dec)
                return dec;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;

            return 0;
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
                    componentPrices[componentName] = $"${componentPrice}"  ?? "0";
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

        private List<string> GetOptionNameTypeHeaders(IEnumerable<object> data)
        {
            return data
                .SelectMany(d =>
                {
                    var optionTaxDetails = d.GetType().GetProperty("OptionTaxDetails")?.GetValue(d) as IEnumerable<object>;
                    return optionTaxDetails ?? new List<object>();
                })
                .Select(option =>
                {
                    var name = option.GetType().GetProperty("OptionName")?.GetValue(option)?.ToString();
                    var type = option.GetType().GetProperty("OptionTypeName")?.GetValue(option)?.ToString();
                    return $"{name} - {type}";
                })
                .Where(header => !string.IsNullOrWhiteSpace(header))
                .Distinct()
                .ToList();
        }

        private Dictionary<string, string> GetOptionPriceValues(object entry)
        {
            var optionPrices = new Dictionary<string, string>();

            var optionList = entry.GetType().GetProperty("OptionTaxDetails")?.GetValue(entry) as IEnumerable<object>;

            if (optionList != null)
            {
                foreach (var option in optionList)
                {
                    var name = option.GetType().GetProperty("OptionName")?.GetValue(option)?.ToString();
                    var type = option.GetType().GetProperty("OptionTypeName")?.GetValue(option)?.ToString();
                    var price = option.GetType().GetProperty("OptionPrice")?.GetValue(option)?.ToString() ?? "0";

                    var key = $"{name} - {type}";
                    optionPrices[key] = $"${price}";
                }
            }

            return optionPrices;
        }



        #endregion End Export Breakdowns


        #region Start Export ExportInvoiceToCsv
        public async Task<IResponseVm<string>> ExportInvoiceReportToCsv(Pagination filter, string filePath,ReportFilterType type)
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

                var invoiceReportData = GetFilteredInvoices(filter);
                var filterLogic = new Dictionary<ReportFilterType, Func<InvoiceVM, bool>>
                {
                { ReportFilterType.Duedate, invoice => invoice.DueDate < DateTime.Now  && invoice.BalanceDue>0},
                { ReportFilterType.Pending, invoice => invoice.BalanceDue > 0 },
                { ReportFilterType.Paid, invoice => invoice.BalanceDue == 0 }
                };
                if (!filterLogic.TryGetValue(type, out var filterCondition))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invalid filter type specified.";
                    return responseVm;
                }
                var filteredInvoices = invoiceReportData.Where(filterCondition).ToList();

                if (filter.ToDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(invoice => invoice.DueDate <= filter.ToDate.Value).ToList();
                }

                if (filter.FormDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(invoice => invoice.DueDate >= filter.FormDate.Value).ToList();
                }


                if (!filteredInvoices.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No invoices match the specified filter criteria.";
                    return responseVm;
                }


                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("InvoiceNumber,ContactFirstName,ContactLastName,IssueDate,DueDate,BalanceDue,TotalAmount");

                foreach (var report in filteredInvoices)
                {
                    var line = $"{report.InvoiceNumber},{report.ContactFirstName},{report.ContactLastName},{report.IssueDate}," +
                               $"{report.DueDate},{report.BalanceDue},{report.TotalAmount}";

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
        

        public List<InvoiceVM> GetFilteredInvoices(Pagination filter)
        {
            var invoicesQuery = (from i in dbContext.TblInvoices
                                 join j in dbContext.TblJobs on i.JobId equals j.JobId
                                 join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                                 where i.IsDelete == false
                                 orderby i.CreatedAt descending
                                 select new InvoiceVM
                                 {
                                     InvoiceId = i.InvoiceId,
                                     InvoiceNumber = i.InvoiceNumber,
                                     JobId = i.JobId,
                                     EstimateId = i.EstimateId,
                                     IssueDate = i.IssueDate,
                                     TotalAmount = i.TotalAmount,
                                     PaidAmount = i.PaidAmount,
                                     BalanceDue = i.BalanceDue,
                                     DueDate = i.DueDate,
                                     Status = i.Status ?? "",
                                     IsActive = i.IsActive,
                                     ContactFirstName = c.FirstName,
                                     ContactLastName = c.LastName
                                 }).ToList();

            filter.SearchValue = filter.SearchValue?.Trim().ToLower();

            var filteredInvoices = invoicesQuery
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (
                        string.IsNullOrEmpty(filter.SearchValue) ||
                        x.InvoiceNumber.ToString().ToLower().Contains(filter.SearchValue) ||
                        (x.ContactFirstName?.ToLower() == filter.SearchValue) ||
                        (x.ContactLastName?.ToLower() == filter.SearchValue) ||
                        (x.ContactFirstName + " " + x.ContactLastName?.ToLower() == filter.SearchValue) ||
                        x.Status.ToLower().Contains(filter.SearchValue) ||
                        x.InvoiceId.ToString() == filter.SearchValue ||
                        x.JobId.ToString() == filter.SearchValue
                    )
                ).ToList();

            foreach (var invoice in filteredInvoices)
            {
                invoice.InvoicePayment = dbContext.TblInvoicePayments
                    .Where(p => p.InvoiceId == invoice.InvoiceId && p.IsDelete == false)
                    .Select(p => new InvoicePaymentVM
                    {
                        InvoicePaymentId = p.InvoicePaymentId,
                        InvoiceId = p.InvoiceId,
                        InvoiceAmount = p.InvoiceAmount,
                        Notes = p.Notes,
                        PaymentDate = p.CreatedAt,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PaymentDate)
                    .ToList();
            }

            return filteredInvoices;
        }
        #endregion Start Export ExportInvoiceToCsv


        #region Start Export ExportOrderToCsv
        public async Task<IResponseVm<string>> ExportOrderReportToCsv(Pagination filter)
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

                var orderReportData = await (reportServices.GetOrderConfirmed(filter)) ;
                if (orderReportData.Data==null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Order Not Found.";
                    return responseVm;
                }
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("EstimateId,OrderNumber,ContactName,OrderDate,ConfirmedDate,TotalAmount");
                foreach (var report in orderReportData.Data)
                {
                    var line = $"{report.EstimateCode??""},{report.OrderNumber??""},{report.jobDetails.ContactFirstName ?? ""},{report.jobDetails.ContactLastName ?? ""}," +
                               $"{report.CreatedDate},{report.CreatedDate},{report.GrandTotal}";
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
        #endregion Start Export ExportOrderToCsv


        #region Start Export ExportTaxtToCsv
        public async Task<IResponseVm<string>> ExportTaxtReportToCsv(Pagination filter)
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

                var orderReportData = await (reportServices.GetTaxReport(filter));
                if (orderReportData.Data == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Order Not Found.";
                    return responseVm;
                }
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("EstimationCode,JobCode,FirstName,LastName,TaxAmount");
                foreach (var report in orderReportData.Data)
                {
                    var line = $"{report.EstimateCode ?? ""},{report.jobDetails?.JobUniqueId ?? ""},{report.jobDetails?.FirstName ?? ""},{report.jobDetails?.FirstName ?? ""}," +
                               $"{report.Tax}";
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
        #endregion Start Export ExportTaxToCsv


        //public class ImportContactMap : ClassMap<ImportContact>
        //{
        //    public ImportContactMap()
        //    {
        //        Map(m => m.ContactType).Name("ContactType", "ContactType*");
        //        Map(m => m.FirstName).Name("FirstName", "firstname", "firstname*");
        //        Map(m => m.LastName).Name("LastName", "lastname", "lastname*");
        //        Map(m => m.Suffix).Name("Suffix", "Suffix", "Suffix*");
        //        Map(m => m.PhoneNumber).Name("PhoneNumber", "PhoneNumber", "PhoneNumber*");
        //        Map(m => m.Email).Name("Email", "email", "email*");
        //        Map(m => m.PhoneNumber).Name("PhoneNumber", "phone", "phone*");
        //        Map(m => m.BillingAddressVm.StreetAddress).Name("StreetAddress", "StreetAddress", "StreetAddress*");
        //        Map(m => m.BillingAddressVm.City).Name("City", "city", "city*");
        //        Map(m => m.BillingAddressVm.ZipCode).Name("ZipCode", "zipcode", "zipcode*");
        //        Map(m => m.BillingAddressVm.StateName).Name("StateName", "StateName", "StateName*");
        //        Map(m => m.BillingAddressVm.StateName).Name("StateName", "StateName", "StateName*");
        //    }
        //}


    }


}



   




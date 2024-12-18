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
        private readonly IMstProductType productType;

        public ImportandExportCSV(BtprojecShuttermanagementDevContext context,IVendorService vendorService, 
            IMaterial material, IContactService contactService, ClaimsProperty claimsProperty,
            IUnitofMeasurementService unitofMeasurement,ITenantService tenantService ,ITaxExemption taxExemptionService, IContactBillingService billingService ,BtprojecShuttermanagementDevContext btprojecShuttermanagementDevContext)
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

            this.productType = productType;
        }

        public async Task<IResponseVm<List<string>>> ImportCsvToDatabase(Stream csvStream)
        {
            IResponseVm<List<string>> responseVm = new ResponseVm<List<string>>();
            List<string> validationErrors = new List<string>();
            List<CreateMaterial> validMaterials = new List<CreateMaterial>();
            int successfulImports = 0;
            int totalRecords = 0;
            var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
            if (IsTenantExists)
            {

                try
                {
                    using (var reader = new StreamReader(csvStream))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var records = csv.GetRecords<ImportMaterial>().ToList();
                        totalRecords = records.Count;
                        int rowNumber = 1;

                        if (records.Count > 0)
                        {
                            bool hasValidationError = false;
                            foreach (var record in records)
                            {
                                if (record.VendorCode == null)
                                {
                                    responseVm.Message = "VendorCode is required";
                                    return responseVm;
                                }

                                var vendorId = vendorService.GetAllQuerable()
                                    .Where(v => v.VendorCode == record.VendorCode && v.IsDelete == false)
                                    .FirstOrDefault();

                                if (vendorId == null)
                                {
                                    responseVm.Message = AppConstant.NoDataFound;
                                    return responseVm;
                                }

                                var productTypeId = dbContext.MstProductTypes
                                    .Where(p => p.ProductName == record.ProductType && p.IsDelete == false)
                                    .FirstOrDefault();

                                if (productTypeId == null)
                                {
                                    responseVm.Message = "ProductType is required.";
                                    return responseVm;
                                }


                                var unitOfMeasurement = unitofMeasurement.GetAllQuerable()
                                    .Where(u => u.UnitofMeasurementName == record.UnitOfMeasurement.Trim() && u.IsDelete == false)
                                    .FirstOrDefault();

                                if (unitOfMeasurement == null)
                                {
                                    responseVm.Message = "UnitOfMeasurement is required.";
                                    return responseVm;
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
                                    VendorId = vendorId.VendorId,
                                    Notes = record.Notes,
                                    ProductType = productTypeId.ProductId.ToString(),
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
                                        validationErrors.Add($"Error with materials: {createMaterialResponse.Message}");
                                    }
                                }

                                if (validationErrors.Any())
                                {
                                    responseVm.IsSuccess = false;
                                    responseVm.Message = "Some Materials failed to be inserted.";
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
                }
                catch (Exception ex)
                {
                    responseVm.Message = AppConstant.ExceptionError + ex.Message;
                }
            }
            else
            {
                responseVm.Message = AppConstant.TenantNotFound;
                return responseVm;
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
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (IsTenantExists)
                {
                    using (var reader = new StreamReader(csvStream))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {

                        var records = csv.GetRecords<ImportContact>().ToList();
                        totalRecords = records.Count;
                        int rowNumber = 1;
                        bool hasValidationError = false;
                        foreach (var record in records)
                        {

                            var contactModel = new Sp_Contact
                            {
                                ContactType = record.ContactType?.Trim(),
                                Name = record.Name?.Trim(),
                                PhoneNumber = record.PhoneNumber.Trim(),
                                Email = record.Email?.Trim(),
                                Website = record.Website.Trim(),
                                Notes = record.Notes,
                                Referral = record.Referral.Trim(),
                                IsBillingAddressSameAsAddress = false,
                            };


                            if (record.BillingAddressVm != null)
                            {
                                contactModel.BillingAddressVm ??= new BillingAddressVm();
                                contactModel.BillingAddressVm.FullName = record.BillingAddressVm.FullName?.Trim();
                                contactModel.BillingAddressVm.PhoneNumber = record.BillingAddressVm.PhoneNumber?.Trim();
                                contactModel.BillingAddressVm.Email = record.BillingAddressVm.Email.Trim();
                                contactModel.BillingAddressVm.StreetAddress = record.BillingAddressVm.StreetAddress.Trim();
                                contactModel.BillingAddressVm.ZipCode = record.BillingAddressVm.ZipCode.Trim();
                                contactModel.BillingAddressVm.City = record.BillingAddressVm.City.Trim();
                                contactModel.BillingAddressVm.State = record.BillingAddressVm.State.Trim();
                            }

                            if (record.TaxExemptionVm != null)
                            {
                                contactModel.TaxExemptionVm ??= new TaxExemption();
                                contactModel.TaxExemptionVm.ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate;
                                contactModel.TaxExemptionVm.EffectiveDate = record.TaxExemptionVm.EffectiveDate;
                                contactModel.TaxExemptionVm.ExpiryDate = record.TaxExemptionVm?.ExpiryDate;
                            }

                            var existingContact = await contactService.GetContactByEmailOrPhone(contactModel.Email, contactModel.PhoneNumber);
                            if (existingContact.Message != null)
                            {
                                validationErrors.Add($"Error in row {rowNumber}: {existingContact.Message}");
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

                            int successfulInserts = 0;
                            foreach (var contactToInsert in validContacts)
                            {
                                var createContactResponse = await contactService.Create_Contact(contactToInsert);

                                if (createContactResponse.IsSuccess)
                                {
                                    successfulInserts++;
                                }
                                else
                                {
                                    validationErrors.Add($"Error with contact: {createContactResponse.Message}");
                                }
                            }

                            if (validationErrors.Any())
                            {
                                responseVm.IsSuccess = false;
                                responseVm.Message = "Some contacts failed to be inserted.";
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
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<dynamic>> ExportContactsToCsvAsync(string filePath)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (IsTenantExists)
                {
                    var Contacts = new List<TblContact>();
                    List<ContactsList> ContactList = new List<ContactsList>();

                    Contacts = (await contactService.GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();

                    foreach (var contact in Contacts)
                    {
                        ContactsList contacts = new ContactsList
                        {
                            ContactId = contact.ContactId,
                            ContactType = contact.ContactType ?? "",
                            Email = contact.Email,
                            PhoneNumber = contact.PhoneNumber,
                            Name = contact.Name,
                            Website = contact.Website,
                            Notes = contact.Notes,
                            Referral = contact.Referral,
                            IsTaxExemption = contact.IsTaxExemption ?? false,
                            IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                            IsActive = contact.IsActive
                        };

                        contacts.billingAddressVm = billingService.GetAllQuerable()
                            .Where(x => x.ContactId == contact.ContactId
                            && x.IsDelete == false).Select(x => new BillingAddressVm
                            {
                                City = x.City,
                                Email = x.Email,
                                FullName = x.FullName,
                                PhoneNumber = x.PhoneNumber,
                                State = x.State,
                                StreetAddress = x.StreetAddress,
                                ZipCode = x.ZipCode,
                            }).FirstOrDefault();
                        contacts.taxExemptionVm = taxExemptionService.GetAllQuerable()
                            .Where(x => x.ContactId == contact.ContactId
                            && x.IsDelete == false).Select(x => new TaxExemption
                            {
                                EffectiveDate = x.EffectiveDate,
                                ExemptionCertificate = x.ExemptionCertificate.Split(new[] {","},StringSplitOptions.None),
                                ExpiryDate = x.ExpiryDate
                            }).FirstOrDefault();
                        ContactList.Add(contacts);
                    }
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("ContactType,Name,PhoneNumber,Email,Website,Notes,Referral,FullName,PhoneNumber,Email,City,State,ZipCode,StreetAddress,ExemptionCertificate,EffectiveDate,ExpiryDate");


                    foreach (var contact in ContactList)
                    {
                        var billingAddress = contact.billingAddressVm ?? new BillingAddressVm();


                        var line = $"{contact.ContactType},{contact.Name},{contact.PhoneNumber},{contact.Email},{contact.Website},{contact.Notes},{contact.Referral}," +
                                   $"{billingAddress.FullName},{billingAddress.PhoneNumber},{billingAddress.Email},{billingAddress.City},{billingAddress.State}," +
                                   $"{billingAddress.ZipCode},{billingAddress.StreetAddress},{contact.taxExemptionVm?.ExemptionCertificate},{contact.taxExemptionVm?.EffectiveDate},{contact.taxExemptionVm?.ExpiryDate}";

                        csvBuilder.AppendLine(line);
                    }

                    try
                    {
                        await File.WriteAllTextAsync(filePath, csvBuilder.ToString());
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.ExportSuccessMsg; 
                    }
                    catch (Exception ex)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.ExceptionError + ex.Message; 
                    }

                    return responseVm;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message; 
                return responseVm;
            }
            return responseVm;
        }


        public async Task<IResponseVm<dynamic>> ExportMaterialsToCsvAsync(string filePath)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();

            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var materialList = (from t2 in dbContext.TblRawMaterials.OrderByDescending(x => x.CreatedAt)
                                    join t1 in dbContext.TblVendors on t2.VendorId equals t1.VendorId
                                    join t3 in dbContext.MstProductTypes on t2.ProductType equals t3.ProductId
                                    where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false
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

                try
                {
                    await File.WriteAllTextAsync(filePath, csvBuilder.ToString());
                    responseVm.Message = AppConstant.ExportSuccessMsg;
                }
                catch (Exception ex)
                {
                    responseVm.Message = AppConstant.ExceptionError + ex.Message;
                }

                return responseVm;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
                return responseVm;
            }
        }

    }
}



   




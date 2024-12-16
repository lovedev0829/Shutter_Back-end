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
        private readonly IMstProductType productType;

        public ImportandExportCSV(BtprojecShuttermanagementDevContext context,IVendorService vendorService, 
            IMaterial material, IContactService contactService, ClaimsProperty claimsProperty,
            IUnitofMeasurementService unitofMeasurement)
        {
            dbContext = context;
            this.vendorService = vendorService;
            this.material = material;
            this.contactService = contactService;
            this.claimsProperty = claimsProperty;
            this.unitofMeasurement = unitofMeasurement;
            this.productType = productType;
        }

        public async Task<IResponseVm<List<string>>> ImportCsvToDatabase(Stream csvStream)
        {
            IResponseVm<List<string>> responseVm = new ResponseVm<List<string>>();
            List<string> validationErrors = new List<string>();
            List<CreateMaterial> validMaterials = new List<CreateMaterial>();
            int successfulImports = 0;
            int totalRecords = 0;
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

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task ExportContactsToCsvAsync(string filePath)
        {
     
            var vendors = vendorService.GetAllQuerable().Where(x=> x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("VendorId,VendorName,PhoneNumber,Email,City,State,StreetAddress,VendorCode");

           
            foreach (var vendor in vendors)
            {
                var line = $"{vendor.VendorId},{vendor.VendorName},{vendor.PhoneNumber},{vendor.Email},{vendor.City},{vendor.State},{vendor.StreetAddress},{vendor.VendorCode}";
                csvBuilder.AppendLine(line);
            }

           
            await File.WriteAllTextAsync(filePath, csvBuilder.ToString());

            Console.WriteLine("Export completed successfully.");
        }
    }
  }



   




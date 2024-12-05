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

namespace ShutterManagement.Business.Services
{

    public class ImportandExportCSV
    {
        private readonly ShutterDbContext dbContext;
        private readonly IVendorService vendorService;
        private readonly IMaterial material;
        private readonly IContactService contactService;
        private readonly ClaimsProperty claimsProperty;

        public ImportandExportCSV(ShutterDbContext context,IVendorService vendorService, IMaterial material, IContactService contactService, ClaimsProperty claimsProperty)
        {
            dbContext = context;
            this.vendorService = vendorService;
            this.material = material;
            this.contactService = contactService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<string> ImportCsvToDatabase(Stream csvStream)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                using (var reader = new StreamReader(csvStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {

                    var records = csv.GetRecords<ImportMaterial>().ToList();

                    if (records.Count > 0)
                    {

                        foreach (var record in records)
                        {
                            if (record.VendorCode == null)
                            {
                                responseVm.Message = "VendorCode is required"; 
                            }
                            var vendorId = vendorService.GetAllQuerable().Where(v => v.VendorCode == record.VendorCode && v.IsDelete == false).FirstOrDefault();
                            if (vendorId == null)
                            {
                                responseVm.Message = AppConstant.NoDataFound;
                            }
                            var materialModel = new CreateMaterial
                            {

                                SKU = record.SKU?.Trim(),
                                MaterialName = record.MaterialName?.Trim(),
                                UnitCost = record.UnitCost,
                                UnitOfMeasurement = record.UnitOfMeasurement?.Trim(),
                                StockQuantity = record.StockQuantity,
                                ReorderLevel = record.ReorderLevel,
                                IsCutSheet = record.IsCutSheet,
                                VendorId = vendorId.VendorId,
                                Notes = record.Notes,
                                ProductType = record.ProductType,

                            };


                            var createMaterialResponse = await material.Create_Material(materialModel);

                            if (!createMaterialResponse.IsSuccess)
                            {

                                Console.WriteLine($"Error importing record with SKU: {record.SKU}. Error: {createMaterialResponse.Message}");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm.Message;
        }
       

        public async Task<string> ImportCsvForContact(Stream csvStream)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                using (var reader = new StreamReader(csvStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {

                    var records = csv.GetRecords<ImportContact>().ToList();



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
                            contactModel.TaxExemptionVm.ExemptionCertificate = record.TaxExemptionVm.ExemptionCertificate;
                            contactModel.TaxExemptionVm.EffectiveDate = record.TaxExemptionVm.EffectiveDate;
                            contactModel.TaxExemptionVm.ExpiryDate = record.TaxExemptionVm?.ExpiryDate;
                        }
                        var createContactResponse = await contactService.Create_Contact(contactModel);

                        if (!createContactResponse.IsSuccess)
                        {

                            responseVm.Message = $"Error: {createContactResponse.Message}";
                            
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }


            return responseVm.Message;

  
                }
            }
        }



   




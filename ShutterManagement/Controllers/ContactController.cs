using Azure;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAllContacts()
        {
            IResponseVm<List<Sp_Contact>> res = new ResponseVm<List<Sp_Contact>>();   
            try
            {
                var contacts = await _contactService.GetAll_Contact();
                res.IsSuccess = true;
                res.Message = "Contacts fetched successfully";
                res.Data = contacts;
                res.TotalRecords = contacts.Count;
            }
            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = $"Error fetching contacts: {ex.Message}";
            }
            return Ok(res);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(Guid id)
        {
            IResponseVm<Sp_Contact> res = new ResponseVm<Sp_Contact>();

            try
            {
                var contact = await _contactService.GetById_Contact(id);
                res.IsSuccess = true;
                res.Message = $"Contact with {id} fetched successfully";
                res.Data = contact;

                if (contact == null)
                {
                    res.IsSuccess = false;
                    res.Message = $"Contact with ID {id} not found";
                    res.Data = null;
                }

            }
            catch (Exception ex)
            {
                res.Message = $"Error fetching contact: {ex.Message}";
            }
            return Ok(res);

        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateContact([FromBody] Sp_Contact model)
        {
            IResponseVm<Sp_Contact> res = new ResponseVm<Sp_Contact>();

            if (model == null)
            {
                res.Message = "Contact data is null";
            }
            try
            {
                await _contactService.Create_Contact(model);
                res.IsSuccess = true;
                res.Message = "Contact added Successfully";
                res.Data = model;
                 CreatedAtAction(
                           nameof(GetContactById),
                           new { id = model.ContactId },
                           model);
                res.TotalRecords = 1;
            }
            catch (Exception ex)
            {
                res.Message = $"Error creating contact: {ex.Message}";
            }
            return Ok(res );    
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateContact(Guid id, [FromBody] Sp_Contact model)
        {
            IResponseVm<Sp_Contact> res = new ResponseVm<Sp_Contact>();

            if (model == null || id != model.ContactId)
            {
                res.Message = "Contact data is invalid or ID mismatch";
            }

            try
            {
                var existingContact = await _contactService.GetById_Contact(id);

                if (existingContact == null)
                {
                    res.Message = "Contact not found.";
                }
  
//            model.ContactId = id;
//            await _contactService.Update_Contact(model);
//            res.IsSuccess = true;
//            res.Message = $"{AppConstant.UpdateSuccessMsg}";
//            return Ok(res);
//        }

                existingContact.ContactId = model.ContactId;
                existingContact.ContactType = model.ContactType;
                existingContact.Name = model.Name;
                existingContact.PhoneNumber = model.PhoneNumber;
                existingContact.Email = model.Email;
                existingContact.Website = model.Website;
                existingContact.Notes = model.Notes;
                existingContact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;

                if (!existingContact.IsBillingAddressSameAsAddress)
                {
                    existingContact.billingAddressVm.BillingAddessId = model.billingAddressVm.BillingAddessId;
                    existingContact.billingAddressVm.Label = model.billingAddressVm.Label;
                    existingContact.billingAddressVm.AddressLine1 = model.billingAddressVm.AddressLine1;
                    existingContact.billingAddressVm.AddressLine2 = model.billingAddressVm.AddressLine2;
                    existingContact.billingAddressVm.City = model.billingAddressVm.City;
                    existingContact.billingAddressVm.State = model.billingAddressVm.State;
                    existingContact.billingAddressVm.ZipCode = model.billingAddressVm.ZipCode;
                    existingContact.billingAddressVm.Country = model.billingAddressVm.Country;

                }
                await _contactService.Update_Contact(model);
                res.IsSuccess = true;
                res.Message = "Contact updated successfully";
                res.Data = model;
                CreatedAtAction(
                  nameof(GetContactById),
                  new { id = model.ContactId },
                 model);
            }
            catch (Exception ex)
            {
                res.Message = $"Error updating contact: {ex.Message}";
            }
            return Ok(res); 
        }

        [HttpPost("ActiveInactive")]
        public async Task<IActionResult> ActiveInActiveContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.ActiveInActiveContact(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.DeleteContact(guid);

            return Ok(res);
        }
    }
}

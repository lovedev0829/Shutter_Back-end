using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.GHL.Services;
using ShutterManagment.GHL.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly GhlService ghlService;

        public ContactController(IContactService contactService,GhlService ghlService)
        {
            _contactService = contactService;
            this.ghlService = ghlService;
        }

        [HttpPost("GetAll")]
        [Authorize(Roles = "ContactView,PartnerContactView")]
        public async Task<IActionResult> GetAllContacts(Pagination filter)
        {
            bool isInContactViewRole = HttpContext.User.IsInRole("PartnerContactView");
            var res = await _contactService.GetAll_Contact(filter, isInContactViewRole);
            res.Data = res.Data?.Where(x => x.IsArchive == false).ToList();
            return Ok(res);
        }


        [HttpPost("Create")]
        [Authorize(Roles = "ContactAdd")]
        public async Task<IActionResult> CreateContact([FromBody] Sp_Contact model)
        {

            var res = await _contactService.Create_Contact(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        [Authorize(Roles = "ContactEdit")]
        public async Task<IActionResult> UpdateContact([FromBody] UpdateContact model)
        {

            var res = await _contactService.Update_Contact(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize(Roles = "ContactEdit")]
        public async Task<IActionResult> ActiveInActiveContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.ActiveInActiveContact(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "ContactDelete")]
        public async Task<IActionResult> DeleteContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.DeleteContact(guid);

            return Ok(res);
        }

        [HttpPost("GetAllArchiveContacts")]
        [Authorize(Roles = "ArchiveCustomerView")]
        public async Task<IActionResult> GetAllArchiveContacts(Pagination filter)
        {
            var res = await _contactService.GetAll_Contact(filter, false);
            // res.Data = res.Data?.Where(x => x.De == true && x.IsActive==false).ToList();
            return Ok(res);
        }
        [HttpPost("ArchiveContacts")]
        [Authorize]
        public async Task<IActionResult> ArchiveContact(ArchiveUser archiveModel)
        {
            var res = await _contactService.ArchiveContacts(archiveModel);
            return Ok(res);
        }


        [HttpGet]
        [Route("GHLContact")]
        [AllowAnonymous]
        public async Task<IActionResult> GHLContact()
        {
            var res = await ghlService.GetContactsAsync();
            return Ok(res);
        }


        [HttpPost("ContactFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> ContactFromGHL(GHLContactTrigger contactData)
        {
            try
            {
                var res = await _contactService.ContactFromGHL(contactData);
                FileLogger.Log("Contact saved", res.Message);
                return Ok(res);
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                return StatusCode(500, $"Internal server error by application: {ex.Message}");
            }
        }

        [HttpPost("UpdateContactFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateContactFromGHL(GHLContactTrigger contactData)
        {
            try
            {
                var res = await _contactService.UpdateContactFromGHL(contactData);
                FileLogger.Log("Contact updated", res.Message);
                return Ok(res);
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                return StatusCode(500, $"Internal server error by application: {ex.Message}");
            }
        }

    }
}


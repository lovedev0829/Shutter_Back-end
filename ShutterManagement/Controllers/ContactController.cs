using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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
        [Authorize(Roles = "ContactView")]
        public async Task<IActionResult> GetAllContacts(Pagination filter)
        {
            IResponseVm<List<Sp_Contact>> res = new ResponseVm<List<Sp_Contact>>();   
            try
            {
                var contacts = await _contactService.GetAll_Contact(filter);
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

        [HttpPost("Create")]
        [Authorize(Roles = "ContactAdd")]
        public async Task<IActionResult> CreateContact([FromBody] Sp_Contact model)
        {
            
             var res =  await _contactService.Create_Contact(model);
            return Ok(res );    
        }

        [HttpPost("Update")]
        [Authorize(Roles = "ContactEdit")]
        public async Task<IActionResult> UpdateContact([FromBody] Sp_Contact model)
        {
           
            var updatedContact = _contactService.Update_Contact(model);
            return Ok(updatedContact);  
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
    }
}

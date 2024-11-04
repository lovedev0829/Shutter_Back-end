using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShutterManagement.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        [Route("ContactCreate")]
        public async Task<IActionResult> CreateContact([FromBody] Sp_Contact model)
        {
            var res = new ResponseVm<Sp_Contact>();

            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                res.Message = "Contact model is invalid." + $"{AppConstant.ModelError}";
                return BadRequest(res);
            }

            try
            {
                await _contactService.Create_Contact(model);
                res.IsSuccess = true;
                res.Message = $"{AppConstant.InsertSuccessMsg}";
                return Ok(res);
            }
            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while creating the contact: " + ex.Message;
                return StatusCode(500, res);
            }
        }


        [HttpPut]
        [Route("ContactUpdate/{id}")]
        public async Task<IActionResult> UpdateContact(Guid id, [FromBody] Sp_Contact model)
        {
            var res = new ResponseVm<Sp_Contact>();
            if (model == null || id == Guid.Empty)
            {
                res.Message = "Invalid contact information.";
            }

            model.ContactId = id;
            await _contactService.Update_Contact(model);
            res.IsSuccess = true;
            res.Message = $"{AppConstant.UpdateSuccessMsg}";
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllContacts")]
        public async Task<IActionResult> GetAllContacts()
        {
            IResponseVm<List<Sp_Contact>> res = new ResponseVm<List<Sp_Contact>>();
            var contacts = await _contactService.GetAll_Contact();
            res.IsSuccess = true;
            res.Message = $"{AppConstant.FetchSuccessMsg}";
            res.Data = contacts;
            return Ok(res);
        }

        [HttpGet]
        [Route("GetContactById/{id}")]
        public async Task<IActionResult> GetContactById(Guid id)
        {
            IResponseVm<Sp_Contact> res = new ResponseVm<Sp_Contact>();
            if (id == Guid.Empty)
            {
                res.Message = $"{AppConstant.InvalidGuid}";
            }

            var contact = await _contactService.GetById_Contact(id);
            if (contact == null)
            {
                res.Message = "Contact not found.";
            }

            res.IsSuccess = true;
            res.Message = $"{AppConstant.FetchSuccessMsg}";
            res.Data = contact;
            return Ok(res);
        }

        [HttpDelete]
        [Route("ContactDelete/{id}")]
        public async Task<IActionResult> DeleteContact(Guid id)
        {
            var res = new ResponseVm<Sp_Contact>();
            if (id == Guid.Empty)
            {
                res.Message = $"{AppConstant.InvalidGuid}";
            }

            await _contactService.Delete_Contact(id);
            res.IsSuccess = true;
            res.Message = $"{AppConstant.DeletedSuccessMsg}";
            return Ok(res);
        }
    }
}

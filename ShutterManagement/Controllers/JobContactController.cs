using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobContactController : ControllerBase
    {
        private readonly IJobContactService jobContactService;

        public JobContactController(IJobContactService jobContactService)
        {
            this.jobContactService = jobContactService;
        }

        [HttpPost("Getall")]
        //[Authorize(Roles = "JobContacteView")]
        public async Task<IActionResult> Getall_JobContact(Pagination pagination)
        {
            var res = await jobContactService.GetAll_JobContact(pagination);
            return Ok(res);
        }

        [HttpPost("Add")]
        //[Authorize(Roles = "JobContacteAdd")]
        public async Task<IActionResult> AddJobContact(CreateJobContact model)
        {
            var res = await jobContactService.Create_JobContact(model);
            return Ok(res);
        }

        [HttpPost("Delete")]
        //[Authorize(Roles = "JobContacteDelete")]
        public async Task<IActionResult> DeleteJobContact(Guid? JobContactId)
        {
            var res = await jobContactService.DeleteJobContact(JobContactId);
            return Ok();
        }

        [HttpPost("Edit")]
        //[Authorize(Roles = "JobContacteEdit")]
        public async Task<IActionResult> EditJobContact(UpdateJobContact model)
        {
            var res = await jobContactService.Update_JobContact(model);
            return Ok();
        }
       

    }
}

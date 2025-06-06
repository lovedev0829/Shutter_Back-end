using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class JobTitleController : ControllerBase
    {
        private readonly IJobTitleService jobTitleService;

        public JobTitleController(IJobTitleService jobTitleService)
        {
            this.jobTitleService = jobTitleService;
        }
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "JobTitleAdd")]
        public async Task<IActionResult> Create_JobTitle([FromBody] CreateJobTitle model)
        {
            var res = await jobTitleService.Create_Title(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "JobTitleEdit")]
        public async Task<IActionResult> Update_JobTitle(UpdateJobTitle model)
        {
            var res = await jobTitleService.Update_Title(model);
            return Ok(res);
        }
        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "JobTitleEdit")]
        public async Task<IActionResult> ActiveInActive(Guid? JobTitleId)
        {
            var res = await jobTitleService.ActiveInActive(JobTitleId);
            return Ok(res);
        }
        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "JobTitleDelete")]
        public async Task<IActionResult> DeleteJobTitle(Guid? JobTitleId)
        {
            var res = await jobTitleService.DeleteTitle(JobTitleId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "JobTitleView")]
        public async Task<IActionResult> GetAllJobTitle(Pagination filter)
        {
            var res = await jobTitleService.GetAll(filter);
            return Ok(res);
        }
    }
    
}

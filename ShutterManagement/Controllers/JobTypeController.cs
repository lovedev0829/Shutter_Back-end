using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTypeController : ControllerBase
    {
        private readonly IJobTypeService jobTypeService;

        public JobTypeController(IJobTypeService jobTypeService)
        {
            this.jobTypeService = jobTypeService;
        }
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "JobTypeAdd")]
        public async Task<IActionResult> Create_JobType([FromBody] CreateJobType model)
        {
            var res = await jobTypeService.Create_JobType(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "JobTypeEdit")]
        public async Task<IActionResult> Update_JobType(UpdateJobType model)
        {
            var res = await jobTypeService.Update_JobType(model);
            return Ok(res);
        }
        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "JobTypeEdit")]
        public async Task<IActionResult> ActiveInActive(Guid? JobTypeId)
        {
            var res = await jobTypeService.ActiveInActive(JobTypeId);
            return Ok(res);
        }
        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "JobTypeDelete")]
        public async Task<IActionResult> DeleteJobType(Guid? JobTypeId)
        {
            var res = await jobTypeService.DeleteJobType(JobTypeId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "JobTypeView")]
        public async Task<IActionResult> GetAllJobType(Pagination filter)
        {
            var res = await jobTypeService.GetAllJobType(filter);
            return Ok(res);
        }
    }
}

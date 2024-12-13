using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstimateController : ControllerBase
    {
        private readonly IEstimateService _EstimateService;

        public EstimateController(IEstimateService EstimateService)
        {
            _EstimateService = EstimateService;
        }

        [HttpPost("GetAll")]
        [Authorize(Roles = "EstimateView")]
        public async Task<IActionResult> GetAllEstimates(Pagination filter)
        {
            var res = await _EstimateService.GetAll_Estimate(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        [Authorize(Roles = "EstimateAdd")]
        public async Task<IActionResult> CreateEstimate([FromBody] CreateEstimate model)
        {

            var res = await _EstimateService.Create_Estimate(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        [Authorize(Roles = "EstimateEdit")]
        public async Task<IActionResult> UpdateEstimate([FromBody] UpdatetEstimate model)
        {

            var res = await _EstimateService.Update_Estimate(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize(Roles = "EstimateEdit")]
        public async Task<IActionResult> ActiveInActiveEstimate(string EstimateId)
        {
            Guid guid = Guid.Parse(EstimateId);
            var res = await _EstimateService.ActiveInActiveEstimate(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "EstimateDelete")]
        public async Task<IActionResult> DeleteEstimate(string EstimateId)
        {
            Guid guid = Guid.Parse(EstimateId);
            var res = await _EstimateService.DeleteEstimate(guid);

            return Ok(res);
        }

        [HttpPost("SendForApproval")]
        [Authorize(Roles = "EstimateEdit")]
        public async Task<IActionResult> SendForApproval(Guid EstimateId)
        {
            var res = await _EstimateService.SendForApproval(EstimateId);
            return Ok(res);
        }

    }
}

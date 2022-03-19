using Home.Services.Vk;
using Home.Web.Areas.App.Models.Vk;
using Home.Web.Areas.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Home.Web.Areas.App.Controllers
{
    [Area("app")]
    [Route("app/[controller]")]
    public class VkController : Controller
    {
        private readonly IActivityAnalyzerService _activityAnalyzerService;
        private readonly ILogger<VkController> _logger;
        private readonly VkMapper _mapper = new VkMapper();

        public VkController(IActivityAnalyzerService activityAnalyzerService, ILogger<VkController> logger)
        {
            _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new VkVM());
        }

        [HttpGet]
        [Route("AjaxAddNewVkUser")]
        public Task<IActionResult> AjaxAddNewVkUser(int id)
        {
            throw new NotImplementedException();
            //var result = await _activityAnalyzerService.AddNewVkUser(id);
            //return result.IsSuccess ? Ok(_mapper.ToNewVkUserResult(result)) : StatusCode(500);
        }

        [HttpGet]
        [Route("AjaxGetUsers")]
        public async Task<IActionResult> AjaxGetUsers(string filterText)
        {
            var result = await _activityAnalyzerService.GetUsersAsync(filterText);
            // TODO: handle error
            return PartialView("_Users", _mapper.ToUsersVM(result));
        }

        [HttpGet]
        [Route("AjaxGetUsersWithActivity")]
        public async Task<IActionResult> AjaxGetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
        {
            //_logger.LogInformation("AjaxGetUsersWithActivity");

            var result = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
            return PartialView("_Users", _mapper.ToUsersVMWithActivity(result));
        }

        [HttpGet]
        [Route("AjaxGetUserActivity")]
        public async Task<IActionResult> AjaxGetUserActivity(int userId, DateTime fromDate, DateTime toDate)
        {
            if (userId == 0)
                return null;
            var result = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
            // TODO: handle error
            return PartialView("_UserActivity", _mapper.ToPeriodUserActivityVM(result));
        }

        [HttpGet]
        [Route("AjaxGetDetailedUserActivity")]
        public async Task<IActionResult> AjaxGetDetailedUserActivity(int userId)
        {
            var result = await _activityAnalyzerService.GetFullTimeUserStatisticsAsync(userId);
            // TODO: handle error
            return PartialView("_DetailedUserActivity", _mapper.ToDetailedUserActivityVM(result));
        }
    }
}

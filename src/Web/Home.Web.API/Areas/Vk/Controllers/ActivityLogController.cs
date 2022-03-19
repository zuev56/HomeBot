using AutoMapper;
using Home.Services.Vk;
using Home.Web.API.Areas.Vk.Models;
using Home.Web.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Home.Web.API.Areas.Vk.Controllers
{
    [Area("vk")]
    [Route("api/vk/[controller]")] // глобальный префикс для маршрутов
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [ApiController] // Реализует проверку модели и возвращает 400, если она не валидна
    public class ActivityLogController : Controller
    {
        private readonly IActivityAnalyzerService _activityAnalyzerService;
        private readonly ILogger<ActivityLogController> _logger;
        private readonly IMapper _mapper;

        public ActivityLogController(
            IActivityAnalyzerService activityAnalyzerService,
            IMapper mapper,
            ILogger<ActivityLogController> logger)
        {
            _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }

        [HttpGet(nameof(GetUsersWithActivity))]
        public async Task<IActionResult> GetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
        {
            var usersWithActivityResult = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
            usersWithActivityResult.AssertResultIsSuccessful();

            return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
        }

        //[HttpGet(nameof(GetUsersWithActivity))]
        //public async Task<IActionResult> GetUsersWithActivity(TableParameters requestParameters)
        //{
        //    // TODO: In future...
        //    throw new NotImplementedException();
        //    //var usersWithActivityResult = await _vkActivityService.GetUsersWithActivityTable(requestParameters);
        //    //usersWithActivityResult.AssertResultIsSuccessful();
        //    //
        //    //return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
        //}


        [HttpGet(nameof(GetPeriodInfo))]
        public async Task<IActionResult> GetPeriodInfo(int userId, DateTime fromDate, DateTime toDate)
        {
            var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
            periodStatisticsResult.AssertResultIsSuccessful();

            return Ok(_mapper.Map<PeriodInfoDto>(periodStatisticsResult.Value));
        }

        [HttpGet(nameof(GetFullTimeInfo))]
        public async Task<IActionResult> GetFullTimeInfo(int userId)
        {
            var fullTimeStatistictResult = await _activityAnalyzerService.GetFullTimeUserStatisticsAsync(userId);
            fullTimeStatistictResult.AssertResultIsSuccessful();

            return Ok(_mapper.Map<FullTimeInfoDto>(fullTimeStatistictResult.Value));
        }



        [HttpGet]
        public IActionResult Test()
        {
            try
            {
                return Ok("Test");
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    InnerExceptionMessage = ex.InnerException?.Message,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace
                });
                //return StatusCode(500);
            }
        }
    }
}

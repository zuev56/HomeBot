using Home.Services.Vk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Home.Web.Areas.ApiVk.Controllers
{
    [Area("apivk")]
    [Route("apivk/[controller]")] // необходим для формирования полного маршрута при использовании короткого маршрута в HttpGet
    public class ActivityLogController : Controller
    {
        private readonly IActivityAnalyzerService _activityAnalyzerService;
        //private readonly IZsLogger _logger;

        public ActivityLogController(IActivityAnalyzerService activityAnalyzerService)
        {
            _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
        }

        [HttpGet("test", Name = "Test1")]
        public IActionResult Test(ushort page = 1)
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


        [HttpGet("User/{id?}", Name = "GetUserActivity")]
        public async Task<IActionResult> Get(int id = 0)
        {
            try
            {
                var fromDate = DateTime.UtcNow.Date - TimeSpan.FromDays(2);
                var toDate = DateTime.UtcNow.Date - TimeSpan.FromDays(1);

                var result = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(id, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, nameof(ActivityLogController));
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

        //// POST api/<ActivityLogController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}
        //
        //// PUT api/<ActivityLogController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}
        //
        //// DELETE api/<ActivityLogController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}

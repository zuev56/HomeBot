using Home.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Home.Web.Areas.Admin.Controllers;

/// <summary>
/// Shows information about sprcific services
/// </summary>
[Area("admin")]
public class ServicesInfoController : Controller
{
    private readonly ServicesInfoService _servicesInfoService;

    public ServicesInfoController(IConfiguration configuration)
    {
        _servicesInfoService = new ServicesInfoService(configuration);
    }

    public IActionResult Index()
    {
        return View(_servicesInfoService.GetServicesInfoList());
    }
}
using Home.Web.Areas.Admin.Models.VisitorInfo;
using Microsoft.AspNetCore.Mvc;

namespace Home.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Shows information about visitor
    /// </summary>
    [Area("admin")]
    public class VisitorInfoController : Controller
    {
        public IActionResult Index()
        {
            var visitor = new Visitor(HttpContext);

            return View(visitor);
        }
    }
}

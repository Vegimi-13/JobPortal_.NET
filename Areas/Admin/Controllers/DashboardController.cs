using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobPortal_ServerSide.Data;

namespace JobPortal_ServerSide.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var stats = new
            {
                Users = _context.Users.Count(),
                Developers = _context.DeveloperProfiles.Count(),
                Companies = _context.Companies.Count()
            };

            return View(stats);
        }
    }
}

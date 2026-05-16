using Microsoft.AspNetCore.Mvc;
using SchoolProject.Services;

namespace SchoolProject.ViewComponents
{
    public class NavViewComponent : ViewComponent
    {
        private readonly NavService _navService;

        public NavViewComponent(NavService navService)
        {
            _navService = navService;
        }

        public IViewComponentResult Invoke()
        {
            var items = _navService.GetSchoolNavItems("bangalore");
            return View(items);
        }
    }
}
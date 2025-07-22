using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/Antiforgery")]
        public IActionResult Antiforgery()
        {
            return View();
        }
        [Route("Error/500")]
        public IActionResult HandleServerError()
        {
            return View("serverError");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("notFound");
                case 403:
                    return View("accessDenied");
                default:
                    return View("serverError");
            }
        }
    }

    


}

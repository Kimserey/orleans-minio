using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OrleansMinio.Client
{
    public class TestOptionsController : Controller
    {
        private TestOptions _options;

        public TestOptionsController(IOptionsSnapshot<TestOptions> optionsAccessor)
        {
            _options = optionsAccessor.Get("test");
        }

        [HttpGet("/test")]
        public IActionResult Get()
        {
            return Json("Options: " + _options.ValueOne + " " + _options.ValueTwo);
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TestSMSThrottleService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotaController : ControllerBase
    {
        private readonly Services.QuotaService _quotaService;

        public QuotaController(Services.QuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        [HttpGet]
        public JsonResult Index(string? number)
        {
            return _quotaService.Status(number);
        }

        [HttpPost]
        public bool CountAndCheck(string number)
        {
            return _quotaService.CountAndCheck(number);
        }

    }
}

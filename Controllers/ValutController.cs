using CurrencyWatcher.DAL.Interfaces;
using CurrencyWatcher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyWatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValutController : ControllerBase
    {
        private readonly IValutService _valutService;

        public ValutController(IValutService valutService)
        {
            _valutService = valutService;
        }

        [HttpGet("~/FillDb")]
        public string FillDb(DateTime date)
        {
            return _valutService.FillDb(date);
        }

        [HttpGet("~/GetJsonReport")]
        public string GetJsonReport(DateTime from, DateTime to, string valutList)
        {
            return _valutService.GetJsonReport(from, to, valutList);   
        }
    }
}

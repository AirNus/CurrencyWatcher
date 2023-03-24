using CurrencyWatcher.DAL.Interfaces;
using CurrencyWatcher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyWatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("~/FillDbForDay")]
        public string FillDb(DateTime date)
        {
            return _currencyService.FillDb(date);
        }

        [HttpGet("~/FillDbForYear")]
        public string FillDbForYear(DateTime from, DateTime to, string valutList)
        {
            return _currencyService.FillDbForYear(from, to, valutList);
        }

        [HttpGet("~/GetJsonReport")]
        public string GetJsonReport(DateTime from, DateTime to, string valutList)
        {
            return _currencyService.GetJsonReport(from, to, valutList);   
        }
    }
}

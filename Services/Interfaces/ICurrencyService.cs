namespace CurrencyWatcher.Services.Interfaces
{
    public interface ICurrencyService
    {
        public string FillDbForYear(DateTime from, DateTime to, string valutList);
        public string GetJsonReport(DateTime from, DateTime to, string valutList);
        public string FillDb(DateTime date);
    }
}

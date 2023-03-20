namespace CurrencyWatcher.Services.Interfaces
{
    public interface IValutService
    {
        public string GetJsonReport(DateTime from, DateTime to, string valutList);
        public string FillDb(DateTime date);
    }
}

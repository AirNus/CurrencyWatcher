using System.Diagnostics.CodeAnalysis;

namespace CurrencyWatcher.DAL
{
    public class Currency
    {
        public int Id { get; set; }

        public DateTime Dadd { get; set; }

        public string? Name { get; set; }

        public int Amount { get; set; }

        public string Code { get; set; }

        public double Rate { get; set; }
    }
}

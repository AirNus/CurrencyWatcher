using Newtonsoft.Json;

namespace CurrencyWatcher.DAL
{
    public class JsonReport
    {
        [JsonIgnore]
        public int Index { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }

        [JsonIgnore]
        public List<double> Values { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Avg { get; set; }
    }
}

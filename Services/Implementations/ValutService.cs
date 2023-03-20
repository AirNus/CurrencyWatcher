using CurrencyWatcher.DAL;
using CurrencyWatcher.DAL.Interfaces;
using CurrencyWatcher.Services.Interfaces;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace CurrencyWatcher.Services.Implementations
{
    public class ValutService : IValutService
    {
        private readonly IBaseRepository<Valut> _valutRepository;
        private ILogger<ValutService> _logger;
        
        public ValutService(IBaseRepository<Valut> valutRepository, ILogger<ValutService> logger)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            _valutRepository = valutRepository;
            _logger = logger;
        }

        public string FillDb(DateTime date)
        {
            string _url = "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt?date=";
            HttpWebRequest request;
            HttpWebResponse response;
            var valutList = new List<Valut>();
            
            request = (HttpWebRequest) WebRequest.Create(_url + date.ToString());
            try
            {
                response =  (HttpWebResponse) request.GetResponse();
            }
            catch(WebException ex)
            {
                response = (HttpWebResponse) ex.Response;
            }

            var stream = response.GetResponseStream();
            //var sr = new StreamReader(stream,Encoding.GetEncoding(response.CharacterSet));


            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        var parser = new TextFieldParser(stream, Encoding.GetEncoding(response.CharacterSet));
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters("|");
                        parser.ReadFields();
                        parser.ReadFields();
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

                        while (!parser.EndOfData)
                        {
                            string[] rows = parser.ReadFields();

                            if (!valutList.Exists(x => x.Code == rows[3]))
                            {
                                valutList.Add(new Valut
                                {
                                    Dadd = date,
                                    Currency = rows[1],
                                    Amount = Convert.ToInt32(rows[2]),
                                    Code = rows[3],
                                    Rate = Convert.ToDouble(rows[4])
                                });
                            }
                        }

                        break;
                    }
                case HttpStatusCode.NotFound:
                    return "Данных не найдено";
                default:
                    return "Что то пошло не так";
            }
            
            if (valutList.Count > 0)
            {
                foreach(var valut in valutList)
                {
                    _valutRepository.Add(valut);
                }
            }
            return "Конец метода";


        }

        public string GetJsonReport(DateTime from, DateTime to, string valutList)
        {
            string _url = "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/year.txt?year=";
            var years = to.Year - from.Year;
            HttpWebRequest request;
            HttpWebResponse response;
            string[] selectedValuts = valutList.Split(',');
            var selectedValutsList = new List<JsonReport>();

            if (selectedValuts.Length == 0 || valutList == string.Empty)
            {
                return "Не заданы валюты";
            }

            for (int i = 0; i <= years; i++)
            {
                request = (HttpWebRequest)WebRequest.Create(_url + (from.Year + i).ToString());
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                //var sr = new StreamReader(stream,Encoding.GetEncoding(response.CharacterSet));
                //string data = sr.ReadToEnd();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            var parser = new TextFieldParser(stream, Encoding.GetEncoding(response.CharacterSet));
                            parser.TextFieldType = FieldType.Delimited;
                            parser.SetDelimiters("|");

                            if(!parser.EndOfData)
                            {
                                string[] headers = parser.ReadFields();

                                foreach (var valut in selectedValuts)
                                {
                                    var index = headers
                                        .Select((element,indx) => new KeyValuePair<string,int>(element,indx))
                                        .FirstOrDefault(x => x.Key.Contains(valut));
                                    if (index.Key is not null)
                                    {
                                        var codeAndAmount = headers[index.Value].Split(' ');
                                        var existsValut = selectedValutsList.FirstOrDefault(x => x.Currency == codeAndAmount[1]);
                                        if (existsValut is null)
                                        {
                                            selectedValutsList.Add(new JsonReport
                                            {
                                                Index = index.Value,
                                                Amount = Convert.ToInt32(codeAndAmount[0]),
                                                Currency = codeAndAmount[1],
                                                Values = new List<double>()
                                            });
                                        }
                                    }
                                }

                                while (!parser.EndOfData)
                                {
                                    string[] rows = parser.ReadFields();

                                    if (!rows[0].Contains("Date"))
                                    {
                                        var currDate = DateTime.ParseExact(rows[0], "dd/MM/yyyy", new CultureInfo("ru-RU"));
                                        if (currDate > from && currDate < to)
                                        {
                                            foreach (var valut in selectedValutsList)
                                            {
                                                valut.Values.Add(Convert.ToDouble(rows[valut.Index]) / valut.Amount);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case HttpStatusCode.NotFound:
                        return "Данных не найдено";
                    default:
                        return "Что то пошло не так";
                }
            }

            string resultJson = "Валюты не были найдены";
            if (selectedValutsList.Count > 0)
            {
                foreach(var valut in selectedValutsList)
                {
                    valut.Min = valut.Values.Min();
                    valut.Max = valut.Values.Max();
                    valut.Avg = valut.Values.Average();
                }

                resultJson = JsonConvert.SerializeObject(selectedValutsList, Formatting.Indented);
            }
            return resultJson;
        }
    }
}


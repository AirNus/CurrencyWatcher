using Azure.Core;
using Azure;
using CurrencyWatcher.DAL;
using CurrencyWatcher.DAL.Interfaces;
using CurrencyWatcher.Services.Interfaces;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace CurrencyWatcher.Services.Implementations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IBaseRepository<Currency> _currencyRepository;
        private ILogger<CurrencyService> _logger;
        
        public CurrencyService(IBaseRepository<Currency> valutRepository, ILogger<CurrencyService> logger)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            _currencyRepository = valutRepository;
            _logger = logger;
        }

        public string FillDb(DateTime date)
        {
            string _url = "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt?date=";
            HttpWebRequest request;
            HttpWebResponse response;
            var valutList = new List<Currency>();
            
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
                                valutList.Add(new Currency
                                {
                                    Dadd = date,
                                    Name = rows[1],
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
                    _currencyRepository.Add(valut);
                }
            }
            return "Конец метода";


        }

        public string GetJsonReport(DateTime from, DateTime to, string valutList)
        {
            var selectedCurrencyList = new List<JsonReport>();
            
            var years = to.Year - from.Year;
            
            string[] selectedCurrency = valutList.Split(',');

            if (selectedCurrency.Length == 0 || valutList == string.Empty)
            {
                return "Не заданы валюты";
            }

            string resultJson = "Валюты не были найдены";

            foreach (var currency in selectedCurrency)
            {
                var list = _currencyRepository.GetAll().Where(x => x.Code == currency && x.Dadd > from && x.Dadd < to).ToList();

                if (list.Count > 0)
                {
                    selectedCurrencyList.Add(new JsonReport()
                    {
                        Amount = list[0].Amount,
                        Currency = currency,
                        Max = list.Max(x => x.Rate),
                        Min = list.Min(x => x.Rate),
                        Avg = list.Average(x => x.Rate)
                    });

                    resultJson = JsonConvert.SerializeObject(selectedCurrencyList, Formatting.Indented);
                }
            }


            return resultJson;
        }


        public string FillDbForYear(DateTime from, DateTime to, string valutList)
        {
            var years = to.Year - from.Year;
            string[] selectedCurrency = valutList.Split(',');
            List<JsonReport> selectedCurrencyList;

            var currencyList = new List<Currency>();

            if (selectedCurrency.Length == 0 || valutList == string.Empty)
            {
                return "Не заданы валюты";
            }

            //Перед заполнением удаляем старые данные за тот же период с той же валютой
            foreach (var currency in selectedCurrency)
            {
                    _currencyRepository.DeleteList(from,to,currency);               
            }

            for (int i = 0; i <= years; i++)
            {
                selectedCurrencyList = GetDataForYear((from.Year + i), selectedCurrency);

                if (selectedCurrencyList is not null && selectedCurrencyList.Count > 0)
                {
                    foreach (var currency in selectedCurrencyList)
                    {
                        foreach (var dailyCurrency in currency.Values)
                        {

                            Currency _currency = new Currency()
                            {
                                Dadd = dailyCurrency.Key,
                                Amount = currency.Amount,
                                Code = currency.Currency,
                                Rate = dailyCurrency.Value
                            };

                            currencyList.Add(_currency);
                        }
                    }
                }
            }    

            _currencyRepository.AddList(currencyList);
            return "Конец метода";
        }


        public List<JsonReport> GetDataForYear(int year, string[] selectedCurrency)
        {
            var selectedCurrencyList = new List<JsonReport>();
            
            HttpWebRequest request;
            HttpWebResponse response;
            
            string _url = "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/year.txt?year=";
            request = (HttpWebRequest)WebRequest.Create(_url + year.ToString());
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

                        if (!parser.EndOfData)
                        {
                            string[] headers = parser.ReadFields();

                            foreach (var valut in selectedCurrency)
                            {
                                var index = headers
                                    .Select((element, indx) => new KeyValuePair<string, int>(element, indx))
                                    .FirstOrDefault(x => x.Key.Contains(valut));
                                if (index.Key is not null)
                                {
                                    var codeAndAmount = headers[index.Value].Split(' ');
                                    var existsValut = selectedCurrencyList.FirstOrDefault(x => x.Currency == codeAndAmount[1]);
                                    if (existsValut is null)
                                    {

                                        selectedCurrencyList.Add(new JsonReport
                                        {
                                            Index = index.Value,
                                            Amount = Convert.ToInt32(codeAndAmount[0]),
                                            Currency = codeAndAmount[1],
                                            Values = new Dictionary<DateTime, double>()
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
                                    foreach (var valut in selectedCurrencyList)
                                    {
                                        valut.Values.Add(currDate,Convert.ToDouble(rows[valut.Index]) / valut.Amount);

                                    }
                                }
                            }
                        }
                        break;
                    }
                case HttpStatusCode.NotFound:
                    return null;
                default:
                    return null;
            }
            return selectedCurrencyList;
        }
    }
}


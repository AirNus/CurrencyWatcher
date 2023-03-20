using CurrencyWatcher.Controllers;
using CurrencyWatcher.DAL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;

namespace CurrencyWatcher
{
    public class TimeChecker
    {
        public static async Task StartTimer(string startTime)
        {

            TimerCallback tm = new TimerCallback(CheckTime);
            Timer timer = new Timer(tm,startTime,0,60000);

        }

        public static async void CheckTime(object startTime)
        { 
            DateTime executeTime = Convert.ToDateTime(startTime.ToString());

            if (executeTime.Hour == DateTime.Now.Hour && executeTime.Minute == DateTime.Now.Minute)
            {

                string _url = "https://localhost:7252/FillDb?date=" + executeTime.Date.ToString("MM.dd.yyyy");
                HttpWebRequest request;
                HttpWebResponse response;

                request = (HttpWebRequest)WebRequest.Create(_url);
                try
                {
                    request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }
            }
        }

    }
}

using AmazonProductAdvtApi;
using AmazonTest.App;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace AmazonTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            
            List<App.Book> books = new List<App.Book>();

            if (Request["search"] != null && Request["search"].Length > 0)
            {
                ViewBag.Search = Tools.CleanInput(Request["search"]);
                // TODO Constructor taking search text doesn't make sense.
                App.Requester request = new App.Requester();
                books = request.ItemSearch(Request["search"]);
            }

            ViewBag.Books = books;
            return View();
        }

        /// <summary>
        /// Currency conversion... used by Ajax on the main page.
        /// </summary>
        /// <param name="currentCurrency"></param>
        /// <param name="newCurrency"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReturnRatio(string currentCurrency, string newCurrency)
        {
            string key = APIKey.GetExchangeKey();

            // Retrieving data.
            string url = "https://openexchangerates.org/api/latest.json?app_id=" + key;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            string jsonString;

            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                jsonString = sr.ReadToEnd();
            }

            dynamic json = JsonConvert.DeserializeObject(jsonString);

            // This is an unfortunate workaround because OpenExchangeRates 
            // have very limited support for Free API keys. 
            // I had to get each currency as USD to get their actual
            // ratio.

            double CurrentAsUSD = 1.0d / (double)json.rates[currentCurrency];
            double NewAsUSD = 1.0 / (double)json.rates[newCurrency];

            double result =  CurrentAsUSD / NewAsUSD;

            return Json(
                    result, 
                    JsonRequestBehavior.AllowGet
                );
        }
    }
}
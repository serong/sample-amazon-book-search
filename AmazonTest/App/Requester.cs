using AmazonProductAdvtApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AmazonTest.App
{
    /// <summary>
    /// Makes requests to the API system and returns 
    /// results.
    /// </summary>
    public class Requester
    {
        private string searchTerm;                      // The search term to make the request with.
        private long lastAPICall;                       // Ms since Epoch time. 

        // Just in case the search returns a lot of results.
        // Since this is a test webapp, I didn't want to be returning
        // all the results while abiding by the "1s between API calls" rule.
        private readonly int MAXPAGES = 3;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="searchTerm"></param>
        public Requester(string searchTerm = "")
        {
            this.searchTerm = searchTerm;
            UpdateLastCall();
        }

        /// <summary>
        /// Sets a new search term for the next search and so on.
        /// </summary>
        /// <param name="newTerm"></param>
        public void SetSearchTerm(string newTerm)
        {
            this.searchTerm = newTerm;
        }

        /// <summary>
        /// Creates a request url as a string. Combining API credentials
        /// with the base url.
        /// </summary>
        /// <param name="page">Default page number</param>
        /// <returns></returns>
        private string PrepareURL(int page)
        {
            // Creating and signing the request URL with SignedRequestHelper class.
            SignedRequestHelper helper = new SignedRequestHelper(APIKey.GetApiKey(), APIKey.GetSecKey(), APIKey.GetDestination());

            String requestString =
                "Service=AWSECommerceService" +
                "&Operation=ItemSearch" +
                "&AssociateTag=" + APIKey.GetAscKey() +
                "&SearchIndex=Books" +
                "&Keywords=" + this.searchTerm +
                "&Sort=salesrank" +
                "&ItemPage=" + page +
                "&ResponseGroup=Images,ItemAttributes";

            String baseUrl = helper.Sign(requestString);
            Debug.WriteLine("\n baseURL: " + baseUrl);

            return baseUrl;
        }

        /// <summary>
        /// Makes the call to the API and returns the result as a string, 
        /// which is later parsed.
        /// 
        /// NOTE: Not used. Originally this would be used my MakeXML().
        /// </summary>
        /// <param name="page">page number</param>
        /// <returns></returns>
        private string GetData(int page)
        {
            string baseUrl = PrepareURL(page);
            string result;

            WaitBeforeCall();       // Delays if the last call is less than 1s ago.

            // Making the request.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                result = sr.ReadToEnd();
            }

            return result;
        }

        /// <summary>
        /// Parse string API result as XML.
        /// </summary>
        /// <param name="page">page number. Default: 1</param>
        /// <returns>XDocument</returns>
        private XmlDocument MakeXML(int page = 1)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string url = PrepareURL(page);
            WaitBeforeCall();                           // Delays if the last call is less than 1s ago.
            xmlDoc.Load(url);

            // Namespaces caused a lot of issues. So before returning the 
            // xdoc, namespaces are removed.
            XDocument xdoc = new XDocument();

            using (var nodeReader = new XmlNodeReader(xmlDoc))
            {
                nodeReader.MoveToContent();
                xdoc = XDocument.Load(nodeReader);
            }

            xdoc = Tools.StripNamespace(xdoc);
            XmlDocument newXmlDoc = new XmlDocument();
            newXmlDoc.LoadXml(xdoc.ToString());
            xmlDoc = newXmlDoc;
 
            Debug.WriteLine("\n XMLDpc String: \n" + xmlDoc.OuterXml);

            return xmlDoc;
        }

        /// <summary>
        /// Checks if the XML return from the API has more
        /// pages.
        /// </summary>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        private Boolean HasPages(XmlDocument xdoc)
        {
            XmlDocument tempDoc = new XmlDocument();
            tempDoc = xdoc;

            string t = tempDoc
                .SelectSingleNode("/ItemSearchResponse/Items/TotalPages")
                .InnerText;
            int total = Int32.Parse(t);

            string s = tempDoc
                .SelectSingleNode("/ItemSearchResponse/Items/Request/ItemSearchRequest/ItemPage")
               .InnerText;
            int start = Int32.Parse(s);

            Debug.WriteLine("\n HasPages(): " + total + " & " + start);

            return total > start;
        }

        /// <summary>
        /// Parse the XML Data as a list of Books.
        /// </summary>
        /// <returns></returns>
        private List<Book> ParseData()
        {
            List<Book> items = new List<Book>();
            XmlDocument xdoc = MakeXML();
            int pageCount = 1;


            while (HasPages(xdoc) && pageCount <= this.MAXPAGES)
            {
                XmlNode booksNode = xdoc.SelectSingleNode("/ItemSearchResponse/Items");
                XmlNodeList books = booksNode.SelectNodes("Item"); // ?

                Debug.WriteLine("\n Books Node: \n" + booksNode.OuterXml);

                foreach (XmlNode book in books)
                {
                    Debug.WriteLine("\n Book Node: \n" + book.OuterXml);

                    XDocument xbook = XDocument.Parse(book.OuterXml);
                    string title = xbook.Element("Item").Element("ItemAttributes").Element("Title").Value;

                    string author = xbook.Element("Item").Element("ItemAttributes").Element("Author") == null ? "N/A" :
                                    xbook.Element("Item").Element("ItemAttributes").Element("Author").Value;

                    // Price check.
                    string priceS;
                    if (xbook.Element("Item").Element("ItemAttributes").Element("ListPrice") == null &&
                        xbook.Element("Item").Element("ItemAttributes").Element("TradeInValue") == null)
                    {
                        // No available prices.
                        priceS = "0";
                    }
                    else if (xbook.Element("Item").Element("ItemAttributes").Element("ListPrice") == null)
                    {
                        priceS = xbook.Element("Item").Element("ItemAttributes").Element("TradeInValue").Element("Amount").Value;
                    }
                    else
                    {
                        priceS = xbook.Element("Item").Element("ItemAttributes").Element("ListPrice").Element("Amount").Value;
                    }
                    float price = (float)Double.Parse(priceS) / 100.0f;

                    string image = xbook.Element("Item").Element("MediumImage") == null ? "" :
                        xbook.Element("Item").Element("MediumImage").Element("URL").Value;

                    string url = xbook.Element("Item").Element("DetailPageURL").Value;

                    Book b = new Book(title, url, author, price, image);
                    items.Add(b);
                }

                pageCount++;
                if (pageCount < this.MAXPAGES)
                {
                    xdoc = MakeXML(pageCount);
                }
            }

            return items;

        }

        /// <summary>
        /// Searches an item in the Amazon API and returns
        /// a list of products.
        /// </summary>
        /// <param name="searchTerm"></param>
        public List<Book> ItemSearch(string searchTerm)
        {
            SetSearchTerm(searchTerm);
            List<Book> items = ParseData();

            return items;
        }

        /// <summary>
        /// Updates the last call to the API using time since
        /// Epoch as miliseconds.
        /// </summary>
        public void UpdateLastCall()
        {
            DateTime now = DateTime.Now;
            this.lastAPICall = (long)(now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Delays the call if the last call to the API is
        /// less than 1s ago.
        /// </summary>
        public void WaitBeforeCall()
        {
            long difference;
            long oldCall = this.lastAPICall;
            UpdateLastCall();
            difference = this.lastAPICall - oldCall;

            if (difference < 1000)
            {
                Thread.Sleep(1100 - (int)difference);
            }

        }
    }
}
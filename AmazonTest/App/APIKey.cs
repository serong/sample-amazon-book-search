using System.Configuration;

namespace AmazonTest.App
{
    /// <summary>
    /// Handles returning necessary API keys from
    /// the PrivateSettings.config.
    /// </summary>
    public class APIKey
    {
        private static readonly string exchangeKey = ConfigurationManager.AppSettings["ExchangeKey"];
        private static readonly string ascKey = ConfigurationManager.AppSettings["AssociateKey"];
        private static readonly string apiKey = ConfigurationManager.AppSettings["APIKey"];
        private static readonly string secKey = ConfigurationManager.AppSettings["SecretKey"];
        private static readonly string destination = "webservices.amazon.com";

        public static string GetDestination()
        {
            return destination;
        }

        public static string GetAscKey()
        {
            return ascKey;
        }

        public static string GetApiKey()
        {
            return apiKey;
        }

        public static string GetSecKey()
        {
            return secKey;
        }

        public static string GetExchangeKey()
        {
            return exchangeKey;
        }
    }
}
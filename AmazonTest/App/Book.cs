using System.Globalization;

namespace AmazonTest.App
{
    public class Book
    {
        // Book data.
        private string title;
        private string author;
        private string image;
        private string url;
        private float price;

        public Book(string title, string url, string author, float price, string image)
        {
            this.title = title;
            this.url = url;
            this.author = author;
            this.price = price;
            this.image = image;
        }

        /// <summary>
        /// Returns book title.
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.title;
        }

        /// <summary>
        /// Returns book url.
        /// </summary>
        /// <returns></returns>
        public string GetUrl()
        {
            return this.url;
        }

        /// <summary>
        /// Returns book author.
        /// </summary>
        /// <returns></returns>
        public string GetAuthor()
        {
            return this.author;
        }

        /// <summary>
        /// Returns price.
        /// </summary>
        /// <returns></returns>
        public float GetPrice()
        {
            return this.price;
        }

        /// <summary>
        /// Returns price as a string, with "." as decimal
        /// point.
        /// </summary>
        /// <returns></returns>
        public string GetPriceStr()
        {
            return this.price.ToString(CultureInfo.GetCultureInfo("en-GB"));

        }

        /// <summary>
        /// Returns an image URL.
        /// </summary>
        /// <returns></returns>
        public string GetImageURL()
        {
            return this.image;
        }

        /// <summary>
        /// For testing purposes.
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            return this.title + ",by " + this.author + "(" + this.price + ", "
                + this.url + ")\nURL: " + this.image + "\n\n";
        }
    }
}
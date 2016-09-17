using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace AmazonTest.App
{
    public class Tools
    {
        // Ref: https://msdn.microsoft.com/en-us/library/844skk0h(v=vs.110).aspx
        public static string CleanInput(string text)
        {
            try
            {
                return Regex.Replace(text, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }

            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        // REF: Stackoverflow.
        public static XDocument StripNamespace(XDocument document)
        {

            foreach (var element in document.Root.DescendantsAndSelf())
            {
                element.Name = element.Name.LocalName;
                element.ReplaceAttributes(GetAttributesWithoutNamespace(element));
            }

            return document;
        }

        // REF: Stackoverflow.
        private static IEnumerable GetAttributesWithoutNamespace(XElement xElement)
        {
            return xElement.Attributes()
                .Where(x => !x.IsNamespaceDeclaration)
                .Select(x => new XAttribute(x.Name.LocalName, x.Value));
        }

    }
}
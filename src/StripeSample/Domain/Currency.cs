
using System;
using System.Collections.Generic;

namespace StripeSample.Domain
{
    public class Currency : Enumeration
    {
        public static Currency USD = new Currency(1, nameof(USD), "en-US");
        public static Currency AUD = new Currency(2, nameof(AUD), "en-AU");

        protected Currency(int id, string name, string language)
            : base(id, name)
        {
            Language = !string.IsNullOrWhiteSpace(language) ? language : throw new ArgumentNullException(nameof(language));
        }

        public string Language { get; private set; }

        public static IEnumerable<Currency> ListAll()
        {
            yield return USD;
            yield return AUD;
        }
    }
}

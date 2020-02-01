
using System;
using System.Collections.Generic;
using System.Linq;

namespace StripeSample.Domain
{
    public class InvoiceStatus : Enumeration
    {
        public static InvoiceStatus Draft = new InvoiceStatus(1, nameof(Draft));
        public static InvoiceStatus Open = new InvoiceStatus(2, nameof(Open));
        public static InvoiceStatus Paid = new InvoiceStatus(3, nameof(Paid));
        public static InvoiceStatus Uncollectible = new InvoiceStatus(4, nameof(Uncollectible));
        public static InvoiceStatus Void = new InvoiceStatus(5, nameof(Void));

        protected InvoiceStatus(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<InvoiceStatus> ListAll()
        {
            yield return Draft;
            yield return Open;
            yield return Paid;
            yield return Uncollectible;
            yield return Void;
        }

        public static InvoiceStatus FindByName(string name)
        {
            return ListAll().FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
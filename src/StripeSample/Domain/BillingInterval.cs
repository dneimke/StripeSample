
using System.Collections.Generic;

namespace StripeSample.Domain
{
    public class BillingInterval : Enumeration
    {
        public static BillingInterval Day = new BillingInterval(1, nameof(Day));
        public static BillingInterval Week = new BillingInterval(2, nameof(Week));
        public static BillingInterval Month = new BillingInterval(3, nameof(Month));
        public static BillingInterval Year = new BillingInterval(4, nameof(Year));

        protected BillingInterval(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<BillingInterval> ListAll()
        {
            yield return Day;
            yield return Week;
            yield return Month;
            yield return Year;
        }
    }
}

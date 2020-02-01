
using System;
using System.Collections.Generic;
using System.Linq;

namespace StripeSample.Domain
{
    public class SubscriptionStatus : Enumeration
    {
        public static SubscriptionStatus Incomplete = new SubscriptionStatus(1, nameof(Incomplete));
        public static SubscriptionStatus IncompleteExpired = new SubscriptionStatus(2, "Incomplete_Expired");
        public static SubscriptionStatus Trialing = new SubscriptionStatus(3, nameof(Trialing));
        public static SubscriptionStatus Active = new SubscriptionStatus(4, nameof(Active));
        public static SubscriptionStatus PastDue = new SubscriptionStatus(5, "Past_Due");
        public static SubscriptionStatus Canceled = new SubscriptionStatus(6, nameof(Canceled));
        public static SubscriptionStatus Unpaid = new SubscriptionStatus(7, nameof(Unpaid));

        protected SubscriptionStatus(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<SubscriptionStatus> ListAll()
        {
            yield return Incomplete;
            yield return IncompleteExpired;
            yield return Trialing;
            yield return Active;
            yield return PastDue;
            yield return Canceled;
            yield return Unpaid;
        }

        public static SubscriptionStatus FindByName(string name)
        {
            return ListAll().FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
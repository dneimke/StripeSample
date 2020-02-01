
using System.Collections.Generic;

namespace StripeSample.Domain
{
    public class CardType : Enumeration
    {
        public static CardType Amex = new CardType(1, nameof(Amex));
        public static CardType Visa = new CardType(2, nameof(Visa));
        public static CardType MasterCard = new CardType(3, nameof(MasterCard));

        protected CardType(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<CardType> ListAll()
        {
            yield return Amex;
            yield return Visa;
            yield return MasterCard;
        }
    }
}

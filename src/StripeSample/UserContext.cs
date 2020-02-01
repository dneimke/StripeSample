using StripeSample.Infrastructure.Data;
using System;
using System.Linq;

namespace StripeSample
{
    public class UserContext
    {
        public Guid Id { get; } = Guid.Empty;
        public string EmailAddress { get; } = "";
        public string CustomerId { get; } = "";
        public bool IsAuthenticated => Id != Guid.Empty;

        public UserContext(SubscriptionsContext dbContext)
        {
            var user = dbContext.ApplicationUser.SingleOrDefault(e => e.Id == Guid.Parse("07b742cc-8c82-43b6-8615-de54635db929"));
            if(user != null)
            {
                Id = user.Id;
                EmailAddress = user.EmailAddress;
                CustomerId = user.CustomerId;
            }
        }
    }
}

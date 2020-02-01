using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System;
using System.Linq;

namespace StripeSample
{
    public class UserContext
    {
        private readonly SubscriptionsContext _dbContext;

        public Guid Id { get; }
        public string EmailAddress { get; }
        public string CustomerId { get; }

        public UserContext(SubscriptionsContext dbContext)
        {
            var user = dbContext.ApplicationUser.FirstOrDefault(e => e.Id == Guid.Parse("07b742cc-8c82-43b6-8615-de54635db929"));
            if(user != null)
            {
                Id = user.Id;
                EmailAddress = user.EmailAddress;
                CustomerId = user.CustomerId;
            }

            _dbContext = dbContext;
        }

        public ApplicationUser GetUser()
        {
            var user = _dbContext.ApplicationUser.FirstOrDefault(e => e.Id == Id);
            return user;
        } 
    }
}

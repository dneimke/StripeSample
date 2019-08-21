﻿using StripeSample.Entities;
using StripeSample.Infrastructure.Data;
using System;
using System.Linq;

namespace StripeSample
{
    public class UserContext
    {
        private readonly ApplicationDbContext _dbContext;

        public Guid Id { get; }
        public string EmailAddress { get; }
        public string CustomerId { get; }

        public UserContext(ApplicationDbContext dbContext)
        {
            var testUser = dbContext.User.FirstOrDefault(e => e.Id == Guid.Parse("07b742cc-8c82-43b6-8615-de54635db929"));
            if(testUser != null)
            {
                Id = testUser.Id;
                EmailAddress = testUser.EmailAddress;
                CustomerId = testUser.CustomerId;
            }

            _dbContext = dbContext;
        }

        public User GetUser()
        {
            return _dbContext.User.FirstOrDefault(e => e.Id == Id);
        } 
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StripeSample.Entities
{
    public class Cart : BaseEntity
    {
        [Required]
        public User User { get; set; }
        public string SessionId { get; set; }
        public CartState CartState { get; set; }
    }

    public enum CartState
    {
        None = 0,
        Created = 1,
        Fulfilled = 2,
        Abandoned = 3
    }
}

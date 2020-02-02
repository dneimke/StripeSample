
namespace StripeSample.Domain
{
    public class Cart : Entity
    {
        public string Email { get; set; }
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

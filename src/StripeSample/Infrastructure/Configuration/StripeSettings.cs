namespace StripeSample.Infrastructure.Configuration
{
    public class StripeSettings
    {
        public string DefaultProductKey { get; set; }
        public string DefaultProductName { get; set; }
        public string DefaultPlanKey { get; set; }
        public string DefaultPlanName { get; set; }
        public int DefaultPlanAmountInCents { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string WebhookSecret { get; set; }
        public string CheckoutSuccessRedirectUrl { get; set; }
    }
}

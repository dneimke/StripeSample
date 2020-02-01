## Application Overview

This sample application shows a simple user flow for performing the following actions:

* Home page displays premium content if the logged-in user has a subscription
* User can view their account details page to see their subscription status
* From the account details page, the user can:
  * Purchase a new subscription
  * Cancel (Pause) an existing subscription
  * Reinstate a paused subscription
  * View all invoices

![alt-text][application-overview]

## Getting Started

Login to your [Stripe dashboard](https://dashboard.stripe.com) and create a Product and Plan.  Then add your development keys and and the product and plan keys to your secrets file:

```json
{
  "StripeSettings:PublicKey": "...",
  "StripeSettings:PrivateKey": "...",
  "StripeSettings:WebhookSecret": "...",
  "StripeSettings:DefaultProductKey": "...",
  "StripeSettings:DefaultProductName": "Your great product",
  "StripeSettings:DefaultPlanKey": "...",
  "StripeSettings:DefaultPlanName": "Monthly",
  "StripeSettings:DefaultPlanAmountInCents": "800",
  "StripeSettings:CheckoutSuccessRedirectUrl": "https://localhost:55965/Subscription/UpgradeCB?sessionId={CHECKOUT_SESSION_ID}"
}
```

## Webhooks

Install the [Stripe CLI](https://github.com/stripe/stripe-cli) and run the following command to start listening.

```ps
.\stripe.exe listen --forward-to localhost:55965/Home/Webhook
``

[application-overview]: ./docs-images/application-overview.png "Application user flows diagram"

## Using the Stripe API

The [Stripe API Reference](https://stripe.com/docs/api) contains helpful articles and API reference documentation for working Products, Plans, Subscriptions, Customers, and Payments.

The how to [Set up a subscription](https://stripe.com/docs/billing/subscriptions/set-up-subscription) article on the Stripe Docs website explains 
how to create subscriptions in code.

You can also get some useful examples for creating card elements at the [following link on GitHub](https://stripe.dev/elements-examples/)

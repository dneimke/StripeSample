

## Using the Stripe API

The [Stripe API Reference](https://stripe.com/docs/api) contains helpful articles and API reference documentation for working Products, Plans, Subscriptions, Customers, and Payments.

Login to the [Stripe dashboard](https://dashboard.stripe.com) and get your test keys and add them to your secrets file:

```json
  "Stripe:PublicKey": "...",
  "Stripe:PrivateKey": "..."
```

At this time, seed the test app with dummy customer details by adding the following properties to your secrets file:

```json
  "TestData:EmailAddress": "ENTER A VALID EMAIL ADDRESS",
  "TestData:InternalCustomerId": "ENTER A UNIQUE STRING",
```

## Subscriptions

The how to [Set up a subscription](https://stripe.com/docs/billing/subscriptions/set-up-subscription) article on the Stripe Docs website explains 
how to create subscriptions in code.

You can also get some useful examples for creating card elements at the [following link on GitHub](https://stripe.dev/elements-examples/)

## Webhooks

Install the [Stripe CLI](https://github.com/stripe/stripe-cli) and run the following command to start listening.

```ps
.\stripe.exe listen --forward-to localhost:55965/Home/Webhook
```

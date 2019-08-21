

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

## Products and Plans

Next you need create a product and a plan that your customers can use to sign-up for the test service.  Uncomment the following form in the Home/Index page
and click the Create Product button to seed your account with a new Product/Plan.

```html
<h1 class="display-4">Product</h1>
<form asp-controller="Home" asp-action="CreatePlan" method="post">
    <input type="submit" class="btn btn-primary" value="Create Product" />
</form>
```
The form posts back to the server where we create a new product and then attach an $8 per-month plan to it.

```csharp
[HttpPost]
public async Task<IActionResult> CreatePlan()
{
    var product = await _paymentService.CreateProduct("Product 1");
    var plan = await _paymentService.CreatePlan(product.Id, "Plan 1", 800);
    return Json(new { ProductId = product.Id, PlanId = plan.Id });
}
```
Take note of the Id for the product and plan and add them to your secrets file:

```json
  "TestData:Plan1:ProductId": "...",
  "TestData:Plan1:PlanId": "..."
```

# Stripe Customers

Customer objects allow you to perform recurring charges, and to track multiple charges, that are associated with the same customer.

An application could create a Stripe customer whenever a new user registers with the site.  At that time, a reference would be added to the Application User account for the Stripe customer Id.

You can learn more about Stripe customers in the [API reference documentation](https://stripe.com/docs/api/customers).

```html
<h1 class="display-4">Customer</h1>
<form asp-controller="Home" asp-action="CreateCustomer" method="post">
    <input type="submit" class="btn btn-primary" value="Create Customer" />
</form>
```
The form posts back to the server where we create a new Stripe customer account

```csharp
[HttpPost]
public async Task<IActionResult> CreateCustomer()
{
    var customerId = Guid.NewGuid().ToString();
    var customer = await _paymentService.CreateCustomer(_testData.EmailAddress, customerId);
    return Json(new { CustomerId = customer.Id, InternalCustomerId = customerId });
}
```

Take note of the Id for the customer and add them to your secrets file:

```json
  "TestData:CustomerId": "..."
```

## Checkout Session

In the HomeController, we create a [checkout session](https://stripe.com/docs/api/checkout/sessions/create) using `CreateCheckoutSession` method of the `StripePaymentService` if the 
current `Customer` does not yet have a `Subscription`.

The customer can then click the subscribe button to pay for a monthly Subscription.


## Webhooks

Install the [Stripe CLI](https://github.com/stripe/stripe-cli) and run the following command to start listening.

```ps
.\stripe.exe listen --forward-to localhost:55965/Home/Webhook
```

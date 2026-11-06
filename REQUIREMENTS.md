## Features

### Sell Policy
- Create a new household or buy-to-let policy
- Validate:
  - Unique policy reference
  - Maximum 60 days in advance
  - Policy duration must be exactly 1 year
  - 1-3 policyholders
  - Policyholders must be at least 16 years old on the policy start date

### Retrieve Policy
- Retrieve a policy by reference

### Renew Policy
- Renew an existing active policy
- Renewal allowed only within 30 days of the end date
- Renewal not allowed after policy expiry
- Auto-renew policies automatically create a payment

### Cancel Policy
- Cancel an active policy
- Calculates:
  - Full refund before policy start
  - Full refund during cooling-off period
  - Pro-rata refund afterwards

### Cancellation Quote
- Retrieve the cancellation refund amount without cancelling the policy
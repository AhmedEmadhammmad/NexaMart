# NexaMart Enterprise — Architectural Blueprint & Development Rules

## 1. Solution Architecture & Layer Isolation
- **Domain Layer (`NexaMart.Domain`):**
  - Pure POCO entities (`Product`, `Category`, `Order`, `OrderItem`, `ApplicationUser`, `CartItem`, `WishlistItem`, `Review`).
  - Business Enums: `UserRoleType`, `OrderStatus`, `PaymentMethod`, `PaymentStatus`.
  - Rule: ZERO external dependencies (0 NuGet packages).
- **Application Layer (`NexaMart.Application`):**
  - Core business contracts, DTOs, and Business Services (`OrderService`, `ProductService`, `EmailService`, etc.).
  - Interfaces: `IPaymobService`, `IEmailService`, `IOrderService`, `IUnitOfWork`, etc.
  - Enforces Single GroupBy LINQ queries for dashboards/stats.
  - Rule: Depends ONLY on Domain.
- **Infrastructure Layer (`NexaMart.Infrastructure`):**
  - `NexaMartDbContext`, EF Core Configurations, `GenericRepository<T>`, and `UnitOfWork`.
  - External Gateway Integrations: `PaymobService` (HTTP client, 3-step flow, HMAC-SHA512 validation).
  - Security & Authentication: BCrypt (`WorkFactor = 12`), JWT Token services, and `DbSeeder`.
  - Rule: Implements abstractions; manages I/O, external network calls, database transactions, and audit timestamps (`CreatedAt`/`UpdatedAt`).
- **Presentation Layer (`NexaMart.Web`):**
  - ASP.NET Core 8.0 MVC.
  - Thin Controllers (2 to 5 lines per action): `CartController`, `PaymentController`, `OrdersController`, etc.
  - Modular Mappings: `StorefrontMappings.cs`, `AdminMappings.cs`, `SuperAdminMappings.cs`.
  - ViewModels with strict Anti-XSS regex and validation annotations.
  - UI Design System: Modern Glassmorphic Floating Toast Notifications (`_ToastNotification.cshtml`) with animated countdown progress bar, auto-dismiss, and non-intrusive viewport placement.
  - Dedicated Admin Layout (`_AdminLayout.cshtml`) using data tables without storefront navbars.

## 2. Coding, Security & Integration Rules for NexaMart
1. **Historical Snapshot Pattern:** `OrderItem` MUST store `ProductName`, `ProductImageUrl`, and `UnitPrice` at purchase time to protect financial history.
2. **Atomic Inventory Checkout:** In `OrderService`, check stock, decrement inventory, and commit in a single transaction via `UnitOfWork.CommitTransactionAsync()` without duplicate `CompleteAsync()` calls.
3. **ChangeTracker Identity Integrity:** In-memory tracked entities (e.g. `cartItems`) must be passed directly to `DeleteRange()` without re-querying from the database to prevent duplicate key tracking collisions.
4. **UnitOfWork vs ExecutionStrategy:** When manual transactions are governed by `UnitOfWork.BeginTransactionAsync()`, do not enable `EnableRetryOnFailure()` on SQL Server options to avoid strategy replay conflicts.
5. **Paymob Payment Lifecycle:**
   - Online payments (`PaymobCard`, `PaymobWallet`) MUST initialize orders with `Status = OrderStatus.Pending` and `PaymentStatus = PaymentStatus.Pending`.
   - Never dispatch order confirmation emails or mark orders as `Confirmed` before receiving and verifying a successful callback/webhook.
   - Paymob requires transaction amounts strictly in Cents (`amount_cents = TotalAmount * 100`).
   - Every callback/webhook must pass cryptographic HMAC-SHA512 verification against `HmacSecret`.
6. **Transactional Notification Rules:**
   - Order Confirmation Invoice: Sent immediately for `CashOnDelivery` orders, and on payment capture (`Paid`) for Paymob orders.
   - Order Cancellation Email: Dispatched immediately upon order cancellation with itemized summary, cancellation reason, and inventory/refund notice.
7. **No Redundant Entity Mapping in Mappers:** Do not set `CreatedAt` manually in `ToEntity()`; let `UnitOfWork.UpdateTimestamps()` handle it.
8. **File Upload Security:** Any image upload must pass Magic Bytes binary validation, 5MB limit, GUID naming, and Path Traversal checks via `FileStorageService`.
9. **Universal Search by ID:** Route by entity resolution: Product -> Category -> Order.
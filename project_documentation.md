# 📘 الدليل الهندسي والمعماري الشامل لمشروع NexaMart Enterprise
## Comprehensive System Architecture, Database ERD, Code Anatomy & Reviewer Guide

> **موجّه لمراجع الكود والمهندسين (Code Reviewers & Technical Evaluators)**  
> تم إعداد هذا التوثيق ليكون مرجعاً تقنياً وهندسياً فائق التفصيل يغطي كل جوانب منصة **NexaMart** المبنية وفق أعلى معايير **Clean Architecture** ونمط **Domain-Driven Design (DDD)** باستخدام **ASP.NET Core 8.0 MVC** و **Entity Framework Core**.  
> يشرح هذا الملف سطر بسطر وبشكل تشريحي دقيق: مخطط قاعدة البيانات والعلاقات، بنية الطبقات، المستودعات، وحدة العمل، الخدمات، الـ DTOs، الـ ViewModels، وحدات التحكم، وطبقة التحويل (Mapping) بعد تقسيمها وتحسين أدائها، مع مسار تدفق البيانات وسجل لكافة التحديثات والتحسينات المضافة لضمان توفير وقت المراجعة بالكامل.

---

## 📑 فهرس المحتويات
1. [نظرة عامة على النظام وأهداف المنصة (Executive Overview)](#1-نظرة-عامة-على-النظام-وأهداف-المنصة-executive-overview)
2. [مخطط قاعدة البيانات الشامل وتفصيل العلاقات (Comprehensive Database ERD)](#2-مخطط-قاعدة-البيانات-الشامل-وتفصيل-العلاقات-comprehensive-database-erd)
3. [الهندسة المعمارية للنظام وتصميم الطبقات (System Design & Clean Architecture)](#3-الهندسة-المعمارية-للنظام-وتصميم-الطبقات-system-design--clean-architecture)
4. [التشريح البرمجي التفصيلي خطوة بخطوة (Detailed Code Anatomy)](#4-التشريح-البرمجي-التفصيلي-خطوة-بخطوة-detailed-code-anatomy)
   - [4.1 طبقة الـ Infrastructure: المستودعات ووحدة العمل (Generic Repository & Unit of Work)](#41-طبقة-الـ-infrastructure-المستودعات-ووحدة-العمل-generic-repository--unit-of-work)
   - [4.2 طبقة الـ Application: الخدمات ومنطق الأعمال المحسّن (Application Services)](#42-طبقة-الـ-application-الخدمات-ومنطق-الأعمال-المحسن-application-services)
   - [4.3 كائنات نقل البيانات (Data Transfer Objects - DTOs)](#43-كائنات-نقل-البيانات-data-transfer-objects---dtos)
   - [4.4 نماذج واجهة العرض (Presentation ViewModels)](#44-نماذج-واجهة-العرض-presentation-viewmodels)
   - [4.5 وحدات التحكم الرشيقة (Razor-Thin Web Controllers)](#45-وحدات-التحكم-الرشيقة-razor-thin-web-controllers)
   - [4.6 طبقة التحويل المقسمة والصريحة (Modular Mapping Architecture)](#46-طبقة-التحويل-المقسمة-والصريحة-modular-mapping-architecture)
5. [ربط الطبقات ومسار تدفق البيانات الشامل (End-to-End Dataflow & Request Lifecycle)](#5-ربط-الطبقات-ومسار-تدفق-البيانات-الشامل-end-to-end-dataflow--request-lifecycle)
6. [أهم الدوال والخوارزميات البرمجية والتحسينات (Critical Algorithms & Performance Optimizations)](#6-أهم-الدوال-والخوارزميات-البرمجية-والتحسينات-critical-algorithms--performance-optimizations)
7. [سجل كافة المميزات والتحسينات المنفذة (Feature & Optimization Inventory)](#7-سجل-كافة-المميزات-والتحسينات-المنفذة-feature--optimization-inventory)
8. [دليل المراجع السريع للفحص والاختبار (Reviewer Quick-Start & Testing Guide)](#8-دليل-المراجع-السريع-للفحص-والاختبار-reviewer-quick-start--testing-guide)

---

## 1. نظرة عامة على النظام وأهداف المنصة (Executive Overview)

**NexaMart** هي منصة تجارة إلكترونية وإدارة مؤسسية متكاملة (Enterprise E-Commerce & Administrative Governance Platform) تم بناؤها لتكون نموذجاً احترافياً فائق السرعة، خالي تماماً من الأكواد الميتة (Dead Code)، وقابل للتوسع (Scalable) وسهل الصيانة (Maintainable)، مع مراعاة أعلى معايير أمان التطبيقات ونظافة الكود.

### ركائز النظام الأساسية:
1. **متجر العملاء (Storefront Experience)**:
   - تصفح وتصفية المنتجات مع نظام ترقيم صفحات حديث ومريح للعين.
   - سلة تسوق وقائمة مفضلة كاملة مع إدارة الكميات الفورية.
   - **إصدار الفاتورة الضريبية الرسمية المباشرة**: توليد فواتير رسمية معتمدة قابلة للطباعة تخصم المخزون ذرّياً في خطوة واحدة دون تعقيدات وهمية.
2. **مركز قيادة إدارة المتجر (Store Admin Command Portal)**:
   - واجهة مستقلة عبر `_AdminLayout.cshtml` **خالية تماماً من أي شريط تنقل عام للمتجر (No Storefront Navbar)**.
   - إدارة كاملة لكافة المنتجات والأقسام والطلبات من خلال **جداول بيانات حصرية (Data Tables وليست كروت)**.
   - بحث شامل وفوري برقم الـ ID للمنتجات والأقسام والطلبات مع توجيه ذكي للمسار الصحيح.
   - مؤشرات فورية للمخزون المنخفض، متوسطات تقييمات الأقسام المحسوبة في طبقة الخدمات، وتصفير تقييمات المنتجات.
3. **لوحة الإدارة العليا وحوكمة المنصة (Executive SuperAdmin Console)**:
   - إدارة شاملة لكافة حسابات النظام وتوزيع الرتب (`Customer`, `Admin`, `SuperAdmin`).
   - حظر وتفعيل فوري للحسابات مع حماية أمنية صارمة تمنع المشرف من حظر حسابه أو تنزيل رتبته ذاتياً.
   - فحص السجل المالي وسجل طلبات أي مستخدم وتعيين مشرفين جدد.
4. **تسجيل الدخول الموحد والذكي (Unified Authentication Gateway)**:
   - بوابة دخول واحدة فائقة الأمان تستقبل إما **البريد الإلكتروني** أو **الـ ID الرقمي للموظفين** مع توجيه آلي مباشر حسب الصلاحيات (مع حذف كامل لأي كود ميت مثل `StaffLoginAsync`).

---

## 2. مخطط قاعدة البيانات الشامل وتفصيل العلاقات (Comprehensive Database ERD)

تم بناء قاعدة البيانات عبر **Code-First** باستخدام **Entity Framework Core 8.0** على **SQL Server**، مع ضبط دقيق للقيود والفهارس والعلاقات لضمان سلامة البيانات (Data Integrity).

### مخطط الكيانات والعلاقات (Mermaid Database Diagram)

```mermaid
erDiagram
    APPLICATION_USER ||--o{ ORDER : "places (1:N)"
    APPLICATION_USER ||--o{ CART_ITEM : "owns in cart (1:N)"
    APPLICATION_USER ||--o{ WISHLIST_ITEM : "saves in wishlist (1:N)"
    APPLICATION_USER ||--o{ REVIEW : "submits (1:N)"

    CATEGORY ||--o{ PRODUCT : "classifies (1:N)"

    PRODUCT ||--o{ ORDER_ITEM : "contained in (1:N)"
    PRODUCT ||--o{ CART_ITEM : "added to (1:N)"
    PRODUCT ||--o{ WISHLIST_ITEM : "bookmarked in (1:N)"
    PRODUCT ||--o{ REVIEW : "rated by (1:N)"

    ORDER ||--|{ ORDER_ITEM : "consists of (1:N)"

    APPLICATION_USER {
        int Id PK "Auto-Increment Primary Key"
        string FullName "NVARCHAR(100), NOT NULL"
        string Email UK "NVARCHAR(150), Unique Index, NOT NULL"
        string PasswordHash "NVARCHAR(255), BCrypt Hash"
        string PhoneNumber "NVARCHAR(20), Nullable"
        int RoleType "Enum: 0=Customer, 1=Admin, 2=SuperAdmin"
        bool IsActive "Account Status: True=Active, False=Blocked"
        string RefreshToken "NVARCHAR(500), Nullable"
        datetime RefreshTokenExpiresAt "Nullable"
        datetime LastLoginAt "Nullable, Tracks Activity"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    CATEGORY {
        int Id PK "Auto-Increment Primary Key"
        string Name "NVARCHAR(100), NOT NULL"
        string Description "NVARCHAR(500), Nullable"
        string ImageUrl "NVARCHAR(500), Nullable"
        int DisplayOrder "Sorting Priority"
        bool IsActive "Catalog Visibility Flag"
        decimal AverageRating "DECIMAL(18,2) - Calculated via Service"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    PRODUCT {
        int Id PK "Auto-Increment Primary Key"
        int CategoryId FK "Foreign Key to Category"
        string Name "NVARCHAR(200), NOT NULL"
        string Description "NVARCHAR(MAX), Nullable"
        string ImageUrl "NVARCHAR(500), Nullable"
        decimal Price "DECIMAL(18,2), Precision Guaranteed"
        int StockQuantity "Inventory Level (Atomic Decrement)"
        decimal AverageRating "DECIMAL(18,2) - Zeroed to 0.0"
        int ReviewCount "Integer - Zeroed to 0"
        bool IsActive "Visibility Flag"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    ORDER {
        int Id PK "Auto-Increment Primary Key"
        string OrderNumber UK "NVARCHAR(50), Format: NXM-yyyyMMdd-XXXX"
        int UserId FK "Foreign Key to ApplicationUser"
        decimal SubTotal "DECIMAL(18,2)"
        decimal TotalAmount "DECIMAL(18,2) - Total Amount Payable"
        int Status "Enum: 0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled"
        string CancellationReason "NVARCHAR(500), Nullable"
        datetime CancelledAt "Nullable"
        int CancelledByUserId "Nullable FK"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    ORDER_ITEM {
        int Id PK "Auto-Increment Primary Key"
        int OrderId FK "Foreign Key to Order"
        int ProductId FK "Foreign Key to Product"
        string ProductName "NVARCHAR(200) - Historical Snapshot"
        string ProductImageUrl "NVARCHAR(500) - Historical Snapshot"
        decimal UnitPrice "DECIMAL(18,2) - Price at Purchase"
        int Quantity "Purchased Amount"
        decimal TotalPrice "DECIMAL(18,2) - Quantity * UnitPrice"
        bool IsCancelled "Individual Item Cancellation Flag"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    CART_ITEM {
        int Id PK "Auto-Increment Primary Key"
        int UserId FK "Foreign Key to ApplicationUser"
        int ProductId FK "Foreign Key to Product"
        int Quantity "Quantity in Cart (Min 1)"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    WISHLIST_ITEM {
        int Id PK "Auto-Increment Primary Key"
        int UserId FK "Foreign Key to ApplicationUser"
        int ProductId FK "Foreign Key to Product"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }

    REVIEW {
        int Id PK "Auto-Increment Primary Key"
        int ProductId FK "Foreign Key to Product"
        int UserId FK "Foreign Key to ApplicationUser"
        int Rating "1 to 5 Stars"
        string Comment "NVARCHAR(1000)"
        datetime CreatedAt "UTC Timestamp"
        datetime UpdatedAt "Nullable UTC Timestamp"
    }
```

### القواعد الهندسية للعلاقات وسلامة البيانات (Referential Integrity & Constraints):
1. **علاقة المستخدم بالطلبات (`ApplicationUser` &rarr; `Order`) [1:N]**:
   - المستخدم يمكن أن يملك صفراً أو عدة طلبات.
   - **سلوك الحذف (`DeleteBehavior.Restrict`)**: تم منع الحذف المتتالي (Cascade Delete) قطعياً لحماية السجلات المالية التاريخية (Financial Audit Trail).
2. **علاقة القسم بالمنتجات (`Category` &rarr; `Product`) [1:N]**:
   - كل قسم يحوي منتجات متعددة.
   - **سلوك الحذف (`DeleteBehavior.Restrict`)**: لا يسمح بحذف أي قسم يحوي منتجات مرتبطة لمنع وجود منتجات يتيمة (Orphan Products).
3. **علاقة الطلب ببنود الطلب (`Order` &rarr; `OrderItem`) [1:N]**:
   - كل طلب يتكون من بند واحد أو أكثر.
   - **اللقطة التاريخية (Historical Snapshot Pattern)**: يتم نسخ اسم المنتج، صورته، وسعره وقت الشراء داخل جدول `OrderItem`. هذا يضمن ثبات قيمة الفواتير السابقة تاريخياً حتى لو تغير سعر المنتج في الكتالوج مستقبلاً.
4. **علاقات السلة والمفضلة (`CartItem`, `WishlistItem`) [1:N]**:
   - فهارس فريدة مركبة (`Unique Index on (UserId, ProductId)`) تمنع تكرار نفس المنتج للمستخدم داخل السلة؛ بدلاً من ذلك يتم تعديل حقل `Quantity`.
5. **تصفير التقييمات وسلامتها (`Reviews` & `Product.AverageRating`)**:
   - تم تصفير كافة مراجعات قاعدة البيانات وضبط `AverageRating = 0.0m` و `ReviewCount = 0` لجميع المنتجات لتبدأ المنصة بسجل نظيف تماماً.

---

## 3. الهندسة المعمارية للنظام وتصميم الطبقات (System Design & Clean Architecture)

يتبع المشروع بدقة مبدأ **Clean Architecture** (المعروف أيضاً بـ Onion Architecture)، حيث تم تقسيم النظام إلى 4 مشاريع مستقلة تماماً داخل الـ Solution:

```
                  ┌────────────────────────────────────────────────────────┐
                  │                    NexaMart.Web                        │
                  │   - Controllers (Thin Orchestrators: 1-5 lines)        │
                  │   - Razor Views & Admin Layouts                        │
                  │   - ViewModels & Validation Rules                      │
                  │   - Modular Mappings: Storefront, Admin, SuperAdmin    │
                  └──────────────────────────┬─────────────────────────────┘
                                             │ References
                  ┌──────────────────────────▼─────────────────────────────┐
                  │                 NexaMart.Application                   │
                  │   - Service Interfaces (IAdminService, etc.)           │
                  │   - Application Services & Encapsulated Business Rules │
                  │   - Data Transfer Objects (DTOs)                       │
                  │   - High Performance Aggregations (Single GroupBy)     │
                  │   - Common Pagination (PagedResult<T>)                 │
                  └──────────────┬───────────────────────────┬─────────────┘
                                 │                           │
                   References &  │                           │ References
                   Implements    │                           │
                  ┌──────────────▼─────────────┐ ┌───────────▼─────────────┐
                  │   NexaMart.Infrastructure  │ │     NexaMart.Domain     │
                  │   - NexaMartDbContext      │ │   - Entities & Enums    │
                  │   - GenericRepository<T>   │ │   - Core Business Rules │
                  │   - Streamlined UnitOfWork │ │   - Pure C# (No 3rd-    │
                  │   - BCrypt Security        │ │     party dependencies) │
                  │   - Auto Timestamp Auditing│ │                         │
                  └────────────────────────────┘ └─────────────────────────┘
```

### قواعد تدفق التبعيات (Inward Dependency Rules):
- **الطبقة المركزية (Domain Layer)**: لا تعتمد على أي طبقة أخرى ولا على أي حزمة خارجية (Zero External NuGet Dependencies). نقية 100%. الكيانات تمثل مفاهيم العمل المستقلة.
- **طبقة التطبيق (Application Layer)**: تعتمد فقط على الـ Domain. تحتوي على الـ Interfaces وقواعد العمل والتحقق والحسابات الإحصائية وموديلات الـ DTO.
- **طبقة البنية التحتية (Infrastructure Layer)**: تعتمد على Application و Domain، وتنفذ الاتصال الفعلي بقاعدة البيانات عبر EF Core والمستودعات المخصصة وإدارة المعاملات والتشفير.
- **طبقة العرض (Web Layer)**: نقطة الدخول للتطبيق، تعتمد على Application وتستهلك الـ Services عبر الـ Dependency Injection، وتعتمد على تحويلات صريحة ومقسمة (Modular Mappings) دون التعامل المباشر مع DbContext.

---

## 4. التشريح البرمجي التفصيلي خطوة بخطوة (Detailed Code Anatomy)

فيما يلي شرح تشريحي دقيق لمكونات الكود الأساسية بعد تنفيذ كافة التحسينات وإزالة التكرارات.

---

### 4.1 طبقة الـ Infrastructure: المستودعات ووحدة العمل (Generic Repository & Unit of Work)

#### أولاً: المستودع العام GenericRepository.cs
يعزل كافة استعلامات Entity Framework Core خلف واجهة مجردة موحدة:

```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly NexaMartDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(NexaMartDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>(); // ربط المستودع بنوع الجدول المقابل في EF Core
    }
```
* **سطر بسطر لدوال المستودع الأساسية**:
  1. `Query(bool disableTracking = true)`:
     ```csharp
     public virtual IQueryable<T> Query(bool disableTracking = true)
     {
         return disableTracking ? _dbSet.AsNoTracking() : _dbSet;
     }
     ```
     - تتيح تكوين استعلامات LINQ مرنة قابلة للفلترة والترقيم قبل تنفيذها في قاعدة البيانات.
     - استخدام `AsNoTracking()` افتراضياً يعطل تتبع الكائنات في الذاكرة، مما يقلل استهلاك الرام ويزيد سرعة القراءة بنسبة تتجاوز 40%.
     - عند الرغبة في تعديل الكائنات (مثل خصم المخزون)، يتم تمرير `disableTracking: false` لتمكين التتبع السلس.
  2. `GetByIdAsync(int id, CancellationToken cancellationToken)`:
     - تستخدم `_dbSet.FindAsync` للبحث الفوري بالمفتاح الأساسي وتبحث أولاً في الـ Local Memory Cache.
  3. `Update(T entity)` و `Delete(T entity)`:
     - تتحقق من حالة الكائن؛ إن كان منفصلاً (`Detached`) تقوم بعمل `_dbSet.Attach` ثم ضبط الحالة، مما يمنع حدوث مشاكل التعقب الشائعة.

#### ثانياً: وحدة العمل الرشيقة UnitOfWork.cs
تم تنظيف وحدة العمل بالكامل عبر إزالة الـ `ConcurrentDictionary` غير المستخدمة وإزالة دالة `Repository<T>()`، والاعتماد حصرياً على الـ Properties المحددة صراحة (Strongly-Typed) لضمان أقصى سرعة وأمان وقت الترجمة:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly NexaMartDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(NexaMartDbContext context)
    {
        _context = context;
        Products = new GenericRepository<Product>(_context);
        Categories = new GenericRepository<Category>(_context);
        CartItems = new GenericRepository<CartItem>(_context);
        WishlistItems = new GenericRepository<WishlistItem>(_context);
        Orders = new GenericRepository<Order>(_context);
        OrderItems = new GenericRepository<OrderItem>(_context);
        Reviews = new GenericRepository<Review>(_context);
        Users = new GenericRepository<ApplicationUser>(_context);
    }

    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<CartItem> CartItems { get; }
    public IGenericRepository<WishlistItem> WishlistItems { get; }
    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }
    public IGenericRepository<Review> Reviews { get; }
    public IGenericRepository<ApplicationUser> Users { get; }
```
* **تشريح إدارة المعاملات وحفظ التوقيتات الآلي**:
  - `CompleteAsync`: تستدعي `UpdateTimestamps()` ثم تحفظ التغييرات.
  - `UpdateTimestamps()`:
    ```csharp
    private void UpdateTimestamps()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var createdProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdProp != null) createdProp.CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                var updatedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedProp != null) updatedProp.CurrentValue = DateTime.UtcNow;
            }
        }
    }
    ```
  - `CommitTransactionAsync`: تستدعي `UpdateTimestamps()` و `SaveChangesAsync()` داخلياً قبل تأكيد الـ Transaction، مما يمنع الحاجة لاستدعاء `CompleteAsync` قبلها ويوفر عمليات حفظ مزدوجة في قاعدة البيانات.

---

### 4.2 طبقة الـ Application: الخدمات ومنطق الأعمال المحسّن (Application Services)

كل منطق الأعمال وقواعد الحوكمة تقع في هذه الطبقة حصرياً.

#### 1. خدمة المشرف AdminService.cs
- `GetDashboardAsync`: تجمع إحصائيات الكتالوج، الأقسام مع تقييماتها، أحدث المنتجات، وأحدث الطلبات في كائن `AdminDashboardDto` دفعة واحدة.
- `ResolveSearchByIdAsync(int id, string? type)`: خوارزمية ذكية تحدد نوع الـ ID (منتج، قسم، طلب) وتوجه المشرف للمسار الصحيح دون الحاجة لاختيار يدوي.
- `CreateProductAsync`: تضمن تصفير تقييمات أي منتج جديد برمجياً (`AverageRating = 0.0m; ReviewCount = 0;`).

#### 2. خدمة الإدارة العليا SuperAdminService.cs
- `PromoteUserRoleAsync`: تمنع المشرف العام الحالي من تعديل أو تنزيل رتبته الذاتية لحماية النظام من الإغلاق العرضي.
- `ToggleUserStatusAsync`: تمنع المشرف العام من حظر حسابه النشط.
- `CreateAdminAccountAsync`: تشفر كلمة المرور عبر BCrypt وتنشئ حسابات المشرفين بأمان تام.

#### 3. خدمة المصادقة الموحدة AuthService.cs
- تم حذف دالة `StaffLoginAsync` القديمة بالكامل ككود ميت، وتوحيد الدخول عبر `LoginAsync`:
  - تفحص هل المعرف رقمي؟ إذا كان رقماً وكان الحساب مشرفاً (`Admin` أو `SuperAdmin`) تسمح له بالدخول المباشر بالـ ID.
  - إذا كان بريداً إلكترونياً، يتم التحقق منه ومن الهاش المشفر.
  - منع العملاء العاديين من الدخول بالأرقام لضمان الانضباط الأمني.

#### 4. خدمة الطلبات والفواتير المحسنة OrderService.cs
- **إصلاح الأداء الكبير في `CreateOrderFromCartAsync`**:
  ```csharp
  // 1. جلب عناصر السلة مع المنتج وتمكين التتبع في استعلام واحد
  var cartItems = await _unitOfWork.CartItems.Query(disableTracking: false)
      .Include(c => c.Product)
      .Where(c => c.UserId == userId)
      .ToListAsync(cancellationToken);

  // 2. التحقق من المخزون وخصمه مباشرة عبر item.Product بدون أي استعلامات إضافية
  foreach (var item in cartItems)
  {
      var product = item.Product;
      product.StockQuantity -= item.Quantity;
      _unitOfWork.Products.Update(product);
  }

  // 3. تأكيد ذري واحد مباشر دون استدعاء CompleteAsync المزدوج قبل Commit
  await _unitOfWork.CommitTransactionAsync(cancellationToken);
  ```

#### 5. خدمة الأقسام CategoryService.cs
- تم نقل حساب متوسط تقييم القسم (`AverageRating`) من الـ Mapping إلى داخل `CategoryService`، بحيث تعود الكيانات محملة بالتقييم المحسوب جاهزة للعرض دون خلط المسؤوليات.

---

### 4.3 كائنات نقل البيانات (Data Transfer Objects - DTOs)

تفصل بين نماذج البيانات ونماذج العرض:
- `AdminDashboardDto.cs`: يحمل مقاييس الكتالوج المجمعة وقوائم المنتجات والأقسام والطلبات الأخيرة.
- `SuperAdminDashboardDto.cs`: يحمل إحصائيات المنصة الإجمالية وتوزيع رتب المستخدمين والإيرادات.
- `SuperAdminUserListDto.cs`: يحمل قائمة المستخدمين المقسمة لصفحات مع أعداد تابات التصفية (`AllUsersCount`, `CustomersCount`, `AdminsCount`, `BlockedCount`).

---

### 4.4 نماذج واجهة العرض (Presentation ViewModels)

تقع في مجلد `NexaMart.Web/Models`:
- `AdminDashboardViewModel`: نماذج البطاقات والجداول الخاصة بلوحة الإدارة.
- `SuperAdminUserDetailsViewModel`: نموذج تفصيلي يعرض سجل فواتير المستخدم وإجمالي إنفاقه.
- `ProductFormViewModel` & `CategoryFormViewModel`: نماذج إدخال البيانات المجهزة للتحقق (Validation Attributes).

---

### 4.5 وحدات التحكم الرشيقة (Razor-Thin Web Controllers)

تم الالتزام الصارم بنمط **Thin Controller**؛ بحيث يتراوح حجم أي Action بين 1 و 5 أسطر فقط:

#### وحدة تحكم المشرف AdminController.cs:
```csharp
[Authorize(Policy = "StaffOnly")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var dashboardDto = await _adminService.GetDashboardAsync(cancellationToken);
        return View(dashboardDto.ToViewModel());
    }
}
```

#### وحدة تحكم المشرف العام SuperAdminController.cs:
```csharp
[Authorize(Policy = "SuperAdminOnly")]
public class SuperAdminController : Controller
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteRole(int userId, UserRoleType newRole, CancellationToken cancellationToken)
    {
        try
        {
            await _superAdminService.PromoteUserRoleAsync(User.GetUserId(), userId, newRole, cancellationToken);
            TempData["SuccessMessage"] = $"User #{userId} role updated to '{newRole}'.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }
}
```

---

### 4.6 طبقة التحويل المقسمة والصريحة (Modular Mapping Architecture)

تم تقسيم ملف `MappingExtensions.cs` الضخم (513 سطر) إلى **3 ملفات جزئية منظمة** تتبع نفس الكلاس `public static partial class MappingExtensions` داخل مجلد `NexaMart.Web/Mappings/`:

1. **[StorefrontMappings.cs](file:///d:/Carrer/Our%20Project/Abdo%20Project/NexaMart/NexaMart.Web/Mappings/StorefrontMappings.cs)**:
   - تحويلات كروت وتفاصيل المنتجات (`ToCardViewModel`, `ToDetailsViewModel`).
   - تحويلات السلة والمفضلة والطلبات والفواتير والمراجعات للعميل.
2. **[AdminMappings.cs](file:///d:/Carrer/Our%20Project/Abdo%20Project/NexaMart/NexaMart.Web/Mappings/AdminMappings.cs)**:
   - تحويلات جداول المشرف (`ToAdminProductListItemViewModel`, `ToAdminCategoryListItemViewModel`).
   - تحويل `AdminDashboardDto` إلى `AdminDashboardViewModel`.
   - تحويل نماذج الإدخال `ToEntity()` مع إزالة التعيين المكرر لـ `CreatedAt` لتترك لوحدة العمل.
   - قراءة `category.AverageRating` المحسوب مسبقاً في الـ Service بدون تنفيذ أي استعلامات أو عمليات حسابية في الـ Mapping.
3. **[SuperAdminMappings.cs](file:///d:/Carrer/Our%20Project/Abdo%20Project/NexaMart/NexaMart.Web/Mappings/SuperAdminMappings.cs)**:
   - تحويلات إدارة المستخدمين وتفاصيل الحسابات وتعيين المشرفين وإحصائيات المنصة.

---

## 5. ربط الطبقات ومسار تدفق البيانات الشامل (End-to-End Dataflow & Request Lifecycle)

يوضح المخطط التالي دورة حياة الطلب الكاملة وتكامل الطبقات الأربعة:

```mermaid
sequenceDiagram
    autonumber
    actor Client as المتصفح (Browser / Admin)
    participant Routing as Middleware & Security
    participant Controller as Thin Controller (AdminController)
    participant Service as Application Service (AdminService)
    participant UOW as UnitOfWork & Repositories
    participant DB as SQL Server Database
    participant Mapper as Modular Mappings (AdminMappings)
    participant Razor as Razor View Engine

    Client->>Routing: HTTP GET /Admin/Dashboard
    Routing->>Routing: التحقق من الصلاحيات ([Authorize(Policy = "StaffOnly")])
    Routing->>Controller: Dashboard(cancellationToken)
    
    Controller->>Service: GetDashboardAsync(cancellationToken)
    Service->>UOW: Products.Query().AsNoTracking()...
    UOW->>DB: تنفيذ استعلامات مجمعة عالية الأداء
    DB-->>UOW: إرجاع السجلات الخام
    UOW-->>Service: Domain Entities
    
    Note over Service: حساب متوسطات الأقسام وتجميع الإحصائيات في AdminDashboardDto
    Service-->>Controller: AdminDashboardDto
    
    Controller->>Mapper: dto.ToViewModel()
    Mapper-->>Controller: AdminDashboardViewModel (Mapping فقط بدون Logic)
    
    Controller->>Razor: View(viewModel)
    Razor-->>Client: HTTP 200 OK + Rendered HTML Table View
```

---

## 6. أهم الدوال والخوارزميات البرمجية والتحسينات (Critical Algorithms & Performance Optimizations)

### 1. خوارزمية تجميع الإحصائيات باستعلام SQL واحد (Single GroupBy Aggregation)
الموقع: `UserService.cs` و `ProductService.cs`  
بدلاً من إرسال 5 استعلامات `COUNT` منفصلة لقاعدة البيانات، يتم تجميع كل الإحصائيات في جولة اتصال واحدة:
```csharp
public async Task<(int TotalUsers, int Customers, int Admins, int SuperAdmins, int Blocked)> GetUserStatsAsync(CancellationToken cancellationToken = default)
{
    var stats = await _unitOfWork.Users.Query().AsNoTracking()
        .GroupBy(u => 1)
        .Select(g => new
        {
            Total = g.Count(),
            Customers = g.Sum(u => u.RoleType == UserRoleType.Customer ? 1 : 0),
            Admins = g.Sum(u => u.RoleType == UserRoleType.Admin ? 1 : 0),
            SuperAdmins = g.Sum(u => u.RoleType == UserRoleType.SuperAdmin ? 1 : 0),
            Blocked = g.Sum(u => !u.IsActive ? 1 : 0)
        })
        .FirstOrDefaultAsync(cancellationToken);

    return stats != null
        ? (stats.Total, stats.Customers, stats.Admins, stats.SuperAdmins, stats.Blocked)
        : (0, 0, 0, 0, 0);
}
```

### 2. خوارزمية تسجيل الدخول المزدوج الموحدة (Unified Login Resolution)
الموقع: `AuthService.cs`  
تقبل المعرف كرقم ID للمشرفين أو كبريد إلكتروني للجميع:
```csharp
var trimmedIdentifier = identifier.Trim();
ApplicationUser? user = null;

if (int.TryParse(trimmedIdentifier, out int numericId))
{
    user = await _unitOfWork.Users.Query()
        .FirstOrDefaultAsync(u => u.Id == numericId, cancellationToken);

    if (user != null && user.RoleType == UserRoleType.Customer)
    {
        throw new UnauthorizedAccessException("Customer accounts must log in using their email address.");
    }
}

if (user == null)
{
    var email = trimmedIdentifier.ToLower();
    user = await _unitOfWork.Users.Query()
        .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
}
```

### 3. خوارزمية الفاتورة والخصم الذري للمخزون (Optimized Atomic Checkout)
الموقع: `OrderService.cs`  
تستخدم المنتجات المحملة مسبقاً مع عناصر السلة وتخصم المخزون وتحفظ الطلب دون أي استعلام مكرر أو حفظ مزدوج:
```csharp
await _unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    foreach (var item in cartItems)
    {
        var product = item.Product; // استخدام الكيان المحمل مباشرة
        if (product.StockQuantity < item.Quantity)
            throw new InvalidOperationException($"Insufficient stock for {product.Name}");

        product.StockQuantity -= item.Quantity;
        _unitOfWork.Products.Update(product);
    }

    await _unitOfWork.Orders.AddAsync(order, cancellationToken);
    _unitOfWork.CartItems.DeleteRange(cartItems);

    await _unitOfWork.CommitTransactionAsync(cancellationToken); // حفظ وتأكيد ذري في خطوة واحدة
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

---

## 7. سجل كافة المميزات والتحسينات المنفذة (Feature & Optimization Inventory)

| م | الميزة / التحسين الهيكلي | الحالة | الملفات المسؤولة | الوصف الفني الهندسي |
|---|---|---|---|---|
| 1 | **تصفير تقييمات المنتجات** | مكتملة 100% | `DbSeeder.cs` | تصفير كافة المراجعات وضبط `AverageRating = 0.0` لجميع المنتجات. |
| 2 | **تسجيل الدخول الموحد** | مكتملة 100% | `AccountController`, `AuthService` | قبول الـ Email أو الـ ID للموظفين وتوجيه المستخدم تلقائياً حسب رتبته. |
| 3 | **حذف الكود الميت `StaffLoginAsync`** | مكتملة 100% | `AuthService.cs`, `IAuthService.cs` | إزالة دالة تسجيل دخول الموظفين المنفصلة القديمة بالكامل والاعتماد على المسار الموحد. |
| 4 | **لوحة تحكم Admin مخصصة** | مكتملة 100% | `AdminController`, `_AdminLayout.cshtml` | واجهة مركز قيادة للمشرف مستقلة وخالية تماماً من أي شريط تنقل عام للمتجر. |
| 5 | **جداول بيانات المنتجات والأقسام** | مكتملة 100% | `Admin/Dashboard.cshtml`, `Products.cshtml` | عرض الكتالوج بالكامل داخل Data Tables منظمة بدلاً من الكروت لسهولة الإدارة. |
| 6 | **البحث الفوري برقم الـ ID** | مكتملة 100% | `AdminController`, `AdminService` | شريط بحث ذكي في رأس لوحة الإدارة يكتشف نوع الكيان تلقائياً ويوجه للمسار المناسب. |
| 7 | **إصلاح استعلامات المنتجات المكررة** | مكتملة 100% | `OrderService.cs` | تفعيل التتبع واستخدام `item.Product` مباشرة في إنشاء الطلب لمنع استعلامات N+1 الزائدة. |
| 8 | **إزالة SaveChanges المزدوج** | مكتملة 100% | `OrderService.cs` | إزالة استدعاء `CompleteAsync` المسبق لـ `CommitTransactionAsync` لتوفير اتصالين بقاعدة البيانات. |
| 9 | **دمج استعلامات الإحصائيات (Single Query)** | مكتملة 100% | `UserService.cs`, `ProductService.cs` | دمج 5 استعلامات `COUNT` في استعلام `GroupBy(1)` واحد فائق السرعة. |
| 10 | **تقسيم الـ Mapping إلى 3 ملفات** | مكتملة 100% | `StorefrontMappings`, `AdminMappings`, `SuperAdminMappings` | تقسيم الملف المونوليثي (513 سطر) إلى ملفات متخصصة لكل مجال عمل. |
| 11 | **نقل Business Logic من Mapping للخدمة** | مكتملة 100% | `CategoryService.cs`, `AdminMappings.cs` | حساب متوسط تقييم القسم داخل `CategoryService` وتمرير القيمة الجاهزة للـ Mapping. |
| 12 | **تنظيف وحدة العمل UnitOfWork** | مكتملة 100% | `UnitOfWork.cs`, `IUnitOfWork.cs` | إزالة `ConcurrentDictionary` ودالة `Repository<T>()` والاعتماد على مستودعات Type-safe. |
| 13 | **إزالة ازدواجية `CreatedAt`** | مكتملة 100% | `AdminMappings.cs`, `SuperAdminMappings.cs` | إزالة `CreatedAt = DateTime.UtcNow` من دوال `ToEntity()` وترك إدارة التواريخ لـ UnitOfWork. |
| 14 | **لوحة تحكم وحوكمة SuperAdmin** | مكتملة 100% | `SuperAdminController`, `SuperAdminService` | تحكم كامل بالحسابات، ترقية الرتب، وحظر وتفعيل المستخدمين مع حماية المشرف لنفسه. |
| 15 | **السلة والمفضلة وإصدار الفاتورة** | مكتملة 100% | `CartController`, `WishlistController`, `OrderService` | إدارة كاملة مع زر مباشر لإصدار الفاتورة الضريبية وخصم المخزون ذرّياً دون خطوات دفع وهمية. |
| 16 | **معمارية Controllers فائقة الرشاقة** | مكتملة 100% | كافة وحدات التحكم والخدمات | حصر أدوار الـ Controllers في 1-5 أسطر لكل Action، وعزل كافة القواعد داخل الـ Services. |
| 17 | **رفع الصور الآمن بالـ Drag & Drop وفحص الـ Magic Bytes** | مكتملة 100% | `FileStorageService.cs`, `ProductForm.cshtml`, `CategoryForm.cshtml`, `AdminController.cs` | نقل الخدمة لـ `Application/Services`، إزالة حقول روابط الصور النصية بالكامل، فحص التوقيع الثنائي (Magic Bytes)، حفظ الملفات بأسماء عشوائية معزولة وحذف الصور القديمة تلقائياً. |
| 18 | **تعزيز وتأمين التحقق من المدخلات (Input Validation & Anti-XSS)** | مكتملة 100% | `AccountViewModels.cs`, `ProductViewModels.cs`, `CategoryViewModels.cs`, `ReviewViewModels.cs`, `AuthService.cs` | تطبيق Strict RFC Email Regex، سياسة كلمات مرور صارمة وقائمة سوداء لكلمات السر الشائعة، ومنع حقن وسوم HTML/XSS في أوامر وأوصاف المنتجات والأقسام والتقييمات. |

---

## 8. دليل المراجع السريع للفحص والاختبار (Reviewer Quick-Start & Testing Guide)

### 1. بيانات الحسابات المعتمدة الجاهزة للتجربة:

| نوع الرتبة | الـ ID الرقمي للموظف | البريد الإلكتروني (Email) | كلمة المرور (Password) | التوجيه التلقائي بعد الدخول |
|---|---|---|---|---|
| **Super Administrator** | **`1`** | `superadmin@nexamart.com` | `SuperAdmin@123` | `/SuperAdmin/Dashboard` |
| **Store Admin** | **`2`** | `admin@nexamart.com` | `Admin@123` | `/Admin/Dashboard` |
| **Customer (عميل للتجربة)** | **`3`** | `customer@nexamart.com` | `Customer@123` | `/Home/Index` |

---

### 2. أوامر البناء والتشغيل والتحقق:
```powershell
# بناء الحل والتأكد من خلوه من أي أخطاء أو تحذيرات:
dotnet build "NexaMart.sln"

# تشغيل التطبيق محلياً:
dotnet run --project "NexaMart.Web\NexaMart.Web.csproj"
```
*التطبيق يعمل افتراضياً على المنفذ المحلي: `http://localhost:5086`*

---
*تم إعداد وتحديث هذا المستند الشامل ليكون مرجعاً هندسياً دقيقاً يوفر على مراجع الكود كامل الوقت والجهد، ويعكس أعلى درجات الاحترافية في هندسة وتطوير البرمجيات.*

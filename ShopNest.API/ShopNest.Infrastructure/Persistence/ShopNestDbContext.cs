using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopNest.Domain.Common;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence;

public sealed class ShopNestDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ShopNestDbContext(
        DbContextOptions<ShopNestDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderShippingAddress> OrderShippingAddresses => Set<OrderShippingAddress>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentTracking> ShipmentTrackings => Set<ShipmentTracking>();
    public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();
    public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes => Set<ReviewHelpfulVote>();
    public DbSet<ProductQuestion> ProductQuestions => Set<ProductQuestion>();
    public DbSet<ProductAnswer> ProductAnswers => Set<ProductAnswer>();
    
    public DbSet<Refund> Refunds => Set<Refund>();
    
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopNestDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion();
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "PasswordResetOtpHash", "EmailVerificationOtpHash",
        "TokenHash", "OtpHash", "Otp", "Secret", "ApiKey", "CreditCard", "Token", "SecretKey"
    };

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries().ToList();
        var utcNow = DateTime.UtcNow;
        var currentUser = GetCurrentUsername();
        var currentUserId = GetCurrentUserId();
        var currentRole = GetCurrentRole();
        var ip = GetIPAddress();
        var userAgent = GetUserAgent();
        var (browser, device, os) = ParseUserAgent(userAgent);
        var correlationId = GetCorrelationId();
        var requestId = GetRequestId();

        var audits = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog auditLog)
            {
                if (entry.State == EntityState.Added)
                {
                    if (auditLog.Id == Guid.Empty) auditLog.Id = Guid.NewGuid();
                    auditLog.CreatedAtUtc = utcNow;
                    auditLog.CreatedBy ??= currentUser;
                    auditLog.IsDeleted = false;

                    auditLog.IPAddress ??= ip;
                    auditLog.Browser ??= browser;
                    auditLog.Device ??= device;
                    auditLog.UserAgent ??= userAgent;
                    auditLog.CorrelationId ??= correlationId;
                    auditLog.RequestId ??= requestId;

                    if (string.IsNullOrEmpty(auditLog.EventType)) auditLog.EventType = "System";
                    if (string.IsNullOrEmpty(auditLog.Module)) auditLog.Module = "System";
                }
                continue;
            }

            if (entry.Entity is BaseEntity baseEntity)
            {
                // Ensure Guid Id is populated on addition
                if (entry.State == EntityState.Added && baseEntity.Id == Guid.Empty)
                {
                    baseEntity.Id = Guid.NewGuid();
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAtUtc = utcNow;
                        baseEntity.CreatedBy ??= currentUser;
                        baseEntity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        baseEntity.UpdatedAtUtc = utcNow;
                        baseEntity.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        // Intercept deletion for soft delete
                        entry.State = EntityState.Modified;
                        baseEntity.IsDeleted = true;
                        baseEntity.DeletedAtUtc = utcNow;
                        baseEntity.DeletedBy = currentUser;
                        break;
                }

                // Check if this change should be audited
                if (entry.Entity is not AuditLog && 
                    entry.Entity is not ActivityLog && 
                    entry.Entity is not LoginHistory && 
                    entry.Entity is not ErrorLog && 
                    entry.Entity is not ApplicationLog && 
                    (entry.State == EntityState.Added || entry.State == EntityState.Modified || baseEntity.IsDeleted))
                {
                    var actionName = entry.State switch
                    {
                        EntityState.Added => "Create",
                        EntityState.Modified => baseEntity.IsDeleted ? "Delete" : "Update",
                        _ => "Unknown"
                    };

                    var moduleName = entry.Entity.GetType().Namespace?.Split('.').LastOrDefault() ?? "System";
                    var entityName = entry.Entity.GetType().Name;
                    var entityId = baseEntity.Id.ToString();

                    var oldValues = new Dictionary<string, object?>();
                    var newValues = new Dictionary<string, object?>();

                    if (entry.State == EntityState.Added)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            var name = prop.Metadata.Name;
                            if (SensitiveProperties.Contains(name))
                                newValues[name] = "[REDACTED]";
                            else
                                newValues[name] = prop.CurrentValue;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            if (prop.IsModified || (baseEntity.IsDeleted && prop.Metadata.Name == "IsDeleted"))
                            {
                                var name = prop.Metadata.Name;
                                if (SensitiveProperties.Contains(name))
                                {
                                    oldValues[name] = "[REDACTED]";
                                    newValues[name] = "[REDACTED]";
                                }
                                else
                                {
                                    oldValues[name] = prop.OriginalValue;
                                    newValues[name] = prop.CurrentValue;
                                }
                            }
                        }
                    }

                    audits.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUserId,
                        Role = currentRole,
                        EventType = "DataChange",
                        Module = moduleName,
                        EntityName = entityName,
                        EntityId = entityId,
                        Action = actionName,
                        OldValues = oldValues.Any() ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
                        NewValues = newValues.Any() ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
                        Description = $"{actionName} {entityName} (Id: {entityId})",
                        IPAddress = ip,
                        Browser = browser,
                        Device = device,
                        UserAgent = userAgent,
                        CorrelationId = correlationId,
                        RequestId = requestId,
                        CreatedAtUtc = utcNow,
                        CreatedBy = currentUser
                    });
                }
            }
        }

        if (audits.Any())
        {
            AuditLogs.AddRange(audits);
        }
    }

    private string GetCurrentUsername()
    {
        try
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(ClaimTypes.Name)?.Value 
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? "System";
            }
        }
        catch
        {
            // Fail silently during design-time migrations or setup
        }

        return "System";
    }

    private Guid? GetCurrentUserId()
    {
        try
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idClaim, out var guid))
            {
                return guid;
            }
        }
        catch { }
        return null;
    }

    private string? GetCurrentRole()
    {
        try
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Role)?.Value;
        }
        catch { return null; }
    }

    private string GetIPAddress()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }

    private string? GetUserAgent()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }
        catch { return null; }
    }

    private string? GetCorrelationId()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers["X-Correlation-Id"].ToString() 
                   ?? _httpContextAccessor?.HttpContext?.Items["CorrelationId"]?.ToString();
        }
        catch { return null; }
    }

    private string? GetRequestId()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?.TraceIdentifier;
        }
        catch { return null; }
    }

    private (string Browser, string Device, string OS) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return ("Unknown", "Unknown", "Unknown");

        var ua = userAgent.ToLower();
        var browser = "Other";
        var os = "Other";
        var device = "Desktop";

        if (ua.Contains("edg")) browser = "Edge";
        else if (ua.Contains("chrome")) browser = "Chrome";
        else if (ua.Contains("safari")) browser = "Safari";
        else if (ua.Contains("firefox")) browser = "Firefox";
        else if (ua.Contains("opera") || ua.Contains("opr")) browser = "Opera";

        if (ua.Contains("windows")) os = "Windows";
        else if (ua.Contains("android")) os = "Android";
        else if (ua.Contains("iphone") || ua.Contains("ipad")) os = "iOS";
        else if (ua.Contains("macintosh") || ua.Contains("mac os")) os = "macOS";
        else if (ua.Contains("linux")) os = "Linux";

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) device = "Mobile";
        else if (ua.Contains("ipad") || ua.Contains("tablet")) device = "Tablet";

        return (browser, device, os);
    }

    // Hangfire cleanup tasks
    public void CleanupExpiredOtps()
    {
        Database.ExecuteSqlRaw("DELETE FROM OtpVerifications WHERE ExpiresAtUtc < GETUTCDATE()");
    }

    public void CleanupRevokedTokens()
    {
        Database.ExecuteSqlRaw("DELETE FROM RefreshTokens WHERE ExpiresAtUtc < GETUTCDATE()");
    }
}

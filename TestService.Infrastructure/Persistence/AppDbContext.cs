using Microsoft.EntityFrameworkCore;
using TestService.Domain.Common;
using TestService.Domain.Entities;
using TestService.Domain.Interfaces;

namespace TestService.Infrastructure.Persistence;

/// <summary>
/// 应用 DbContext：唯一负责 EF Core 的实体映射与查询。
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SysUser> Users => Set<SysUser>();
    public DbSet<SysRole> Roles => Set<SysRole>();
    public DbSet<SysUserRole> UserRoles => Set<SysUserRole>();
    public DbSet<CusCustomer> Customers => Set<CusCustomer>();
    public DbSet<ProProduct> Products => Set<ProProduct>();
    public DbSet<SoSalesOrder> SalesOrders => Set<SoSalesOrder>();
    public DbSet<SoSalesOrderItem> SalesOrderItems => Set<SoSalesOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 全局：软删除过滤
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(FullAuditedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }

        // SysUser
        modelBuilder.Entity<SysUser>(b =>
        {
            b.ToTable("sys_user");
            b.HasKey(x => x.Id);
            b.Property(x => x.Username).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            b.Property(x => x.Salt).HasMaxLength(64);
            b.Property(x => x.RealName).HasMaxLength(64);
            b.Property(x => x.Email).HasMaxLength(128);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.HasMany(x => x.UserRoles).WithOne(x => x.User!)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // SysRole
        modelBuilder.Entity<SysRole>(b =>
        {
            b.ToTable("sys_role");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(64).IsRequired();
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        // SysUserRole
        modelBuilder.Entity<SysUserRole>(b =>
        {
            b.ToTable("sys_user_role");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            b.HasOne(x => x.Role).WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // CusCustomer
        modelBuilder.Entity<CusCustomer>(b =>
        {
            b.ToTable("cus_customer");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.ContactName).HasMaxLength(64);
            b.Property(x => x.ContactPhone).HasMaxLength(32);
            b.Property(x => x.ContactEmail).HasMaxLength(128);
            b.Property(x => x.Address).HasMaxLength(255);
            b.Property(x => x.Remark).HasMaxLength(500);
        });

        // ProProduct
        modelBuilder.Entity<ProProduct>(b =>
        {
            b.ToTable("pro_product");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Category).HasMaxLength(64);
            b.Property(x => x.Unit).HasMaxLength(16);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.CostPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.Description).HasMaxLength(500);
        });

        // SoSalesOrder
        modelBuilder.Entity<SoSalesOrder>(b =>
        {
            b.ToTable("so_sales_order");
            b.HasKey(x => x.Id);
            b.Property(x => x.OrderNo).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.OrderNo).IsUnique();
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Remark).HasMaxLength(500);
            b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.Order!)
                .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // SoSalesOrderItem
        modelBuilder.Entity<SoSalesOrderItem>(b =>
        {
            b.ToTable("so_sales_order_item");
            b.HasKey(x => x.Id);
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : FullAuditedEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}

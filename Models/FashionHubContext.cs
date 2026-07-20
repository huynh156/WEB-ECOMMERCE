using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FashionHubWeb.Models;

public partial class FashionHubContext : DbContext
{
    public FashionHubContext()
    {
    }

    public FashionHubContext(DbContextOptions<FashionHubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryPromotion> CategoryPromotions { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPromotion> ProductPromotions { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Shipper> Shippers { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=FashionHub;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__DAD4F3BE058A0163");

            entity.Property(e => e.BrandId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("BrandID");
            entity.Property(e => e.BrandName).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD797F550C64D");

            entity.Property(e => e.CartId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CartID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Carts__ProductID__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Carts__UserID__5DCAEF64");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B1675726C");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<CategoryPromotion>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CategoryId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryPromotion1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CategoryPromotion");
            entity.Property(e => e.PromotionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PromotionID");

            entity.HasOne(d => d.Category).WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryP__Categ__5EBF139D");

            entity.HasOne(d => d.Promotion).WithMany()
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryP__Promo__5FB337D6");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupons__384AF1DA6104679C");

            entity.HasIndex(e => e.CouponCode, "UQ__Coupons__D3490800FE708E19").IsUnique();

            entity.Property(e => e.CouponId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CouponID");
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("fixed");
            entity.Property(e => e.MaxDiscount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinOrderValue).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF76EBA00A");

            entity.Property(e => e.OrderId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("OrderID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.CouponId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CouponID");
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ShipperId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ShipperID");
            entity.Property(e => e.ShippingMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StatusId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("StatusID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Orders__CouponID__4CA06362");

            entity.HasOne(d => d.Shipper).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShipperId)
                .HasConstraintName("FK_Orders_Shippers");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Orders_Status");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Orders__UserID__49C3F6B7");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30CC674C780");

            entity.Property(e => e.OrderDetailId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("OrderID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderDeta__Order__4F7CD00D");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderDeta__Produ__5070F446");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED7920045D");

            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.BrandId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("BrandID");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CategoryID");
            entity.Property(e => e.CouponId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CouponID");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.SlugName).HasMaxLength(255);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Products__BrandI__68487DD7");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Products__Catego__693CA210");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Products)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Products__Coupon__6A30C649");
        });

        modelBuilder.Entity<ProductPromotion>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.ProductPromotionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductPromotionID");
            entity.Property(e => e.PromotionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PromotionID");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPr__Produ__66603565");

            entity.HasOne(d => d.Promotion).WithMany()
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPr__Promo__6754599E");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.Property(e => e.PromotionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PromotionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.Property(e => e.PurchaseOrderId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PurchaseOrderID");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.ReceivedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("pending");
            entity.Property(e => e.SupplierId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__PurchaseO__Suppl__6D0D32F4");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.PurchaseOrderDetailId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PurchaseOrderDetailID");
            entity.Property(e => e.PurchaseOrderId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PurchaseOrderID");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", true)
                .HasColumnType("decimal(21, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__PurchaseO__Produ__6B24EA82");

            entity.HasOne(d => d.PurchaseOrder).WithMany()
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("FK__PurchaseO__Purch__6C190EBB");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AEEFFAA9FA");

            entity.Property(e => e.ReviewId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ReviewID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Reviews__Product__6E01572D");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Reviews__UserID__6EF57B66");
        });

        modelBuilder.Entity<Shipper>(entity =>
        {
            entity.HasKey(e => e.ShipperId).HasName("PK__Shippers__1F8AFFB94B709BC4");

            entity.Property(e => e.ShipperId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ShipperID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ShipperName).HasMaxLength(255);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Status__C8EE204306200B68");

            entity.ToTable("Status");

            entity.Property(e => e.StatusId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("StatusID");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ__Supplier__A9D10534FD77EA38").IsUnique();

            entity.Property(e => e.SupplierId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.ContactName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC3B701C68");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E486578D2E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534311213BF").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RandomKey).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__233189CB0F97A9B7");

            entity.ToTable("Wishlist");

            entity.Property(e => e.WishlistId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("WishlistID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Wishlist__Produc__6FE99F9F");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Wishlist__UserID__70DDC3D8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

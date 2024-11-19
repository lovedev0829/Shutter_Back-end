using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShutterManagement.DAL.Context;

public partial class BtprojecShuttermanagementDevContext : DbContext
{
    public BtprojecShuttermanagementDevContext()
    {
    }

    public BtprojecShuttermanagementDevContext(DbContextOptions<BtprojecShuttermanagementDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstModule> MstModules { get; set; }

    public virtual DbSet<MstModulePermission> MstModulePermissions { get; set; }

    public virtual DbSet<MstProductType> MstProductTypes { get; set; }

    public virtual DbSet<TblAddress> TblAddresses { get; set; }

    public virtual DbSet<TblAssembly> TblAssemblies { get; set; }

    public virtual DbSet<TblComponent> TblComponents { get; set; }

    public virtual DbSet<TblComponentMaterial> TblComponentMaterials { get; set; }

    public virtual DbSet<TblContact> TblContacts { get; set; }

    public virtual DbSet<TblContactBillingAddress> TblContactBillingAddresses { get; set; }

    public virtual DbSet<TblContactRole> TblContactRoles { get; set; }

    public virtual DbSet<TblEstimate> TblEstimates { get; set; }

    public virtual DbSet<TblInvoice> TblInvoices { get; set; }

    public virtual DbSet<TblJob> TblJobs { get; set; }

    public virtual DbSet<TblJobComponent> TblJobComponents { get; set; }

    public virtual DbSet<TblJobStatus> TblJobStatuses { get; set; }

    public virtual DbSet<TblMeasurement> TblMeasurements { get; set; }

    public virtual DbSet<TblOption> TblOptions { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblRawMaterial> TblRawMaterials { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolesPermission> TblRolesPermissions { get; set; }

    public virtual DbSet<TblTaxExemption> TblTaxExemptions { get; set; }

    public virtual DbSet<TblTaxRate> TblTaxRates { get; set; }

    public virtual DbSet<TblTenant> TblTenants { get; set; }

    public virtual DbSet<TblTenantBrandingInfo> TblTenantBrandingInfos { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

    public virtual DbSet<TblVendor> TblVendors { get; set; }

    public virtual DbSet<TblVendorOrder> TblVendorOrders { get; set; }

    public virtual DbSet<TblVendorOrderItem> TblVendorOrderItems { get; set; }

    public virtual DbSet<TblWindow> TblWindows { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=103.93.16.117;Database=btprojec_shuttermanagement_dev;user id=btprojec_admin;password=Bwy0w65ixN*bsE9wy;Integrated Security=false;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("btprojec_admin");

        modelBuilder.Entity<MstModule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC07A52C80C7");

            entity.ToTable("MstModule", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ModuleDisplayName).HasMaxLength(200);
            entity.Property(e => e.ModuleName).HasMaxLength(200);
        });

        modelBuilder.Entity<MstModulePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC07A88EEEB2");

            entity.ToTable("MstModule_Permission", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Icon).HasMaxLength(500);
            entity.Property(e => e.ModulePermission).HasMaxLength(100);
            entity.Property(e => e.PermissionDisplayName).HasMaxLength(100);
            entity.Property(e => e.Url)
                .HasMaxLength(1000)
                .HasColumnName("URL");

            entity.HasOne(d => d.Module).WithMany(p => p.MstModulePermissions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__MstModule__Modul__58D1301D");
        });

        modelBuilder.Entity<MstProductType>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__MstProdu__B40CC6CD3183F577");

            entity.ToTable("MstProductType", "dbo");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__TblAddre__091C2AFBF6AE7E7F");

            entity.ToTable("TblAddress", "dbo");

            entity.Property(e => e.AddressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Contact).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblAddres__Conta__59C55456");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblAddres__Tenan__3FD07829");
        });

        modelBuilder.Entity<TblAssembly>(entity =>
        {
            entity.HasKey(e => e.AssemblyId).HasName("PK__TblAssem__1C02BDABE36F3A37");

            entity.ToTable("TblAssembly", "dbo");

            entity.Property(e => e.AssemblyId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.AssemblyName).HasMaxLength(255);
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TblComponent>(entity =>
        {
            entity.HasKey(e => e.ComponentId).HasName("PK__TblCompo__D79CF04E96E58156");

            entity.ToTable("TblComponent", "dbo");

            entity.Property(e => e.ComponentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ComponentName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblCompon__Tenan__79FD19BE");
        });

        modelBuilder.Entity<TblComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.ComponentMaterialId).HasName("PK__TblCompo__82FB769C2564F192");

            entity.ToTable("TblComponentMaterial", "dbo");

            entity.Property(e => e.ComponentMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WasteFactor).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Component).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblCompon__Compo__5AB9788F");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.RawMaterialId)
                .HasConstraintName("FK__TblCompon__RawMa__5BAD9CC8");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblCompon__Tenan__7AF13DF7");
        });

        modelBuilder.Entity<TblContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__TblConta__5C66259BCAB00ED9");

            entity.ToTable("TblContact", "dbo");

            entity.Property(e => e.ContactId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ContactType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Referral).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Website).HasMaxLength(255);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblContacts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblContac__Tenan__5CA1C101");
        });

        modelBuilder.Entity<TblContactBillingAddress>(entity =>
        {
            entity.HasKey(e => e.BillingAddressId).HasName("PK__TblConta__90BDB2B1163BA372");

            entity.ToTable("TblContact_billingAddress", "dbo");

            entity.Property(e => e.BillingAddressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.FullName)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.StreetAddress).HasMaxLength(250);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Contact).WithMany(p => p.TblContactBillingAddresses)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblContac__Conta__5D95E53A");
        });

        modelBuilder.Entity<TblContactRole>(entity =>
        {
            entity.HasKey(e => e.ContactRoleId).HasName("PK__TblConta__38FF7537246E4F89");

            entity.ToTable("TblContactRole", "dbo");

            entity.Property(e => e.ContactRoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.RoleType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblContactRoles)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblContac__Conta__5E8A0973");
        });

        modelBuilder.Entity<TblEstimate>(entity =>
        {
            entity.HasKey(e => e.EstimateId).HasName("PK__TblEstim__ABEBF4B5AEEFB2E9");

            entity.ToTable("TblEstimate", "dbo");

            entity.HasIndex(e => e.EstimateNumber, "UQ__TblEstim__0B3C039013833176").IsUnique();

            entity.Property(e => e.EstimateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstimateNumber).HasMaxLength(50);
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.TblEstimates)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblEstima__JobId__5F7E2DAC");
        });

        modelBuilder.Entity<TblInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__TblInvoi__D796AAB5ED5E9A49");

            entity.ToTable("TblInvoice", "dbo");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__TblInvoi__D776E981C4B274EE").IsUnique();

            entity.Property(e => e.InvoiceId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BalanceDue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.TblInvoices)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblInvoic__JobId__607251E5");
        });

        modelBuilder.Entity<TblJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__TblJob__056690C26EFF9CAD");

            entity.ToTable("TblJob", "dbo");

            entity.Property(e => e.JobId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.JobLabel).HasMaxLength(255);
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblJob__AddressI__6166761E");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblJob__ContactI__625A9A57");

            entity.HasOne(d => d.Status).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__TblJob__StatusId__0E04126B");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblJob__TenantId__65F62111");
        });

        modelBuilder.Entity<TblJobComponent>(entity =>
        {
            entity.HasKey(e => e.JobComponentId).HasName("PK__TblJobCo__E85D56C0B9BBDFB8");

            entity.ToTable("TblJobComponent", "dbo");

            entity.Property(e => e.JobComponentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.ComponentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.ProductionEndDate).HasColumnType("datetime");
            entity.Property(e => e.ProductionStartDate).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Assembly).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.AssemblyId)
                .HasConstraintName("FK__TblJobCom__Assem__634EBE90");

            entity.HasOne(d => d.Component).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblJobCom__Compo__6442E2C9");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblJobCom__JobId__65370702");
        });

        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__tblJobSt__C8EE20634E71110F");

            entity.ToTable("tblJobStatus", "dbo");

            entity.Property(e => e.StatusId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.StatusName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblMeasurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__TblMeasu__85599FB873C0E0A4");

            entity.ToTable("TblMeasurement", "dbo");

            entity.Property(e => e.MeasurementId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.TblMeasurements)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblMeasur__Addre__662B2B3B");
        });

        modelBuilder.Entity<TblOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__TblOptio__92C7A1FFEA70BF74");

            entity.ToTable("TblOptions", "dbo");

            entity.Property(e => e.OptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.OptionName).HasMaxLength(255);
            entity.Property(e => e.OptionType).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ProductType).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.ProductTypeId)
                .HasConstraintName("FK__TblOption__Produ__10E07F16");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblOption__Tenan__7DCDAAA2");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__TblPayme__9B556A3882F4472D");

            entity.ToTable("TblPayment", "dbo");

            entity.Property(e => e.PaymentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__TblPaymen__Invoi__671F4F74");
        });

        modelBuilder.Entity<TblRawMaterial>(entity =>
        {
            entity.HasKey(e => e.RawMaterialId).HasName("PK__TblRawMa__D99FAE11851EE539");

            entity.ToTable("TblRawMaterial", "dbo");

            entity.Property(e => e.RawMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(255);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitOfMeasurement)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblRawMat__Tenan__7908F585");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__TblRawMat__Vendo__681373AD");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblRoles__3214EC072F86598F");

            entity.ToTable("TblRoles", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsShow).HasDefaultValue(false);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(100);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblRoles)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblRoles__Tenant__6AEFE058");
        });

        modelBuilder.Entity<TblRolesPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblRoles__3214EC07B4BAD913");

            entity.ToTable("TblRoles_Permission", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ModulePermission).WithMany(p => p.TblRolesPermissions)
                .HasForeignKey(d => d.ModulePermissionId)
                .HasConstraintName("FK__TblRoles___Modul__2EA5EC27");

            entity.HasOne(d => d.Role).WithMany(p => p.TblRolesPermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblRoles___RoleI__2DB1C7EE");
        });

        modelBuilder.Entity<TblTaxExemption>(entity =>
        {
            entity.HasKey(e => e.TaxExemptionId).HasName("PK__TblTaxEx__BCE64BE6436D5A55");

            entity.ToTable("TblTaxExemption", "dbo");

            entity.Property(e => e.TaxExemptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.ExemptionCertificate).HasMaxLength(255);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblTaxExemptions)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblTaxExe__Conta__6BE40491");
        });

        modelBuilder.Entity<TblTaxRate>(entity =>
        {
            entity.HasKey(e => e.TaxRateId).HasName("PK__TblTaxRa__B114CEC1EE287C42");

            entity.ToTable("TblTaxRate", "dbo");

            entity.Property(e => e.TaxRateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Rate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTenant>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("PK__TblTenan__2E9B47E14A989554");

            entity.ToTable("TblTenant", "dbo");

            entity.Property(e => e.TenantId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code).HasMaxLength(128);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(128);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LastName).HasMaxLength(128);
            entity.Property(e => e.MiddleName).HasMaxLength(128);
            entity.Property(e => e.ModifiedBy).HasColumnName("ModifiedBY");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(128);

            entity.HasOne(d => d.BrandingInfoNavigation).WithMany(p => p.TblTenants)
                .HasForeignKey(d => d.BrandingInfo)
                .HasConstraintName("FK__TblTenant__Brand__6CD828CA");

            entity.HasOne(d => d.ParentTenant).WithMany(p => p.InverseParentTenant)
                .HasForeignKey(d => d.ParentTenantId)
                .HasConstraintName("FK__TblTenant__Paren__6DCC4D03");
        });

        modelBuilder.Entity<TblTenantBrandingInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblTenan__3214EC077ED83EF7");

            entity.ToTable("TblTenant_BrandingInfo", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser__3213E83F28028BF7");

            entity.ToTable("TblUser", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CreatedAT");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(100);
            entity.Property(e => e.Pwd)
                .HasMaxLength(128)
                .HasColumnName("pwd");
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblUser__TenantI__6EC0713C");
        });

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser___3214EC0755EA2C88");

            entity.ToTable("TblUser_Roles", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUser_R__RoleI__70A8B9AE");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblUser_R__UserI__719CDDE7");
        });

        modelBuilder.Entity<TblVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__TblVendo__FC8618F33D6BB854");

            entity.ToTable("TblVendor", "dbo");

            entity.Property(e => e.VendorId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VendorName).HasMaxLength(255);
        });

        modelBuilder.Entity<TblVendorOrder>(entity =>
        {
            entity.HasKey(e => e.VendorOrderId).HasName("PK__TblVendo__477DCB3BE9BC2A7D");

            entity.ToTable("TblVendorOrder", "dbo");

            entity.HasIndex(e => e.OrderNumber, "UQ__TblVendo__CAC5E7437EC0F4A6").IsUnique();

            entity.Property(e => e.VendorOrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblVendor__JobId__72910220");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__TblVendor__Vendo__73852659");
        });

        modelBuilder.Entity<TblVendorOrderItem>(entity =>
        {
            entity.HasKey(e => e.VendorOrderItemId).HasName("PK__TblVendo__A5401ACCA1A820E9");

            entity.ToTable("TblVendorOrderItem", "dbo");

            entity.Property(e => e.VendorOrderItemId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.ItemDescription).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.VendorOrder).WithMany(p => p.TblVendorOrderItems)
                .HasForeignKey(d => d.VendorOrderId)
                .HasConstraintName("FK__TblVendor__Vendo__74794A92");
        });

        modelBuilder.Entity<TblWindow>(entity =>
        {
            entity.HasKey(e => e.WindowId).HasName("PK__TblWindo__1EEC642937E205D0");

            entity.ToTable("TblWindow", "dbo");

            entity.Property(e => e.WindowId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Address).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblWindow__Addre__756D6ECB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

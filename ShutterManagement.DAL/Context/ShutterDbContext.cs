using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShutterManagement.DAL.Context;

public partial class ShutterDbContext : DbContext
{
    public ShutterDbContext()
    {
    }

    public ShutterDbContext(DbContextOptions<ShutterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstModule> MstModules { get; set; }

    public virtual DbSet<MstModulePermission> MstModulePermissions { get; set; }

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

    public virtual DbSet<Tbttest> Tbttests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=103.93.16.117;Database=btprojec_shuttermanagement_dev;user id=btprojec_admin;password=Bwy0w65ixN*bsE9wy;Integrated Security=false;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MstModule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC071549A1AC");

            entity.ToTable("MstModule");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ModuleDisplayName).HasMaxLength(200);
            entity.Property(e => e.ModuleName).HasMaxLength(200);
        });

        modelBuilder.Entity<MstModulePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC07D283D662");

            entity.ToTable("MstModule_Permission");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ModulePermission).HasMaxLength(100);
            entity.Property(e => e.PermissionDisplayName).HasMaxLength(100);

            entity.HasOne(d => d.Module).WithMany(p => p.MstModulePermissions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__MstModule__Modul__22FF2F51");
        });

        modelBuilder.Entity<TblAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__TblAddre__091C2AFB1A8F5F02");

            entity.ToTable("TblAddress");

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
                .HasConstraintName("FK__TblAddres__Conta__571DF1D5");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblAddres__Tenan__58671BC9");
        });

        modelBuilder.Entity<TblAssembly>(entity =>
        {
            entity.HasKey(e => e.AssemblyId).HasName("PK__TblAssem__1C02BDAB5C078190");

            entity.ToTable("TblAssembly");

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
            entity.HasKey(e => e.ComponentId).HasName("PK__TblCompo__D79CF04E02A088B0");

            entity.ToTable("TblComponent");

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
        });

        modelBuilder.Entity<TblComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.ComponentMaterialId).HasName("PK__TblCompo__82FB769CCD541E20");

            entity.ToTable("TblComponentMaterial");

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
                .HasConstraintName("FK__TblCompon__Compo__7A672E12");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.RawMaterialId)
                .HasConstraintName("FK__TblCompon__RawMa__7B5B524B");
        });

        modelBuilder.Entity<TblContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__TblConta__5C66259B7FCD5D4D");

            entity.ToTable("TblContact");

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
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Website).HasMaxLength(255);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblContacts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblContac__Tenan__7FB5F314");
        });

        modelBuilder.Entity<TblContactBillingAddress>(entity =>
        {
            entity.HasKey(e => e.BillingAddressId).HasName("PK__TblConta__90BDB2B120EB5B56");

            entity.ToTable("TblContact_billingAddress");

            entity.Property(e => e.BillingAddressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Contact).WithMany(p => p.TblContactBillingAddresses)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblContac__Conta__7DCDAAA2");
        });

        modelBuilder.Entity<TblContactRole>(entity =>
        {
            entity.HasKey(e => e.ContactRoleId).HasName("PK__TblConta__38FF753705156FD5");

            entity.ToTable("TblContactRole");

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
                .HasConstraintName("FK__TblContac__Conta__5165187F");
        });

        modelBuilder.Entity<TblEstimate>(entity =>
        {
            entity.HasKey(e => e.EstimateId).HasName("PK__TblEstim__ABEBF4B5E1101214");

            entity.ToTable("TblEstimate");

            entity.HasIndex(e => e.EstimateNumber, "UQ__TblEstim__0B3C039071EEEA41").IsUnique();

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
                .HasConstraintName("FK__TblEstima__JobId__19DFD96B");
        });

        modelBuilder.Entity<TblInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__TblInvoi__D796AAB5CB02CE26");

            entity.ToTable("TblInvoice");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__TblInvoi__D776E98192A0510F").IsUnique();

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
                .HasConstraintName("FK__TblInvoic__JobId__22751F6C");
        });

        modelBuilder.Entity<TblJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__TblJob__056690C200F0FB9E");

            entity.ToTable("TblJob");

            entity.Property(e => e.JobId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.JobLabel).HasMaxLength(255);
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblJob__AddressI__114A936A");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblJob__ContactI__10566F31");
        });

        modelBuilder.Entity<TblJobComponent>(entity =>
        {
            entity.HasKey(e => e.JobComponentId).HasName("PK__TblJobCo__E85D56C0C9E15A19");

            entity.ToTable("TblJobComponent");

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
                .HasConstraintName("FK__TblJobCom__Assem__6166761E");

            entity.HasOne(d => d.Component).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblJobCom__Compo__607251E5");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblJobCom__JobId__5F7E2DAC");
        });

        modelBuilder.Entity<TblMeasurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__TblMeasu__85599FB80D6BC2E1");

            entity.ToTable("TblMeasurement");

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
                .HasConstraintName("FK__TblMeasur__Addre__66603565");
        });

        modelBuilder.Entity<TblOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__TblOptio__92C7A1FFFD451DEE");

            entity.Property(e => e.OptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.OptionName).HasMaxLength(255);
            entity.Property(e => e.OptionType).HasMaxLength(100);
            entity.Property(e => e.ProductType).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__TblPayme__9B556A386A6E8F3B");

            entity.ToTable("TblPayment");

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
                .HasConstraintName("FK__TblPaymen__Invoi__2A164134");
        });

        modelBuilder.Entity<TblRawMaterial>(entity =>
        {
            entity.HasKey(e => e.RawMaterialId).HasName("PK__TblRawMa__D99FAE11D40CD8DA");

            entity.ToTable("TblRawMaterial");

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

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__TblRawMat__Vendo__6E01572D");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblRoles__3214EC0763166A18");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(100);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblRoles)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblRoles__Tenant__28B808A7");
        });

        modelBuilder.Entity<TblRolesPermission>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TblRoles_Permission");

            entity.HasOne(d => d.ModulePermission).WithMany()
                .HasForeignKey(d => d.ModulePermissionId)
                .HasConstraintName("FK__TblRoles___Modul__6D6238AF");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblRoles___RoleI__6C6E1476");
        });

        modelBuilder.Entity<TblTaxExemption>(entity =>
        {
            entity.HasKey(e => e.TaxExemptionId).HasName("PK__TblTaxEx__BCE64BE6FDD15890");

            entity.ToTable("TblTaxExemption");

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
                .HasConstraintName("FK__TblTaxExe__Conta__3864608B");
        });

        modelBuilder.Entity<TblTaxRate>(entity =>
        {
            entity.HasKey(e => e.TaxRateId).HasName("PK__TblTaxRa__B114CEC16BD68FB4");

            entity.ToTable("TblTaxRate");

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
            entity.HasKey(e => e.TenantId).HasName("PK__TblTenan__2E9B47E1C470B793");

            entity.ToTable("TblTenant");

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
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(128);

            entity.HasOne(d => d.BrandingInfoNavigation).WithMany(p => p.TblTenants)
                .HasForeignKey(d => d.BrandingInfo)
                .HasConstraintName("FK__TblTenant__Brand__1B5E0D89");

            entity.HasOne(d => d.ParentTenant).WithMany(p => p.InverseParentTenant)
                .HasForeignKey(d => d.ParentTenantId)
                .HasConstraintName("FK__TblTenant__Paren__57DD0BE4");
        });

        modelBuilder.Entity<TblTenantBrandingInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblTenan__3214EC075DE33688");

            entity.ToTable("TblTenant_BrandingInfo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNmber).HasMaxLength(100);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser__3213E83FB218A81D");

            entity.ToTable("TblUser");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CreatedAT");
            entity.Property(e => e.Email).HasMaxLength(100);
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
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblUser__TenantI__02925FBF");
        });

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser___3214EC07FEFC8739");

            entity.ToTable("TblUser_Roles");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUser_R__RoleI__2D7CBDC4");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblUser_R__UserI__2C88998B");
        });

        modelBuilder.Entity<TblVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__TblVendo__FC8618F3F888C73F");

            entity.ToTable("TblVendor");

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
            entity.HasKey(e => e.VendorOrderId).HasName("PK__TblVendo__477DCB3BDFBD73C3");

            entity.ToTable("TblVendorOrder");

            entity.HasIndex(e => e.OrderNumber, "UQ__TblVendo__CAC5E743872E5A1D").IsUnique();

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
                .HasConstraintName("FK__TblVendor__JobId__489AC854");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__TblVendor__Vendo__47A6A41B");
        });

        modelBuilder.Entity<TblVendorOrderItem>(entity =>
        {
            entity.HasKey(e => e.VendorOrderItemId).HasName("PK__TblVendo__A5401ACC861C3F6C");

            entity.ToTable("TblVendorOrderItem");

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
                .HasConstraintName("FK__TblVendor__Vendo__503BEA1C");
        });

        modelBuilder.Entity<TblWindow>(entity =>
        {
            entity.HasKey(e => e.WindowId).HasName("PK__TblWindo__1EEC6429684B2A18");

            entity.ToTable("TblWindow");

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
                .HasConstraintName("FK__TblWindow__Addre__5EBF139D");
        });

        modelBuilder.Entity<Tbttest>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Tbttest");

            entity.Property(e => e.PasswordEncrypted)
                .HasMaxLength(128)
                .HasColumnName("Password_Encrypted");
            entity.Property(e => e.Pwd)
                .HasMaxLength(128)
                .HasColumnName("pwd");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

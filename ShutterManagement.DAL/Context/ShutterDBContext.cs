using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShutterManagement.DAL.Context;

public partial class ShutterDBContext : DbContext
{
    public ShutterDBContext()
    {
    }

    public ShutterDBContext(DbContextOptions<ShutterDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstComponentPrice> MstComponentPrices { get; set; }

    public virtual DbSet<MstContactType> MstContactTypes { get; set; }

    public virtual DbSet<MstJobStatus> MstJobStatuses { get; set; }

    public virtual DbSet<MstJobTitle> MstJobTitles { get; set; }

    public virtual DbSet<MstJobType> MstJobTypes { get; set; }

    public virtual DbSet<MstModule> MstModules { get; set; }

    public virtual DbSet<MstModulePermission> MstModulePermissions { get; set; }

    public virtual DbSet<MstPlaceholder> MstPlaceholders { get; set; }

    public virtual DbSet<MstPowderCoatColor> MstPowderCoatColors { get; set; }

    public virtual DbSet<MstProductType> MstProductTypes { get; set; }

    public virtual DbSet<MstRegion> MstRegions { get; set; }

    public virtual DbSet<MstState> MstStates { get; set; }

    public virtual DbSet<MstUnitofMeasurement> MstUnitofMeasurements { get; set; }

    public virtual DbSet<TblAddress> TblAddresses { get; set; }

    public virtual DbSet<TblAppointmentSchedule> TblAppointmentSchedules { get; set; }

    public virtual DbSet<TblBlobdocument> TblBlobdocuments { get; set; }

    public virtual DbSet<TblComponent> TblComponents { get; set; }

    public virtual DbSet<TblComponentMaterial> TblComponentMaterials { get; set; }

    public virtual DbSet<TblContact> TblContacts { get; set; }

    public virtual DbSet<TblContactBillingAddress> TblContactBillingAddresses { get; set; }

    public virtual DbSet<TblContactRole> TblContactRoles { get; set; }

    public virtual DbSet<TblDiscount> TblDiscounts { get; set; }

    public virtual DbSet<TblEmailConfiguration> TblEmailConfigurations { get; set; }

    public virtual DbSet<TblEmailType> TblEmailTypes { get; set; }

    public virtual DbSet<TblEstimate> TblEstimates { get; set; }

    public virtual DbSet<TblEstimateApproved> TblEstimateApproveds { get; set; }

    public virtual DbSet<TblEstimateWindow> TblEstimateWindows { get; set; }

    public virtual DbSet<TblEstimateWindowProduct> TblEstimateWindowProducts { get; set; }

    public virtual DbSet<TblEstimateWindowProductComponent> TblEstimateWindowProductComponents { get; set; }

    public virtual DbSet<TblEstimateWindowProductComponentMaterial> TblEstimateWindowProductComponentMaterials { get; set; }

    public virtual DbSet<TblEstimateWindowProductOption> TblEstimateWindowProductOptions { get; set; }

    public virtual DbSet<TblExpression> TblExpressions { get; set; }

    public virtual DbSet<TblGhlCustomField> TblGhlCustomFields { get; set; }

    public virtual DbSet<TblGhlStage> TblGhlStages { get; set; }

    public virtual DbSet<TblGhlStagesmapping> TblGhlStagesmappings { get; set; }

    public virtual DbSet<TblGhlUser> TblGhlUsers { get; set; }

    public virtual DbSet<TblGhlconfiguration> TblGhlconfigurations { get; set; }

    public virtual DbSet<TblInvoice> TblInvoices { get; set; }

    public virtual DbSet<TblInvoicePayment> TblInvoicePayments { get; set; }

    public virtual DbSet<TblJob> TblJobs { get; set; }

    public virtual DbSet<TblJobComponent> TblJobComponents { get; set; }

    public virtual DbSet<TblJobContact> TblJobContacts { get; set; }

    public virtual DbSet<TblJobSchedule> TblJobSchedules { get; set; }

    public virtual DbSet<TblJobStageStatusAutomAtionHistory> TblJobStageStatusAutomAtionHistories { get; set; }

    public virtual DbSet<TblJobStatus> TblJobStatuses { get; set; }

    public virtual DbSet<TblLocation> TblLocations { get; set; }

    public virtual DbSet<TblManagePartnersProduct> TblManagePartnersProducts { get; set; }

    public virtual DbSet<TblManagePartnersProductsOption> TblManagePartnersProductsOptions { get; set; }

    public virtual DbSet<TblMeasurement> TblMeasurements { get; set; }

    public virtual DbSet<TblNotification> TblNotifications { get; set; }

    public virtual DbSet<TblNotificationTemplate> TblNotificationTemplates { get; set; }

    public virtual DbSet<TblOption> TblOptions { get; set; }

    public virtual DbSet<TblOptionsType> TblOptionsTypes { get; set; }

    public virtual DbSet<TblOrder> TblOrders { get; set; }

    public virtual DbSet<TblOrderWindow> TblOrderWindows { get; set; }

    public virtual DbSet<TblOrderWindowProduct> TblOrderWindowProducts { get; set; }

    public virtual DbSet<TblOrderWindowProductComponent> TblOrderWindowProductComponents { get; set; }

    public virtual DbSet<TblOrderWindowProductComponentMaterial> TblOrderWindowProductComponentMaterials { get; set; }

    public virtual DbSet<TblOrderWindowProductOption> TblOrderWindowProductOptions { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblProductComponent> TblProductComponents { get; set; }

    public virtual DbSet<TblRawMaterial> TblRawMaterials { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolesPermission> TblRolesPermissions { get; set; }

    public virtual DbSet<TblStripeConfiguration> TblStripeConfigurations { get; set; }

    public virtual DbSet<TblStripePayment> TblStripePayments { get; set; }

    public virtual DbSet<TblSubtenantProductMarkup> TblSubtenantProductMarkups { get; set; }

    public virtual DbSet<TblTaxExemption> TblTaxExemptions { get; set; }

    public virtual DbSet<TblTaxRate> TblTaxRates { get; set; }

    public virtual DbSet<TblTemplate> TblTemplates { get; set; }

    public virtual DbSet<TblTenant> TblTenants { get; set; }

    public virtual DbSet<TblTenantBrandingInfo> TblTenantBrandingInfos { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

    public virtual DbSet<TblVendor> TblVendors { get; set; }

    public virtual DbSet<TblVendorOrder> TblVendorOrders { get; set; }

    public virtual DbSet<TblVendorOrderItem> TblVendorOrderItems { get; set; }

    public virtual DbSet<TblWindow> TblWindows { get; set; }

    public virtual DbSet<TblWindowProduct> TblWindowProducts { get; set; }

    public virtual DbSet<WindowOption> WindowOptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:shuttermanagementdb.database.windows.net,1433;Initial Catalog=shutterdev;Persist Security Info=False;User ID=shutter_admin;Password=TaKugAbiGfb4eU$y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("btprojec_admin");

        modelBuilder.Entity<MstComponentPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstCompo__3214EC07CE2CF811");

            entity.ToTable("MstComponentPrice", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ComponentName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.PriceNonRated).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PriceRated).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductType).HasMaxLength(100);
        });

        modelBuilder.Entity<MstContactType>(entity =>
        {
            entity.HasKey(e => e.MstContactId).HasName("PK__MstConta__407CE16C591FEDD4");

            entity.ToTable("MstContactType", "dbo");

            entity.Property(e => e.MstContactId).ValueGeneratedNever();
            entity.Property(e => e.MstContactName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstJobStatus>(entity =>
        {
            entity.HasKey(e => e.JobStatusId).HasName("PK__MstJobSt__EB4857215FCC685F");

            entity.ToTable("MstJobStatus", "dbo");

            entity.Property(e => e.JobStatusId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.JobStatusName)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstJobTitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstJobTi__3214EC0783B9D382");

            entity.ToTable("MstJobTitle", "dbo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.JobTitleName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstJobType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstJobTy__3214EC07804264B3");

            entity.ToTable("MstJobType", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.JobTypeName).HasMaxLength(128);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstModule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC077F40F9DB");

            entity.ToTable("MstModule", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ModuleDisplayName).HasMaxLength(200);
            entity.Property(e => e.ModuleName).HasMaxLength(200);
        });

        modelBuilder.Entity<MstModulePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstModul__3214EC0770579C30");

            entity.ToTable("MstModule_Permission", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Icon).IsUnicode(false);
            entity.Property(e => e.ModulePermission).HasMaxLength(100);
            entity.Property(e => e.PermissionDisplayName).HasMaxLength(100);
            entity.Property(e => e.Url)
                .HasMaxLength(1000)
                .HasColumnName("URL");

            entity.HasOne(d => d.Module).WithMany(p => p.MstModulePermissions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__MstModule__Modul__6ABAD62E");
        });

        modelBuilder.Entity<MstPlaceholder>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MstPlaceholder", "dbo");

            entity.Property(e => e.Dbvalue).HasColumnName("DBValue");
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<MstPowderCoatColor>(entity =>
        {
            entity.ToTable("MstPowderCoatColor", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ColorName).HasMaxLength(100);
            entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstProductType>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__MstProdu__B40CC6CD0F2D407A");

            entity.ToTable("MstProductType", "dbo");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstRegion>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MstRegion", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RegionName).HasMaxLength(200);
        });

        modelBuilder.Entity<MstState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstState__3213E83F2A9E3802");

            entity.ToTable("MstState", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.StateCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StateName).HasMaxLength(100);
        });

        modelBuilder.Entity<MstUnitofMeasurement>(entity =>
        {
            entity.HasKey(e => e.UnitofMeasurementId).HasName("PK__MstUnito__21E7C5530ACB5AD1");

            entity.ToTable("MstUnitofMeasurement", "dbo");

            entity.Property(e => e.UnitofMeasurementId)
                .ValueGeneratedNever()
                .HasColumnName("unitofMeasurementId");
            entity.Property(e => e.UnitofMeasurementName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("unitofMeasurementName");
        });

        modelBuilder.Entity<TblAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__TblAddre__091C2AFBAEF0EE66");

            entity.ToTable("TblAddress", "dbo");

            entity.Property(e => e.AddressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccessCode).HasMaxLength(128);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(128);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(128);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(128);
            entity.Property(e => e.StreetAddress).HasMaxLength(128);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Contact).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblAddres__Conta__6D9742D9");

            entity.HasOne(d => d.StateNavigation).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.State)
                .HasConstraintName("FK__TblAddres__State__7073AF84");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblAddres__Tenan__7167D3BD");
        });

        modelBuilder.Entity<TblAppointmentSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblAppoi__3213E83F68F4DF76");

            entity.ToTable("TblAppointmentSchedule", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.GhlEventId)
                .HasMaxLength(100)
                .HasColumnName("GHL_EventId");
            entity.Property(e => e.GhlUserId)
                .HasMaxLength(100)
                .HasColumnName("GHL_UserId");
            entity.Property(e => e.LastSync).HasColumnType("datetime");
            entity.Property(e => e.ScheduleDateTime).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.TblAppointmentSchedules)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblAppoin__JobId__74444068");

            entity.HasOne(d => d.User).WithMany(p => p.TblAppointmentSchedules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblAppoin__UserI__753864A1");
        });

        modelBuilder.Entity<TblBlobdocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__tblBlobd__1ABEEF0F6E68EC0C");

            entity.ToTable("tblBlobdocument", "dbo");

            entity.Property(e => e.DocumentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblComponent>(entity =>
        {
            entity.HasKey(e => e.ComponentId).HasName("PK__TblCompo__D79CF04E18539337");

            entity.ToTable("TblComponent", "dbo");

            entity.HasIndex(e => e.TenantId, "IX_TblComponent_TenantId");

            entity.Property(e => e.ComponentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ComponentName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblCompon__Tenan__762C88DA");
        });

        modelBuilder.Entity<TblComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.ComponentMaterialId).HasName("PK__TblCompo__82FB769C0DAA0A8D");

            entity.ToTable("TblComponentMaterial", "dbo");

            entity.HasIndex(e => e.ComponentId, "IX_TblComponentMaterial_ComponentId");

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
                .HasConstraintName("FK__TblCompon__Compo__7908F585");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.RawMaterialId)
                .HasConstraintName("FK__TblCompon__RawMa__7BE56230");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblComponentMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblCompon__Tenan__00AA174D");
        });

        modelBuilder.Entity<TblContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__TblConta__5C66259BCBF12F12");

            entity.ToTable("TblContact", "dbo");

            entity.Property(e => e.ContactId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ContactType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContactUniqueId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GhlcontactId).HasColumnName("GHLContactId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastSync).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Referral).HasMaxLength(100);
            entity.Property(e => e.Suffix)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Website).HasMaxLength(255);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblContacts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblContac__Tenan__019E3B86");
        });

        modelBuilder.Entity<TblContactBillingAddress>(entity =>
        {
            entity.HasKey(e => e.BillingAddressId).HasName("PK__TblConta__90BDB2B17FB0249D");

            entity.ToTable("TblContact_billingAddress", "dbo");

            entity.Property(e => e.BillingAddressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.StreetAddress).HasMaxLength(250);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Contact).WithMany(p => p.TblContactBillingAddresses)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblContac__Conta__047AA831");
        });

        modelBuilder.Entity<TblContactRole>(entity =>
        {
            entity.HasKey(e => e.ContactRoleId).HasName("PK__TblConta__38FF753760ADA5BE");

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
                .HasConstraintName("FK__TblContac__Conta__075714DC");
        });

        modelBuilder.Entity<TblDiscount>(entity =>
        {
            entity.HasKey(e => e.DiscountId).HasName("PK__TblDisco__E43F6D9609C9E69B");

            entity.ToTable("TblDiscount", "dbo");

            entity.Property(e => e.DiscountId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ApplyFor)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CouponCode).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.FixedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PercentageDiscount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.TblDiscounts)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Customer");
        });

        modelBuilder.Entity<TblEmailConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblEmail__3214EC07A14B0CF4");

            entity.ToTable("TblEmailConfiguration", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccessExpirationtime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.From).HasMaxLength(255);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.RefreshExpirationtime).HasColumnType("datetime");
            entity.Property(e => e.ReplyTo).HasMaxLength(255);
            entity.Property(e => e.SmtpServer).HasMaxLength(255);
            entity.Property(e => e.UseSsl).HasColumnName("UseSSL");
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        modelBuilder.Entity<TblEmailType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblEmail__3214EC07E9C47EBA");

            entity.ToTable("tblEmailType", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsOauth).HasColumnName("IsOAuth");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TblEstimate>(entity =>
        {
            entity.HasKey(e => e.EstimateId).HasName("PK__TblEstim__ABEBF4B5B0E547C5");

            entity.ToTable("TblEstimate", "dbo");

            entity.Property(e => e.EstimateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountCoupon).HasMaxLength(200);
            entity.Property(e => e.EstimateCode).HasMaxLength(50);
            entity.Property(e => e.EstimateName).HasMaxLength(200);
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TermsCondition).HasMaxLength(2000);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotaltaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.TblEstimates)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblEstima__JobId__0B27A5C0");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimates)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__12C8C788");
        });

        modelBuilder.Entity<TblEstimateApproved>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblEstim__3214EC07D58A4060");

            entity.ToTable("TblEstimateApproved", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CurrentDatetime).HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(20)
                .HasColumnName("IPaddress");
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<TblEstimateWindow>(entity =>
        {
            entity.HasKey(e => e.EstWindowId).HasName("PK__TblEstim__F0A94F98617D84EC");

            entity.ToTable("TblEstimateWindow", "dbo");

            entity.HasIndex(e => e.AddressId, "IX_TblEstimateWindow_AddressId");

            entity.HasIndex(e => e.EstimateId, "IX_TblEstimateWindow_EstimateId");

            entity.HasIndex(e => e.TenantId, "IX_TblEstimateWindow_TenantId");

            entity.HasIndex(e => e.WindowId, "IX_TblEstimateWindow_WindowId");

            entity.Property(e => e.EstWindowId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BuildHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BuildWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.IsRated).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Address).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblEstima__Addre__1A69E950");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__2022C2A6");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__25DB9BFC");

            entity.HasOne(d => d.Window).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__TblEstima__Windo__2B947552");
        });

        modelBuilder.Entity<TblEstimateWindowProduct>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductId).HasName("PK__TblEstim__30AAFF25CD63B514");

            entity.ToTable("TblEstimateWindowProduct", "dbo");

            entity.HasIndex(e => e.EstWindowId, "IX_TblEstimateWindowProduct_EstWindowId");

            entity.HasIndex(e => e.EstimateId, "IX_TblEstimateWindowProduct_EstimateId");

            entity.HasIndex(e => e.TaxId, "IX_TblEstimateWindowProduct_TaxId");

            entity.HasIndex(e => e.TenantId, "IX_TblEstimateWindowProduct_TenantId");

            entity.HasIndex(e => e.WindowProductId, "IX_TblEstimateWindowProduct_WindowProductId");

            entity.Property(e => e.EstimateWindowProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.City).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.ComponentMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Municipality).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductTypeName).HasMaxLength(50);
            entity.Property(e => e.State).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TaxName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WholeSalePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.EstWindow).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.EstWindowId)
                .HasConstraintName("FK__TblEstima__EstWi__3429BB53");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__2E70E1FD");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblEstima__TaxId__351DDF8C");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__361203C5");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblEstima__Windo__3BCADD1B");
        });

        modelBuilder.Entity<TblEstimateWindowProductComponent>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductComponentId).HasName("PK__TblEstim__198C24738FFA307B");

            entity.ToTable("TblEstimateWindowProductComponent", "dbo");

            entity.HasIndex(e => e.EstimateId, "IX_TblEstimateWindowProductComponent_EstimateId");

            entity.HasIndex(e => e.TenantId, "IX_TblEstimateWindowProductComponent_TenantId");

            entity.Property(e => e.EstimateWindowProductComponentId).ValueGeneratedNever();
            entity.Property(e => e.ComponentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ComponentName).HasMaxLength(255);
            entity.Property(e => e.ComponentPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductTypeName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Wastefactor).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindowProductComponents)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__3EA749C6");

            entity.HasOne(d => d.EstimateWindowProduct).WithMany(p => p.TblEstimateWindowProductComponents)
                .HasForeignKey(d => d.EstimateWindowProductId)
                .HasConstraintName("FK__TblEstima__Estim__4277DAAA");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProductComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__45544755");
        });

        modelBuilder.Entity<TblEstimateWindowProductComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductComponentMaterialId).HasName("PK__TblEstim__13E8FE878F7998DF");

            entity.ToTable("TblEstimateWindowProductComponentMaterial", "dbo");

            entity.HasIndex(e => e.EstimateId, "IX_TblEstimateWindowProductComponentMaterial_EstimateId");

            entity.HasIndex(e => e.ProductType, "IX_TblEstimateWindowProductComponentMaterial_ProductType");

            entity.HasIndex(e => e.TenantId, "IX_TblEstimateWindowProductComponentMaterial_TenantId");

            entity.Property(e => e.EstimateWindowProductComponentMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(255);
            entity.Property(e => e.MaterialPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RawMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.StockQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitOfMeasurement)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__4B0D20AB");

            entity.HasOne(d => d.EstimateWindowProductComponent).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.EstimateWindowProductComponentId)
                .HasConstraintName("FK__TblEstima__Estim__4CF5691D");

            entity.HasOne(d => d.ProductTypeNavigation).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.ProductType)
                .HasConstraintName("FK__TblEstima__Produ__4EDDB18F");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__51BA1E3A");
        });

        modelBuilder.Entity<TblEstimateWindowProductOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblEstim__3213E83FF65429E0");

            entity.ToTable("TblEstimateWindowProductOption", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MunicipilityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OptionPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionTypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StateTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TaxName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.EstimateWindowProduct).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.EstimateWindowProductId)
                .HasConstraintName("FK__TblEstima__Estim__54968AE5");

            entity.HasOne(d => d.WindowOption).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.WindowOptionId)
                .HasConstraintName("FK__TblEstima__Windo__567ED357");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblEstima__Windo__558AAF1E");
        });

        modelBuilder.Entity<TblExpression>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblExpre__3214EC07C79583BE");

            entity.ToTable("TblExpression", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpressionName).HasMaxLength(100);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.ProductTypeNavigation).WithMany(p => p.TblExpressions)
                .HasForeignKey(d => d.ProductType)
                .HasConstraintName("FK__TblExpres__Produ__5772F790");
        });

        modelBuilder.Entity<TblGhlCustomField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblGhlCu__3214EC0707CF4025");

            entity.ToTable("TblGhlCustomFields", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DataType).HasMaxLength(50);
            entity.Property(e => e.DateAdded).HasColumnType("datetime");
            entity.Property(e => e.DocumentType).HasMaxLength(50);
            entity.Property(e => e.FieldKey).HasMaxLength(255);
            entity.Property(e => e.FieldName).HasMaxLength(255);
            entity.Property(e => e.GhlFieldId).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LocationId).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasMaxLength(100);
            entity.Property(e => e.Placeholder).HasMaxLength(255);
        });

        modelBuilder.Entity<TblGhlStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblGHL_S__3213E83F2B6542FA");

            entity.ToTable("tblGHL_Stage", "dbo");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.GhlLastSync)
                .HasColumnType("datetime")
                .HasColumnName("GHL_lastSync");
            entity.Property(e => e.GhlPipelineId).HasColumnName("GHL_PipelineId");
            entity.Property(e => e.GhlPipelineName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GHL_PipelineName");
            entity.Property(e => e.GhlStageId).HasColumnName("GHL_StageId");
            entity.Property(e => e.GhlStageName).HasColumnName("GHL_StageName");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblGhlStagesmapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblGHlSt__3213E83F74DAC733");

            entity.ToTable("TblGHlStagesmapping", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblGhlUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblGHL_U__3214EC078DD1B770");

            entity.ToTable("tblGHL_Users", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.GhlCalendarId)
                .HasMaxLength(100)
                .HasColumnName("GHL_CalendarId");
            entity.Property(e => e.GhlCalendarName)
                .HasMaxLength(100)
                .HasColumnName("GHL_CalendarName");
            entity.Property(e => e.GhlEmail)
                .HasMaxLength(100)
                .HasColumnName("GHL_Email");
            entity.Property(e => e.GhlFirstName)
                .HasMaxLength(100)
                .HasColumnName("GHL_FirstName");
            entity.Property(e => e.GhlLastName)
                .HasMaxLength(100)
                .HasColumnName("GHL_LastName");
            entity.Property(e => e.GhlLastSync)
                .HasColumnType("datetime")
                .HasColumnName("GHL_lastSync");
            entity.Property(e => e.GhlUserId)
                .HasMaxLength(100)
                .HasColumnName("GHL_UserId");
            entity.Property(e => e.GhlUserName)
                .HasMaxLength(100)
                .HasColumnName("GHL_UserName");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblGhlconfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblGHLCo__3214EC07C5D4B042");

            entity.ToTable("TblGHLConfiguration", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccessExpirationtime).HasColumnType("datetime");
            entity.Property(e => e.AccessToken).HasColumnName("Access_token");
            entity.Property(e => e.LastSync)
                .HasColumnType("datetime")
                .HasColumnName("lastSync");
            entity.Property(e => e.RefreshExpirationtime).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken).HasColumnName("Refresh_token");
        });

        modelBuilder.Entity<TblInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__TblInvoi__D796AAB505594428");

            entity.ToTable("TblInvoice", "dbo");

            entity.HasIndex(e => e.EstimateId, "IX_TblInvoice_EstimateId");

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
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblInvoices)
                .HasForeignKey(d => d.EstimateId)
                .HasConstraintName("FK__TblInvoic__Estim__5A4F643B");

            entity.HasOne(d => d.Job).WithMany(p => p.TblInvoices)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblInvoic__JobId__5D2BD0E6");
        });

        modelBuilder.Entity<TblInvoicePayment>(entity =>
        {
            entity.HasKey(e => e.InvoicePaymentId).HasName("PK__TblInvoi__A79CC7FEB628386C");

            entity.ToTable("TblInvoicePayment", "dbo");

            entity.HasIndex(e => e.InvoiceId, "IX_TblInvoicePayment_InvoiceId");

            entity.Property(e => e.InvoicePaymentId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoiceAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithMany(p => p.TblInvoicePayments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__TblInvoic__Invoi__60083D91");
        });

        modelBuilder.Entity<TblJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__TblJob__056690C29CD631F3");

            entity.ToTable("TblJob", "dbo");

            entity.HasIndex(e => e.AddressId, "IX_TblJob_AddressId");

            entity.HasIndex(e => e.ContactId, "IX_TblJob_ContactId");

            entity.HasIndex(e => e.JobStatus, "IX_TblJob_JobStatus");

            entity.HasIndex(e => e.TenantId, "IX_TblJob_TenantId");

            entity.Property(e => e.JobId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GhlContactId)
                .HasMaxLength(100)
                .HasColumnName("GHL_ContactId");
            entity.Property(e => e.GhlOpportunityId)
                .HasMaxLength(100)
                .HasColumnName("GHL_OpportunityId");
            entity.Property(e => e.GhlStageId).HasColumnName("GHL_StageId");
            entity.Property(e => e.GhlStageName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GHL_StageName");
            entity.Property(e => e.GhluserId).HasColumnName("GHLUserId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.JobName).HasMaxLength(250);
            entity.Property(e => e.JobUniqueId).HasMaxLength(200);
            entity.Property(e => e.LastAsync).HasColumnType("datetime");
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Address).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblJob__AddressI__62E4AA3C");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TblJob__Contact");

            entity.HasOne(d => d.JobStatusNavigation).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.JobStatus)
                .HasConstraintName("FK__TblJob__JobStatu__6B79F03D");

            entity.HasOne(d => d.PowderCoatColor).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.PowderCoatColorId)
                .HasConstraintName("FK_TblJob_MstPowderCoatColor");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblJob__TenantId__6F4A8121");
        });

        modelBuilder.Entity<TblJobComponent>(entity =>
        {
            entity.HasKey(e => e.JobComponentId).HasName("PK__TblJobCo__E85D56C045886DA3");

            entity.ToTable("TblJobComponent", "dbo");

            entity.HasIndex(e => e.AssemblyId, "IX_TblJobComponent_AssemblyId");

            entity.HasIndex(e => e.ComponentId, "IX_TblJobComponent_ComponentId");

            entity.HasIndex(e => e.JobId, "IX_TblJobComponent_JobId");

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
                .HasConstraintName("FK__TblJobCom__Assem__740F363E");

            entity.HasOne(d => d.Component).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblJobCom__Compo__76EBA2E9");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobComponents)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblJobCom__JobId__79C80F94");
        });

        modelBuilder.Entity<TblJobContact>(entity =>
        {
            entity.HasKey(e => e.JobContactId).HasName("PK__TblJobCo__BB491B5F410AD4D2");

            entity.ToTable("TblJobContact", "dbo");

            entity.HasIndex(e => e.ContactId, "IX_TblJobContact_ContactId");

            entity.HasIndex(e => e.ContactTypeId, "IX_TblJobContact_ContactTypeId");

            entity.HasIndex(e => e.JobId, "IX_TblJobContact_JobId");

            entity.Property(e => e.JobContactId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblJobCon__Conta__00750D23");

            entity.HasOne(d => d.ContactType).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.ContactTypeId)
                .HasConstraintName("FK__TblJobCon__Conta__0169315C");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_tbljobcontact_tbljob_JobId");
        });

        modelBuilder.Entity<TblJobSchedule>(entity =>
        {
            entity.HasKey(e => e.JobScheduleId).HasName("PK__TblJobSc__2EB48C45632F8AB8");

            entity.ToTable("TblJobSchedule", "dbo");

            entity.Property(e => e.JobScheduleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.GhlEventId).HasColumnName("GHL_EventId");
            entity.Property(e => e.GhlUserId)
                .HasMaxLength(100)
                .HasColumnName("GHL_UserId");
            entity.Property(e => e.LasSync).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.EstimateId)
                .HasConstraintName("FK__TblJobSch__Estim__035179CE");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_TblJobSchedule_TblJob");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.JobTitleId)
                .HasConstraintName("FK__TblJobSch__JobTi__07220AB2");

            entity.HasOne(d => d.User).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblJobSch__UserI__0AF29B96");
        });

        modelBuilder.Entity<TblJobStageStatusAutomAtionHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__TblJobSt__4D7B4ABDA0902E46");

            entity.ToTable("TblJobStageStatusAutomAtionHistory", "dbo");

            entity.Property(e => e.HistoryId).ValueGeneratedNever();
            entity.Property(e => e.ChangedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__tblJobSt__C8EE206387B35599");

            entity.ToTable("tblJobStatus", "dbo");

            entity.Property(e => e.StatusId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.StatusName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__TblLocat__E7FEA49754613090");

            entity.ToTable("TblLocation", "dbo");

            entity.Property(e => e.LocationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LocationName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblManagePartnersProduct>(entity =>
        {
            entity.HasKey(e => e.ManageProductId).HasName("PK__TblManag__E07C429BF6BA77BB");

            entity.ToTable("TblManagePartnersProducts", "dbo");

            entity.Property(e => e.ManageProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MarkupType).HasMaxLength(20);
            entity.Property(e => e.MaxPartnerPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PartnerSellingPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PriceType).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WholeSalePrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.TblManagePartnersProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_TblManagePartnersProducts_Product");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblManagePartnersProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblManage__TaxId__0FB750B3");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblManagePartnersProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK_TblManagePartnersProducts_ManageProductTenant");
        });

        modelBuilder.Entity<TblManagePartnersProductsOption>(entity =>
        {
            entity.HasKey(e => e.ManageProductOptionId).HasName("PK__TblManag__5912BD4EC3881CFD");

            entity.ToTable("TblManagePartnersProductsOptions", "dbo");

            entity.Property(e => e.ManageProductOptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MarkupOptionType).HasMaxLength(20);
            entity.Property(e => e.MaxPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PartnerSellingPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PriceType).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WholeSalePrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ManageProduct).WithMany(p => p.TblManagePartnersProductsOptions)
                .HasForeignKey(d => d.ManageProductId)
                .HasConstraintName("FK_TblManagePartnersProducts_ManageProduct");

            entity.HasOne(d => d.Option).WithMany(p => p.TblManagePartnersProductsOptions)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK_TblOptions_ManagePartnerProductOptionId");

            entity.HasOne(d => d.OptionType).WithMany(p => p.TblManagePartnersProductsOptions)
                .HasForeignKey(d => d.OptionTypeId)
                .HasConstraintName("FK_TBLOptionsType_ManagePartnerProductOptionTypeId");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblManagePartnersProductsOptions)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblManage__TaxId__1293BD5E");
        });

        modelBuilder.Entity<TblMeasurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__TblMeasu__85599FB844359F7E");

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
                .HasConstraintName("FK__TblMeasur__Addre__16644E42");
        });

        modelBuilder.Entity<TblNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblNotif__3214EC071BF575A5");

            entity.ToTable("TblNotification", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Createdate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblNotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblNotif__3214EC07CF12646C");

            entity.ToTable("TblNotificationTemplate", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<TblOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__TblOptio__92C7A1FF8D319BC4");

            entity.ToTable("TblOptions", "dbo");

            entity.HasIndex(e => e.ProductId, "IX_TblOptions_ProductId");

            entity.HasIndex(e => e.ProductTypeId, "IX_TblOptions_ProductTypeId");

            entity.HasIndex(e => e.TaxId, "IX_TblOptions_TaxId");

            entity.HasIndex(e => e.TenantId, "IX_TblOptions_TenantId");

            entity.Property(e => e.OptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionName).HasMaxLength(255);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_TblOption_Product");

            entity.HasOne(d => d.ProductType).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.ProductTypeId)
                .HasConstraintName("FK_TblOption_ProductType");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblOption__TaxId__1940BAED");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblOption__Tenan__1A34DF26");
        });

        modelBuilder.Entity<TblOptionsType>(entity =>
        {
            entity.HasKey(e => e.OptionTypeId).HasName("PK__TblOptio__96A2FCCE10931DA0");

            entity.ToTable("TblOptions_Type", "dbo");

            entity.Property(e => e.OptionTypeId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.HidePreview).HasColumnName("hidePreview");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.OptionType).HasMaxLength(200);

            entity.HasOne(d => d.Option).WithMany(p => p.TblOptionsTypes)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK__TblOption__Optio__1EF99443");
        });

        modelBuilder.Entity<TblOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__TblOrder__C3905BCF4ACD9A2C");

            entity.ToTable("TblOrder", "dbo");

            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountCoupon).HasMaxLength(200);
            entity.Property(e => e.EstimateCode).HasMaxLength(50);
            entity.Property(e => e.EstimateName).HasMaxLength(200);
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.OrderName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TermsCondition).HasMaxLength(2000);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotaltaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(3);
        });

        modelBuilder.Entity<TblOrderWindow>(entity =>
        {
            entity.HasKey(e => e.OrderWindowId).HasName("PK__TblOrder__861D635826845FAD");

            entity.ToTable("TblOrderWindow", "dbo");

            entity.Property(e => e.OrderWindowId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(3);
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TblOrderWindowProduct>(entity =>
        {
            entity.HasKey(e => e.OrderWindowProductId).HasName("PK__TblOrder__91558924636B8EE7");

            entity.ToTable("TblOrderWindowProduct", "dbo");

            entity.Property(e => e.OrderWindowProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.City).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.ComponentMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Municipality).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductMarkup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductTypeName).HasMaxLength(50);
            entity.Property(e => e.State).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TaxName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(3);
            entity.Property(e => e.WholeSalePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblOrderWindowProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblOrderW__TaxId__21D600EE");
        });

        modelBuilder.Entity<TblOrderWindowProductComponent>(entity =>
        {
            entity.HasKey(e => e.OrderWindowProductComponentId).HasName("PK__TblOrder__381A8AF6ECF80FC2");

            entity.ToTable("TblOrderWindowProductComponent", "dbo");

            entity.Property(e => e.OrderWindowProductComponentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ComponentName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(3);
        });

        modelBuilder.Entity<TblOrderWindowProductComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.OrderWindowProductComponentMaterialId).HasName("PK__TblOrder__FECF878F59675489");

            entity.ToTable("TblOrderWindowProductComponentMaterial", "dbo");

            entity.Property(e => e.OrderWindowProductComponentMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MaterialName).HasMaxLength(255);
            entity.Property(e => e.MaterialPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.StockQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitOfMeasurement)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(3);
        });

        modelBuilder.Entity<TblOrderWindowProductOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblOrder__3213E83F31153770");

            entity.ToTable("TblOrderWindowProductOption", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MunicipilityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OptionPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StateTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.OrderWindowProduct).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.OrderWindowProductId)
                .HasConstraintName("FK__TblOrderW__Order__22CA2527");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblOrderW__TaxId__23BE4960");

            entity.HasOne(d => d.WindowOption).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.WindowOptionId)
                .HasConstraintName("FK__TblOrderW__Windo__25A691D2");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblOrderW__Windo__24B26D99");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__TblPayme__9B556A3835A1E0C4");

            entity.ToTable("TblPayment", "dbo");

            entity.Property(e => e.PaymentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PaymentTypeStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__TblPaymen__Invoi__269AB60B");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__TblProdu__B40CC6CD53DD59A6");

            entity.ToTable("TblProduct", "dbo");

            entity.HasIndex(e => e.TaxId, "IX_TblProduct_TaxId");

            entity.HasIndex(e => e.TenantId, "IX_TblProduct_TenantId");

            entity.Property(e => e.ProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblProduc__TaxId__297722B6");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblProduc__Tenan__2A6B46EF");
        });

        modelBuilder.Entity<TblProductComponent>(entity =>
        {
            entity.HasKey(e => e.ProductComponentId).HasName("PK__TblProdu__E29891DFF460AAD5");

            entity.ToTable("TblProductComponent", "dbo");

            entity.HasIndex(e => e.ComponentId, "IX_TblProductComponent_ComponentId");

            entity.HasIndex(e => e.ProductId, "IX_TblProductComponent_ProductId");

            entity.Property(e => e.ProductComponentId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PricingExpression).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Component).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblProduc__Compo__2D47B39A");

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__TblProduc__Produ__30242045");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblProduc__Tenan__33008CF0");
        });

        modelBuilder.Entity<TblRawMaterial>(entity =>
        {
            entity.HasKey(e => e.RawMaterialId).HasName("PK__TblRawMa__D99FAE1118F34176");

            entity.ToTable("TblRawMaterial", "dbo");

            entity.HasIndex(e => e.ProductType, "IX_TblRawMaterial_ProductType");

            entity.HasIndex(e => e.TenantId, "IX_TblRawMaterial_TenantId");

            entity.HasIndex(e => e.VendorId, "IX_TblRawMaterial_VendorId");

            entity.Property(e => e.RawMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaterialName).HasMaxLength(255);
            entity.Property(e => e.NonRatedUnitCost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Non_Rated_UnitCost");
            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.StockQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitOfMeasurement)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ProductTypeNavigation).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.ProductType)
                .HasConstraintName("FK__TblRawMat__Produ__35DCF99B");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblRawMat__Tenan__38B96646");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK_TblRawMaterial_TblVendor");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblRoles__3214EC07D61E5A5E");

            entity.ToTable("TblRoles", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsShow).HasDefaultValue(false);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblRolesPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblRoles__3214EC076DBD9E1C");

            entity.ToTable("TblRoles_Permission", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ModulePermission).WithMany(p => p.TblRolesPermissions)
                .HasForeignKey(d => d.ModulePermissionId)
                .HasConstraintName("FK__TblRoles___Modul__3C89F72A");
        });

        modelBuilder.Entity<TblStripeConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblStrip__3213E83F9A3AC6A1");

            entity.ToTable("TblStripeConfiguration", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CancelUrl).HasColumnName("cancelUrl");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.SuccessUrl).HasColumnName("successUrl");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblStripePayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblStrip__3214EC071975A2C1");

            entity.ToTable("TblStripePayment", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(100)
                .HasColumnName("customer_email");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customer_name");
            entity.Property(e => e.ModeifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(100)
                .HasColumnName("payment_mode");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StripeEventType).HasMaxLength(100);
            entity.Property(e => e.StripeTransactionId)
                .HasMaxLength(100)
                .HasColumnName("stripeTransactionId");
        });

        modelBuilder.Entity<TblSubtenantProductMarkup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblSubte__3214EC07F8DCB7F1");

            entity.ToTable("TblSubtenantProductMarkup", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTaxExemption>(entity =>
        {
            entity.HasKey(e => e.TaxExemptionId).HasName("PK__TblTaxEx__BCE64BE67CB47657");

            entity.ToTable("TblTaxExemption", "dbo");

            entity.Property(e => e.TaxExemptionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblTaxExemptions)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblTaxExe__Conta__405A880E");
        });

        modelBuilder.Entity<TblTaxRate>(entity =>
        {
            entity.HasKey(e => e.TaxRateId).HasName("PK__TblTaxRa__B114CEC13BEFDE50");

            entity.ToTable("TblTaxRate", "dbo");

            entity.Property(e => e.TaxRateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Municipality).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Rate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.State).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TaxName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblTaxRates)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblTaxRat__Tenan__442B18F2");
        });

        modelBuilder.Entity<TblTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__TblTempl__F87ADD2774913B78");

            entity.ToTable("TblTemplate", "dbo");

            entity.Property(e => e.TemplateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.TemplateCategory)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TemplateName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTenant>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("PK__TblTenan__2E9B47E17CF1381E");

            entity.ToTable("TblTenant", "dbo");

            entity.Property(e => e.TenantId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code).HasMaxLength(128);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(128);
            entity.Property(e => e.GhluserId).HasColumnName("GHLUserId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.IsGhlconfigSameAsTenant).HasColumnName("IsGHLConfigSameAsTenant");
            entity.Property(e => e.LastName).HasMaxLength(128);
            entity.Property(e => e.MiddleName).HasMaxLength(128);
            entity.Property(e => e.ModifiedBy).HasColumnName("ModifiedBY");
            entity.Property(e => e.Suffix)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(128);

            entity.HasOne(d => d.BrandingInfoNavigation).WithMany(p => p.TblTenants)
                .HasForeignKey(d => d.BrandingInfo)
                .HasConstraintName("FK__TblTenant__Brand__4707859D");

            entity.HasOne(d => d.ParentTenant).WithMany(p => p.InverseParentTenant)
                .HasForeignKey(d => d.ParentTenantId)
                .HasConstraintName("FK__TblTenant__Paren__49E3F248");
        });

        modelBuilder.Entity<TblTenantBrandingInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblTenan__3214EC07F9C3B2C7");

            entity.ToTable("TblTenant_BrandingInfo", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(200);
            entity.Property(e => e.CompanyName).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.InvoicePrefix).HasMaxLength(20);
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MarkupPer)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.ZipCode).HasMaxLength(200);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser__3213E83F8272E2C2");

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
            entity.Property(e => e.ForgetPassword).IsUnicode(false);
            entity.Property(e => e.GhlUserId)
                .HasMaxLength(100)
                .HasColumnName("GHL_UserId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.IsTenantAdmin).HasDefaultValue(false);
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
            entity.Property(e => e.Suffix)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.JobTitleId)
                .HasConstraintName("FK__TblUser__JobTitl__4CC05EF3");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblUser__TenantI__4F9CCB9E");
        });

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblUser___3214EC073A434D27");

            entity.ToTable("TblUser_Roles", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUser_R__RoleI__5555A4F4");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblUser_R__UserI__5832119F");
        });

        modelBuilder.Entity<TblVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__TblVendo__FC8618F3504EEF1C");

            entity.ToTable("TblVendor", "dbo");

            entity.HasIndex(e => e.State, "IX_TblVendor_State");

            entity.HasIndex(e => e.TenantId, "IX_TblVendor_TenantId");

            entity.Property(e => e.VendorId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CompanyName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(200);
            entity.Property(e => e.StreetAddress)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VendorCode).HasMaxLength(100);
            entity.Property(e => e.VendorName).HasMaxLength(255);
            entity.Property(e => e.ZipCode).HasMaxLength(200);

            entity.HasOne(d => d.StateNavigation).WithMany(p => p.TblVendors)
                .HasForeignKey(d => d.State)
                .HasConstraintName("FK_tblvendor_tblstate");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblVendors)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblVendor__Tenan__5B0E7E4A");
        });

        modelBuilder.Entity<TblVendorOrder>(entity =>
        {
            entity.HasKey(e => e.VendorOrderId).HasName("PK__TblVendo__477DCB3BEBA5AC58");

            entity.ToTable("TblVendorOrder", "dbo");

            entity.HasIndex(e => e.JobId, "IX_TblVendorOrder_JobId");

            entity.HasIndex(e => e.TenentId, "IX_TblVendorOrder_TenentId");

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
                .HasConstraintName("FK__TblVendor__JobId__5EDF0F2E");

            entity.HasOne(d => d.Tenent).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.TenentId)
                .HasConstraintName("FK__TblVendor__Tenen__61BB7BD9");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__TblVendor__Vendo__6497E884");
        });

        modelBuilder.Entity<TblVendorOrderItem>(entity =>
        {
            entity.HasKey(e => e.VendorOrderItemId).HasName("PK__TblVendo__A5401ACCA314DE78");

            entity.ToTable("TblVendorOrderItem", "dbo");

            entity.HasIndex(e => e.MaterialId, "IX_TblVendorOrderItem_MaterialId");

            entity.HasIndex(e => e.VendorOrderId, "IX_TblVendorOrderItem_VendorOrderId");

            entity.Property(e => e.VendorOrderItemId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.ItemDescription).HasMaxLength(255);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Material).WithMany(p => p.TblVendorOrderItems)
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK__TblVendor__Mater__6774552F");

            entity.HasOne(d => d.VendorOrder).WithMany(p => p.TblVendorOrderItems)
                .HasForeignKey(d => d.VendorOrderId)
                .HasConstraintName("FK__TblVendor__Vendo__6A50C1DA");
        });

        modelBuilder.Entity<TblWindow>(entity =>
        {
            entity.HasKey(e => e.WindowId).HasName("PK__TblWindo__1EEC6429BF1126D2");

            entity.ToTable("TblWindow", "dbo");

            entity.HasIndex(e => e.AddressId, "IX_TblWindow_AddressId");

            entity.HasIndex(e => e.JobId, "IX_TblWindow_JobId");

            entity.HasIndex(e => e.TenantId, "IX_TblWindow_TenantId");

            entity.Property(e => e.WindowId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.IsRated).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Address).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblWindow__Addre__6D2D2E85");

            entity.HasOne(d => d.Job).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblWindow__JobId__70099B30");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblWindow__Tenan__72E607DB");
        });

        modelBuilder.Entity<TblWindowProduct>(entity =>
        {
            entity.HasKey(e => e.WindowProductId).HasName("PK__tblWindo__28B91C826544FF51");

            entity.ToTable("tblWindowProduct", "dbo");

            entity.HasIndex(e => e.ProductId, "IX_TblWindowProduct_ProductId");

            entity.HasIndex(e => e.ProductTypeId, "IX_TblWindowProduct_ProductTypeId");

            entity.HasIndex(e => e.TenantId, "IX_TblWindowProduct_TenantId");

            entity.Property(e => e.WindowProductId).ValueGeneratedNever();
            entity.Property(e => e.BuildHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BuildWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__tblWindow__Produ__75C27486");

            entity.HasOne(d => d.ProductType).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.ProductTypeId)
                .HasConstraintName("FK__tblWindow__Produ__76B698BF");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__tblWindow__Tenan__7B7B4DDC");

            entity.HasOne(d => d.Window).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__tblWindow__Windo__004002F9");
        });

        modelBuilder.Entity<WindowOption>(entity =>
        {
            entity.HasKey(e => e.WindowoptionId).HasName("PK__WindowOp__F9F31DB2FA334FD4");

            entity.ToTable("WindowOption", "dbo");

            entity.HasIndex(e => e.OptionTypeId, "IX_WindowOption_OptionTypeId");

            entity.HasIndex(e => e.TenantId, "IX_WindowOption_TenantId");

            entity.HasIndex(e => e.WindowId, "IX_WindowOption_WindowId");

            entity.HasIndex(e => e.WindowProductId, "IX_WindowOption_WindowProductId");

            entity.Property(e => e.WindowoptionId).ValueGeneratedNever();
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.OptionType).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.OptionTypeId)
                .HasConstraintName("FK__WindowOpt__Optio__01342732");

            entity.HasOne(d => d.Tenant).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__WindowOpt__Tenan__041093DD");

            entity.HasOne(d => d.Window).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__WindowOpt__Windo__07E124C1");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__WindowOpt__Windo__06ED0088");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

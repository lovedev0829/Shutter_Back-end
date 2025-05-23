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

    public virtual DbSet<MstProductType> MstProductTypes { get; set; }

    public virtual DbSet<MstRegion> MstRegions { get; set; }

    public virtual DbSet<MstState> MstStates { get; set; }

    public virtual DbSet<MstUnitofMeasurement> MstUnitofMeasurements { get; set; }

    public virtual DbSet<TblAddress> TblAddresses { get; set; }

    public virtual DbSet<TblAppointmentSchedule> TblAppointmentSchedules { get; set; }

    public virtual DbSet<TblComponent> TblComponents { get; set; }

    public virtual DbSet<TblComponentMaterial> TblComponentMaterials { get; set; }

    public virtual DbSet<TblContact> TblContacts { get; set; }

    public virtual DbSet<TblContactBillingAddress> TblContactBillingAddresses { get; set; }

    public virtual DbSet<TblContactRole> TblContactRoles { get; set; }

    public virtual DbSet<TblDiscount> TblDiscounts { get; set; }

    public virtual DbSet<TblEstimate> TblEstimates { get; set; }

    public virtual DbSet<TblEstimateApproved> TblEstimateApproveds { get; set; }

    public virtual DbSet<TblEstimateWindow> TblEstimateWindows { get; set; }

    public virtual DbSet<TblEstimateWindowProduct> TblEstimateWindowProducts { get; set; }

    public virtual DbSet<TblEstimateWindowProductComponent> TblEstimateWindowProductComponents { get; set; }

    public virtual DbSet<TblEstimateWindowProductComponentMaterial> TblEstimateWindowProductComponentMaterials { get; set; }

    public virtual DbSet<TblEstimateWindowProductOption> TblEstimateWindowProductOptions { get; set; }

    public virtual DbSet<TblExpression> TblExpressions { get; set; }

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

    public virtual DbSet<TblTemplatePlaceholder> TblTemplatePlaceholders { get; set; }

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
        => optionsBuilder.UseSqlServer("Server=103.93.16.117;Database=btprojec_shuttermanagement_uat;user id=btprojec_admin;password=Bwy0w65ixN*bsE9wy;Integrated Security=false;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("btprojec_admin");

        modelBuilder.Entity<MstComponentPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstCompo__3214EC070EAF3683");

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
            entity.HasKey(e => e.MstContactId).HasName("PK__MstConta__407CE16C606CDB01");

            entity.ToTable("MstContactType", "dbo");

            entity.Property(e => e.MstContactId).ValueGeneratedNever();
            entity.Property(e => e.MstContactName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstJobStatus>(entity =>
        {
            entity.HasKey(e => e.JobStatusId).HasName("PK__MstJobSt__EB485721C90CE1FD");

            entity.ToTable("MstJobStatus", "dbo");

            entity.Property(e => e.JobStatusId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.JobStatusName)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstJobTitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MstJobTi__3214EC07381DB551");

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
            entity.HasKey(e => e.Id).HasName("PK__MstJobTy__3214EC0740773D67");

            entity.ToTable("MstJobType", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.JobTypeName).HasMaxLength(128);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

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
            entity.Property(e => e.Icon).IsUnicode(false);
            entity.Property(e => e.ModulePermission).HasMaxLength(100);
            entity.Property(e => e.PermissionDisplayName).HasMaxLength(100);
            entity.Property(e => e.Url)
                .HasMaxLength(1000)
                .HasColumnName("URL");

            entity.HasOne(d => d.Module).WithMany(p => p.MstModulePermissions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__MstModule__Modul__58D1301D");
        });

        modelBuilder.Entity<MstPlaceholder>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MstPlaceholder", "dbo");

            entity.Property(e => e.Dbvalue).HasColumnName("DBValue");
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
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
            entity.HasKey(e => e.Id).HasName("PK__MstState__3213E83FEB8BE5D3");

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
            entity.HasKey(e => e.UnitofMeasurementId).HasName("PK__MstUnito__21E7C5537FB3558A");

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
            entity.HasKey(e => e.AddressId).HasName("PK__TblAddre__091C2AFBF6AE7E7F");

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
                .HasConstraintName("FK__TblAddres__Conta__59C55456");

            entity.HasOne(d => d.StateNavigation).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.State)
                .HasConstraintName("FK__TblAddres__State__58DC1D15");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblAddres__Tenan__3FD07829");
        });

        modelBuilder.Entity<TblAppointmentSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblAppoi__3213E83F377B440E");

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
                .HasConstraintName("FK__TblAppoin__JobId__6ECB5E34");

            entity.HasOne(d => d.User).WithMany(p => p.TblAppointmentSchedules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblAppoin__UserI__261B931E");
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
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
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

        modelBuilder.Entity<TblDiscount>(entity =>
        {
            entity.HasKey(e => e.DiscountId).HasName("PK__TblDisco__E43F6D96ED6254AF");

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

        modelBuilder.Entity<TblEstimate>(entity =>
        {
            entity.HasKey(e => e.EstimateId).HasName("PK__TblEstim__ABEBF4B51ABC6B71");

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
                .HasConstraintName("FK__TblEstima__JobId__07AC1A97");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimates)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__08A03ED0");
        });

        modelBuilder.Entity<TblEstimateApproved>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblEstim__3214EC074DAE958A");

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
            entity.HasKey(e => e.EstWindowId).HasName("PK__TblEstim__F0A94F98E0C7CBA1");

            entity.ToTable("TblEstimateWindow", "dbo");

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
                .HasConstraintName("FK__TblEstima__Addre__0A888742");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__0C70CFB4");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__0B7CAB7B");

            entity.HasOne(d => d.Window).WithMany(p => p.TblEstimateWindows)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__TblEstima__Windo__1CA7377D");
        });

        modelBuilder.Entity<TblEstimateWindowProduct>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductId).HasName("PK__TblEstim__30AAFF258DFDC384");

            entity.ToTable("TblEstimateWindowProduct", "dbo");

            entity.Property(e => e.EstimateWindowProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.City).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Municipality).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
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
                .HasConstraintName("FK__TblEstima__EstWi__50D0E6F9");

            entity.HasOne(d => d.Estimate).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblEstima__Estim__0F4D3C5F");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblEstima__TaxId__04859529");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__0E591826");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblEstimateWindowProducts)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblEstima__Windo__3296789C");
        });

        modelBuilder.Entity<TblEstimateWindowProductComponent>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductComponentId).HasName("PK__TblEstim__198C247355EF57A0");

            entity.ToTable("TblEstimateWindowProductComponent", "dbo");

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
                .HasConstraintName("FK__TblEstima__Estim__05C3D225");

            entity.HasOne(d => d.EstimateWindowProduct).WithMany(p => p.TblEstimateWindowProductComponents)
                .HasForeignKey(d => d.EstimateWindowProductId)
                .HasConstraintName("FK__TblEstima__Estim__51C50B32");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProductComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblEstima__Tenan__04CFADEC");
        });

        modelBuilder.Entity<TblEstimateWindowProductComponentMaterial>(entity =>
        {
            entity.HasKey(e => e.EstimateWindowProductComponentMaterialId).HasName("PK__TblEstim__13E8FE878C48B58A");

            entity.ToTable("TblEstimateWindowProductComponentMaterial", "dbo");

            entity.Property(e => e.EstimateWindowProductComponentMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(255);
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
                .HasConstraintName("FK__EstimateW__Estim__1BB31344");

            entity.HasOne(d => d.EstimateWindowProductComponent).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.EstimateWindowProductComponentId)
                .HasConstraintName("FK__TblEstima__Estim__52B92F6B");

            entity.HasOne(d => d.ProductTypeNavigation).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.ProductType)
                .HasConstraintName("FK__EstimateW__Produ__19CACAD2");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblEstimateWindowProductComponentMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__EstimateW__Tenan__1ABEEF0B");
        });

        modelBuilder.Entity<TblEstimateWindowProductOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblEstim__3213E83F9C96ED79");

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
                .HasConstraintName("FK__TblEstima__Estim__25E688F4");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblEstima__TaxId__26DAAD2D");

            entity.HasOne(d => d.WindowOption).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.WindowOptionId)
                .HasConstraintName("FK__TblEstima__Windo__24F264BB");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblEstimateWindowProductOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblEstima__Windo__23FE4082");
        });

        modelBuilder.Entity<TblExpression>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblExpre__3214EC071EF09971");

            entity.ToTable("TblExpression", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpressionName).HasMaxLength(100);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.ProductTypeNavigation).WithMany(p => p.TblExpressions)
                .HasForeignKey(d => d.ProductType)
                .HasConstraintName("FK__TblExpres__Produ__353DDB1D");
        });

        modelBuilder.Entity<TblGhlStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblGHL_S__3213E83FA5D504FE");

            entity.ToTable("tblGHL_Stage", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
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
            entity.HasKey(e => e.Id).HasName("PK__TblGHlSt__3213E83F082DBD6A");

            entity.ToTable("TblGHlStagesmapping", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblGhlUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblGHL_U__3214EC0715BD3216");

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
            entity.HasKey(e => e.Id).HasName("PK__TblGHLCo__3214EC074F1C0B7E");

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
            entity.HasKey(e => e.InvoiceId).HasName("PK__TblInvoi__D796AAB5ED5E9A49");

            entity.ToTable("TblInvoice", "dbo");

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
                .HasConstraintName("FK__TblInvoic__Estim__149C0161");

            entity.HasOne(d => d.Job).WithMany(p => p.TblInvoices)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblInvoic__JobId__607251E5");
        });

        modelBuilder.Entity<TblInvoicePayment>(entity =>
        {
            entity.HasKey(e => e.InvoicePaymentId).HasName("PK__TblInvoi__A79CC7FE3E232C75");

            entity.ToTable("TblInvoicePayment", "dbo");

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
                .HasConstraintName("FK__TblInvoic__Invoi__52CE3E04");
        });

        modelBuilder.Entity<TblJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__TblJob__056690C26EFF9CAD");

            entity.ToTable("TblJob", "dbo");

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
                .HasConstraintName("FK__TblJob__AddressI__6166761E");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblJob__ContactI__625A9A57");

            entity.HasOne(d => d.JobStatusNavigation).WithMany(p => p.TblJobs)
                .HasForeignKey(d => d.JobStatus)
                .HasConstraintName("FK__TblJob__JobStatu__278EDA44");

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

        modelBuilder.Entity<TblJobContact>(entity =>
        {
            entity.HasKey(e => e.JobContactId).HasName("PK__TblJobCo__BB491B5F0239F7CC");

            entity.ToTable("TblJobContact", "dbo");

            entity.Property(e => e.JobContactId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Contact).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("FK__TblJobCon__Conta__28A2FA0E");

            entity.HasOne(d => d.ContactType).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.ContactTypeId)
                .HasConstraintName("FK__TblJobCon__Conta__5E3FF0B0");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobContacts)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_tbljobcontact_tbljob_JobId");
        });

        modelBuilder.Entity<TblJobSchedule>(entity =>
        {
            entity.HasKey(e => e.JobScheduleId).HasName("PK__TblJobSc__2EB48C451454220D");

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
                .HasConstraintName("FK__TblJobSch__Estim__27AED5D5");

            entity.HasOne(d => d.Job).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_TblJobSchedule_TblJob");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.JobTitleId)
                .HasConstraintName("FK__TblJobSch__JobTi__4B2D1C3C");

            entity.HasOne(d => d.User).WithMany(p => p.TblJobSchedules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblJobSch__UserI__4C214075");
        });

        modelBuilder.Entity<TblJobStageStatusAutomAtionHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__TblJobSt__4D7B4ABDB0385C6F");

            entity.ToTable("TblJobStageStatusAutomAtionHistory");

            entity.Property(e => e.HistoryId).ValueGeneratedNever();
            entity.Property(e => e.ChangedAt).HasColumnType("datetime");
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

        modelBuilder.Entity<TblLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__TblLocat__E7FEA4979D18AAB8");

            entity.ToTable("TblLocation", "dbo");

            entity.Property(e => e.LocationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LocationName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblManagePartnersProduct>(entity =>
        {
            entity.HasKey(e => e.ManageProductId).HasName("PK__TblManag__E07C429B7F9C28E8");

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
                .HasConstraintName("FK__TblManage__TaxId__2B9F624A");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblManagePartnersProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK_TblManagePartnersProducts_ManageProductTenant");
        });

        modelBuilder.Entity<TblManagePartnersProductsOption>(entity =>
        {
            entity.HasKey(e => e.ManageProductOptionId).HasName("PK__TblManag__5912BD4E4EAB5C00");

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
                .HasConstraintName("FK__TblManage__TaxId__2AAB3E11");
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

        modelBuilder.Entity<TblNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblNotif__3214EC0706DB6A2E");

            entity.ToTable("TblNotification", "dbo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Createdate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__TblOptio__92C7A1FFEA70BF74");

            entity.ToTable("TblOptions", "dbo");

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
                .HasConstraintName("FK__TblOption__TaxId__0579B962");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblOptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblOption__Tenan__7DCDAAA2");
        });

        modelBuilder.Entity<TblOptionsType>(entity =>
        {
            entity.HasKey(e => e.OptionTypeId).HasName("PK__TblOptio__96A2FCCE87EE3A55");

            entity.ToTable("TblOptions_Type", "dbo");

            entity.Property(e => e.OptionTypeId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.OptionType).HasMaxLength(200);

            entity.HasOne(d => d.Option).WithMany(p => p.TblOptionsTypes)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK__TblOption__Optio__1C1D2798");
        });

        modelBuilder.Entity<TblOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__TblOrder__C3905BCFDFE5CDF2");

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
            entity.HasKey(e => e.OrderWindowId).HasName("PK__TblOrder__861D63585D53762E");

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
            entity.HasKey(e => e.OrderWindowProductId).HasName("PK__TblOrder__915589245D3C0736");

            entity.ToTable("TblOrderWindowProduct", "dbo");

            entity.Property(e => e.OrderWindowProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdditionalWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ArchHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.City).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Height).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Length).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Municipality).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
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
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblOrderWindowProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblOrderW__TaxId__3DBE1285");
        });

        modelBuilder.Entity<TblOrderWindowProductComponent>(entity =>
        {
            entity.HasKey(e => e.OrderWindowProductComponentId).HasName("PK__TblOrder__381A8AF68221E24A");

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
            entity.HasKey(e => e.OrderWindowProductComponentMaterialId).HasName("PK__TblOrder__FECF878FF94DC8D6");

            entity.ToTable("TblOrderWindowProductComponentMaterial", "dbo");

            entity.Property(e => e.OrderWindowProductComponentMaterialId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MaterialName).HasMaxLength(255);
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
            entity.HasKey(e => e.Id).HasName("PK__TblOrder__3213E83F716A4742");

            entity.ToTable("TblOrderWindowProductOption", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MunicipilityTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StateTax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.OrderWindowProduct).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.OrderWindowProductId)
                .HasConstraintName("FK__TblOrderW__Order__4376EBDB");

            entity.HasOne(d => d.Tax).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__TblOrderW__TaxId__446B1014");

            entity.HasOne(d => d.WindowOption).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.WindowOptionId)
                .HasConstraintName("FK__TblOrderW__Windo__4282C7A2");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.TblOrderWindowProductOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__TblOrderW__Windo__418EA369");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__TblPayme__9B556A38349E201F");

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
                .HasConstraintName("FK__TblPaymen__Invoi__5792F321");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__TblAssem__1C02BDABE36F3A37");

            entity.ToTable("TblProduct", "dbo");

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
                .HasConstraintName("FK__TblProduc__TaxId__7172C0B5");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblProduc__Tenan__5D2BD0E6");
        });

        modelBuilder.Entity<TblProductComponent>(entity =>
        {
            entity.HasKey(e => e.ProductComponentId).HasName("PK__TblProdu__E29891DF9ACD9DBE");

            entity.ToTable("TblProductComponent", "dbo");

            entity.Property(e => e.ProductComponentId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PricingExpression).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Component).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK__TblProduc__Compo__5A4F643B");

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__TblProduc__Produ__5B438874");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblProductComponents)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblProduc__Tenan__5C37ACAD");
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
                .HasConstraintName("FK__TblRawMat__Produ__39AD8A7F");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblRawMat__Tenan__7908F585");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblRawMaterials)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK_TblRawMaterial_TblVendor");
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

        modelBuilder.Entity<TblStripeConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblStrip__3213E83F14DD4311");

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
            entity.HasKey(e => e.Id).HasName("PK__TblStrip__3214EC07FE6CDD49");

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
            entity.HasKey(e => e.Id).HasName("PK__TblSubte__3214EC077FC08328");

            entity.ToTable("TblSubtenantProductMarkup", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
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
                .HasConstraintName("FK__TblTaxRat__Tenan__5EDF0F2E");
        });

        modelBuilder.Entity<TblTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__TblTempl__F87ADD27F9BFA496");

            entity.ToTable("TblTemplate", "dbo");

            entity.Property(e => e.TemplateId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.TemplateCategory)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TemplateName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblTemplates)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblTempla__Tenan__31D75E8D");
        });

        modelBuilder.Entity<TblTemplatePlaceholder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblTempl__3214EC07B9CC024F");

            entity.ToTable("TblTemplatePlaceholder", "dbo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
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
            entity.Property(e => e.Suffix)
                .HasMaxLength(100)
                .IsUnicode(false);
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
                .HasConstraintName("FK__TblUser__JobTitl__2116E6DF");

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
                .HasConstraintName("FK__TblVendor__Tenan__324172E1");
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

            entity.HasOne(d => d.Tenent).WithMany(p => p.TblVendorOrders)
                .HasForeignKey(d => d.TenentId)
                .HasConstraintName("FK__TblVendor__Tenen__041093DD");

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
                .HasConstraintName("FK__TblVendor__Mater__0504B816");

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
            entity.Property(e => e.IsRated).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Width).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Address).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__TblWindow__Addre__756D6ECB");

            entity.HasOne(d => d.Job).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__TblWindow__JobId__52793849");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblWindows)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__TblWindow__Tenan__5090EFD7");
        });

        modelBuilder.Entity<TblWindowProduct>(entity =>
        {
            entity.HasKey(e => e.WindowProductId).HasName("PK__tblWindo__28B91C826392FBD3");

            entity.ToTable("tblWindowProduct", "dbo");

            entity.Property(e => e.WindowProductId).ValueGeneratedNever();
            entity.Property(e => e.BuildHeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BuildWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__tblWindow__Produ__4DB4832C");

            entity.HasOne(d => d.ProductType).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.ProductTypeId)
                .HasConstraintName("FK__tblWindow__Produ__4EA8A765");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__tblWindow__Tenan__51851410");

            entity.HasOne(d => d.Window).WithMany(p => p.TblWindowProducts)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__tblWindow__Windo__4CC05EF3");
        });

        modelBuilder.Entity<WindowOption>(entity =>
        {
            entity.HasKey(e => e.WindowoptionId).HasName("PK__WindowOp__F9F31DB279AABAC9");

            entity.ToTable("WindowOption", "dbo");

            entity.Property(e => e.WindowoptionId).ValueGeneratedNever();
            entity.Property(e => e.AdditionalCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.OptionType).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.OptionTypeId)
                .HasConstraintName("FK__WindowOpt__Optio__636EBA21");

            entity.HasOne(d => d.Tenant).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__WindowOpt__Tenan__1BE81D6E");

            entity.HasOne(d => d.Window).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__WindowOpt__Windo__1AF3F935");

            entity.HasOne(d => d.WindowProduct).WithMany(p => p.WindowOptions)
                .HasForeignKey(d => d.WindowProductId)
                .HasConstraintName("FK__WindowOpt__Windo__19FFD4FC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

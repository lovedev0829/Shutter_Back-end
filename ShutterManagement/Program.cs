using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.DAL.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Net;
using ShutterManagement.Services;
using ShutterManagement.Core;
using ShutterManagement;
using System.Configuration;
using AutoMapper;
using ShutterManagement.Business;
using ShutterManagement.Core.Common;
using Microsoft.Extensions.FileProviders;
using DinkToPdf.Contracts;
using DinkToPdf;
using ShutterManagement.Stripe;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration= builder.Configuration;
builder.Configuration.AddConstants();

builder.Services.AddDbContext<ShutterDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.CommandTimeout(60)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedCorsPolicy",
        builder => builder
            //.SetIsOriginAllowedToAllowWildcardSubdomains()
            //.WithOrigins("http://localhost:4200", "https://localhost:90", "https://*.sentridocs.com", "http://*.sentridocs.com")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
        //.AllowCredentials()
        );
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shutter Management API", Version = "v1" });
    options.CustomSchemaIds(x => x.FullName);

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", securitySchema);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    });
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true
    };
    
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddScoped<DataServices>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ClaimsProperty>();
builder.Services.AddScoped<FileUploadHelper>();
builder.Services.AddScoped<IMasterCreate, MasterCreate>();
builder.Services.AddScoped<ITenantService, TenantServices>();
builder.Services.AddScoped<ITenantBrandingService, TenantBrandingService>();
builder.Services.AddScoped<ISubtenantService, SubtenantServices>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IAddress, AddressService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITaxExemption, TaxExemptionService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IMaterial, MaterialService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IOption, OptionService>();
builder.Services.AddScoped<IOptionsTypeService, OptionsTypeService>();
builder.Services.AddScoped<IUserRoleService,UserRoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IModulePermission, ModuleService>();
builder.Services.AddScoped<IContactService,ContactServices>();  
builder.Services.AddScoped<IContactBillingService,ContactBillingService>();  
builder.Services.AddScoped<ITaxExemption, TaxExemptionService>();
builder.Services.AddScoped<IJobTitleService, JobTitleService>();
builder.Services.AddScoped<IComponentMaterial, ComponentMaterial>();
builder.Services.AddScoped<IMstProductType, ProductTypeService>();
builder.Services.AddScoped<IProductComponentService,ProductComponentService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUnitofMeasurementService, UnitofMeasurementService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobTypeService, JobTypeService>();
builder.Services.AddScoped<IJobStatusService, JobStatusService>();
builder.Services.AddScoped<ITaxRate, TaxService>();
builder.Services.AddScoped<ImportandExportCSV>();
builder.Services.AddScoped<IRegion,RegionServices>();
builder.Services.AddScoped<IMstContactType,MstContactTypeService>();
builder.Services.AddScoped<IWindowService,WindowService>();
builder.Services.AddScoped<IWindowProductService,WindowProduct>();
builder.Services.AddScoped<IWindowOptionService, WindowOptionService>();
builder.Services.AddScoped<ITemplatesService, TemplateService>();
builder.Services.AddScoped<IMstPlaceholderService, MstPlaceholderService>();
builder.Services.AddScoped<IVendorOrderService, VendorOrderService>();
builder.Services.AddScoped<IVendorItemService, VendorOrderItemService>();
builder.Services.AddScoped<IEstimateService, EstimateService>();
builder.Services.AddScoped<IEstimateWindowService, EstimateWindowService>();
builder.Services.AddScoped<IEstimateWindowProductService, EstimateWindowProductService>();
builder.Services.AddScoped<IEstimateWindowProductComponentService, EstimateWindowProductComponentService>();
builder.Services.AddScoped<IEstimateWindowProductComponentMaterialService, EstimateWindowProductComponentMaterialService>();
builder.Services.AddScoped<IExpressionService, ExpressionService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderWindowService, OrderWindowService>();
builder.Services.AddScoped<IOrderWindowProductService, OrderWindowProductService>();
builder.Services.AddScoped<IOrderWindowProductComponentService, OrderWindowProductComponentService>();
builder.Services.AddScoped<IOrderWindowProductComponentMaterialService, OrderWindowProductComponentMaterialService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoicePaymentService, InvoicePaymentService>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();
builder.Services.AddScoped<IComponentPriceService, ComponentPriceService>();
builder.Services.AddScoped<IJobSchedule, JobScheduleService>();
builder.Services.AddScoped<IJobContactService, JobContactService>();
builder.Services.AddScoped<IReportServices, ReportServices>();
builder.Services.AddScoped<IDashboard, DashboardService>();
builder.Services.AddScoped<IEstimationBreakdownService,EstimationBreakdownService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ISubtenantProductMarkup, SubtenantProductMarkup>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGHLConfiguration, GHLConfigurationService>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));



builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Register the StripeService as a singleton
builder.Services.AddSingleton<StripeService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    string path = configuration.GetSection("FileUploadPhysicalPathLocal").Value.ToString();
    string virtualPath = configuration.GetSection("FileVirtualPathLocal").Value.ToString();
    AppConstant.FileUploadPath = path;
    AppConstant.FileVirtualPath = virtualPath;
   app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(path),
        RequestPath = new PathString("/File"),
        EnableDirectoryBrowsing = false
    });
}
else
{
    string path = configuration.GetSection("FileUploadPhysicalPathDev").Value.ToString();
    string virtualPath = configuration.GetSection("FileVirtualPathDev").Value.ToString();

    AppConstant.FileUploadPath = path;
    AppConstant.FileVirtualPath = virtualPath;
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(path),
        RequestPath = new PathString("/Files"),
        EnableDirectoryBrowsing = false
    });
}
app.UseCors("AllowedCorsPolicy");
app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

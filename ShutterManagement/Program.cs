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

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration= builder.Configuration;
builder.Configuration.AddConstants();

builder.Services.AddDbContext<ShutterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.CommandTimeout(60)));


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
//// Swagger Configuration
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shutter Management API", Version = "v1" });
//    options.CustomSchemaIds(x => x.FullName);
//});

//// JWT Authentication Configuration

//builder.Services.AddSwaggerGen(options =>
//{
//    //options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mortgage API", Version = "v1" });
//    var securitySchema = new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorizations header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "bearer",
//        Reference = new OpenApiReference
//        {
//            Type = ReferenceType.SecurityScheme,
//            Id = "Authorization"
//        }
//    };
//    options.AddSecurityDefinition("Authorization", securitySchema);
//    var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Authorization" } } };
//    options.AddSecurityRequirement(securityRequirement);
//    options.CustomSchemaIds(x => x.FullName);
//});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(cfg =>
//{
//    cfg.RequireHttpsMetadata = false;
//    cfg.SaveToken = true;
//    cfg.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidIssuer = AppConstant.JwtIssuer,
//        ValidAudience = AppConstant.JwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey)),
//        ValidateLifetime = true,
//        ValidateAudience = true,
//        ValidateIssuer = true,
//        ValidateIssuerSigningKey = true,
//    };

//    Console.WriteLine(AppConstant.DBConnectionString);
//    Console.WriteLine();
//    Console.WriteLine();
//    Console.WriteLine();
//    Console.WriteLine();

//});
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMasterCreate, MasterCreate>();
builder.Services.AddScoped<ITenantService, TenantServices>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubtenantService,SubtenantServices>();
builder.Services.AddScoped<DataServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shutter Management API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

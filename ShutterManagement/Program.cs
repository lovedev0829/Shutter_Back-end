using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.DAL.Context;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ShutterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
    sqlOptions => sqlOptions.CommandTimeout(60))); ;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMasterCreate, MasterCreate>();
builder.Services.AddScoped<ITenantService, TenantServices>();
builder.Services.AddScoped<DataServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

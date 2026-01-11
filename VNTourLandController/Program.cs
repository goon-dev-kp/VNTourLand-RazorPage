using BLL.Services.Implement;
using BLL.Services.Interface;
using DAL.UnitOfWork;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using System;

var builder = WebApplication.CreateBuilder(args);

// ?? Add DbContext (dùng connection string t? appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ?? Add configuration binding
builder.Services.Configure<SePayOptions>(builder.Configuration.GetSection("SePayOptions"));

// ?? Add services to DI container
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IRequestOfCustomerService, RequestOfCustomerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISepayService, SepayService>();

// ?? Add HTTP client factory
builder.Services.AddHttpClient();

// ?? Add controller & options
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(new
            {
                success = false,
                message = "Invalid request",
                errors = context.ModelState
            });
        };
    });

// ?? Add Swagger with basic setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VNTourLand API", Version = "v1" });
});

// ?? Add CORS (cho phép g?i t? RazorPage ho?c FE khác)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ?? Use Swagger UI in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ?? Middleware pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();

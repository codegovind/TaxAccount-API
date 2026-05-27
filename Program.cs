using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaxAccount.Authorization;
using TaxAccount.Data;
using TaxAccount.Middleware;
using TaxAccount.Services;
using TaxAccount.Validators;

// Configure Serilog first before anything else
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/taxaccount-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Starting TaxAccount API...");

    var builder = WebApplication.CreateBuilder(args);
    // Use Serilog
    builder.Host.UseSerilog();

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

    // In-Memory Cache
    builder.Services.AddMemoryCache();

    // ── HTTP Context Accessor (for TenantService) ──
    builder.Services.AddHttpContextAccessor();

    // ── Services ──
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IInvoiceService, InvoiceService>();
    builder.Services.AddScoped<IStockService, StockService>();
    builder.Services.AddScoped<DataSeeder>();
    builder.Services.AddScoped<IPurchaseService, PurchaseService>();
    builder.Services.AddScoped<IContactService, ContactService>();
    
    // Accounting & Compliance Services
    builder.Services.AddScoped<IAccountingService, AccountingService>();
    builder.Services.AddScoped<IEWayBillService, EWayBillService>();
    builder.Services.AddScoped<ITenantSettingService, TenantSettingService>();
    
    // Database
     builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    });    

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    //var secretKey = jwtSettings["SecretKey"]!;
    //not null secrect key not forgiving
        var secretKey = jwtSettings["SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT SecretKey is missing in configuration");
        }


    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
    options.TokenValidationParameters = new TokenValidationParameters
        {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey))
        };
    });

    // Register permission handler
    builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

    // Register each permission as a policy
    builder.Services.AddAuthorization(options =>
    {
    var permissions = new[]
    {
        "products.view", "products.create", "products.edit", "products.delete",
        "invoices.view", "invoices.create", "invoices.approve",
        "reports.view", "users.manage", "accounts.manage",
        "contacts.manage", "stock.manage"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
    });

    // Add Angular Cores 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
                policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:8080",
                "http://taxaccount-frontend.s3-website.ap-south-1.amazonaws.com",
                "https://d2lrr9q3q8iecd.cloudfront.net",
                "https://d3dpmdc1qjwvnh.cloudfront.net"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    //Run pending Migrations is any
    using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // This command checks the database and runs any pending migrations automatically!
        context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}
    // Global Exception Middleware
    app.UseMiddleware<ExceptionMiddleware>();

    // Add Angular frontend
    app.UseCors("AllowAngular");

    app.UseSwagger();
    app.UseSwaggerUI();

    // Log every request
    app.UseSerilogRequestLogging();

    // authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Test endpoints
    app.MapGet("/hello", () =>
    {
        Log.Information("Hello endpoint was called");
        return "Hello from TaxAccount!";
    });

    Log.Information("TaxAccount API started successfully");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "TaxAccount API failed to start");
}
finally
{
    Log.CloseAndFlush();
}


// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
// app.MapGet("/hello", () => "Hello from Docker!");
// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Suppress the automatic 400 response from [ApiController] so our controllers
// can return a clean { message: "..." } shape instead of the ASP.NET default
// { title: "One or more validation errors occurred.", errors: {...} }
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// Configure Entity Framework with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register application services
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Configure CORS
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins(allowedOrigins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Interview Scheduling System API",
        Version = "v1",
        Description = "API for Automated Interview Scheduling System with AI Conflict Resolution"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Interview Scheduler API V1");
    c.RoutePrefix = "swagger";
});

// app.UseHttpsRedirection();

// Serve static frontend files from wwwroot
// UseDefaultFiles must come BEFORE UseStaticFiles so that "/" serves index.html
// login.html and register.html are served directly as static files — no special handling needed
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback: for any route that is NOT an API call and NOT a real file,
// return index.html so client-side navigation works.
// Real .html files (login.html, register.html) are already served by UseStaticFiles above
// and will never reach this fallback.
app.MapFallback(async (HttpContext context) =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    // Let API misses and requests for files-with-extensions return 404
    if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
        System.IO.Path.HasExtension(path))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    // All other unmatched routes → serve index.html (SPA client-side routing)
    context.Response.ContentType = "text/html; charset=utf-8";
    var indexPath = System.IO.Path.Combine(app.Environment.WebRootPath, "index.html");
    await context.Response.SendFileAsync(indexPath);
});

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

app.Run();


using CampusEats.Api.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------
// 1. Controller and API Configuration
// ---------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------------------------------------
// 2. CORS Configuration
// Allow Vercel Production and Preview URLs
// ---------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "https://sef-lab-11-nine.vercel.app",
                "https://sef-lab-11-o5wmzn6lc-saneths-projects.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------------------------------------
// 3. JWT Authentication Configuration
// ---------------------------------------

var jwt = builder.Configuration.GetSection("Jwt");

var jwtKey = jwt["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key must be configured using an environment variable.");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwt["Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateLifetime = true
            };
    });

builder.Services.AddAuthorization();

// ---------------------------------------
// 4. Swagger Configuration with JWT
// ---------------------------------------

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste ONLY the token - no 'Bearer ' prefix."
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        });
});

// ---------------------------------------
// 5. Application Services
// ---------------------------------------

builder.Services.AddScoped<IMenuService, MenuService>();

// ---------------------------------------
// 6. PostgreSQL Database Configuration
// ---------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Default must be configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ---------------------------------------
// 7. Build Application
// ---------------------------------------

var app = builder.Build();

// ---------------------------------------
// 8. Seed Initial Menu Data
// Database migrations must be applied first
// ---------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(
            new MenuItem
            {
                Name = "Kottu Roti",
                Price = 750m,
                Category = "Mains"
            },
            new MenuItem
            {
                Name = "Fried Rice",
                Price = 850m,
                Category = "Mains"
            },
            new MenuItem
            {
                Name = "Watalappan",
                Price = 350m,
                Category = "Dessert"
            }
        );

        db.SaveChanges();
    }
}

// ---------------------------------------
// 9. Swagger and HTTPS Configuration
// ---------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
}

// Render handles HTTPS at the hosting platform.

// ---------------------------------------
// 10. Routing and CORS Middleware
// ---------------------------------------

app.UseRouting();

app.UseCors("Frontend");

// ---------------------------------------
// 11. Authentication and Authorization
// ---------------------------------------

app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------
// 12. Map Controllers
// ---------------------------------------

app.MapControllers();

// ---------------------------------------
// 13. Run Application
// ---------------------------------------

app.Run();

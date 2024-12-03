using Microsoft.EntityFrameworkCore;
using app_baleares.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using app_baleares.Helpers;
using NetCoreBackend.Models;
using NetCoreBackend.Services;
using Microsoft.OpenApi.Models;
using NetCoreBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using app_baleares.Services.Interfaces;
using app_baleares.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "x-token",
        In = ParameterLocation.Header,
        Description = "Introduce el token en el header 'x-token'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKeyAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var secretKey = builder.Configuration["ServerSettings:SecretKey"];
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/AuthView/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Headers.ContainsKey("x-token"))
            {
                context.Token = context.Request.Headers["x-token"];
            }
            return Task.CompletedTask;
        }
    };
});
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/AuthView/Login";
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//        options.SlidingExpiration = true;
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//    })
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(key),
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.Zero
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                if (context.Request.Headers.ContainsKey("x-token"))
//                {
//                    context.Token = context.Request.Headers["x-token"];
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseCors("CorsPolicy");
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthView}/{action=Login}/{id?}");

app.Run();

using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Helpers;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// 1️⃣ Add basic services
// ---------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------
// 2️⃣ Register custom services (DI)
// ---------------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IAspNetUsersService, AspNetUsersService>();
builder.Services.AddTransient<IAspNetRolesService, AspNetRolesService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ISysApiService, SysApiService>();
builder.Services.AddScoped<IEmailSenderService, SendEmailService>();
builder.Services.AddSingleton<Globals>();
builder.Services.AddHttpContextAccessor();

// Product-related services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ---------------------------
// 3️⃣ Add DbContext + Identity
// ---------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AspNetUsers, AspNetRoles>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---------------------------
// 4️⃣ JWT Configuration
// ---------------------------
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSection = builder.Configuration.GetSection("Jwt");
string? key = jwtSection["Key"];
string? issuer = jwtSection["Issuer"];
string? audience = jwtSection["Audience"];

if (string.IsNullOrEmpty(key))
    throw new Exception("JWT Key is missing in configuration (appsettings.json).");

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

// ---------------------------
// 5️⃣ Session (optional)
// ---------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------------------------
// 6️⃣ Swagger with JWT
// ---------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LVTN_BE_COFFE", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer' [space] + token.\n\nVí dụ: `Bearer abc123xyz`"
    });

    c.AddSecurityRequirement(new Microso

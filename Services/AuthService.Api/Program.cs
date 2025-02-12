using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Current environment
var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

// 🔹 Настройка PostgreSQL
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(currentEnvironment)));

// 🔹 Настройка Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// 🔹 Настройка JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Используем аутентификацию и авторизацию
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Эндпоинт регистрации
app.MapPost("/regdfsister", async (UserManager<ApplicationUser> userManager, string email, string password) =>
{

    return Results.Ok(builder.Configuration.GetConnectionString(currentEnvironment));
});

// 🔹 Эндпоинт регистрации
app.MapPost("/register", async (UserManager<ApplicationUser> userManager, string email, string password) =>
{
    var user = new ApplicationUser { UserName = email, Email = email };
    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded) return Results.BadRequest(result.Errors);
    return Results.Ok("User registered");
});

// 🔹 Эндпоинт логина
app.MapPost("/login", async (UserManager<ApplicationUser> userManager, string email, string password) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null || !await userManager.CheckPasswordAsync(user, password))
        return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
    };

    var token = new JwtSecurityToken(
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Issuer"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );

    return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
});

// 🔹 Эндпоинт добавления роли
app.MapPost("/add-role", async (UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, string email, string role) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null) return Results.NotFound("User not found");

    if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));

    await userManager.AddToRoleAsync(user, role);
    return Results.Ok($"Role '{role}' added to {email}");
}).RequireAuthorization();

app.Run();

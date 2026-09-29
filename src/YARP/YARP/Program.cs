using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var yarpConfigPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "Config"));

foreach (var file in Directory.GetFiles(yarpConfigPath, "*.json"))
    builder.Configuration.AddJsonFile(file, optional: false, reloadOnChange: true);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("fixed", fixedOpt =>
    {
        fixedOpt.PermitLimit = 100;
        fixedOpt.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = "tinkerfoundry-auth",
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("default", policy => policy.RequireAuthenticatedUser());
});

builder.WebHost.ConfigureKestrel(opt => opt.AddServerHeader = false);

var app = builder.Build();

app.UseExceptionHandler(a => a.Run(async context =>
{
    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("Something went wrong.");
}));

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { System.Net.IPAddress.Parse("127.0.0.1") }
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Calm down, i am alive...!");
app.MapGet("/health", () => Results.Ok());

app.MapReverseProxy();

app.Run();
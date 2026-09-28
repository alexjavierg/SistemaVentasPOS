using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddOpenApi();

// Configure EF Core with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure CORS (Restrict to frontend / local clients)
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5209";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins.Split(','))
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrWhiteSpace(jwtKey)) jwtKey = "F4llb4ck_S3cr3t_K3y_For_JWT_Th4t_Is_L0ng_En0ugh";
var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // True in prod
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "VentasApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "VentasClients",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();



using (var scope = app.Services.CreateScope())
{
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
    {
        context.Database.Migrate();
        context.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Tallas (Id INT AUTO_INCREMENT PRIMARY KEY, Nombre VARCHAR(50) NOT NULL);");
        var count = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM Tallas;");
        // We can just rely on normal seed for Tallas if we want, but ExecuteSqlRaw doesn't return count easily in this way.
        // Let's insert manually if empty
        bool hasTallas = context.Tallas.Any();
        if (!hasTallas)
        {
            context.Database.ExecuteSqlRaw("INSERT INTO Tallas (Nombre) VALUES ('ESTÁNDAR'), ('S'), ('M'), ('L'), ('XL');");
        }
    }
    catch { }
}
// Seed / Update Empty QRs
using (var scope2 = app.Services.CreateScope())
{
    var context2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
    var modelsSinQr = context2.Modelos.Where(m => string.IsNullOrEmpty(m.CodigoQR)).ToList();
    if (modelsSinQr.Any())
    {
        var rnd = new Random();
        foreach (var m in modelsSinQr)
        {
            m.CodigoQR = rnd.Next(10000000, 99999999).ToString();
        }
        context2.SaveChanges();
    }
}

app.MapControllers();

app.Run();











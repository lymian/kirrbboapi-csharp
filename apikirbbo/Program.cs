using apikirbbo.Data;
using apikirbbo.Repositories;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SqlConnectionFactory>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<CompraRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "apikirbbo",
            ValidAudience = "kirbboshop",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mi_clave_supersecreta_32chars!mimi"))
        };
    });
// Agrega la política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:4200",
                  "http://localhost:4300"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Solo si usas autenticación basada en cookies
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngularApp");


app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Repositories;
using ProyectoRedes.Services;

var builder = WebApplication.CreateBuilder(args);


// Configuraci?n de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Conexi?n a BD
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar DbContext
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurar servicios
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IRoundRepository, RoundRepository>();
builder.Services.AddScoped<IEnemyRepository, EnemyRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddControllers();

var app = builder.Build();

// Aplicar la politica de CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();

app.UseStaticFiles();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
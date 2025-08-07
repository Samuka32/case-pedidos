using PedidosApi.Application.Services;
using PedidosApi.Infrastructure.Repositories;
using AppInterfaces = PedidosApi.Application.Interfaces;
using DomainInterfaces = PedidosApi.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories - usando interfaces da Application
builder.Services.AddScoped<AppInterfaces.IPedidoRepository, JsonPedidoRepository>();
builder.Services.AddScoped<AppInterfaces.IEstoqueRepository, JsonEstoqueRepository>();

// Services - usando interfaces do Domain
builder.Services.AddScoped<DomainInterfaces.IPedidoService, PedidoService>();
builder.Services.AddScoped<DomainInterfaces.IEstoqueService, EstoqueService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

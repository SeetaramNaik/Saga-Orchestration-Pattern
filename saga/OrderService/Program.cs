using MassTransit;
using OrderService.Models;
using Shared.Events;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderService.Consumers;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",new OpenApiInfo {Title = "Order API", Version = "v1"});
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderRequestCompletedEventConsumer>();
    x.AddConsumer<OrderRequestFailedEventConsumer>();
    x.UsingRabbitMq((context, config) =>
    {
        
        config.Host(builder.Configuration.GetConnectionString("RabbitMQConnection"));
        config.ReceiveEndpoint(RabbitMqSettings.OrderRequestCompletedEventQueueName, x =>
        {
            x.ConfigureConsumer<OrderRequestCompletedEventConsumer>(context);
        });
        config.ReceiveEndpoint(RabbitMqSettings.OrderRequestFailedEventQueueName, x =>
        {
            x.ConfigureConsumer<OrderRequestFailedEventConsumer>(context);
        });
    });
});

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

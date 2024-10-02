using MassTransit;
using Microsoft.OpenApi.Models;
using PaymentService.Consumers;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",new OpenApiInfo {Title = "Payment API", Version = "v1"});
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StockReservedRequestPaymentConsumer>();
    x.UsingRabbitMq((context, config) =>
    {
        // config.Host(new Uri("rabbitmq://localhost"), h =>
        // {
        //     h.Username("guest");
        //     h.Password("guest");
        // });
        config.Host(builder.Configuration.GetConnectionString("RabbitMQConnection"));
        config.ReceiveEndpoint(RabbitMqSettings.PaymentStockReservedRequestQueueName, e =>
        {
            e.ConfigureConsumer<StockReservedRequestPaymentConsumer>(context);
        });
    });
});

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

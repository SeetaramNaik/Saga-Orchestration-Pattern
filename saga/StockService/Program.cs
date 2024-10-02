using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Messages;
using StockService.Consumers;
using StockService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",new OpenApiInfo {Title = "Stock API", Version = "v1"});
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<StockRollbackMessageConsumer>();
    x.UsingRabbitMq((context, config) =>
    {
        // config.Host(new Uri("rabbitmq://localhost"), h =>
        // {
        //     h.Username("guest");
        //     h.Password("guest");
        // });
        config.Host(builder.Configuration.GetConnectionString("RabbitMQConnection"));
        config.ReceiveEndpoint(RabbitMqSettings.StockOrderCreatedEventQueueName, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
        config.ReceiveEndpoint(RabbitMqSettings.StockRollbackQueueName, e =>
        {
            e.ConfigureConsumer<StockRollbackMessageConsumer>(context);
        });
    });
});

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock API v1"));
}

app.UseHttpsRedirection();
    
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();


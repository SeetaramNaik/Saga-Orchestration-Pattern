using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SagaOrchestrationService;
using SagaOrchestrationService.Data;
using SagaOrchestrationService.Models;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",new OpenApiInfo {Title = "Saga Orchestrator API", Version = "v1"});
});

builder.Services.AddMassTransit(config =>
{
    config.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder1) =>
        {
            builder1.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection"), m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            });
        });
    });
    
    config.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
    {
        // configure.Host(new Uri("rabbitmq://localhost"), h =>
        // {
        //     h.Username("guest");
        //     h.Password("guest");
        // });
        configure.Host(builder.Configuration.GetConnectionString("RabbitMQConnection"));
        configure.ReceiveEndpoint(RabbitMqSettings.OrderSaga, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(provider);
        });
    }));
    
});

builder.Services.AddMassTransitHostedService();
builder.Services.AddHostedService<Worker>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saga Orchestrator API v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();


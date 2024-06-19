using IvoryFunctions.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.RabbitMq;

var host = WebApplication.CreateBuilder(args);
host.Services.AddLogging(x => x.AddConsole());

if (args.Contains("--rabbitmq"))
{
    var rabbitmqUsername = Guid.NewGuid().ToString("N");
    var rabbitmqPassword = Guid.NewGuid().ToString("N");

    var rabbitmqContainer = new RabbitMqBuilder()
        .WithUsername(rabbitmqUsername)
        .WithPassword(rabbitmqPassword)
        .Build();
    await rabbitmqContainer.StartAsync();

    host.Services.AddIvoryFunctions(ifCfg =>
    {
        ifCfg.ConfigureMassTransit((bus, configureFunctions) =>
        {
            bus.UsingRabbitMq((ctx, busCfg) =>
            {
                busCfg.Host(rabbitmqContainer.Hostname, "/", h =>
                {
                    h.Username(rabbitmqUsername);
                    h.Password(rabbitmqPassword);
                });

                configureFunctions(ctx, busCfg);
            });
        });
    });
}
else
{
    host.Services.AddIvoryFunctions();
}

var app = host.Build();

app.UseIvoryFunctions();

await app.RunAsync();

//await rabbitmqContainer.StopAsync();

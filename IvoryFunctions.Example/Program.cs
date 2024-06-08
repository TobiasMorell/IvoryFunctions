using System.Reflection;
using IvoryFunctions.Extensions;
using IvoryFunctions.Http;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var rabbitmqUsername = Guid.NewGuid().ToString("N");
var rabbitmqPassword = Guid.NewGuid().ToString("N");

// var rabbitmqContainer = new RabbitMqBuilder()
//     .WithUsername(rabbitmqUsername)
//     .WithPassword(rabbitmqPassword)
//     .Build();
// await rabbitmqContainer.StartAsync();

var host = WebApplication.CreateBuilder(args);
host.Services.AddControllers().AddApplicationPart(typeof(HttpTriggerAttribute).Assembly).AddControllersAsServices();
host.Services.AddLogging(x => x.AddConsole());
host.Services.AddMassTransit(cfg =>
        {
            var fns = cfg.AddIvoryFunctions();

            /*cfg.UsingRabbitMq(
                (ctx, busCfg) =>
                {
                    busCfg.Host(
                        rabbitmqContainer.IpAddress,
                        "/",
                        h =>
                        {
                            h.Username(rabbitmqUsername);
                            h.Password(rabbitmqPassword);
                        }
                    );
                    ctx.SetupMassTransitFunctionsQueueTriggers(busCfg, fns);
                }
            );*/

            cfg.UsingInMemory(
                (ctx, busCfg) =>
                {
                    ctx.SetupIvoryFunctionsQueueTriggers(busCfg, fns);
                }
            );
        });

var app = host.Build();

app.MapControllers();
await app.RunAsync();

//await rabbitmqContainer.StopAsync();

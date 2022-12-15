using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuevosRegistrosService;
using System;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        string CONNSTR = System.Configuration.ConfigurationManager.ConnectionStrings["AzureBusConnection"].ConnectionString;
        //string CONNSTRDB = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

        services.AddSingleton((s) =>
        {
            return new ServiceBusClient(CONNSTR, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
        });

        services.AddSingleton<IAzServiceBus, AzServiceBus>();



    })
    .UseWindowsService()
    .Build();



await host.RunAsync();

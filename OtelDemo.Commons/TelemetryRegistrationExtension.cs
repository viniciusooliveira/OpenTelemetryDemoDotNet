using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelDemo.Commons.Services;

namespace OtelDemo.Commons
{
    public static class TelemetryRegistrationExtension
    {
        public static void AddTelemetry(this IServiceCollection services, AppSettings settings)
        {
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: settings.AppName,
                    serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("hostname", Dns.GetHostName()),
                    new KeyValuePair<string, object>("env", settings.EnvironmentName)
                });

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(otel =>
                {
                    otel.IncludeScopes = false;
                    otel.IncludeFormattedMessage = true;
                    otel.SetResourceBuilder(resourceBuilder);
                    otel.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(settings.OtlpEndpoint);
                        o.Protocol = OtlpExportProtocol.Grpc;

                        // o.HttpClientFactory = () =>
                        // {
                        //     var handler = new HttpClientHandler();
                        //     handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                        //     handler.ServerCertificateCustomValidationCallback = 
                        //         (httpRequestMessage, cert, cetChain, policyErrors) =>
                        //         {
                        //             return true;
                        //         };
                        //
                        //     return new HttpClient(handler);
                        // };

                        o.ExportProcessorType = ExportProcessorType.Simple;
                    });

                    // otel.AddConsoleExporter();
                });

                // logging.AddJsonConsole();
            });

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .AddSource(settings.AppName)
                        .AddLegacySource(settings.AppName)
                        .SetErrorStatusOnException()
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation(o =>
                        {
                            o.RecordException = true;
                        })
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(settings.OtlpEndpoint);
                            o.ExportProcessorType = ExportProcessorType.Simple;
                            
                        });
                })
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddMeter(settings.AppName)
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(settings.OtlpEndpoint);
                            o.ExportProcessorType = ExportProcessorType.Simple;
                            o.ExportProcessorType = ExportProcessorType.Batch;
                            o.BatchExportProcessorOptions = new BatchExportActivityProcessorOptions
                            {
                                ScheduledDelayMilliseconds = 5000,
                                ExporterTimeoutMilliseconds = 10000
                            };
                        });
                });

            services.AddSingleton<MetricService>();
            services.AddSingleton<TraceService>();
        }
    }
}
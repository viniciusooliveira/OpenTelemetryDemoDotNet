using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
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
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(settings.AppName)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("hostname", Dns.GetHostName())
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
                        o.ExportProcessorType = ExportProcessorType.Batch;
                    });
                });
            });

            services.AddOpenTelemetry()
                .ConfigureResource(r => 
                    r.AddService(
                        serviceName: settings.AppName,
                        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: Environment.MachineName
                        )
                )
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
                        .AddRedisInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(settings.OtlpEndpoint);
                            o.ExportProcessorType = ExportProcessorType.Batch;
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
            
            // services.AddOpenTelemetryTracing(builder =>
            //     builder
            //         .SetErrorStatusOnException()
            //         .SetResourceBuilder(resourceBuilder)
            //         .SetSampler(
            //             new ParentBasedSampler(new TraceIdRatioBasedSampler(1)))
            //         .AddAspNetCoreInstrumentation()
            //         .AddSource(settings.AppName)
            //         .AddLegacySource(settings.AppName)
            //         .AddHttpClientInstrumentation(o =>
            //         {
            //             o.RecordException = true;
            //         })
            //         .AddRedisInstrumentation()
            //         .AddOtlpExporter(o =>
            //             {
            //                 o.Endpoint = new Uri(settings.OtlpEndpoint);
            //                 o.ExportProcessorType = ExportProcessorType.Batch;
            //             }
            //         )
            // );
            //
            // services.AddOpenTelemetryMetrics(builder =>
            //     builder
            //         .SetResourceBuilder(resourceBuilder)
            //         .AddAspNetCoreInstrumentation()
            //         .AddMeter(settings.AppName)
            //         .AddOtlpExporter(o =>
            //             {
            //                 o.Endpoint = new Uri(settings.OtlpEndpoint);
            //                 o.ExportProcessorType = ExportProcessorType.Simple;
            //                 o.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
            //                 {
            //                     ExportIntervalMilliseconds = 5000
            //                 };
            //                 o.AggregationTemporality = AggregationTemporality.Delta;
            //             }
            //         )
            // );

            services.AddSingleton<MetricService>();
            services.AddSingleton<TraceService>();
        }
    }
}
using System;
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace OtelDemo.Commons.Services
{
    public class TraceService
    {
        private readonly string _appName;
        private readonly TextMapPropagator _propagator = new TraceContextPropagator();
        private readonly TracerProvider _tracerProvider;
        private Tracer _tracer;

        public TraceService(TracerProvider tracerProvider, AppSettings settings)
        {
            _tracerProvider = tracerProvider;
            _appName = settings.AppName;
        }

        public Tracer GetTracer()
        {
            return _tracer ??= _tracerProvider?.GetTracer(_appName);
        }

        public TelemetrySpan StartActiveSpan(string name, SpanKind kind = SpanKind.Internal,
            in SpanContext parentContext = default, SpanAttributes initialAttributes = null,
            IEnumerable<Link> links = null, DateTimeOffset startTime = default)
        {
            return GetTracer()?.StartActiveSpan(name, kind, parentContext, initialAttributes, links, startTime);
        }

        public IDictionary<string, object> CreateContextHeaders(
            TelemetrySpan activity)
        {
            if (activity is not null)
            {
                var headers = new Dictionary<string, object>();
                _propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), headers,
                    (properties, key, value) => { headers[key] = value; });
                return headers;
            }

            return null;
        }
    }
}
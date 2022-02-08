using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using OpenTelemetry.Metrics;

namespace OtelDemo.Commons.Services
{
    public class MetricService
    {
        private readonly string _appName;
        private readonly Meter _mainMeter;
        private readonly MeterProvider _meterProvider;
        private readonly Dictionary<string, object> _instruments = new();

        public MetricService(MeterProvider meterProvider, AppSettings settings)
        {
            _appName = settings.AppName;
            _meterProvider = meterProvider;
            _mainMeter = new Meter(settings.AppName);
        }


        public void AddToCounter<T>(string name, T delta, string description = null, string unit = null,
            KeyValuePair<string, object>[] tags = null) where T : struct
        {
            try
            {
                CreateCounter<T>(name, description, unit);
                var counter = (Counter<T>)_instruments[name];
                counter.Add(delta, InjectTags(tags));
            }
            catch
            {
                // ignored
            }
        }

        public void RecordToHistogram<T>(string name, T value, string description = null, string unit = null,
            KeyValuePair<string, object>[] tags = null) where T : struct
        {
            try
            {
                CreateHistogram<T>(name, description, unit);
                var histogram = (Histogram<T>)_instruments[name];
                histogram.Record(value, InjectTags(tags));
            }
            catch
            {
                // ignored
            }
        }

        public void CreateObservableGauge<T>(string name, Func<T> observeValue, string description = null,
            string unit = null, KeyValuePair<string, object>[] tags = null)
            where T : struct
        {
            if (!_instruments.ContainsKey(name))
                _instruments.Add(name, _mainMeter.CreateObservableGauge(name,
                    () => new Measurement<T>(observeValue.Invoke(), InjectTags(tags)), unit, description));
        }

        private KeyValuePair<string, object>[] InjectTags(KeyValuePair<string, object>[] tags)
        {
            tags ??= new KeyValuePair<string, object>[] { };
            return tags
                .Append(new KeyValuePair<string, object>("entity.name", _appName))
                .Append(new KeyValuePair<string, object>("service.name", _appName))
                .ToArray();
        }

        private void CreateCounter<T>(string name, string description = null, string unit = null)
            where T : struct
        {
            if (!_instruments.ContainsKey(name))
                _instruments.Add(name, _mainMeter.CreateCounter<T>(name, unit, description));
        }

        private void CreateHistogram<T>(string name, string description = null, string unit = null)
            where T : struct
        {
            if (!_instruments.ContainsKey(name))
                _instruments.Add(name, _mainMeter.CreateHistogram<T>(name, unit, description));
        }
    }
}
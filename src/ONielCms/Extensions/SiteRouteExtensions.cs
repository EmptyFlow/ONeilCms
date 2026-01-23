using ONielCms.Services;
using ONielCommon.Entities;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ONielCms.Extensions {

    public record ProcessorElementParameter(string Name, string Value);

    public record ProcessorElement(string Name, IEnumerable<ProcessorElementParameter> Parameters);

    public static class SiteRouteExtensions {

        private static ConcurrentDictionary<string, IEnumerable<ProcessorElement>> _processorsCache = [];

        extension(SiteRoute target) {

            public IEnumerable<ProcessorElement> GetProcessors() {
                var processors = target.Processors;
                if ( string.IsNullOrEmpty(processors) ) return [];

                if ( !_processorsCache.ContainsKey(processors) ) return _processorsCache[processors];

                var processorItems = JsonSerializer.Deserialize(processors, OnielCmsJsonContext.Default.IEnumerableProcessorElement);
                if ( processorItems?.Any() == true ) _processorsCache.TryAdd(processors, processorItems);

                return processorItems != null ? processorItems : [];
            }

        }

    }

}
;
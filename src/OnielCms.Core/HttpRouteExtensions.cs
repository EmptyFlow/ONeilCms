using System.Collections.Concurrent;

namespace OnielCms.Core
{
	public static class HttpRouteExtensions
	{

		private static ConcurrentDictionary<string, IEnumerable<ProcessorElement>> _processorsCache = [];

		extension(HttpRoute target)
		{

			public IEnumerable<ProcessorElement> GetProcessors(IProcessorsDeserializer processorsDeserializer)
			{
				var processors = target.Processors;
				if (string.IsNullOrEmpty(processors)) return [];

				if (!_processorsCache.ContainsKey(processors)) return _processorsCache[processors];

				var processorItems = processorsDeserializer.Deserialize(processors);
				if (processorItems?.Any() == true) _processorsCache.TryAdd(processors, processorItems);

				return processorItems != null ? processorItems : [];
			}

		}

	}

}

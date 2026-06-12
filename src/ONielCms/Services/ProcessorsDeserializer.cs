using OnielCms.Core;
using System.Text.Json;

namespace ONielCms.Services
{

	public class ProcessorsDeserializer : IProcessorsDeserializer
	{

		public IEnumerable<ProcessorElement> Deserialize(string processors)
		{
			return JsonSerializer.Deserialize(processors, OnielCmsJsonContext.Default.IEnumerableProcessorElement) ?? [];
		}

	}

}

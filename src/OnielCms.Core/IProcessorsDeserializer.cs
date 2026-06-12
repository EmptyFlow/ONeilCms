namespace OnielCms.Core
{
	public interface IProcessorsDeserializer
	{

		IEnumerable<ProcessorElement> Deserialize(string processors);

	}

}

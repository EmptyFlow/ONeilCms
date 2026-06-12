namespace OnielCms.Core
{

	public class RouteParameters
	{

		public readonly Dictionary<string, string[]> Query;

		public readonly Dictionary<string, string> Route;

		public RouteParameters(Dictionary<string, string[]> query, Dictionary<string, string> route)
		{
			Query = query;
			Route = route;
		}
	}

}

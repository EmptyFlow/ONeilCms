using Microsoft.AspNetCore.Http;

namespace OnielCms.Core
{

	public static class ProcessorsShared
	{

		private static NullResult InnerNoResult = new NullResult();

		public static readonly ValueTask EmptyValueTask = new();

		public static readonly ValueTask<IResult> NoResultTask = new (InnerNoResult);

		public static readonly IResult NoResult = InnerNoResult;

		public static readonly ValueTask<IResult> Status401Task = new(Results.StatusCode(401));

	}

}

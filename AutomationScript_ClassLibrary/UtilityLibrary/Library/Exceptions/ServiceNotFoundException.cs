namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ServiceNotFoundException : Exception
	{
		public ServiceNotFoundException()
		{
		}

		public ServiceNotFoundException(string message) : base(message)
		{
		}

		public ServiceNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ServiceNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
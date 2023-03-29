namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ServiceNotDeletedException : Exception
	{
		public ServiceNotDeletedException()
		{
		}

		public ServiceNotDeletedException(string message) : base(message)
		{
		}

		public ServiceNotDeletedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ServiceNotDeletedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
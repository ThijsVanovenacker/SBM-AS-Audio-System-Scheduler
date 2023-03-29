namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ServiceNotAddedException : Exception
	{
		public ServiceNotAddedException()
		{
		}

		public ServiceNotAddedException(string message) : base(message)
		{
		}

		public ServiceNotAddedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ServiceNotAddedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
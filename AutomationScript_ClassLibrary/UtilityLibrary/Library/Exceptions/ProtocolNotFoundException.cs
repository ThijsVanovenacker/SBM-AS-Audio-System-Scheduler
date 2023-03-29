namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ProtocolNotFoundException : Exception
	{
		public ProtocolNotFoundException()
		{
		}

		public ProtocolNotFoundException(string message) : base(message)
		{
		}

		public ProtocolNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ProtocolNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
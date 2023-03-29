namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class CancelUserInteractionException : Exception
	{
		public CancelUserInteractionException()
		{
		}

		public CancelUserInteractionException(string message) : base(message)
		{
		}

		public CancelUserInteractionException(string message, Exception inner) : base(message, inner)
		{
		}

		protected CancelUserInteractionException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
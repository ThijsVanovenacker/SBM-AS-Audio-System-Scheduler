namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ElementNotStartedException : Exception
	{
		public ElementNotStartedException()
		{
		}

		public ElementNotStartedException(string message) : base(message)
		{
		}

		public ElementNotStartedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ElementNotStartedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
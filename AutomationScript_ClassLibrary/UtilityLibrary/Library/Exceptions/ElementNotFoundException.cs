namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ElementNotFoundException : Exception
	{
		public ElementNotFoundException()
		{
		}

		public ElementNotFoundException(string message) : base(message)
		{
		}

		public ElementNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ElementNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
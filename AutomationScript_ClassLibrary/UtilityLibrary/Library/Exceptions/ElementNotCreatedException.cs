namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ElementNotCreatedException : Exception
	{
		public ElementNotCreatedException()
		{
		}

		public ElementNotCreatedException(string message) : base(message)
		{
		}

		public ElementNotCreatedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ElementNotCreatedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
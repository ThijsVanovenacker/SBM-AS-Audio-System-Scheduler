using System;

namespace Skyline.DataMiner.Library.Exceptions
{
	[Serializable]
	public class PropertyUpdateException : Exception
	{
		public PropertyUpdateException()
		{
		}

		public PropertyUpdateException(string message) : base(message)
		{
		}

		public PropertyUpdateException(string message, Exception inner) : base(message, inner)
		{
		}

		protected PropertyUpdateException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
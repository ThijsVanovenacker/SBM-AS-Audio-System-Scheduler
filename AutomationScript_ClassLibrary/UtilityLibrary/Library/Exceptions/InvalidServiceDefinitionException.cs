namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class InvalidServiceDefinitionException : Exception
	{
		public InvalidServiceDefinitionException()
		{
		}

		public InvalidServiceDefinitionException(string message) : base(message)
		{
		}

		public InvalidServiceDefinitionException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvalidServiceDefinitionException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
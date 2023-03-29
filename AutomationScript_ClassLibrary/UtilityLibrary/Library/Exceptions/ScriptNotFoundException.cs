namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class ScriptNotFoundException : Exception
	{
		public ScriptNotFoundException()
		{
		}

		public ScriptNotFoundException(string message) : base(message)
		{
		}

		public ScriptNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ScriptNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
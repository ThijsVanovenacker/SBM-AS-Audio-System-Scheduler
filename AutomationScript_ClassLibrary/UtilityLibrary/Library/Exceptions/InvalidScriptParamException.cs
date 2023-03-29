namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	[Serializable]
	public class InvalidScriptParamException : Exception
	{
		public InvalidScriptParamException()
		{
		}

		public InvalidScriptParamException(string message)
			: base(message)
		{
		}

		public InvalidScriptParamException(string message, string paramName)
			: base(string.Format("{0}\r\nParameter: {1}", message, paramName))
		{
		}

		public InvalidScriptParamException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public InvalidScriptParamException(string message, int paramId)
			: base(string.Format("{0}\r\nParameter: {1}", message, paramId))
		{
		}

		protected InvalidScriptParamException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
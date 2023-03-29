namespace Skyline.DataMiner.Library.Exceptions
{
	[System.Serializable]
	public class UserNotAllowedException : System.Exception
	{
		public UserNotAllowedException()
		{
		}

		public UserNotAllowedException(string message) : base(message)
		{
		}

		public UserNotAllowedException(string message, System.Exception inner) : base(message, inner)
		{
		}

		protected UserNotAllowedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
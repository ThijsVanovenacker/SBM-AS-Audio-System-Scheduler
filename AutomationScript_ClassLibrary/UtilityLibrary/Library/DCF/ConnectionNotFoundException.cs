namespace Skyline.DataMiner.Library.DCF
{
	using System;

	/// <summary>
	/// Represents the errors that occurs when a DCF Connection no longer exists.
	/// </summary>
	[Serializable]
	public class ConnectionNotFoundException : Exception
	{
		public ConnectionNotFoundException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionNotFoundException"/> class with a specified error message.
		/// </summary>
		/// <param name="connectionID">ID of the connection.</param>
		public ConnectionNotFoundException(int connectionID)
			  : base(string.Format("Connection {0} doesn't exist", connectionID))
		{
		}

		public ConnectionNotFoundException(string message) : base(message)
		{
		}

		public ConnectionNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionNotFoundException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The<see cref="StreamingContext"/> that contains contextual information.</param>
		/// <exception cref="ArgumentNullException">The info parameter is null.</exception>
		/// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/> is zero (0).</exception>
		protected ConnectionNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			  : base(info, context)
		{
		}
	}
}
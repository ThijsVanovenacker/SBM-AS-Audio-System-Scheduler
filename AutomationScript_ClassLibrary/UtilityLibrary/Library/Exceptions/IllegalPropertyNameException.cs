using System;

namespace Skyline.DataMiner.Library.Exceptions
{
	/// <summary>
	/// Class for ServiceReservationInstanceNotFoundException definition.
	/// </summary>
	[Serializable]
	public class IllegalPropertyNameException : Exception
	{
		/// <summary>
		/// Default Constructor for IllegalPropertyNameException.
		/// </summary>
		public IllegalPropertyNameException()
		{
		}

		/// <summary>
		/// Constructor with support for message string.
		/// </summary>
		/// <param name="message">Message string.</param>
		public IllegalPropertyNameException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor with default message based on the reservation ID and also Inner Expections.
		/// </summary>
		/// <param name="message">Message string.</param>
		/// <param name="innerException">InnerException Object Reference.</param>
		public IllegalPropertyNameException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Support for Serializable attribute.
		/// </summary>
		/// <param name="info">SerializationInfo object.</param>
		/// <param name="context">Streaming Context object.</param>
		protected IllegalPropertyNameException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
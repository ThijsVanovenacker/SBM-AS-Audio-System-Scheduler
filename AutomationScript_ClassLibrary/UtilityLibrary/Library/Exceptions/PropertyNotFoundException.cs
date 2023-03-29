namespace Skyline.DataMiner.Library.Exceptions
{
	using System;

	/// <summary>
	/// Represents the error that occurs when a property doesn't exist.
	/// </summary>
	[Serializable]
	public class PropertyNotFoundException : Exception
	{
		public PropertyNotFoundException()
		{
		}

		public PropertyNotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class with a specified error message.
		/// </summary>
		/// <param name="objectID">ID of the object where the property should belong.</param>
		/// <param name="propertyName">Name of the property.</param>
		public PropertyNotFoundException(string objectID, string propertyName)
			  : base(string.Format("Property {0} on {1} doesn't exist", propertyName, objectID))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class with a specified error message.
		/// </summary>
		/// <param name="objectID">ID of the object where the property should belong.</param>
		/// <param name="propertyID">ID of the property.</param>
		public PropertyNotFoundException(int objectID, int propertyID)
			  : base(string.Format("Property with ID {0} on {1} doesn't exist", propertyID, objectID))
		{
		}

		public PropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The<see cref="StreamingContext"/> that contains contextual information.</param>
		/// <exception cref="ArgumentNullException">The info parameter is null.</exception>
		/// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/> is zero (0).</exception>
		protected PropertyNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			  : base(info, context)
		{
		}
	}
}
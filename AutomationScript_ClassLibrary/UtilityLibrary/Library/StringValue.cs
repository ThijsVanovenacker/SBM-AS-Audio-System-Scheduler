namespace Skyline.DataMiner.Library
{
	using System;

	/// <summary>
	/// String value for <see cref="Enum"/> members.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class StringValueAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StringValueAttribute"/> class.
		/// </summary>
		/// <param name="value">Value to be set.</param>
		public StringValueAttribute(string value)
		{
			this.Value = value;
		}

		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		public string Value { get; private set; }
	}
}
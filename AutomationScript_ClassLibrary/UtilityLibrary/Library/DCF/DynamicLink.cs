namespace Skyline.DataMiner.Library.DCF
{
	using System;

	/// <summary>
	/// Represents the value of a DCF Interface Dynamic Link column (ID: 65095).
	/// </summary>
	public class DynamicLink
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicLink"/> class.
		/// </summary>
		/// <param name="value">Value from column '[Interface Dynamic Link]'.</param>
		public DynamicLink(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			var parts = value.Split(';');

			if (parts.Length != 2)
			{
				return;
			}

			this.ParameterGroupId = Convert.ToInt32(parts[0]);
			this.TableKey = parts[1];
		}

		/// <summary>
		/// Gets the Id of the parameter group that exported the interface, if that was exported from a table. Otherwise will be null.
		/// </summary>
		public int? ParameterGroupId { get; private set; }

		/// <summary>
		/// Gets the Key of the table that exported the interface, if that was exported from a table. Otherwise will be null.
		/// </summary>
		public string TableKey { get; private set; }
	}
}
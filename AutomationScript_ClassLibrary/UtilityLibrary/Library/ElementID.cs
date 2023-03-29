namespace Skyline.DataMiner.Library
{
	using System;
	using System.Linq;

	/// <summary>
	/// Represents a DataMiner Element Id.
	/// </summary>
	public struct ElementID
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ElementID" /> struct.
		/// </summary>
		/// <param name="dmaId">32-bit integer with the DataMiner Agent Id.</param>
		/// <param name="elementId">32-bit integer with the Element Id.</param>
		public ElementID(int dmaId, int elementId)
			: this()
		{
			this.DmaId = (uint)dmaId;
			this.ElementId = (uint)elementId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ElementID" /> struct.
		/// </summary>
		/// <param name="dmaId">32-bit unsigned integer with the DataMiner Agent Id.</param>
		/// <param name="elementId">32-bit unsigned integer with the Element Id.</param>
		public ElementID(uint dmaId, uint elementId)
			: this()
		{
			this.DmaId = dmaId;
			this.ElementId = elementId;
		}

		/// <summary>
		/// Gets or sets the DataMiner Agent Id.
		/// </summary>
		public uint DmaId { get; set; }

		/// <summary>
		/// Gets or sets the Element Id.
		/// </summary>
		public uint ElementId { get; set; }

		/// <summary>
		/// Converts a string with format DmaId/ElementId to an ElementID object.
		/// </summary>
		/// <param name="source">String with format DmaId/ElementId.</param>
		/// <returns>An ElementID object with the parsed data.</returns>
		public static explicit operator ElementID(string source)
		{
			return Parse(source);
		}

		/// <summary>
		/// Converts an ElementID into a string with format DmaId/ElementId.
		/// </summary>
		/// <param name="source">ElementID object.</param>
		/// <returns>A string with format DmaId/ElementId.</returns>
		public static explicit operator string(ElementID source)
		{
			return source.ToString();
		}

		/// <summary>
		/// Converts a string with format DmaId/ElementId to an ElementID object.
		/// </summary>
		/// <param name="source">String with format DmaId/ElementId.</param>
		/// <returns>An ElementID object with the parsed data.</returns>
		/// <exception cref="ArgumentException">
		/// When the input string does not follow the format DmaId/ElementId.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
		public static ElementID Parse(string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (!source.Contains('/'))
			{
				throw new ArgumentException(string.Format("{0} does not follow the format DmaId/ElementId", source), "source");
			}

			var parts = source.Split('/');

			if (parts.Length != 2)
			{
				throw new ArgumentException(string.Format("{0} does not follow the format DmaId/ElementId", source), "source");
			}

			return new ElementID(Convert.ToUInt32(parts[0]), Convert.ToUInt32(parts[1]));
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the value of this instance.</returns>
		public override string ToString()
		{
			return string.Format("{0}/{1}", this.DmaId, this.ElementId);
		}
	}
}
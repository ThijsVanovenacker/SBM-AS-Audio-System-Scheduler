namespace Skyline.DataMiner.Library.Protocol
{
	/// <summary>
	/// Class used to make set parameters to SLProtocol.
	/// </summary>
	public class SetParameterRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SetParameterRequest" /> class.
		/// </summary>
		/// <param name="id">ID of the parameter to set.</param>
		/// <param name="value">Value to set on the parameter.</param>
		public SetParameterRequest(int id, object value)
		{
			this.Id = id;
			this.Value = value;
		}

		/// <summary>
		/// Gets the ID of the parameter to set the value.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Gets the value to set on the parameter.
		/// </summary>
		public object Value { get; private set; }
	}
}
namespace Skyline.DataMiner.Library.Snmp
{
	/// <summary>
	/// Represents the possible SNMP RowStatus options.
	/// </summary>
	public enum RowStatus
	{
		Active = 1,
		NotInService = 2,
		NotReady = 3,
		CreateAndGo = 4,
		CreateAndWait = 5,
		Destroy = 6
	}

	/// <summary>
	/// Defines the administrative state options of an interface.
	/// </summary>
	public enum AdminStatus
	{
		/// <summary>
		/// The interface shall be enabled.
		/// </summary>
		Up = 1,

		/// <summary>
		/// The interface shall be disabled.
		/// </summary>
		Down = 2
	}

	/// <summary>
	/// Represents a boolean value as defined in RFC2579.
	/// </summary>
	public enum TruthValue
	{
		True = 1,
		False = 2
	}
}
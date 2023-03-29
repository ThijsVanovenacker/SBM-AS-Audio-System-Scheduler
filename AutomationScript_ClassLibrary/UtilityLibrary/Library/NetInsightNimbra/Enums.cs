namespace Skyline.DataMiner.Library.NetInsightNimbra
{
	/// <summary>
	/// Defines the Acceptable Frame Type options.
	/// </summary>
	public enum AcceptableFrameType
	{
		/// <summary>
		/// All frames, regardless of whether they have a VLAN tag or not, shall be accepted.
		/// </summary>
		All = 1,

		/// <summary>
		/// Only frames that contain a non-zero VLAN id in the VLAN tag shall be accepted.
		/// </summary>
		VlanTagged = 2,

		/// <summary>
		/// Only frames without a VLAN tag or with VLAN id zero shall be accepted.
		/// </summary>
		Untagged = 3
	}

	public enum DcpVersion
	{
		Auto = 1,
		Dcp2 = 2,
		Dcp3 = 3
	}

	public enum DestinationMode
	{
		Unicast = 1,
		Multicast = 2,
		Unspecified = 3
	}

	public enum ExpectChannelStatusOptions
	{
		ExpectedToTerminate = 1,
		NotExceptedToTerminate = 2
	}

	/// <summary>
	/// Defines the options on how the forwarding function shall handle unicast frames when it does not know what interface to use for reaching a destination.
	/// </summary>
	public enum MacMode
	{
		/// <summary>
		/// Equivalent to <see cref="Mac" /> if the forwarding function is connected to at least three interfaces, otherwise equivalent to <see cref="NoMac" />.
		/// </summary>
		Auto = 1,

		/// <summary>
		///  The forwarding function will learn what interfaces to use for a destination.
		///  This is done by looking at the source MAC address on received frames.
		///  When the forwarding function has learned what interface to use, it will forward the frame on that interface only.
		/// </summary>
		Mac = 2,

		/// <summary>
		/// The forwarding function will forward the frame on all its interfaces.
		/// </summary>
		NoMac = 3
	}

	/// <summary>
	/// Defines the Ethernet Interface Priority Mode options.
	/// </summary>
	public enum PriorityMode
	{
		/// <summary>
		/// The user bits are used.
		/// </summary>
		Ethernet = 1,

		/// <summary>
		/// The differential service code point (DSCP) field of the IP header is translated using the ethFwdDiffservTable into a flow group.
		/// </summary>
		DifferentialService = 2
	}

	public enum ProtectionType
	{
		None = 0,
		Hitless = 1,
		Standby = 2
	}

	/// <summary>
	/// Defines the different protection expected options.
	/// </summary>
	public enum ProtectionTypeExpected
	{
		Unavailable = 1,
		Unprotected = 2,
		StandbyProtected = 3,
		HitlessCapable = 4,
		HitlessProtected = 5
	}

	public enum ReestablishMethod
	{
		Exponential = 1
	}

	/// <summary>
	/// Defines the possible types of Source Routes.
	/// </summary>
	public enum RouteType
	{
		/// <summary>
		/// The channel must only pass the hops defined in the source route.
		/// </summary>
		Strict = 1,

		/// <summary>
		/// The channel must pass all the hops defined in the source route.
		/// </summary>
		Loose = 2
	}

	/// <summary>
	/// Defines the available options to handle incoming spanning tree messages.
	/// </summary>
	public enum SpanningTree
	{
		/// <summary>
		/// If the device does not implement the spanning-tree protocol, or if exactly two interfaces are connected to the forwarding function,
		/// then it will act as if <see cref="Forward" /> has been specified.
		/// Otherwise, it will act as if <see cref="Process" /> has been specified.
		/// </summary>
		Auto = 1,

		/// <summary>
		/// Received spanning-tree messages are broadcasted to all interfaces except the interface where the message was received. No further processing is done.
		/// </summary>
		Forward = 2,

		/// <summary>
		/// Received spanning-tree messages are dropped without any further processing.
		/// </summary>
		Drop = 3,

		/// <summary>
		/// Received spanning-tree messages are processed according to the spanning-tree protocol.
		/// </summary>
		Process = 4
	}

	public enum SuppressAlarm
	{
		NotSupported = 0,
		None = 1,
		Warning = 2,
		All = 3
	}

	/// <summary>
	/// Defines the transmitted frames options.
	/// </summary>
	public enum TxFrameType
	{
		/// <summary>
		///  VLAN tags on forwarded frames are removed according to setting of ethFwdVLANMode. 'VLAN Mode (ETH Fwd Func Table)'.
		/// </summary>
		VlanTagged = 2,

		/// <summary>
		/// VLAN tags on forwarded frames are removed according to setting of ethFwdVLANMode. 'VLAN Mode (ETH Fwd Func Table)'.
		/// </summary>
		Untagged = 3,

		/// <summary>
		/// VLAN id is transported together with frame, but the frame is in itself not tagged.
		/// </summary>
		Legacy = 4
	}

	/// <summary>
	/// Defines the options that can be performed on received frames.
	/// </summary>
	public enum VlanMode
	{
		/// <summary>
		/// Ignore VLAN tags on received frames. VLAN tags are not added nor removed from the frames.
		/// If a frame has a VLAN tag, then it is retained. If a frame does not have a VLAN tag, then no tag is added.
		/// </summary>
		Transparent = 1,

		/// <summary>
		/// VLAN-handling in accordance with IEEE 802.1Q.
		/// Frames are forwarded and filtered according to customer VLAN tag.
		/// Frames that are not tagged are tagged with default VLAN tag as configured on the interface.
		/// </summary>
		Customer = 2,

		/// <summary>
		/// VLAN-handling in accordance with IEEE 802.1ad.
		/// Frames are forwarded and filtered according to service VLAN tag.Any customer VLAN is ignored.
		/// Frames that are not tagged are tagged with default VLAN tag as configured on the interface.
		/// </summary>
		Provider = 3
	}
}
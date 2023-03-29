namespace Skyline.DataMiner.Library.NetInsightNimbra.Manager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Skyline.DataMiner.Library.Snmp;

	/// <summary>
	/// Represents the request used to add a Destination.
	/// </summary>
	public class AddDestinationRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Configure Destination";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20112;
			}
		}

		/// <summary>
		/// Gets or sets the DSTI of the destination TTP.
		/// </summary>
		public int DestinationDSTI { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ElementID" /> of the destination node.
		/// </summary>
		public ElementID DestinationElementID { get; set; }

		/// <summary>
		/// Gets or sets the key of the destination. 'Originating TTP Destinations Table'.
		/// </summary>
		public string DestinationTableKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.DestinationTableKey,
				this.DestinationElementID,
				this.DestinationDSTI);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 6)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid ConfigureDestinationRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.DestinationTableKey = parts[3];
			this.DestinationElementID = (ElementID)parts[4];
			this.DestinationDSTI = Convert.ToInt32(parts[5]);
		}
	}

	/// <summary>
	/// Represents the response received when configuring an ITS Destination.
	/// </summary>
	public class AddDestinationResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the destination. 'Originating TTP Destinations Table'.
		/// </summary>
		public string DestinationTableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.DestinationTableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.DestinationTableKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to add a vlan membership.
	/// </summary>
	public class AddVlanMembershipRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Add Vlan Membership";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20370;
			}
		}

		/// <summary>
		/// Gets or sets a list with the vlans that should be added.
		/// </summary>
		public List<VlanConfig> Vlans { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			try
			{
				var other = command.DeserializeDataContractJsonObject<AddVlanMembershipRequest>();

				this.Id = other.Id;
				this.DmaId = other.DmaId;
				this.ElementId = other.ElementId;
				this.Vlans = other.Vlans;
			}
			catch (SerializationException)
			{
				// keeps backward compatibility
				var parts = command.Split(';');

				this.Vlans = new List<VlanConfig>();

				for (int i = 3; i < parts.Length; i += 4)
				{
					var vlan = new VlanConfig
					{
						InterfaceId = parts[i],
						CustomerId = Convert.ToInt32(parts[i + 1]),
						Purpose = parts[i + 2],
						VlanId = Convert.ToInt32(parts[i + 3])
					};

					this.Vlans.Add(vlan);
				}

				this.Id = Guid.Parse(parts[0]);
				this.DmaId = Convert.ToInt32(parts[1]);
				this.ElementId = Convert.ToInt32(parts[2]);
			}
		}

		/// <summary>
		/// Represents a VLAN Configuration.
		/// </summary>
		public struct VlanConfig
		{
			/// <summary>
			/// Gets or sets the VLAN Customer ID.
			/// </summary>
			public int CustomerId { get; set; }

			/// <summary>
			/// Gets or sets the ID of the interface where the VLAN should be set.
			/// </summary>
			public string InterfaceId { get; set; }

			/// <summary>
			/// Gets or sets a string representing the purpose of the VLAN. The string is for administrative purpose.
			/// </summary>
			public string Purpose { get; set; }

			/// <summary>
			/// Gets or sets the ID of the VLAN.
			/// </summary>
			public int VlanId { get; set; }
		}
	}

	/// <summary>
	/// Represents the response received when adding a vlan membership.
	/// </summary>
	public class AddVlanMembershipResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets a list with the keys of the vlans that were added.
		/// </summary>
		public List<string> Vlans { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			var other = response.DeserializeDataContractJsonObject<AddVlanMembershipResponse>();

			this.Success = other.Success;
			this.Vlans = other.Vlans;
		}
	}

	/// <summary>
	/// Represents the request used to configure a connection.
	/// </summary>
	public class ConfigureConectionRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets or sets the connections acceptable bit rate.
		/// </summary>
		public ulong AcceptableBitrate { get; set; }

		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Configure Connection";
			}
		}

		/// <summary>
		/// Gets or sets the channel precedence.
		/// A channel with precedence is established and torn down before any channel without precedence.
		/// </summary>
		public TruthValue ChannelPrecedence { get; set; }

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20110;
			}
		}

		/// <summary>
		/// Gets or sets a <see cref="TruthValue" /> describing if the connection shall be established with the defined parameters.
		/// </summary>
		public TruthValue ConnectionEstablished { get; set; }

		/// <summary>
		/// Gets or sets the key of the connection.
		/// </summary>
		public string ConnectionKey { get; set; }

		/// <summary>
		/// Gets or sets the connections DCP version.
		/// </summary>
		public DcpVersion DcpVersion { get; set; }

		/// <summary>
		/// Gets or sets the maximum time in milliseconds between two attempts to establish the channel.
		/// </summary>
		public int MaximumIntervalTime { get; set; }

		/// <summary>
		/// Gets or sets the minimum time in milliseconds between two attempts to establish the channel.
		/// </summary>
		public int MinimumIntervalTime { get; set; }

		/// <summary>
		/// Gets or sets the type of algorithm or method used to re-establish channels that for some reason fail to be established or is torn down.
		/// </summary>
		public ReestablishMethod ReestablishMethod { get; set; }

		/// <summary>
		/// Gets or sets the requested payload capacity in bits per second for the service using the channel.
		/// </summary>
		public ulong RequestedBitrate { get; set; }

		/// <summary>
		/// Gets or sets the generation of alarms.
		/// </summary>
		public string SuppressRouteAlarms { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.ConnectionKey,
				this.AcceptableBitrate,
				this.RequestedBitrate,
				(int)this.ReestablishMethod,
				this.MinimumIntervalTime,
				this.MaximumIntervalTime,
				(int)this.ConnectionEstablished,
				(int)this.ChannelPrecedence,
				this.SuppressRouteAlarms,
				(int)this.DcpVersion);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 12 && parts.Length != 13)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid ConfigureConectionRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.ConnectionKey = parts[3];
			this.AcceptableBitrate = Convert.ToUInt64(parts[4]);
			this.RequestedBitrate = Convert.ToUInt64(parts[5]);
			this.ReestablishMethod = (ReestablishMethod)Convert.ToInt32(parts[6]);
			this.MinimumIntervalTime = Convert.ToInt32(parts[7]);
			this.MaximumIntervalTime = Convert.ToInt32(parts[8]);
			this.ConnectionEstablished = (TruthValue)Convert.ToInt32(parts[9]);
			this.ChannelPrecedence = (TruthValue)Convert.ToInt32(parts[10]);
			this.SuppressRouteAlarms = parts[11];

			// Dcp Version, to be backwards compatible we need to check if the command has this parameter or not,
			// if not we will configure with Dcp2 which is the default one
			this.DcpVersion = parts.Length > 12 ? (DcpVersion)Convert.ToInt32(parts[12]) : DcpVersion.Dcp2;
		}
	}

	/// <summary>
	/// Represents the response received when configuring a connection.
	/// </summary>
	public class ConfigureConectionResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			// Manager should be updated check if the configurations were done correctly.
			// For now it just returns '-1'
			return "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			// Nothing to do for now, in the future we should at least check if the connection was set successfully
			this.Success = true;
		}
	}

	/// <summary>
	/// Represents the request used to configure a Destination.
	/// </summary>
	public class ConfigureDestinationRequest : AddDestinationRequest
	{
		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20114;
			}
		}
	}

	/// <summary>
	/// Represents the request used to configure the protection of an ITS TTP.
	/// </summary>
	public class ConfigureItsProtectionRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Configure ITS Protection";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20105;
			}
		}

		/// <summary>
		/// Gets or sets a value defining the protection type that shall be used.
		/// </summary>
		public ProtectionType ProtectionType { get; set; }

		/// <summary>
		/// Gets or sets the expected protection state.
		/// When the current protection state is lower than the expected protection state, an alarm is raised.
		/// The alarm is cleared when the expected protection state is equal or higher then the current protection state.
		/// </summary>
		public ProtectionTypeExpected ProtectionTypeExpected { get; set; }

		/// <summary>
		/// Gets or sets the key of the TTP where the Protection should be configured.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TtpKey, (int)this.ProtectionType, (int)this.ProtectionTypeExpected);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 6)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid ConfigureItsProtectionRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TtpKey = parts[3];
			this.ProtectionType = (ProtectionType)Convert.ToInt32(parts[4]);
			this.ProtectionTypeExpected = (ProtectionTypeExpected)Convert.ToInt32(parts[5]);
		}
	}

	/// <summary>
	/// Represents the response received when configuring the protection of an ITS TTP.
	/// </summary>
	public class ConfigureItsProtectionResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			// Nothing to do for now, in the future we should at least check if the connection was set successfully
			this.Success = true;
		}
	}

	/// <summary>
	/// Represents the request used to create an Ethernet Group.
	/// </summary>
	public class CreateEthernetGroupRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Ethernet Group";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20373;
			}
		}

		/// <summary>
		/// Gets or sets the Key of the Device. 'ETH Device Table'.
		/// </summary>
		public string EthDeviceKey { get; set; }

		/// <summary>
		/// Gets or sets the Ethernet Group expected channel status.
		/// </summary>
		public ExpectChannelStatusOptions ExpectChannelStatus { get; set; }

		/// <summary>
		/// Gets or sets the protection type.
		/// </summary>
		public ProtectionType HitlessProtection { get; set; }

		/// <summary>
		/// Gets or sets the protection type expected.
		/// </summary>
		public ProtectionTypeExpected ProtectionStatus { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.EthDeviceKey, (int)this.HitlessProtection, (int)this.ProtectionStatus, (int)this.ExpectChannelStatus);
		}

		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 7)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateEthernetGroupRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.EthDeviceKey = parts[3];
			this.HitlessProtection = (ProtectionType)Convert.ToInt32(parts[4]);
			this.ProtectionStatus = (ProtectionTypeExpected)Convert.ToInt32(parts[5]);
			this.ExpectChannelStatus = (ExpectChannelStatusOptions)Convert.ToInt32(parts[6]);
		}
	}

	/// <summary>
	/// Represents the response received when creating an Ethernet Group.
	/// </summary>
	public class CreateEthernetGroupResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the ID of the newly created Ethernet Group.
		/// </summary>
		public string EthernetGroupId
		{
			get
			{
				return this.EthernetGroupKey.Split('.')[1];
			}
		}

		/// <summary>
		/// Gets or sets the Key of the newly created Ethernet Group. 'Ethernet Interface Groups'.
		/// </summary>
		public string EthernetGroupKey { get; set; }

		public Guid Id { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.EthernetGroupKey != null ? string.Join(";", this.Id, this.EthernetGroupKey) : "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				var parts = response.Split(';');
				this.Id = Guid.Parse(parts[0]);
				this.EthernetGroupKey = parts[1];
			}
		}
	}

	/// <summary>
	/// Represents the request used to create an ETS TTP.
	/// </summary>
	public class CreateEtsTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets or sets an <see cref="AcceptableFrameType" /> to define which type of frames will be accepted by the TTP.
		/// </summary>
		public AcceptableFrameType AcceptableFrameType { get; set; }

		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create ETS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20300;
			}
		}

		/// <summary>
		/// Gets or sets an identification number representing the customer or user using the interface.
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets a value that specifies the Ethernet priority assigned to a frame that has arrived via this port, and that does not have a VLAN tag.
		/// </summary>
		public int DefaultEthPriority { get; set; }

		/// <summary>
		/// Gets or sets the frames that do have any priority information are assigned this traffic class.
		/// </summary>
		public int DefaultTrafficClass { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the ETS TTP shall establish unicast or multicast connection.
		/// </summary>
		public DestinationMode DestinationMode { get; set; }

		/// <summary>
		/// Gets or sets the key of the Ethernet physical port. 'ETH Device Table'.
		/// </summary>
		public string EthDeviceKey { get; set; }

		/// <summary>
		/// Gets or sets a values that controls whether a channel is expected to be terminated on this ETS TTP or not.
		/// This value can only be set if the <see cref="DestinationMode" /> is <see cref="DestinationMode.Multicast" />.
		/// </summary>
		public TruthValue ExpectedChannel { get; set; }

		/// <summary>
		/// Gets or sets a string that represents a flow group to traffic class map.
		/// </summary>
		public string FlowGroupMap { get; set; }

		/// <summary>
		/// Gets or sets the ID of the Forwarding Functions associated with this TTP. 'ETH Fwd Func Table'.
		/// </summary>
		public int ForwardFunctionId { get; set; }

		/// <summary>
		/// Gets or sets the key of the Interface Group associated with this TTP. Table 'Ethernet Interface Groups'.
		/// The value -1 denotes that the interface does not belong to an interface group.
		/// </summary>
		public int InterfaceGroup { get; set; }

		/// <summary>
		/// Gets or sets a value that specifies if MAC learning is enabled on the interface.
		/// </summary>
		public TruthValue Learning { get; set; }

		/// <summary>
		/// Gets or sets a values that selects the method to use when assigning a flow group to a frame that arrives via this interface.
		/// </summary>
		public PriorityMode PriorityMode { get; set; }

		/// <summary>
		/// Gets or sets a string representing the purpose of the interface. The string is for administrative purpose.
		/// </summary>
		public string Purpose { get; set; }

		/// <summary>
		/// Gets or sets a value that defines if degraded signal on the sink interface shall result in generation of AIS.
		/// </summary>
		public TruthValue SnkDegAis { get; set; }

		/// <summary>
		/// Gets or sets the number of consecutive bad seconds that are needed for generating degraded signal alarm and/or AIS.
		/// </summary>
		public uint SnkDegPeriod { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in number of dropped frames per second, for generating a degraded second.
		/// A degraded second is generated when the number of dropped frames are equal or larger than the this threshold.
		/// A value of 0 disables degraded seconds detection.
		/// </summary>
		public uint SnkDegThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in Mbps, that is needed for generating a minor alarm.
		/// </summary>
		public uint SnkMinorReducedBitrateHighThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, Bps, that is needed for generating a minor alarm.
		/// </summary>
		public uint SnkMinorReducedBitrateThreshold { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="TruthValue" /> that defines if Reduced Bit Rate signal shall result in generation of AIS.
		/// </summary>
		public TruthValue SnkReducedBitrateAis { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in Mbps, that is needed for generating a major alarm and/or AIS.
		/// </summary>
		public uint SnkReducedBitrateHighThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, Bps, that is needed for generating a major alarm and/or AIS.
		/// </summary>
		public uint SnkReducedBitrateThreshold { get; set; }

		/// <summary>
		/// Gets or sets the number of consecutive bad seconds that are needed for generating degraded signal alarm and/or AIS.
		/// </summary>
		public uint SrcDegPeriod { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in number of dropped frames per second, for generating a degraded second.
		/// A degraded second is generated when the number of dropped frames are equal or larger than the this threshold.
		/// A value of 0 disables degraded seconds detection.
		/// </summary>
		public uint SrcDegThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in Mbps, that is needed for generating a minor.
		/// </summary>
		public uint SrcMinorReducedBitrateHighThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, Bps that is needed for generating a minor alarm.
		/// </summary>
		public uint SrcMinorReducedBitrateThreshold { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="TruthValue" /> that defines if Reduced Bit Rate signal shall result in generation of AIS.
		/// </summary>
		public TruthValue SrcReducedBitrateAis { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in Mbps, that is needed for generating a major alarm and/or AIS.
		/// </summary>
		public uint SrcReducedBitrateHighThreshold { get; set; }

		/// <summary>
		/// Gets or sets the threshold, in Bps, that is needed for generating a major alarm and/or AIS.
		/// </summary>
		public uint SrcReducedBitrateThreshold { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="TxFrameType" /> that indicates the action that will be done in the forwarded frames.
		/// </summary>
		public TxFrameType TxFrameType { get; set; }

		/// <summary>
		/// Gets or sets the value that will be tagged if an untagged frame is received on this interface.
		/// </summary>
		public string Vlan { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.EthDeviceKey,
				(int)this.DestinationMode,
				string.Empty, // Not supported
				(int)this.ExpectedChannel,
				this.CustomerId,
				this.Purpose,
				this.ForwardFunctionId,
				(int)this.AcceptableFrameType,
				(int)this.TxFrameType,
				this.Vlan,
				this.DefaultEthPriority,
				(int)this.PriorityMode,
				this.DefaultTrafficClass,
				this.FlowGroupMap,
				(int)this.Learning,
				this.SrcDegThreshold,
				this.SnkDegThreshold,
				this.SrcDegPeriod,
				this.SnkDegPeriod,
				this.SrcReducedBitrateThreshold,
				this.SnkReducedBitrateThreshold,
				this.SrcReducedBitrateHighThreshold,
				this.SnkReducedBitrateHighThreshold,
				(int)this.SrcReducedBitrateAis,
				(int)this.SnkReducedBitrateAis,
				(int)this.SnkDegAis,
				this.SrcMinorReducedBitrateThreshold,
				this.SnkMinorReducedBitrateThreshold,
				this.SrcMinorReducedBitrateHighThreshold,
				this.SnkMinorReducedBitrateHighThreshold,
				this.InterfaceGroup);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 34)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateEtsTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.EthDeviceKey = parts[3];
			this.DestinationMode = (DestinationMode)Convert.ToInt32(parts[4]);
			this.ExpectedChannel = (TruthValue)Convert.ToInt32(parts[6]);
			this.CustomerId = Convert.ToInt32(parts[7]);
			this.Purpose = parts[8];
			this.ForwardFunctionId = Convert.ToInt32(parts[9]);
			this.AcceptableFrameType = (AcceptableFrameType)Convert.ToInt32(parts[10]);
			this.TxFrameType = (TxFrameType)Convert.ToInt32(parts[11]);
			this.Vlan = parts[12];
			this.DefaultEthPriority = Convert.ToInt32(parts[13]);
			this.PriorityMode = (PriorityMode)Convert.ToInt32(parts[14]);
			this.DefaultTrafficClass = Convert.ToInt32(parts[15]);
			this.FlowGroupMap = parts[16];
			this.Learning = (TruthValue)Convert.ToInt32(parts[17]);
			this.SrcDegThreshold = Convert.ToUInt32(parts[18]);
			this.SnkDegThreshold = Convert.ToUInt32(parts[19]);
			this.SrcDegPeriod = Convert.ToUInt32(parts[20]);
			this.SnkDegPeriod = Convert.ToUInt32(parts[21]);
			this.SrcReducedBitrateThreshold = Convert.ToUInt32(parts[22]);
			this.SnkReducedBitrateThreshold = Convert.ToUInt32(parts[23]);
			this.SrcReducedBitrateHighThreshold = Convert.ToUInt32(parts[24]);
			this.SnkReducedBitrateHighThreshold = Convert.ToUInt32(parts[25]);
			this.SrcReducedBitrateAis = (TruthValue)Convert.ToInt32(parts[26]);
			this.SnkReducedBitrateAis = (TruthValue)Convert.ToUInt32(parts[27]);
			this.SnkDegAis = (TruthValue)Convert.ToUInt32(parts[28]);
			this.SrcMinorReducedBitrateThreshold = Convert.ToUInt32(parts[29]);
			this.SnkMinorReducedBitrateThreshold = Convert.ToUInt32(parts[30]);
			this.SrcMinorReducedBitrateHighThreshold = Convert.ToUInt32(parts[31]);
			this.SnkMinorReducedBitrateHighThreshold = Convert.ToUInt32(parts[32]);
			this.InterfaceGroup = Convert.ToInt32(parts[33]);
		}
	}

	/// <summary>
	/// Represents the response received when creating an ETS TTP.
	/// </summary>
	public class CreateEtsTtpResponse : CreateTtpResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey != null ? string.Join(";", this.TtpKey, this.DSTI, this.ConnectionKey) : "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			string[] parts = response.Split(';');

			if (response == "-1" || parts.Length != 3)
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = parts[0];
				this.DSTI = Convert.ToInt32(parts[1]);
				this.ConnectionKey = parts[2];
			}
		}
	}

	/// <summary>
	/// Represents the request used to create a Forwarding Function.
	/// </summary>
	public class CreateForwardingFunctionRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Forwarding Function";
			}
		}

		/// <summary>
		/// Gets or sets the timeout period for aging out dynamically learned forwarding information.
		/// The value 0 means that aging is disabled and that the learned entries are not removed from the lookup table.
		/// </summary>
		public TimeSpan AgingTime { get; set; }

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20320;
			}
		}

		/// <summary>
		/// Gets or sets an identification number representing the customer or user using the interface.
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the key of the Ethernet physical port. 'ETH Device Table'.
		/// </summary>
		public string EthDeviceKey { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="TruthValue" /> that specifies how the forwarding function shall handle Jumbo frames.
		/// </summary>
		public TruthValue JumboFrames { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="MacMode" /> that specifies how the forwarding function shall handle unicast frames when it does not know what interface to use for reaching a destination.
		/// </summary>
		public MacMode MacMode { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="TruthValue" /> that defines if faults should be propagated to all interfaces belonging to this forwarding function.
		/// </summary>
		public TruthValue PropagateFaults { get; set; }

		/// <summary>
		/// Gets or sets a string representing the purpose of the interface. The string is for administrative purpose.
		/// </summary>
		public string Purpose { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="SpanningTree" /> that defines how spanning tree messages are handled.
		/// </summary>
		public SpanningTree SpanningTree { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="VlanMode" /> that defines what actions should be performed on received frames.
		/// </summary>
		public VlanMode VlanMode { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.EthDeviceKey,
				this.CustomerId,
				this.Purpose,
				(int)this.JumboFrames,
				(int)this.MacMode,
				(int)this.SpanningTree,
				(int)this.VlanMode,
				this.AgingTime.TotalSeconds,
				(int)this.PropagateFaults);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 12)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateForwardingFunctionRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.EthDeviceKey = parts[3];
			this.CustomerId = Convert.ToInt32(parts[4]);
			this.Purpose = parts[5];
			this.JumboFrames = (TruthValue)Convert.ToInt32(parts[6]);
			this.MacMode = (MacMode)Convert.ToInt32(parts[7]);
			this.SpanningTree = (SpanningTree)Convert.ToInt32(parts[8]);
			this.VlanMode = (VlanMode)Convert.ToInt32(parts[9]);
			this.AgingTime = TimeSpan.FromSeconds(Convert.ToInt32(parts[10]));
			this.PropagateFaults = (TruthValue)Convert.ToInt32(parts[11]);
		}
	}

	/// <summary>
	/// Represents the response received when creating a Forwarding Function.
	/// </summary>
	public class CreateForwardingFunctionResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the ID of the Forwarding Function. 'ethFwdIndex'.
		/// </summary>
		public int Id
		{
			get
			{
				return Convert.ToInt32(this.TableKey.Split('.')[1]);
			}
		}

		/// <summary>
		/// Gets or sets the key of the Forwarding Functions table. 'ETH Fwd Func Table', format 'ethDevIndex'.'ethFwdIndex'.
		/// </summary>
		public string TableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TableKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to create an ITS Sink TTP.
	/// </summary>
	public class CreateItsSinkTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Destination ITS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20101;
			}
		}

		/// <summary>
		/// Gets or sets an identification number representing the customer or user using the interface.
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the key of the ITS interface where this TTP will be created.
		/// </summary>
		public string InterfaceKey { get; set; }

		/// <summary>
		/// Gets or sets a string representing the purpose of the interface. The string is for administrative purpose.
		/// </summary>
		public string Purpose { get; set; }

		/// <summary>
		/// Gets or sets the alarm suppression mode.
		/// When alarms are suppressed, no alarms will be generated if a fault situation is detected on the trail termination point (TTP).
		/// </summary>
		public SuppressAlarm SuppressAlarm { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.InterfaceKey,
				this.CustomerId,
				this.Purpose,
				(int)this.SuppressAlarm);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 7)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateItsSinkTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.InterfaceKey = parts[3];
			this.CustomerId = Convert.ToInt32(parts[4]);
			this.Purpose = parts[5];
			this.SuppressAlarm = (SuppressAlarm)Convert.ToInt32(parts[6]);
		}
	}

	/// <summary>
	/// Represents the response received when creating an ITS Sink TTP.
	/// </summary>
	public class CreateItsSinkTtpResponse : CreateTtpResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey != null ? string.Join(";", this.TtpKey, this.DSTI) : "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			string[] parts = response.Split(';');

			if (response == "-1" || parts.Length != 2)
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = parts[0];
				this.DSTI = Convert.ToInt32(parts[1]);
			}
		}
	}

	/// <summary>
	/// Represents the request used to create an ITS Source TTP.
	/// </summary>
	public class CreateItsSourceTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Source ITS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20100;
			}
		}

		/// <summary>
		/// Gets or sets an identification number representing the customer or user using the interface.
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the key of the ITS interface where this TTP will be created.
		/// </summary>
		public string InterfaceKey { get; set; }

		/// <summary>
		/// Gets or sets a value that defines the mode of the connection.
		/// </summary>
		public DestinationMode Mode { get; set; }

		/// <summary>
		/// Gets or sets a string representing the purpose of the interface. The string is for administrative purpose.
		/// </summary>
		public string Purpose { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.InterfaceKey,
				this.CustomerId,
				this.Purpose,
				(int)this.Mode);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 7)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateItsSourceTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.InterfaceKey = parts[3];
			this.CustomerId = Convert.ToInt32(parts[4]);
			this.Purpose = parts[5];
			this.Mode = (DestinationMode)Convert.ToInt32(parts[6]);
		}
	}

	/// <summary>
	/// Represents the response received when creating an ITS Source TTP.
	/// </summary>
	public class CreateItsSourceTtpResponse : CreateTtpResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey != null ? string.Join(";", this.TtpKey, this.DSTI, this.ConnectionKey) : "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			string[] parts = response.Split(';');

			if (response == "-1" || parts.Length != 3)
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = parts[0];
				this.DSTI = Convert.ToInt32(parts[1]);
				this.ConnectionKey = parts[2];
			}
		}
	}

	/// <summary>
	/// Represents the request used to create a Source Route Hop.
	/// </summary>
	public class CreateRouteHopRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Source Route Hop";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20340;
			}
		}

		/// <summary>
		/// Gets or sets the identity of the board where the DTM interface is located that shall be used for leaving the node.
		/// A value of zero(0) means that any board may be used.
		/// </summary>
		public int NextHopInterfaceBoardId { get; set; }

		/// <summary>
		/// Gets or sets the identity of the port on the board specified in chmgrSourceRouteFirstIfCard that shall be used for leaving the node.
		/// A value of zero(0) means that any port may be used.
		/// </summary>
		public int NextHopInterfacePortId { get; set; }

		/// <summary>
		/// Gets or sets the name of the Node that will be used in the next hop.
		/// </summary>
		public string NextHopNodeName { get; set; }

		/// <summary>
		/// Gets or sets the key of the Source Route to where this hop belongs.
		/// </summary>
		public string SourceRouteKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.SourceRouteKey, this.NextHopNodeName, this.NextHopInterfaceBoardId, this.NextHopInterfacePortId);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 7)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateRouteHopRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.SourceRouteKey = parts[3];
			this.NextHopNodeName = parts[4];
			this.NextHopInterfaceBoardId = Convert.ToInt32(parts[5]);
			this.NextHopInterfacePortId = Convert.ToInt32(parts[6]);
		}
	}

	/// <summary>
	/// Represents the response received when creating a Source Route Hop.
	/// </summary>
	public class CreateRouteHopResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the newly create Source Route Hop key.
		/// </summary>
		public string RouteHopTableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.RouteHopTableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.RouteHopTableKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to create a Source Route.
	/// </summary>
	public class CreateSourceRouteRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Create Source Route";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20330;
			}
		}

		/// <summary>
		/// Gets or sets the identity of the board where the DTM interface is located that shall be used for leaving the node.
		/// A value of zero (0) means that any board may be used.
		/// </summary>
		public int InterfaceBoardId { get; set; }

		/// <summary>
		/// Gets or sets the identity of the port on the board specified in chmgrSourceRouteFirstIfBoard that shall be used for leaving the node.
		/// A value of zero(0) means that any port may be used.
		/// </summary>
		public int InterfacePortId { get; set; }

		/// <summary>
		/// Gets or sets the name of the Source Route.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the type of source route that shall be used.
		/// </summary>
		public RouteType RouteType { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.Name, (int)this.RouteType, this.InterfaceBoardId, this.InterfacePortId);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 7)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid CreateSourceRouteRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.Name = parts[3];
			this.RouteType = (RouteType)Convert.ToInt32(parts[4]);
			this.InterfaceBoardId = Convert.ToInt32(parts[5]);
			this.InterfacePortId = Convert.ToInt32(parts[6]);
		}
	}

	/// <summary>
	/// Represents the response received when creating a Source Route.
	/// </summary>
	public class CreateSourceRouteResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the newly created Source Route.
		/// </summary>
		public string RouteTableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.RouteTableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.RouteTableKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the response received when creating a TTP.
	/// </summary>
	public abstract class CreateTtpResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the reference to the corresponding connection (Originating TTP Configurations Table).
		/// </summary>
		public string ConnectionKey { get; set; }

		/// <summary>
		/// Gets or sets the DSTI (DTM Service Type Instance) for the source trail termination point.
		/// The value is used by the connection to refer to this trail termination point.
		/// The DSTI must be unique among the ITS source trail termination points.
		/// </summary>
		public int DSTI { get; set; }

		/// <summary>
		/// Gets or sets the table key of the newly create TTP.
		/// </summary>
		public string TtpKey { get; set; }
	}

	/// <summary>
	/// Represents the request used to remove a Destination.
	/// </summary>
	public class DeleteDestinationRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Remove Destination";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20113;
			}
		}

		/// <summary>
		/// Gets or sets the key of the destination. 'Originating TTP Destinations Table'.
		/// </summary>
		public string DestinationTableKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.DestinationTableKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteDestinationRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.DestinationTableKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting a destination.
	/// </summary>
	public class DeleteDestinationResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the destination. 'Originating TTP Destinations Table'.
		/// </summary>
		public string DestinationTableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.DestinationTableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.DestinationTableKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete an Ethernet Group.
	/// </summary>
	public class DeleteEthernetGroupRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Ethernet Group";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20376;
			}
		}

		/// <summary>
		/// Gets or sets the Key of the Ethernet Group that should be deleted.
		/// </summary>
		public string EthernetGroupKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.EthernetGroupKey);
		}

		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteEthernetGroupRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.EthernetGroupKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting an Ethernet Group.
	/// </summary>
	public class DeleteEthernetGroupResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the Key of the Ethernet Group that should be deleted.
		/// </summary>
		public string EthernetGroupKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.EthernetGroupKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.EthernetGroupKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete an ETS TTP.
	/// </summary>
	public class DeleteEtsTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete ETS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20302;
			}
		}

		/// <summary>
		/// Gets or sets the Key of the TTP.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TtpKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteEtsTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TtpKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting an ETS TTP.
	/// </summary>
	public class DeleteEtsTtpResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the Key of the TTP.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete a Forwarding Function.
	/// </summary>
	public class DeleteForwardingFunctionRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Forwarding Function";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20321;
			}
		}

		/// <summary>
		/// Gets or sets the Key of the Forwarding Function that should be deleted.
		/// </summary>
		public string ForwardingFunctionKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.ForwardingFunctionKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteForwardingFunctionRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.ForwardingFunctionKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting a Forwarding Function.
	/// </summary>
	public class DeleteForwardingFunctionResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the Key of the Forwarding Function.
		/// </summary>
		public string ForwardingFunctionKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.ForwardingFunctionKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.ForwardingFunctionKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete an ITS Sink TTP.
	/// </summary>
	public class DeleteItsSinkTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Destination ITS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20103;
			}
		}

		/// <summary>
		/// Gets or sets the key of the TTP that should be deleted.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TtpKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteItsSinkTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TtpKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting an ITS Sink TTP.
	/// </summary>
	public class DeleteItsSinkTtpResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the Sink TTP.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete an ITS Source TTP.
	/// </summary>
	public class DeleteItsSourceTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Source ITS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20102;
			}
		}

		/// <summary>
		/// Gets or sets the key of the TTP that should be deleted.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TtpKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteItsSourceTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TtpKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting an ITS Source TTP.
	/// </summary>
	public class DeleteItsSourceTtpResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the Sink TTP.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TtpKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.TtpKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to remove a route hop.
	/// </summary>
	public class DeleteRouteHopRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Remove Route Hop";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20341;
			}
		}

		/// <summary>
		/// Gets or sets the key of the route hop. 'Source Route Hop Table'.
		/// </summary>
		public string RouteHopKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(
				";",
				this.Id,
				this.DmaId,
				this.ElementId,
				this.RouteHopKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteRouteHopRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.RouteHopKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting a route hop.
	/// </summary>
	public class DeleteRouteHopResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the route hop. 'Source Route Hop Table'.
		/// </summary>
		public string RouteHopKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.RouteHopKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.RouteHopKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete a Source Route.
	/// </summary>
	public class DeleteSourceRouteRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Source Route";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20331;
			}
		}

		/// <summary>
		/// Gets or sets the key of the Source Route that should be deleted.
		/// </summary>
		public string RouteKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.RouteKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 4)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid DeleteSourceRouteRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.RouteKey = parts[3];
		}
	}

	/// <summary>
	/// Represents the response received when deleting a Source Route.
	/// </summary>
	public class DeleteSourceRouteResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the Source Route TTP.
		/// </summary>
		public string RouteKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.RouteKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}
			else
			{
				this.Success = true;
				this.RouteKey = response;
			}
		}
	}

	/// <summary>
	/// Represents the request used to delete a VLAN.
	/// </summary>
	public class DeleteVlanRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Delete Vlan Membership";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20378;
			}
		}

		/// <summary>
		/// Gets or sets a list with the keys of the vlans that should be deleted.
		/// </summary>
		public List<string> Vlans { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			var other = command.DeserializeDataContractJsonObject<DeleteVlanRequest>();

			this.Id = other.Id;
			this.DmaId = other.DmaId;
			this.ElementId = other.ElementId;
			this.Vlans = other.Vlans;
		}
	}

	/// <summary>
	/// Represents the response received when deleting a VLAN.
	/// </summary>
	public class DeleteVlanResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets a list with the keys of the VLANs that were deleted.
		/// </summary>
		public List<string> Vlans { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			var other = response.DeserializeDataContractJsonObject<DeleteVlanResponse>();

			this.Success = other.Success;
			this.Vlans = other.Vlans;
		}
	}

	/// <summary>
	/// Represents a generic request that can be sent 'NetInsight Nimbra Application Manager'.
	/// </summary>
	public abstract class NetInsightNimbraRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NetInsightNimbraRequest" /> class.
		/// </summary>
		protected NetInsightNimbraRequest()
		{
			this.Id = Guid.NewGuid();
		}

		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public abstract string Action { get; }

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public abstract int CommandParamId { get; }

		/// <summary>
		/// Gets or sets the DataMiner Agent ID where the element, to where the request is destined, is located.
		/// </summary>
		public int DmaId { get; set; }

		/// <summary>
		/// Gets or sets the ID of the Element to where the request is destined.
		/// </summary>
		public int ElementId { get; set; }

		/// <summary>
		/// Gets or sets an unique identifier for the request.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public abstract string GetCommand();

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public abstract void ParseStringCommand(string command);
	}

	/// <summary>
	/// Represents a generic response received from 'NetInsight Nimbra Application Manager'.
	/// </summary>
	public abstract class NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets a value indicating whether the operation was successful or not.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public abstract string GetResponse();

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public abstract void ParseResponse(string response);
	}

	/// <summary>
	/// Represents the request used to refresh the Services Table.
	/// </summary>
	public class RefreshServiceTableRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Refresh Service Tables";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20150;
			}
		}

		/// <summary>
		/// Gets or sets a list of Destinations nodes where the Services Table should be refreshed.
		/// </summary>
		public List<ElementID> DestinationNodes { get; set; }

		/// <summary>
		/// Gets or sets a list of Source nodes where the Services Table should be refreshed.
		/// </summary>
		public List<ElementID> SourceNodes { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, string.Join("|", this.SourceNodes), string.Join("|", this.DestinationNodes));
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 3)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid RefreshServiceTableRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.SourceNodes = !string.IsNullOrWhiteSpace(parts[1])
							? new List<ElementID>(parts[1].Split('|').Select(x => (ElementID)x))
							: new List<ElementID>();
			this.DestinationNodes = !string.IsNullOrWhiteSpace(parts[2])
								? new List<ElementID>(parts[2].Split('|').Select(x => (ElementID)x))
								: new List<ElementID>();
		}
	}

	/// <summary>
	/// Represents the response received when refreshing the Services Table.
	/// </summary>
	public class RefreshServiceTableResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			this.Success = response == "-1";
		}
	}

	/// <summary>
	/// Represents the request used to set the Ethernet Group on an ETS TTP.
	/// </summary>
	public class SetEthernetGroupOnEtsTtpRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Point ETS TTPs to Ethernet Group";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20374;
			}
		}

		/// <summary>
		/// Gets or sets the ID of Ethernet Group that should be assigned to the given TTPs.
		/// </summary>
		public string EthernetGroupId { get; set; }

		/// <summary>
		/// Gets or sets the key of the main TTP.
		/// </summary>
		public string MainTtpKey { get; set; }

		/// <summary>
		/// Gets or sets the key of the protection TTP.
		/// </summary>
		public string ProtectionTtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.EthernetGroupId, this.MainTtpKey, this.ProtectionTtpKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 6)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid SetEthernetGroupOnEtsTtpRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.EthernetGroupId = parts[3];
			this.MainTtpKey = parts[4];
			this.ProtectionTtpKey = parts[5];
		}
	}

	/// <summary>
	/// Represents the response received when setting the Ethernet Group on an ETS TTP.
	/// </summary>
	public class SetEthernetGroupOnEtsTtpResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			this.Success = response == "-1";
		}
	}

	/// <summary>
	/// Represents the request used to change the Admin State of an ETS TTP.
	/// </summary>
	public class SetEtsTtpAdminStateRequest : SetTtpAdminStateRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Enable ETS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20304;
			}
		}
	}

	/// <summary>
	/// Represents the request used to set a Forwarding Function on an Ethernet Interface.
	/// </summary>
	public class SetForwardingFunctionOnEthInterfaceRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Set Forwarding Function on Ethernet Interface";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20322;
			}
		}

		/// <summary>
		/// Gets or sets the key of the Ethernet Interface where the Forwarding Function should be set.
		/// </summary>
		public string EthInterfaceKey { get; set; }

		/// <summary>
		/// Gets or sets the ID of the Forwarding Function to set on the given Ethernet Interface.
		/// </summary>
		public int ForwardingFunctionId { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			var other = command.DeserializeDataContractJsonObject<SetForwardingFunctionOnEthInterfaceRequest>();

			this.Id = other.Id;
			this.DmaId = other.DmaId;
			this.ElementId = other.ElementId;
			this.EthInterfaceKey = other.EthInterfaceKey;
			this.ForwardingFunctionId = other.ForwardingFunctionId;
		}
	}

	/// <summary>
	/// Represents the response received when setting the Forwarding Function on an Ethernet Interface.
	/// </summary>
	public class SetForwardingFunctionOnEthInterfaceResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			var other = response.DeserializeDataContractJsonObject<SetForwardingFunctionOnEthInterfaceResponse>();

			this.Success = other.Success;
		}
	}

	/// <summary>
	/// Represents the request used to set the Purpose on an Ethernet Group.
	/// </summary>
	public class SetPurposeOnEthernetGroupRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Set Purpose on Ethernet Group";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20379;
			}
		}

		/// <summary>
		/// Gets or sets the Key of the Ethernet Group where the Purpose should be set.
		/// </summary>
		public string EthernetGroupKey { get; set; }

		/// <summary>
		/// Gets or sets a string representing the purpose of the Ethernet Group. The string is for administrative purpose.
		/// </summary>
		public string Purpose { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var other = command.DeserializeDataContractJsonObject<SetPurposeOnEthernetGroupRequest>();
			this.Id = other.Id;
			this.DmaId = other.DmaId;
			this.ElementId = other.ElementId;
			this.EthernetGroupKey = other.EthernetGroupKey;
			this.Purpose = other.Purpose;
		}
	}

	/// <summary>
	/// Represents the response received when setting the Purpose on an Ethernet Group.
	/// </summary>
	public class SetPurposeOnEthernetGroupResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.SerializeDataContractJsonObject();
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			var other = response.DeserializeDataContractJsonObject<SetPurposeOnEthernetGroupResponse>();

			this.Success = other.Success;
		}
	}

	/// <summary>
	/// Represents the request used to change the Admin State of an ITS Source TTP.
	/// </summary>
	public class SetSourceItsTtpAdminStateRequest : SetTtpAdminStateRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Enable Source ITS TTP";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20104;
			}
		}
	}

	/// <summary>
	/// Represents the request used to assign a source route to a connection.
	/// </summary>
	public class SetSourceRouteRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets the action that this request performs.
		/// </summary>
		public override string Action
		{
			get
			{
				return "Set Source Route";
			}
		}

		/// <summary>
		/// Gets the ID of the parameter to where the request should be sent.
		/// </summary>
		public override int CommandParamId
		{
			get
			{
				return 20350;
			}
		}

		/// <summary>
		/// Gets or sets the key of the Source Route that should be assigned to the connection.
		/// </summary>
		public string RouteKey { get; set; }

		/// <summary>
		/// Gets or sets the key of the 'Chmgr ODescr Channel Table'.
		/// </summary>
		public string TableKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TableKey, this.RouteKey);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 5)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid SetTtpAdminStateRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TableKey = parts[3];
			this.RouteKey = parts[4];
		}
	}

	/// <summary>
	/// Represents the response received when assigning a source route to a connection.
	/// </summary>
	public class SetSourceRouteResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets or sets the key of the 'Chmgr ODescr Channel Table'.
		/// </summary>
		public string TableKey { get; set; }

		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return this.TableKey ?? "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			if (response == "-1")
			{
				this.Success = false;
			}

			this.TableKey = response;
		}
	}

	/// <summary>
	/// Represents a generic request used to change the Admin State of a TTP.
	/// </summary>
	public abstract class SetTtpAdminStateRequest : NetInsightNimbraRequest
	{
		/// <summary>
		/// Gets or sets the Admin state that should be set.
		/// </summary>
		public AdminStatus AdminStatus { get; set; }

		/// <summary>
		/// Gets or sets the key of the TTP where the Admin state set should be performed.
		/// </summary>
		public string TtpKey { get; set; }

		/// <summary>
		/// Gets the command that should be sent to 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <returns>A <see cref="string" /> with the command that should be sent to 'NetInsight Nimbra Application Manager'.</returns>
		public override string GetCommand()
		{
			return string.Join(";", this.Id, this.DmaId, this.ElementId, this.TtpKey, (int)this.AdminStatus);
		}

		/// <summary>
		/// Fills the current instance with the raw data present in <paramref name="command" />.
		/// This method should parse the data generated in method <see cref="GetCommand" />.
		/// </summary>
		/// <param name="command"><see cref="string" /> with the raw command data.</param>
		public override void ParseStringCommand(string command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			var parts = command.Split(';');

			if (parts.Length != 5)
			{
				throw new InvalidCommandException(string.Format("{0} is not a valid SetTtpAdminStateRequest command", command));
			}

			this.Id = Guid.Parse(parts[0]);
			this.DmaId = Convert.ToInt32(parts[1]);
			this.ElementId = Convert.ToInt32(parts[2]);
			this.TtpKey = parts[3];
			this.AdminStatus = (AdminStatus)Convert.ToInt32(parts[4]);
		}
	}

	/// <summary>
	/// Represents the response received when changing the Admin State of an  TTP.
	/// </summary>
	public class SetTtpAdminStateResponse : NetInsightNimbraResponse
	{
		/// <summary>
		/// Gets the response that should be sent to the caller.
		/// </summary>
		/// <returns>A <see cref="string" /> with the response that should be sent to caller.</returns>
		public override string GetResponse()
		{
			return "-1";
		}

		/// <summary>
		/// Parses the response received from 'NetInsight Nimbra Application Manager'.
		/// </summary>
		/// <param name="response">A <see cref="string" /> with the raw response received from 'NetInsight Nimbra Application Manager'.</param>
		public override void ParseResponse(string response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}

			this.Success = response == "-1";
		}
	}
}
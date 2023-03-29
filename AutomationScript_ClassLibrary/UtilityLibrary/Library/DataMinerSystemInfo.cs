namespace Skyline.DataMiner.Library
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Interop.SLDms;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.Filters;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	/// <summary>
	/// Contains methods to get different information from a DataMiner System.
	/// </summary>
	public static class DataMinerSystemInfo
	{
		/// <summary>
		/// Generates an Information Event.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public static void GenerateInformation(string message)
		{
			try
			{
				var request = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message)
				{
					Status = GenerateAlarmMessage.AlarmStatus.Cleared
				};

				Engine.SLNet.SendSingleResponseMessage(request);
			}
			catch (Exception)
			{
				// ignore
			}
		}

		/// <summary>
		/// Gets All Protocols from a DMS.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> of <see cref="GetProtocolsResponseMessage"/> with all Protocols on the DMS.
		/// </returns>
		public static IEnumerable<GetProtocolsResponseMessage> GetAllProtocols()
		{
			var getInfoMessage = new GetInfoMessage(InfoType.Protocols);

			var dmsMessage = Engine.SLNet.SendMessage(getInfoMessage);

			return dmsMessage.Cast<GetProtocolsResponseMessage>();
		}

		/// <summary>
		/// Gets the information of all DataMiner agents in a cluster.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> of <see cref="GetDataMinerInfoResponseMessage"/> with the information of all DataMiner agents in a cluster.
		/// </returns>
		public static IEnumerable<GetDataMinerInfoResponseMessage> GetDataMinerAgentsInfo()
		{
			var getInfoMessage = new GetInfoMessage(InfoType.DataMinerInfo);

			var dmsMessage = Engine.SLNet.SendMessage(getInfoMessage);

			return dmsMessage.Cast<GetDataMinerInfoResponseMessage>();
		}

		/// <summary>
		/// Gets information events.
		/// </summary>
		/// <param name="startTime">Start time.</param>
		/// <param name="endTime">End time.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="AlarmEventMessage"/> containing all information events from start till end time.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If end is lower or equal to start time.</exception>
		/// <exception cref="DataMinerException">If some error occurred in DataMiner.</exception>
		public static IEnumerable<AlarmEventMessage> GetInformationEvents(DateTime startTime, DateTime endTime)
		{
			if (endTime <= startTime)
			{
				throw new ArgumentOutOfRangeException("endTime", "End time must be after Start time");
			}

			DMSMessage[] messages = null;
			try
			{
				var alarmDetailsFromDbMessage = new GetAlarmDetailsFromDbMessage(new AlarmFilter(), startTime, endTime, false, true);
				messages = Engine.SLNet.SendMessage(alarmDetailsFromDbMessage);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Couldn't get information events from {0} until {1}", startTime, endTime), e);
			}

			return GetInformationEventsIterator(messages);
		}

		/// <summary>
		/// Gets a DataMiner Element lite info by element id.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner Agent.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <returns><see cref="LiteElementInfoEvent"/> object with the Element info.</returns>
		/// <exception cref="ElementNotFoundException">If the element couldn't be found.</exception>
		public static LiteElementInfoEvent GetLiteElementById(int dmaId, int elementId)
		{
			try
			{
				var getLiteElementInfo = new GetLiteElementInfo(true)
				{
					DataMinerID = dmaId,
					ElementID = elementId
				};

				var dmsMessage = Engine.SLNet.SendMessage(getLiteElementInfo);
				return (LiteElementInfoEvent)dmsMessage[0];
			}
			catch (Exception e)
			{
				throw new ElementNotFoundException(string.Format("Element {0}/{1} doesn't exist", dmaId, elementId), e);
			}
		}

		/// <summary>
		/// Gets a DataMiner Element lite info by element name.
		/// </summary>
		/// <param name="elementName">Name of the Element.</param>
		/// <returns><see cref="LiteElementInfoEvent"/>Object with the Element info.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="elementName"/> is null.</exception>
		/// <exception cref="ElementNotFoundException">If the element couldn't be found.</exception>
		public static LiteElementInfoEvent GetLiteElementByName(string elementName)
		{
			if (elementName == null)
			{
				throw new ArgumentNullException("elementName");
			}

			try
			{
				var getLiteElementInfo = new GetLiteElementInfo(true)
				{
					NameFilter = elementName
				};
				var dmsMessage = Engine.SLNet.SendMessage(getLiteElementInfo);
				return (LiteElementInfoEvent)dmsMessage[0];
			}
			catch (Exception e)
			{
				throw new ElementNotFoundException(string.Format("Element {0} doesn't exist", elementName), e);
			}
		}

		/// <summary>
		/// Gets a DataMiner Lite Service by ID.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner Agent running the service.</param>
		/// <param name="serviceId">Id of the service.</param>
		/// <returns>A <see cref="LiteServiceInfoEvent"/> object with the Service info.</returns>
		/// <exception cref="ServiceNotFoundException">If there's no service with the provided name.</exception>
		public static LiteServiceInfoEvent GetLiteServiceById(int dmaId, int serviceId)
		{
			var getServiceById = new GetLiteServiceInfo
			{
				DataMinerID = dmaId,
				ServiceID = serviceId
			};

			var result = Engine.SLNet.SendSingleResponseMessage(getServiceById) as LiteServiceInfoEvent;

			if (result == null)
			{
				throw new ServiceNotFoundException(string.Format("Service {0}/{1} doesn't exist", dmaId, serviceId));
			}

			return result;
		}

		/// <summary>
		/// Gets a DataMiner Lite Service by Name.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <returns>A <see cref="LiteServiceInfoEvent"/> object with the Service info.</returns>
		/// <exception cref="ArgumentException">If <paramref name="serviceName"/> is <see cref="String.IsNullOrWhiteSpace(string)"/>.</exception>
		/// <exception cref="ServiceNotFoundException">If there's no service with the provided name.</exception>
		public static LiteServiceInfoEvent GetLiteServiceByName(string serviceName)
		{
			if (string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentException("message", "serviceName");
			}

			var getServiceByName = new GetLiteServiceInfo
			{
				NameFilter = serviceName,
			};

			var result = Engine.SLNet.SendSingleResponseMessage(getServiceByName) as LiteServiceInfoEvent;

			if (result == null)
			{
				throw new ServiceNotFoundException(string.Format("Service {0} doesn't exist", serviceName));
			}

			return result;
		}

		/// <summary>
		/// Gets an <see cref="IEnumerable{T}"/> of <see cref="PropertyConfig"/> with all registered properties.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PropertyConfig"/> with all registered properties.</returns>
		public static IEnumerable<PropertyConfig> GetRegisteredProperties()
		{
			try
			{
				var response = Engine.SLNet.SendSingleResponseMessage(new GetInfoMessage(InfoType.PropertyConfiguration)) as GetPropertyConfigurationResponse;

				if (response == null)
				{
					return Enumerable.Empty<PropertyConfig>();
				}

				return response.Properties;
			}
			catch (Exception)
			{
				return Enumerable.Empty<PropertyConfig>();
			}
		}

		/// <summary>
		/// Gets the script information.
		/// </summary>
		/// <param name="name">Name of the script to fetch the information.</param>
		/// <returns>A <see cref="GetScriptInfoResponseMessage"/> object with the script information.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
		/// <exception cref="ScriptNotFoundException">If there no script with the given name.</exception>
		public static GetScriptInfoResponseMessage GetScriptInfo(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			var getScriptMessage = new GetScriptInfoMessage
			{
				Name = name
			};

			try
			{
				var response = Engine.SLNet.SendSingleResponseMessage(getScriptMessage) as GetScriptInfoResponseMessage;

				if (response == null)
				{
					throw new ScriptNotFoundException(name);
				}

				return response;
			}
			catch (Exception e)
			{
				throw new ScriptNotFoundException(name, e);
			}
		}

		/// <summary>
		/// Checks if an element is active (in SLDMS).
		/// </summary>
		/// <param name="dmaId">ID of the DMA on which the element to be checked is located.</param>
		/// <param name="elementId">ID of the element to be checked.</param>
		/// <returns>True if the element is active. False otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/>is less than zero.</exception>
		public static bool IsElementActiveInSlDms(int dmaId, int elementId)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be a positive number.");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be a positive number.");
			}

			DMS dms = new DMS();

			try
			{
				object state;
				dms.Notify(91/*DMS_GET_ELEMENT_STATE*/, 0, Convert.ToUInt32(dmaId), Convert.ToUInt32(elementId), out state);

				string elementState = Convert.ToString(state);
				if (elementState.Equals(ElementState.Active.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			catch (Exception)
			{
				// ignore
			}

			return false;
		}

		/// <summary>
		/// Checks if an element is fully loaded in SLElement.
		/// </summary>
		/// <param name="dmaId">ID of the DMA on which the element to be checked is located.</param>
		/// <param name="elementId">ID of the element to be checked.</param>
		/// <returns>True if the element is fully loaded. False otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/>is less than zero.</exception>
		public static bool IsElementLoadedInSlElement(int dmaId, int elementId)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be a positive number.");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be a positive number.");
			}

			var message = new SetDataMinerInfoMessage
			{
				DataMinerID = dmaId,
				HostingDataMinerID = dmaId,
				Uia1 = new UIA
				{
					Uia = new[]
					{
						Convert.ToUInt32(dmaId),
						Convert.ToUInt32(elementId)
					}
				},
				What = (int)NotifyType.NT_ELEMENT_STARTUP_COMPLETE
			};
			try
			{
				var responseMessage = Engine.SLNet.SendSingleResponseMessage(message) as SetDataMinerInfoResponseMessage;
					
				if (responseMessage == null)
				{
					return false;
				}

				return Convert.ToBoolean(responseMessage.RawData);
			}
			catch (Exception)
			{
				// ignore
			}

			return false;
		}

		/// <summary>
		/// Checks if an element is fully loaded in SLNet.
		/// </summary>
		/// <param name="dmaId">ID of the DMA on which the element to be checked is located.</param>
		/// <param name="elementId">ID of the element to be checked.</param>
		/// <returns>True if the element is fully loaded. False otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/>is less than zero.</exception>
		public static bool IsElementLoadedInSlNet(int dmaId, int elementId)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be a positive number.");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be a positive number.");
			}

			GetElementProtocolMessage getElementProtocolMessage = new GetElementProtocolMessage(dmaId, elementId);

			try
			{
				var responseMessage = Engine.SLNet.SendSingleResponseMessage(getElementProtocolMessage) as GetElementProtocolResponseMessage;

				if (!responseMessage.WasBuiltWithUnsafeData)
				{
					return true;
				}
			}
			catch (Exception)
			{
				// ignore
			}

			return false;
		}

		/// <summary>
		/// Checks whether a property is registered.
		/// </summary>
		/// <param name="property">Property to be checked.</param>
		/// <returns>True if the property is registered;otherwise false.</returns>
		/// <exception cref="ArgumentException">If <paramref name="property"/> is null.</exception>
		public static bool IsPropertyRegistered(PropertyInfo property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}

			return IsPropertyRegistered(property.Name, property.DataType);
		}

		/// <summary>
		/// Checks whether a property is registered.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="propertyType">Type of the property.</param>
		/// <returns>True if the property is registered;otherwise false.</returns>
		/// <exception cref="ArgumentException">If <paramref name="propertyName"/> or <paramref name="propertyType"/> are <see cref="string.IsNullOrWhiteSpace(string)"/>.</exception>
		public static bool IsPropertyRegistered(string propertyName, string propertyType)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException("propertyName cannot be empty or null", "propertyName");
			}

			if (string.IsNullOrWhiteSpace(propertyType))
			{
				throw new ArgumentException("propertyType cannot be empty or null", "propertyType");
			}

			try
			{
				return GetRegisteredProperties().Any(p => p.Name == propertyName && string.Equals(p.Type, propertyType, StringComparison.InvariantCultureIgnoreCase));
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Registers a property in DataMiner system.
		/// </summary>
		/// <param name="property">Property to be registered.</param>
		/// <param name="propertyType">Type of the property.</param>
		/// <returns>True if the property was registered;otherwise false.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="propertyType"/> is <see cref="string.IsNullOrWhiteSpace(string)"/>.</exception>
		public static bool RegisterPropertyConfig(PropertyInfo property, string propertyType)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}

			if (string.IsNullOrWhiteSpace(propertyType))
			{
				throw new ArgumentException("propertyType cannot be empty or null", "propertyType");
			}

			var properties = GetRegisteredProperties().ToArray();

			if (properties.Any(p => p.Name == property.Name && string.Equals(p.Type, propertyType, StringComparison.InvariantCultureIgnoreCase)))
			{
				return true;
			}

			try
			{
				var response = Engine.SLNet.SendSingleResponseMessage(new AddPropertyConfigMessage(new PropertyConfig(properties.Length, property.Name, propertyType))) as UpdatePropertyConfigResponse;

				if (response == null)
				{
					return false;
				}

				return response.ID == properties.Length;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if a given service Name exists in the System.
		/// </summary>
		/// <param name="serviceName">Service Name to look for in the DMS.</param>
		/// <returns>Returns true if the serviceName exists in the DMS.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="serviceName"/> is null.</exception>
		public static bool ServiceExists(string serviceName)
		{
			if (serviceName == null)
			{
				throw new ArgumentNullException("serviceName");
			}

			try
			{
				object state;
				new DMS().Notify(48, 0, serviceName, null, out state);

				var result = state as int[];

				if (result == null)
				{
					return false;
				}

				return result[0] == -1 && result[1] == -1 && result[2] == -1;
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>
		/// Sets simulation on Element according to value defined on <paramref name="status"/>.
		/// </summary>
		/// <param name="dmaId">DataMiner Agent ID.</param>
		/// <param name="elementId">Element ID.</param>
		/// <param name="status">Simulation Status. Can be True or False.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/> is less than zero.</exception>
		public static void SetElementSimulation(int dmaId, int elementId, bool status)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be positive number");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be positive number");
			}

			var setSimulation = new SetDataMinerInfoMessage
			{
				StrInfo2 = status.ToString(),
				What = (int)NotifyType.AssignSimulation,
				Uia1 = new UIA
				{
					Uia = new uint[]
					{
						Convert.ToUInt32(dmaId),
						Convert.ToUInt32(elementId)
					}
				}
			};

			try
			{
				Engine.SLNet.SendSingleResponseMessage(setSimulation);
			}
			catch (DataMinerException e)
			{
				throw new DataMinerException(string.Format("Couldn't set simulation to {0} on element {1} and dmaID {2} ", status, elementId, dmaId), e);
			}
		}

		/// <summary>
		/// Sets <paramref name="protocolName"/> to production with version <paramref name="protocolVersion"/>.
		/// </summary>
		/// <param name="protocolName">Name of Protocol.</param>
		/// <param name="protocolVersion">Version of Protocol.</param>
		public static void SetProtocolToProduction(string protocolName, string protocolVersion)
		{
			if (protocolName == null)
			{
				throw new ArgumentNullException("protocolName");
			}

			if (protocolVersion == null)
			{
				throw new ArgumentNullException("protocolVersion");
			}

			var setDataMinerInfo = new SetDataMinerInfoMessage
			{
				bInfo1 = int.MaxValue,
				bInfo2 = 1,
				DataMinerID = -1,
				HostingDataMinerID = -1,
				IInfo1 = int.MaxValue,
				IInfo2 = int.MaxValue,
				What = (int)NotifyType.SetAsCurrentProtoocol,
				Sa1 = new SA
				{
					Sa = new[] { protocolName, protocolVersion }
				}
			};

			try
			{
				Engine.SLNet.SendSingleResponseMessage(setDataMinerInfo);
			}
			catch (DataMinerException e)
			{
				throw new DataMinerException(string.Format("Couldn't set protocol {0} to production with version {1} ", protocolName, protocolVersion), e);
			}
		}

		/// <summary>
		/// Gets a DataMiner Lite Service by Id.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner Agent running the service.</param>
		/// <param name="serviceId">Id of the service.</param>
		/// <param name="serviceInfo">Out variable that will hold the service info if it succeeds;otherwise null.</param>
		/// <returns>True if the service could be found;otherwise false.</returns>
		public static bool TryGetLiteServiceById(int dmaId, int serviceId, out LiteServiceInfoEvent serviceInfo)
		{
			try
			{
				serviceInfo = GetLiteServiceById(dmaId, serviceId);
				return true;
			}
			catch (Exception)
			{
				serviceInfo = null;
				return false;
			}
		}

		/// <summary>
		/// Gets a DataMiner Lite Service by Name.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <param name="serviceInfo">Out variable that will hold the service info if it succeeds;otherwise null.</param>
		/// <returns>True if the service could be found;otherwise false.</returns>
		public static bool TryGetLiteServiceByName(string serviceName, out LiteServiceInfoEvent serviceInfo)
		{
			try
			{
				serviceInfo = GetLiteServiceByName(serviceName);
				return true;
			}
			catch (Exception)
			{
				serviceInfo = null;
				return false;
			}
		}

		/// <summary>
		/// Checks if an element is active in SLDMS and fully loaded in SLElement and SLNet.
		/// </summary>
		/// <param name="dmaId">ID of the DMA on which the element to be checked is located.</param>
		/// <param name="elementId">ID of the element to be checked.</param>
		/// <param name="timeout">Number of milliseconds to wait for the element to be active.</param>
		/// <returns>True if element is active. false otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/>is less than zero.</exception>
		public static bool WaitForElementStartup(int dmaId, int elementId, int timeout)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be a positive number.");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be a positive number.");
			}

			bool result = false;
			try
			{
				// Checks if an element is active (in SLDMS).
				result = SpinWait.SpinUntil(() => IsElementActiveInSlDms(dmaId, elementId), timeout);
				if (!result)
				{
					return false;
				}

				// Checks if an element is fully loaded in SLElement.
				result = SpinWait.SpinUntil(() => IsElementLoadedInSlElement(dmaId, elementId), timeout);
				if (!result)
				{
					return false;
				}

				// Checks if an element is fully loaded in SLNet.
				result = SpinWait.SpinUntil(() => IsElementLoadedInSlNet(dmaId, elementId), timeout);
				if (!result)
				{
					return false;
				}
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		/// <summary>
		/// Waits for a service to be deleted.
		/// </summary>
		/// <param name="name">Name of the service.</param>
		/// <param name="timeout">Time to wait for the service deletion.</param>
		/// <returns>True if the service was deleted within the given timeout;otherwise false.</returns>
		/// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.IsNullOrWhiteSpace(string)"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeout"/> is <see cref="TimeSpan.Zero"/>.</exception>
		public static bool WaitForServiceDeletion(string name, TimeSpan timeout)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("message", "name");
			}

			if (timeout == TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", "timeout needs to be positive");
			}

			return MiscExtensions.RetryUntilSuccessOrTimeout(
				() => !ServiceExists(name),
				timeout,
				100);
		}

		/// <summary>
		/// Waits for a service to be deleted.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner Agent running the service.</param>
		/// <param name="serviceId">Id of the service.</param>
		/// <param name="timeout">Time to wait for the service deletion.</param>
		/// <returns>True if the service was deleted within the given timeout;otherwise false.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeout"/> is <see cref="TimeSpan.Zero"/>.</exception>
		public static bool WaitForServiceDeletion(int dmaId, int serviceId, TimeSpan timeout)
		{
			if (timeout == TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", "timeout needs to be positive");
			}

			LiteServiceInfoEvent serviceInfo;
			return MiscExtensions.RetryUntilSuccessOrTimeout(
				() => !TryGetLiteServiceById(dmaId, serviceId, out serviceInfo),
				timeout,
				100);
		}

		/// <summary>
		/// Iterator for method <see cref="GetInformationEvents"/>.
		/// </summary>
		/// <param name="messages">Message responses from SLNet process.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="AlarmEventMessage"/> containing all information events from start till end time.</returns>
		private static IEnumerable<AlarmEventMessage> GetInformationEventsIterator(DMSMessage[] messages)
		{
			if (messages == null)
			{
				yield break;
			}

			foreach (var eventMessage in messages.OfType<AlarmEventMessage>())
			{
				yield return eventMessage;
			}
		}
	}
}
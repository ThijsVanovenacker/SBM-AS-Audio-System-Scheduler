namespace Skyline.DataMiner.Library.Automation
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.DCF;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	/// <summary>
	/// Class with <see cref="Engine"/> extension methods.
	/// </summary>
	public static class EngineExtensionMethods
	{
		/// <summary>
		/// Adds or sets a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to add.</param>
		/// <param name="row">Row to be added.</param>
		/// <param name="keysColumnIdx">Index of the column that contains the keys. Default value = 0.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="row"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the operation failed.</exception>
		public static void AddOrSetRow(this Engine engine, int dmaId, int elementId, int tableId, string key, object[] row, uint keysColumnIdx = 0)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			DataMinerTablesHelper.AddOrSetRow(dmaId, elementId, tableId, key, row, keysColumnIdx);
		}

		/// <summary>
		/// Adds a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="row">Row to be added.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="row"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void AddRow(this Engine engine, int dmaId, int elementId, int tableId, object[] row)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			DataMinerTablesHelper.AddRow(dmaId, elementId, tableId, row);
		}

		/// <summary>
		/// Adds a DataMiner Service.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="serviceInfo"><see cref="ServiceInfoEventMessage"/> object with the info of the service to add.</param>
		/// <param name="viewIds">Ids of the DataMiner Views where the service should be added.</param>
		/// <returns>The Id of the new service.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/>, <paramref name="serviceInfo"/> or <paramref name="viewIds"/> are null.</exception>
		/// <exception cref="ServiceNotAddedException">If the service could not be added to DataMiner.</exception>
		public static int AddService(this Engine engine, ServiceInfoEventMessage serviceInfo, params int[] viewIds)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (serviceInfo == null)
			{
				throw new ArgumentNullException("serviceInfo");
			}

			if (viewIds == null)
			{
				throw new ArgumentNullException("viewIds");
			}

			try
			{
				var addServiceMessage = new AddServiceMessage
				{
					DataMinerID = serviceInfo.DataMinerID,
					HostingDataMinerID = serviceInfo.HostingAgentID,
					Service = serviceInfo,
					ViewIDs = viewIds
				};

				var dmsMessage = engine.SendSLNetSingleResponseMessage(addServiceMessage);
				var response = dmsMessage as AddServiceResponseMessage;

				return response.NewID;
			}
			catch (Exception e)
			{
				throw new ServiceNotAddedException(string.Format("Failed to create Service {0}", serviceInfo.Name), e);
			}
		}

		/// <summary>
		/// Creates a new DataMiner View.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="viewName">The view name to be added.</param>
		/// <param name="parentViewID">The parent view id.</param>
		/// <returns>The ID of the created view. -1 if the view wasn't created.</returns>
		public static int CreateNewView(this Engine engine, string viewName, int parentViewID = -1)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (viewName == null)
			{
				throw new ArgumentNullException("viewName");
			}

			var request = new SetDataMinerInfoMessage
			{
				What = 2,
				Sa1 = new SA
				{
					Sa = new[] { viewName, Convert.ToString(parentViewID) }
				}
			};

			var result = engine.SendSLNetSingleResponseMessage(request) as SetDataMinerInfoResponseMessage;

			if (result != null)
			{
				return result.iRet;
			}

			return -1;
		}

		/// <summary>
		/// Deletes a row from a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to be deleted.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="key"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void DeleteRow(this Engine engine, int dmaId, int elementId, int tableId, string key)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			DataMinerTablesHelper.DeleteRow(dmaId, elementId, tableId, key);
		}

		/// <summary>
		/// Deletes a DataMiner Service.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="serviceId">ID of the DataMiner Service.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		/// <exception cref="ServiceNotDeletedException">If the service couldn't delete the service.</exception>
		public static void DeleteService(this Engine engine, int dmaId, int serviceId)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			try
			{
				var message = new SetDataMinerInfoMessage
				{
					DataMinerID = dmaId,
					HostingDataMinerID = dmaId,
					Uia1 = new UIA
					{
						Uia = new[]
						{
							Convert.ToUInt32(dmaId),
							Convert.ToUInt32(serviceId)
						}
					},
					What = (int)NotifyType.DeleteService
				};

				engine.SendSLNetSingleResponseMessage(message);
			}
			catch (Exception ex)
			{
				throw new ServiceNotDeletedException(string.Format("Failed to delete service {0}/{1}", dmaId, serviceId), ex);
			}
		}

		/// <summary>
		/// Deletes a DataMiner Service.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="serviceName">Name of the DataMiner Service.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		/// <exception cref="ServiceNotDeletedException">If the service couldn't delete the service.</exception>
		public static void DeleteService(this Engine engine, string serviceName)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			try
			{
				var message = new SetDataMinerInfoMessage
				{
					StrInfo1 = serviceName,
					What = (int)NotifyType.DeleteService
				};

				engine.SendSLNetSingleResponseMessage(message);
			}
			catch (Exception ex)
			{
				throw new ServiceNotDeletedException(string.Format("Failed to delete service {0}", serviceName), ex);
			}
		}

		/// <summary>
		/// Deletes a DataMiner View.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="viewId">The view name to be added.</param>
		/// <returns>The ID of the created view. -1 if the view wasn't created.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="viewId"/> is less than 1.</exception>
		public static int DeleteView(this Engine engine, int viewId)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (viewId < 1)
			{
				throw new ArgumentOutOfRangeException("viewId", "viewId needs to be positive number");
			}

			var request = new SetDataMinerInfoMessage
			{
				What = 4,
				IInfo1 = viewId,
				IInfo2 = -1
			};

			var result = engine.SendSLNetSingleResponseMessage(request) as SetDataMinerInfoResponseMessage;

			if (result != null)
			{
				return result.iRet;
			}

			return -1;
		}

		/// <summary>
		/// Checks if Element with <paramref name="elementName"/> exists on DataMiner.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="elementName">Name of the element.</param>
		/// <returns>True if found. False otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="elementName"/> are null.</exception>
		public static bool ElementExists(this Engine engine, string elementName)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (elementName == null)
			{
				throw new ArgumentNullException("elementName");
			}

			try
			{
				return engine.FindElement(elementName) != null;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if Element with <paramref name="dmaId"/> and <paramref name="elementId"/> exists on DataMiner.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <returns>True if found. False otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/> or <paramref name="elementId"/> are less than zero.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static bool ElementExists(this Engine engine, int dmaId, int elementId)
		{
			if (dmaId < 0)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be a positive number.");
			}

			if (elementId < 0)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be a positive number.");
			}

			try
			{
				return engine.FindElement(dmaId, elementId) != null;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the element information.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="elementId"><see cref="ElementID"/> object with the element's DataMiner Agent Id and element Id.</param>
		/// <returns>An <see cref="Element"/> object with the element information.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static Element FindElement(this Engine engine, ElementID elementId)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return engine.FindElement((int)elementId.DmaId, (int)elementId.ElementId);
		}

		/// <summary>
		/// Finds the SLA Element for a Service.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="service">Service from which the SLA element should be fetched.</param>
		/// <returns>The SLA Element for a Service.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="service"/> are null.</exception>
		/// <exception cref="ElementNotFoundException">If there are no SLA Element for the Service.</exception>
		public static Element FindSlaElementForService(this Engine engine, Service service)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			var slaElements = engine.FindElementsByProtocol("Skyline SLA Definition Basic");

			if (slaElements == null)
			{
				throw new ElementNotFoundException("There's no SLA Elements");
			}

			var serviceKey = string.Join("/", service.DmaId, service.ServiceId);
			Func<Element, bool> slaElementSelector = element =>
			{
				if (element.RawInfo == null ||
					element.RawInfo.PortInfo == null ||
					element.RawInfo.PortInfo.Length == 0 ||
					element.RawInfo.PortInfo[0] == null)
				{
					return false;
				}

				return element.RawInfo.PortInfo[0].BusAddress == serviceKey;
			};

			var slaElement = slaElements.SingleOrDefault(slaElementSelector);

			if (slaElement == null)
			{
				throw new ElementNotFoundException(string.Format("There's no SLA Element for Service {0}", service.Name));
			}

			return slaElement;
		}

		/// <summary>
		/// Generates an information event if run in DEBUG mode.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="message">Message to show in the information event.</param>
		/// <exception cref="ArgumentNullException">If any argument is null.</exception>
		[Conditional("DEBUG")]
		public static void GenerateDebugInformation(this Engine engine, string message)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (message == null)
			{
				throw new ArgumentNullException("message");
			}

			engine.GenerateInformation(message);
		}

		/// <summary>
		/// Gets All Views from a DataMiner System.
		/// </summary>
		/// <param name="engine"><see cref="Engine" /> instance used to communicate with DataMiner.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ViewInfoEventMessage"/> object with all Views on the DMS.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static IEnumerable<ViewInfoEventMessage> GetAllViews(this Engine engine)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			var getInfoMessage = new GetInfoMessage(InfoType.ViewInfo);

			var dmsMessage = engine.SendSLNetMessage(getInfoMessage);

			return dmsMessage.Cast<ViewInfoEventMessage>();
		}

		/// <summary>
		/// Gets a column with the desired format from any Element on the DMS.
		/// </summary>
		/// <typeparam name="T">Type of the Column.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnIdx">Index of the desired column.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> with the desired column. <see cref="Enumerable.Empty{T}"/> is it was not possible to get the column values.</returns>
		public static IEnumerable<T> GetColumn<T>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint columnIdx)
			where T : IConvertible
		{
			return DataMinerTablesHelper.GetColumn<T>(dmaId, elementId, tableId, columnIdx);
		}

		/// <summary>
		/// Gets two columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets three columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets four columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets five columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets six columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets seven columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="T7">Type of the seventh Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
			where T7 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets eight columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="T7">Type of the seventh Column.</typeparam>
		/// <typeparam name="T8">Type of the eighth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
			where T7 : IConvertible
			where T8 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets nine columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="T7">Type of the seventh Column.</typeparam>
		/// <typeparam name="T8">Type of the eighth Column.</typeparam>
		/// <typeparam name="T9">Type of the ninth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
			where T7 : IConvertible
			where T8 : IConvertible
			where T9 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets ten columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="T7">Type of the seventh Column.</typeparam>
		/// <typeparam name="T8">Type of the eighth Column.</typeparam>
		/// <typeparam name="T9">Type of the ninth Column.</typeparam>
		/// <typeparam name="T10">Type of the tenth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
			this Engine engine,
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
			where T7 : IConvertible
			where T8 : IConvertible
			where T9 : IConvertible
			where T10 : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			return DataMinerTablesHelper.GetColumns(dmaId, elementId, tableId, columnsIdx, returnSelector);
		}

		/// <summary>
		/// Gets the information about a DCF Connection.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaID">ID of the DataMiner agent.</param>
		/// <param name="elementID">ID of the Element.</param>
		/// <param name="connectionID">ID of the DCF Connection.</param>
		/// <returns>A <see cref="InterfaceConnectionInfoEventMessage.InterfaceConnectionInfo"/> with the connection info.</returns>
		/// <exception cref="ConnectionNotFoundException">If the connection doesn't exist.</exception>
		public static InterfaceConnectionInfoEventMessage.InterfaceConnectionInfo GetConnectionInfo(this Engine engine, int dmaID, int elementID, int connectionID)
		{
			try
			{
				var elementConnections = engine.GetElementConnections(dmaID, elementID);
				return elementConnections.Single(connection => connection.ID == connectionID);
			}
			catch (Exception)
			{
				throw new ConnectionNotFoundException(connectionID);
			}
		}

		/// <summary>
		/// Gets the property of a DCF Connection.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaID">ID of the DataMiner agent.</param>
		/// <param name="elementID">ID of the Element.</param>
		/// <param name="connectionID">ID of the DCF Connection.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>An <see cref="InterfaceConnectionInfoEventMessage.InterfaceConnectionProperty"/> with the property info.</returns>
		/// <exception cref="ConnectionNotFoundException">If the connection doesn't exist.</exception>
		/// <exception cref="PropertyNotFoundException">If the property doesn't exist.</exception>
		public static InterfaceConnectionInfoEventMessage.InterfaceConnectionProperty GetConnectionProperty(this Engine engine, int dmaID, int elementID, int connectionID, string propertyName)
		{
			try
			{
				return engine.GetConnectionInfo(dmaID, elementID, connectionID).Properties.Single(property => property.Name == propertyName);
			}
			catch (ConnectionNotFoundException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new PropertyNotFoundException(connectionID.ToString(), propertyName);
			}
		}

		/// <summary>
		/// Gets the DCF Connections of a DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine" /> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the element.</param>
		/// <returns><see cref="InterfaceConnectionInfoEventMessage.InterfaceConnectionInfo" /> array with all Element's DCF connections.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static InterfaceConnectionInfoEventMessage.InterfaceConnectionInfo[] GetElementConnections(
			this Engine engine,
			int dmaId,
			int elementId)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			var message = new GetElementInterfaceConnectionsByIDMessage(dmaId, elementId);
			DMSMessage[] dmsMessages = engine.SendSLNetMessage(message);
			return ((InterfaceConnectionInfoEventMessage)dmsMessages[0]).InterfaceConnections;
		}

		/// <summary>
		/// Gets the interfaces of a DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine" /> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the element.</param>
		/// <param name="exported">Defines if interfaces exported to DVEs should be included.</param>
		/// <returns><see cref="InterfaceInfoEventMessage.InterfaceInfo" /> array with all Element's interfaces.</returns>
		public static InterfaceInfoEventMessage.InterfaceInfo[] GetElementInterfaces(
			this Engine engine,
			int dmaId,
			int elementId,
			bool exported)
		{
			var message = new GetElementInterfacesByIDMessage(dmaId, elementId) { Exported = exported };
			DMSMessage[] dmsMessages = engine.SendSLNetMessage(message);
			return ((InterfaceInfoEventMessage)dmsMessages[0]).Interfaces;
		}

		/// <summary>
		/// Gets the ids of the views (doesn't include root view) of a DataMiner object (Element, Service, etc.).
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">Id of the DataMiner agent where the object is running.</param>
		/// <param name="objectId">Id of the object.</param>
		/// <returns>An <see cref="Array"/> of <see cref="uint"/> with the ids of the views where the object is.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static uint[] GetObjectViews(this Engine engine, int dmaId, int objectId)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			var request = new SetDataMinerInfoMessage
			{
				What = (int)NotifyType.GetViewsOfElement,
				IInfo1 = dmaId,
				IInfo2 = objectId
			};

			var result = engine.SendSLNetSingleResponseMessage(request) as SetDataMinerInfoResponseMessage;

			if (result == null ||
				result.Uia == null ||
				result.Uia.Uia == null)
			{
				return new uint[0];
			}

			return result.Uia.Uia;
		}

		/// <summary>
		/// Gets the information of the specified DataMiner Protocol.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="name">Name of the protocol.</param>
		/// <param name="version">Version of the protocol.</param>
		/// <param name="raw">Flag indicating if raw data should be used.</param>
		/// <returns>A <see cref="GetProtocolInfoResponseMessage"/> object with the protocol information.</returns>
		public static GetProtocolInfoResponseMessage GetProtocolInfo(
			this Engine engine,
			string name,
			string version = "Production",
			bool raw = true)
		{
			try
			{
				return ProtocolInfoCache.GetCacheProtocol(Engine.SLNetRaw, name, version);
			}
			catch (Exception e)
			{
				throw new ProtocolNotFoundException(string.Format("Protocol: {0} Version: {1} doesn't exist", name, version), e);
			}
		}

		/// <summary>
		/// Gets the value of an Automation Script parameter.
		/// </summary>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <returns>An object with the parameter value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="paramName"/> are null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="paramName"/> is <see cref="String.Empty"/> or a white space.</exception>
		/// <exception cref="InvalidScriptParamException">If there is no parameter with the given name.</exception>
		public static T GetScriptParamValue<T>(this Engine engine, string paramName)
			where T : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (paramName == null)
			{
				throw new ArgumentNullException("paramName");
			}

			if (string.IsNullOrWhiteSpace(paramName))
			{
				throw new ArgumentException("parameter name empty", "paramName");
			}

			var scriptParam = engine.GetScriptParam(paramName);

			if (scriptParam == null)
			{
				throw new InvalidScriptParamException("parameter is not defined", paramName);
			}

			return scriptParam.Value.ChangeType<T>();
		}

		/// <summary>
		/// Gets the value of an Automation Script parameter.
		/// </summary>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <param name="selector">Transform function applied to the parameter raw value.</param>
		/// <returns>An object with the parameter value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/>, <paramref name="paramName"/> or <paramref name="selector"/> are null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="paramName"/> is <see cref="String.Empty"/> or a white space.</exception>
		/// <exception cref="InvalidScriptParamException">If there is no parameter with the given name.</exception>
		public static T GetScriptParamValue<T>(this Engine engine, string paramName, Func<string, T> selector)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (paramName == null)
			{
				throw new ArgumentNullException("paramName");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			if (string.IsNullOrWhiteSpace(paramName))
			{
				throw new ArgumentException("parameter name empty", "paramName");
			}

			var scriptParam = engine.GetScriptParam(paramName);

			if (scriptParam == null)
			{
				throw new InvalidScriptParamException("parameter is not defined", paramName);
			}

			return selector(scriptParam.Value);
		}

		/// <summary>
		/// Gets the value of an Automation Script parameter.
		/// </summary>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="paramId">ID of the parameter.</param>
		/// <param name="selector">Transform function applied to the parameter raw value.</param>
		/// <returns>An object with the parameter value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="selector"/> are null.</exception>
		/// <exception cref="InvalidScriptParamException">If there is no parameter with the given ID.</exception>
		public static T GetScriptParamValue<T>(this Engine engine, int paramId, Func<string, T> selector)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			var scriptParam = engine.GetScriptParam(paramId);

			if (scriptParam == null)
			{
				throw new InvalidScriptParamException("parameter is not defined", paramId);
			}

			return selector(scriptParam.Value);
		}

		/// <summary>
		/// Gets the value of an Automation Script parameter.
		/// </summary>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="paramId">ID of the parameter.</param>
		/// <returns>An object with the parameter value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		/// <exception cref="InvalidScriptParamException">If there is no parameter with the given ID.</exception>
		public static T GetScriptParamValue<T>(this Engine engine, int paramId)
			where T : IConvertible
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			var scriptParam = engine.GetScriptParam(paramId);

			if (scriptParam == null)
			{
				throw new InvalidScriptParamException("parameter is not defined", paramId);
			}

			return scriptParam.Value.ChangeType<T>();
		}

		/// <summary>
		/// Gets the available Scripts in the DMS.
		/// </summary>
		/// <param name="engine">Link with DataMiner Systems.</param>
		/// <returns>Returns the List of Scripts.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		public static string[] GetScripts(this Engine engine)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			GetInfoMessage getScriptsInfo = new GetInfoMessage
			{
				DataMinerID = -1,
				HostingDataMinerID = -1,
				Type = InfoType.Scripts
			};

			DMSMessage[] scriptsInfoMessage = engine.SendSLNetMessage(getScriptsInfo);

			if (scriptsInfoMessage == null)
			{
				return new string[0];
			}

			List<string> scriptNames = new List<string>();

			foreach (var message in scriptsInfoMessage)
			{
				var getScriptsInfoMessage = message as GetScriptsResponseMessage;

				if (getScriptsInfoMessage == null)
				{
					continue;
				}

				scriptNames.AddRange(getScriptsInfoMessage.Scripts);
			}

			return scriptNames.ToArray();
		}

		/// <summary>
		/// If <paramref name="enable"/> is true, an information event will be generated; otherwise nothing will be done.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="message">Message to show in the information event.</param>
		/// <param name="enable">If true, an information event will be generated; otherwise nothing will be done.</param>
		public static void Log(this Engine engine, string message, bool enable)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (message == null)
			{
				throw new ArgumentNullException("message");
			}

			if (enable)
			{
				engine.GenerateInformation(message);
			}
		}

		/// <summary>
		/// Collects the number of rows present in an element table.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tablePid">ID of the table.</param>
		/// <returns>The number of rows.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>,<paramref name="elementId"/> or <paramref name="tablePid"/> are lower or equal to 0.</exception>
		public static int RowCount(this Engine engine, int dmaId, int elementId, int tablePid)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (dmaId <= 0)
			{
				throw new ArgumentOutOfRangeException("dmaId");
			}

			if (elementId <= 0)
			{
				throw new ArgumentOutOfRangeException("elementId");
			}

			if (tablePid <= 0)
			{
				throw new ArgumentOutOfRangeException("tablePid");
			}

			var element = engine.FindElement(new ElementID(dmaId, elementId));

			if (element == null)
			{
				throw new ElementNotFoundException(string.Format("Element {0}/{1} was not found.", dmaId, elementId));
			}

			object[] columns = element.GetColumns(tablePid, new[] { 0 });

			if (columns.Length == 0)
			{
				return 0;
			}

			return ((object[])columns[0]).Length;
		}

		/// <summary>
		/// Sets a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the rows that should be updated.</param>
		/// <param name="row">Row to be added.</param>
		public static void SetRow(this Engine engine, int dmaId, int elementId, int tableId, string key, object[] row)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			DataMinerTablesHelper.SetRow(dmaId, elementId, tableId, key, row);
		}

		/// <summary>
		/// Adds a DataMiner Service and waits for it's creation.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="serviceInfo"><see cref="ServiceInfoEventMessage"/> object with the info of the service to add.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the time to wait for the service creation, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="viewIds">Ids of the DataMiner Views where the service should be added.</param>
		/// <returns>The Id of the new service.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/>, <paramref name="serviceInfo"/> or <paramref name="viewIds"/> are null.</exception>
		/// <exception cref="ServiceNotAddedException">If the service could not be added to DataMiner.</exception>
		/// <exception cref="ServiceNotFoundException">If the service could not be retrieved after creation from DataMiner.</exception>
		public static Service SynchronousAddService(this Engine engine, ServiceInfoEventMessage serviceInfo, TimeSpan timeout, params int[] viewIds)
		{
			var newServiceId = AddService(engine, serviceInfo, viewIds);
			var dmaId = serviceInfo.DataMinerID;

			// Wait for service creation
			Service service = null;
			Func<bool> waitForServiceCreation = () =>
			{
				service = engine.FindService(dmaId, newServiceId);
				return service != null;
			};

			if (SpinWait.SpinUntil(waitForServiceCreation, timeout))
			{
				return service;
			}
			else
			{
				throw new ServiceNotFoundException(string.Format("Service {0}/{1} doesn't exist", dmaId, newServiceId));
			}
		}

		/// <summary>
		/// Finds the SLA Element for a Service.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="service">Service from which the SLA element should be fetched. Null if no the function failed.</param>
		/// <param name="element">Out parameter used to store the result.</param>
		/// <returns>True if an Element was found;otherwise false.</returns>
		public static bool TryFindSlaElementForService(this Engine engine, Service service, out Element element)
		{
			try
			{
				element = engine.FindSlaElementForService(service);
				return true;
			}
			catch (Exception)
			{
				element = null;
				return false;
			}
		}
	}
}
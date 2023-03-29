namespace Skyline.DataMiner.Library
{
	using System;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Contains static methods to interact with DataMiner Element parameter's.
	/// </summary>
	public static class DataMinerParameterHelper
	{
		private static readonly Lazy<PersistentConnectionStore> SlNet = new Lazy<PersistentConnectionStore>(() => Engine.SLNet);

		/// <summary>
		/// Gets a parameter value.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the desired Parameter.</param>
		/// <returns>An object with the raw parameter value.</returns>
		public static object GetParameter(int dmaId, int elementId, int parameterId)
		{
			var response = GetParameterResponseMessage(dmaId, elementId, parameterId);

			return response.Value.InteropValue;
		}

		/// <summary>
		/// Gets a parameter value.
		/// </summary>
		/// <param name="elementId">If of the Element.</param>
		/// <param name="parameterId">Id of the desired Parameter.</param>
		/// <returns>An object with the raw parameter value.</returns>
		public static object GetParameter(this ElementID elementId, int parameterId)
		{
			return GetParameter((int)elementId.DmaId, (int)elementId.ElementId, parameterId);
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(int dmaId, int elementId, int parameterId, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = parameterId,
					Value = new ParameterValue(value),
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} parameter {2}", dmaId, elementId, parameterId), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(int dmaId, int elementId, int parameterId, double value)
		{
			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = parameterId,
					Value = new ParameterValue(value),
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} parameter {2}", dmaId, elementId, parameterId), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(int dmaId, int elementId, int parameterId, bool value)
		{
			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = parameterId,
					Value = new ParameterValue(Convert.ToInt32(value)),
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} parameter {2}", dmaId, elementId, parameterId), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(int dmaId, int elementId, int parameterId, int value)
		{
			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = parameterId,
					Value = new ParameterValue(Convert.ToInt32(value)),
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} parameter {2}", dmaId, elementId, parameterId), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(this ElementID elementId, int parameterId, double value)
		{
			SetParameter((int)elementId.DmaId, (int)elementId.ElementId, parameterId, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(this ElementID elementId, int parameterId, int value)
		{
			SetParameter((int)elementId.DmaId, (int)elementId.ElementId, parameterId, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(this ElementID elementId, int parameterId, bool value)
		{
			SetParameter((int)elementId.DmaId, (int)elementId.ElementId, parameterId, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameter(this ElementID elementId, int parameterId, string value)
		{
			SetParameter((int)elementId.DmaId, (int)elementId.ElementId, parameterId, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> or <paramref name="primaryKey"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string primaryKey, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if (primaryKey == null)
			{
				throw new ArgumentNullException("primaryKey");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = columnId,
					Value = new ParameterValue(value),
					TableIndex = primaryKey,
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} column {2} with key {3}", dmaId, elementId, columnId, primaryKey), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If  <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string primaryKey, double value)
		{
			if (primaryKey == null)
			{
				throw new ArgumentNullException("primaryKey");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = columnId,
					Value = new ParameterValue(value),
					TableIndex = primaryKey,
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} column {2} with key {3}", dmaId, elementId, columnId, primaryKey), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If  <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string primaryKey, bool value)
		{
			if (primaryKey == null)
			{
				throw new ArgumentNullException("primaryKey");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = columnId,
					Value = new ParameterValue(Convert.ToInt32(value)),
					TableIndex = primaryKey,
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} column {2} with key {3}", dmaId, elementId, columnId, primaryKey), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If  <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string primaryKey, int value)
		{
			if (primaryKey == null)
			{
				throw new ArgumentNullException("primaryKey");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = columnId,
					Value = new ParameterValue(value),
					TableIndex = primaryKey,
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set parameter on Element {0}/{1} column {2} with key {3}", dmaId, elementId, columnId, primaryKey), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> or <paramref name="primaryKey"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(ElementID elementId, int columnId, string primaryKey, string value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, primaryKey, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(ElementID elementId, int columnId, string primaryKey, double value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, primaryKey, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(ElementID elementId, int columnId, string primaryKey, bool value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, primaryKey, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner column by primary key.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="primaryKey">Primary key of the row where the value should be set.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="primaryKey"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(ElementID elementId, int columnId, string primaryKey, int value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, primaryKey, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner parameter and waits for the value to be set.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the parameter.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="timeout">Maximum time to wait for the parameter set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterWithWait(this ElementID elementId, int parameterId, string value, TimeSpan timeout)
		{
			SetParameter(elementId, parameterId, value);

			SpinWait.SpinUntil(() => Convert.ToString(GetParameter(elementId, parameterId)) == value, timeout);
		}

		/// <summary>
		/// Gets a <see cref="GetParameterResponseMessage"/> with the Element's parameter info.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="parameterId">Id of the desired Parameter.</param>
		/// <param name="tableIndex">Key to filter the desired row(s).</param>
		/// <returns>A <see cref="GetParameterResponseMessage"/>Element's parameter info.</returns>
		internal static GetParameterResponseMessage GetParameterResponseMessage(
			int dmaId,
			int elementId,
			int parameterId,
			string tableIndex = "")
		{
			var message = new GetParameterMessage(dmaId, elementId, parameterId)
			{
				TableIndex = tableIndex
			};

			var dmsMessages = SlNet.Value.SendMessage(message);
			return (GetParameterResponseMessage)dmsMessages[0];
		}
	}
}
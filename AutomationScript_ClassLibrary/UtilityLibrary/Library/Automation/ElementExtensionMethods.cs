namespace Skyline.DataMiner.Library.Automation
{
	using System;
	using System.Linq;
	using System.Threading;
	using Interop.SLDms;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.Common.Attributes;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using SLNetMessages = Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Class with <see cref="Element"/> extension methods.
	/// </summary>
	public static class ElementExtensionMethods
	{
		/// <summary>
		/// Adds a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="element">Element where the row will be added.</param>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to add.</param>
		/// <param name="row">Row to be added.</param>
		/// <param name="keysColumnIdx">Index of the column that contains the keys. Default value = 0.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="element"/>, <paramref name="engine"/> or <paramref name="row"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="tableId"/> is 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void AddOrSetRow(this Element element, Engine engine, int tableId, string key, object[] row, uint keysColumnIdx = 0)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			engine.AddOrSetRow(element.DmaId, element.ElementId, tableId, key, row, keysColumnIdx);
		}

		/// <summary>
		/// Adds a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="element">Element where the row will be added.</param>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="row">Row to be added.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="element"/>, <paramref name="engine"/> or <paramref name="row"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="tableId"/> is 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void AddRow(this Element element, Engine engine, int tableId, object[] row)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			engine.AddRow(element.DmaId, element.ElementId, tableId, row);
		}

		/// <summary>
		/// Check if the given parameter contains the specified value.
		/// </summary>
		/// <param name="element"><see cref="Element"/> where the sets should be made.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <param name="paramValue">Value that should be checked.</param>
		/// <returns>True if the parameter value is equal to the specified value;otherwise false.</returns>
		public static bool CheckParamValue(this Element element, string paramName, object paramValue)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (paramName == null)
			{
				throw new ArgumentNullException("paramName");
			}

			if (paramName == string.Empty)
			{
				// Exception for buttons
				return false;
			}

			try
			{
				string value = Convert.ToString(paramValue);

				if (element.GetParameterDisplay(paramName) == value)
				{
					return true;
				}

				if (Convert.ToString(element.GetParameter(paramName)) == value)
				{
					return true;
				}
			}
			catch (DataMinerException)
			{
				// ignore
			}

			return false;
		}

		/// <summary>
		/// Check if the given parameter contains the specified value.
		/// </summary>
		/// <param name="element"><see cref="Element"/> where the sets should be made.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <param name="paramValue">Value that should be checked.</param>
		/// <param name="key">Specifies the table key if the parameter belongs to a table.</param>
		/// <returns>True if the parameter value is equal to the specified value;otherwise false.</returns>
		public static bool CheckParamValue(this Element element, string paramName, object paramValue, string key)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (paramName == null)
			{
				throw new ArgumentNullException("paramName");
			}

			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (paramName == string.Empty)
			{
				// Exception for buttons
				return false;
			}

			try
			{
				if (element.GetParameterDisplayByPrimaryKey(paramName, key) == Convert.ToString(paramValue))
				{
					return true;
				}

				if (Convert.ToString(element.GetParameterByPrimaryKey(paramName, key)) == Convert.ToString(paramValue))
				{
					return true;
				}
			}
			catch (DataMinerException)
			{
				// ignore
			}

			return false;
		}

		/// <summary>
		/// Check if the given parameter contains the specified value.
		/// </summary>
		/// <param name="element"><see cref="Element"/> where the sets should be made.</param>
		/// <param name="paramId">Id of the parameter.</param>
		/// <param name="paramValue">Value that should be checked.</param>
		/// <returns>True if the parameter value is equal to the specified value;otherwise false.</returns>
		public static bool CheckParamValue(this Element element, int paramId, object paramValue)
		{
			try
			{
				var value = Convert.ToString(paramValue);

				return Convert.ToString(element.GetParameter(paramId)) == value ||
					element.GetParameterDisplay(paramId) == value;
			}
			catch (DataMinerException)
			{
				return false;
			}
		}

		/// <summary>
		/// Check if the given parameter contains the specified value.
		/// </summary>
		/// <param name="element"><see cref="Element"/> where the sets should be made.</param>
		/// <param name="paramId">Id of the parameter.</param>
		/// <param name="paramValue">Value that should be checked.</param>
		/// <param name="key">Specifies the table key if the parameter belongs to a table.</param>
		/// <returns>True if the parameter value is equal to the specified value;otherwise false.</returns>
		public static bool CheckParamValue(this Element element, int paramId, object paramValue, string key)
		{
			try
			{
				var value = Convert.ToString(paramValue);

				return Convert.ToString(element.GetParameterByPrimaryKey(paramId, key)) == value ||
					element.GetParameterDisplayByPrimaryKey(paramId, key) == value;
			}
			catch (DataMinerException)
			{
				return false;
			}
		}

		/// <summary>
		/// Deletes a row from table with the given <paramref name="tableId"/> and <paramref name="key"/> on <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The element holding the table.</param>
		/// <param name="engine">The engine reference.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">The row key.</param>
		public static void DeleteRow(this Element element, Engine engine, int tableId, string key)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			engine.DeleteRow(element.DmaId, element.ElementId, tableId, key);
		}

		/// <summary>
		/// Deletes a row from table with the given <paramref name="tableId"/> and <paramref name="key"/> on <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The element holding the table.</param>
		/// <param name="engine">The engine reference.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">The row key.</param>
		/// <param name="timeout">Time to wait for the row deletion.</param>
		/// <returns>True if the row was successfully deleted within the <paramref name="timeout"/>;otherwise false.</returns>
		public static bool DeleteRowWithWait(this Element element, Engine engine, int tableId, string key, TimeSpan timeout)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			engine.DeleteRow(element.DmaId, element.ElementId, tableId, key);

			return SpinWait.SpinUntil(() => !engine.GetColumn<string>(element.DmaId, element.ElementId, tableId, 0).Contains(key), timeout);
		}

		/// <summary>
		/// Returns columns from table.
		/// Needs reference to C:\Skyline DataMiner\Files\Interop.SLDms.dll in Automation editor.
		/// </summary>
		/// <param name="element">The element where the columns should be fetched.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="columnIdxs">Indexes of the columns that need to be returned.</param>
		/// <returns>Object array of columns.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="element"/> or <paramref name="columnIdxs"/> are null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="columnIdxs"/> is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="tableId"/> is 0 or negative.</exception>
		[DllImport("Interop.SLDms.dll")]
		public static object[] GetColumns(this Element element, int tableId, int[] columnIdxs)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (columnIdxs == null)
			{
				throw new ArgumentNullException("columnIdxs");
			}

			if (columnIdxs.Length == 0)
			{
				throw new ArgumentException("columnIdxs cannot be empty", "columnIdxs");
			}

			if (tableId < 1)
			{
				throw new ArgumentOutOfRangeException("tableId", "tableId needs to be higher than 0");
			}

			var dms = new DMS();
			var ids = new[] { (uint)element.DmaId, (uint)element.ElementId };

			object returnValue;
			dms.Notify(87, 0, ids, tableId, out returnValue);

			var table = (object[])returnValue;
			if (table == null || table.Length <= 4)
			{
				return new object[0];
			}

			var columns = (object[])table[4];

			if (columns == null || columns.Length <= columnIdxs.Max())
			{
				return new object[0];
			}

			var rowCount = ((object[])columns[0]).Length;

			var returnColumns = new object[columnIdxs.Length];
			for (int i = 0; i < columnIdxs.Length; i++)
			{
				returnColumns[i] = new object[rowCount];
			}

			for (int i = 0; i < columnIdxs.Length; i++)
			{
				var column = (object[])columns[columnIdxs[i]];
				for (int j = 0; j < rowCount; j++)
				{
					var cell = (object[])column[j];
					if (cell != null && cell.Length > 0)
					{
						((object[])returnColumns[i])[j] = cell[0];
					}
				}
			}

			return returnColumns;
		}

		/// <summary>
		/// This Method aims to do a set on parameter that is a context menu.
		/// </summary>
		/// <param name="element">Target Element.</param>
		/// <param name="engine">Link with SLScripting.</param>
		/// <param name="contextMenuPid">Context Menu Parameter ID.</param>
		/// <param name="key">Table Key. A Context menu should already be attached to a specific Table.</param>
		/// <param name="value">Value that you aim to set through the context menu write button.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="element"/> or <paramref name="columnIdxs"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="tableId"/> is 0 or negative.</exception>
		/// <exception cref="DataMinerException">If an error occur while sending the slnet message.</exception>
		public static void SetContextMenuParameter(this Element element, Engine engine, int contextMenuPid, string key, string value)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (contextMenuPid < 0)
			{
				throw new ArgumentOutOfRangeException("contextMenuPid");
			}

			try
			{
				SetDataMinerInfoMessage message = new SetDataMinerInfoMessage
				{
					DataMinerID = element.DmaId,
					ElementID = -1,
					HostingDataMinerID = -1,
					IInfo1 = 0,
					IInfo2 = 0,
					Sa2 = new Net.Messages.SA(new[] { Guid.NewGuid().ToString(), value, key }),
					Uia1 = new Net.Messages.UIA(new[] { element.DmaId, element.ElementId, contextMenuPid }),
					What = (int)SLNetMessages.NotifyType.NT_ALL_TRAP_INFO
				};

				engine.SendSLNetMessage(message);
			}
			catch (Exception e)
			{
				throw new DataMinerException(
					string.Format("Failed to set context menu parameter {0} on Element {1}/{2} table index {3} with value {4}", contextMenuPid, element.DmaId, element.ElementId, key, value), e);
			}
		}

		/// <summary>
		/// Sets a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="element">Element where the row will be added.</param>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to set.</param>
		/// <param name="row">Row to be set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="element"/>, <paramref name="engine"/> or <paramref name="row"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="tableId"/> is 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void SetRow(this Element element, Engine engine, int tableId, string key, object[] row)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			engine.SetRow(element.DmaId, element.ElementId, tableId, key, row);
		}
	}
}
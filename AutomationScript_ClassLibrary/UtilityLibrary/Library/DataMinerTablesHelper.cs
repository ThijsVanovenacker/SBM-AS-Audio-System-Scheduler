namespace Skyline.DataMiner.Library
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	/// <summary>
	/// Contains static methods to interact with DataMiner Element tables.
	/// </summary>
	public static class DataMinerTablesHelper
	{
		private static readonly Lazy<PersistentConnectionStore> SlNet = new Lazy<PersistentConnectionStore>(() => Engine.SLNet);

		/// <summary>
		/// Adds or sets a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to add.</param>
		/// <param name="row">Row to be added.</param>
		/// <param name="keysColumnIdx">Index of the column that contains the keys. Default value = 0.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="row"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the operation failed.</exception>
		public static void AddOrSetRow(int dmaId, int elementId, int tableId, string key, object[] row, uint keysColumnIdx = 0)
		{
			var keys = GetColumn<string>(dmaId, elementId, tableId, keysColumnIdx);

			if (keys.Contains(key))
			{
				SetRow(dmaId, elementId, tableId, key, row);
			}
			else
			{
				AddRow(dmaId, elementId, tableId, row);
			}
		}

		/// <summary>
		/// Adds a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="row">Row to be added.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="row"/> is null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="row"/> doesn't contain any cells.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void AddRow(int dmaId, int elementId, int tableId, object[] row)
		{
			if (row == null)
			{
				throw new ArgumentNullException("row");
			}

			if (row.Length == 0)
			{
				throw new ArgumentException("row needs to have at least one cell", "row");
			}

			if (dmaId < 1)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be higher than 0");
			}

			if (elementId < 1)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be higher than 0");
			}

			if (tableId < 1)
			{
				throw new ArgumentOutOfRangeException("tableId", "tableId needs to be higher than 0");
			}

			try
			{
				var request = new SetDataMinerInfoMessage
				{
					DataMinerID = dmaId,
					ElementID = elementId,
					What = 149,
					Var1 = new[] { (uint)dmaId, (uint)elementId, (uint)tableId },
					Var2 = row
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to add row on Element {0}/{1} table {2}", dmaId, elementId, tableId), e);
			}
		}

		/// <summary>
		/// Adds a row to a table in the provided DataMiner Element and waits until the row is added.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="row">Row to be added.</param>
		/// <param name="timeout">Time to wait for the row creation.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="row"/> is null.</exception>
		/// <exception cref="ArgumentException">If <paramref name="row"/> doesn't contain any cells.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void AddRowWithWait(int dmaId, int elementId, int tableId, object[] row, TimeSpan timeout)
		{
			AddRow(dmaId, elementId, tableId, row);

			var key = row[0].ToString();
			SpinWait.SpinUntil(() => GetColumn<string>(dmaId, elementId, tableId, 0).Contains(key), timeout);
		}

		/// <summary>
		/// Deletes a row from a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to be deleted.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void DeleteRow(int dmaId, int elementId, int tableId, string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (dmaId < 1)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be higher than 0");
			}

			if (elementId < 1)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be higher than 0");
			}

			if (tableId < 1)
			{
				throw new ArgumentOutOfRangeException("tableId", "tableId needs to be higher than 0");
			}

			try
			{
				var request = new SetDataMinerInfoMessage
				{
					DataMinerID = dmaId,
					ElementID = elementId,
					What = 156,
					Var1 = new[] { (uint)dmaId, (uint)elementId, (uint)tableId },
					Var2 = key
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to delete row on Element {0}/{1} table {2} key {3}", dmaId, elementId, tableId, key), e);
			}
		}

		/// <summary>
		/// Deletes rows from a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="keys">Row keys to be deleted.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="keys"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		public static void DeleteRows(int dmaId, int elementId, int tableId, params string[] keys)
		{
			foreach (var key in keys)
			{
				DeleteRow(dmaId, elementId, tableId, key);
			}
		}

		/// <summary>
		/// Deletes rows from a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="keys">Row keys to be deleted.</param>
		/// <param name="timeout">Time to wait for the row deletion.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="keys"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		/// <returns>True if the row was successfully deleted within the <paramref name="timeout"/>;otherwise false.</returns>
		public static bool DeleteRowsWithWait(int dmaId, int elementId, int tableId, string[] keys, TimeSpan timeout)
		{
			DeleteRows(dmaId, elementId, tableId, keys);
			return SpinWait.SpinUntil(() => !keys.Intersect(GetColumn<string>(dmaId, elementId, tableId, 0)).Any(), timeout);
		}

		/// <summary>
		/// Deletes a row from a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the row to be deleted.</param>
		/// <param name="timeout">Time to wait for the row deletion.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the add row failed.</exception>
		/// <returns>True if the row was successfully deleted within the <paramref name="timeout"/>;otherwise false.</returns>
		public static bool DeleteRowWithWait(int dmaId, int elementId, int tableId, string key, TimeSpan timeout)
		{
			DeleteRow(dmaId, elementId, tableId, key);
			return SpinWait.SpinUntil(() => !GetColumn<string>(dmaId, elementId, tableId, 0).Contains(key), timeout);
		}

		/// <summary>
		/// Gets a column with the desired format from any Element on the DMS.
		/// </summary>
		/// <typeparam name="T">Type of the Column.</typeparam>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnIdx">Index of the desired column.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> with the desired column. <see cref="Enumerable.Empty{T}"/> is it was not possible to get the column values.</returns>
		public static IEnumerable<T> GetColumn<T>(
			int dmaId,
			int elementId,
			int tableId,
			uint columnIdx)
			where T : IConvertible
		{
			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			object[] column = null;

			try
			{
				column = response.Value.ArrayValue[columnIdx].ArrayValue.Select(x => x.ArrayValue[0].InteropValue).ToArray();
			}
			catch (Exception)
			{
				yield break;
			}

			for (var i = 0; i < column.Length; i++)
			{
				yield return column[i].ChangeType<T>();
			}
		}

		/// <summary>
		/// Gets two columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
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
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
		{
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 2)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 2");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);

			return GetColumnsIterator(firstColumn, secondColumn, returnSelector);
		}

		/// <summary>
		/// Gets three columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
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
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
		{
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 3)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 3");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, returnSelector);
		}

		/// <summary>
		/// Gets four columns from any Element on the DMS and returns an array with the given selector.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 4)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 4");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 5)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 5");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 6)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 6");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 7)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 7");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, seventhColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 8)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 8");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);
			var eighthColumn = ColumnSelector(response, columnsIdx[7]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, seventhColumn, eighthColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 9)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 9");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);
			var eighthColumn = ColumnSelector(response, columnsIdx[7]);
			var ninthColumn = ColumnSelector(response, columnsIdx[8]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, seventhColumn, eighthColumn, ninthColumn, returnSelector);
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
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 10)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 10");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);
			var eighthColumn = ColumnSelector(response, columnsIdx[7]);
			var ninthColumn = ColumnSelector(response, columnsIdx[8]);
			var tenthColumn = ColumnSelector(response, columnsIdx[9]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, seventhColumn, eighthColumn, ninthColumn, tenthColumn, returnSelector);
		}

		/// <summary>
		/// Gets eleven columns from any Element on the DMS and returns an array with the given selector.
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
		/// <typeparam name="T11">Type of the eleventh Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> returnSelector)
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
			where T11 : IConvertible
		{
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 11)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 11");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);
			var eighthColumn = ColumnSelector(response, columnsIdx[7]);
			var ninthColumn = ColumnSelector(response, columnsIdx[8]);
			var tenthColumn = ColumnSelector(response, columnsIdx[9]);
			var eleventhColumn = ColumnSelector(response, columnsIdx[10]);

			return GetColumnsIterator(firstColumn, secondColumn, thirdColumn, fourthColumn, fifthColumn, sixthColumn, seventhColumn, eighthColumn, ninthColumn, tenthColumn, eleventhColumn, returnSelector);
		}

		/// <summary>
		/// Gets twelfth columns from any Element on the DMS and returns an array with the given selector.
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
		/// <typeparam name="T11">Type of the eleventh Column.</typeparam>
		/// <typeparam name="T12">Type of the twelfth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="tableId">Id of the Table.</param>
		/// <param name="columnsIdx">Array with the Columns Indexes.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Number of columns doesn't match the number of returned members.
		/// </exception>
		public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
			int dmaId,
			int elementId,
			int tableId,
			uint[] columnsIdx,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> returnSelector)
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
			where T11 : IConvertible
			where T12 : IConvertible
		{
			if (columnsIdx == null)
			{
				throw new ArgumentNullException("columnsIdx");
			}

			if (returnSelector == null)
			{
				throw new ArgumentNullException("returnSelector");
			}

			if (columnsIdx.Length != 12)
			{
				throw new ArgumentOutOfRangeException(
					"columnsIdx",
					"Number of columns has to be 12");
			}

			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, tableId);

			var firstColumn = ColumnSelector(response, columnsIdx[0]);
			var secondColumn = ColumnSelector(response, columnsIdx[1]);
			var thirdColumn = ColumnSelector(response, columnsIdx[2]);
			var fourthColumn = ColumnSelector(response, columnsIdx[3]);
			var fifthColumn = ColumnSelector(response, columnsIdx[4]);
			var sixthColumn = ColumnSelector(response, columnsIdx[5]);
			var seventhColumn = ColumnSelector(response, columnsIdx[6]);
			var eighthColumn = ColumnSelector(response, columnsIdx[7]);
			var ninthColumn = ColumnSelector(response, columnsIdx[8]);
			var tenthColumn = ColumnSelector(response, columnsIdx[9]);
			var eleventhColumn = ColumnSelector(response, columnsIdx[10]);
			var twelfthColumn = ColumnSelector(response, columnsIdx[11]);

			return GetColumnsIterator(
				firstColumn,
				secondColumn,
				thirdColumn,
				fourthColumn,
				fifthColumn,
				sixthColumn,
				seventhColumn,
				eighthColumn,
				ninthColumn,
				tenthColumn,
				eleventhColumn,
				twelfthColumn,
				returnSelector);
		}

		/// <summary>
		/// Gets a parameter cell value.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the desired column.</param>
		/// <param name="key">Key of the desired row.</param>
		/// <returns>An object with the raw parameter value.</returns>
		public static object GetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string key)
		{
			var response = DataMinerParameterHelper.GetParameterResponseMessage(dmaId, elementId, columnId, key);

			return response.Value.InteropValue;
		}

		/// <summary>
		/// Gets a parameter cell value.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the desired column.</param>
		/// <param name="key">Key of the desired row.</param>
		/// <returns>An object with the raw parameter value.</returns>
		public static object GetParameterByPrimaryKey(this ElementID elementId, int columnId, string key)
		{
			return GetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, key);
		}

		/// <summary>
		/// Sets the value of a DataMiner table cell.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="key">Primary key of the row.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> and <paramref name="value"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

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
					ParameterId = columnId,
					TableIndex = key,
					Value = new ParameterValue(value),
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set cell on Element {0}/{1} column {2} key {3}", dmaId, elementId, columnId, key), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner table cell.
		/// </summary>
		/// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="key">Primary key of the row.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> and <paramref name="value"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(int dmaId, int elementId, int columnId, string key, int value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			try
			{
				var request = new SetParameterMessage
				{
					DataMinerID = dmaId,
					ElId = elementId,
					DisableInformationEventMessage = true,
					ParameterId = columnId,
					TableIndex = key,
					Value = new ParameterValue(value),
					TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set cell on Element {0}/{1} column {2} key {3}", dmaId, elementId, columnId, key), e);
			}
		}

		/// <summary>
		/// Sets the value of a DataMiner table cell.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="key">Primary key of the row.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> and <paramref name="value"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(this ElementID elementId, int columnId, string key, string value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, key, value);
		}

		/// <summary>
		/// Sets the value of a DataMiner table cell.
		/// </summary>
		/// <param name="elementId">Id of the Element.</param>
		/// <param name="columnId">Id of the column.</param>
		/// <param name="key">Primary key of the row.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="key"/> and <paramref name="value"/> are null.</exception>
		/// <exception cref="DataMinerException">If the set failed.</exception>
		public static void SetParameterByPrimaryKey(this ElementID elementId, int columnId, string key, int value)
		{
			SetParameterByPrimaryKey((int)elementId.DmaId, (int)elementId.ElementId, columnId, key, value);
		}

		/// <summary>
		/// Sets a row to a table in the provided DataMiner Element.
		/// </summary>
		/// <param name="dmaId">ID of the DataMiner Agent.</param>
		/// <param name="elementId">ID of the DataMiner Element.</param>
		/// <param name="tableId">ID of the table.</param>
		/// <param name="key">Key of the rows that should be updated.</param>
		/// <param name="row">Row to be added.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="row"/> or <paramref name="key"/> are null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="dmaId"/>, <paramref name="elementId"/> or <paramref name="tableId"/> are 0 or negative.</exception>
		/// <exception cref="DataMinerException">If the set row failed.</exception>
		public static void SetRow(int dmaId, int elementId, int tableId, string key, object[] row)
		{
			if (row == null)
			{
				throw new ArgumentNullException("row");
			}

			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (dmaId < 1)
			{
				throw new ArgumentOutOfRangeException("dmaId", "dmaId needs to be higher than 0");
			}

			if (elementId < 1)
			{
				throw new ArgumentOutOfRangeException("elementId", "elementId needs to be higher than 0");
			}

			if (tableId < 1)
			{
				throw new ArgumentOutOfRangeException("tableId", "tableId needs to be higher than 0");
			}

			try
			{
				var request = new SetDataMinerInfoMessage
				{
					DataMinerID = dmaId,
					ElementID = elementId,
					What = 225,
					Var1 = new object[] { dmaId, elementId, tableId, key },
					Var2 = row
				};

				SlNet.Value.SendSingleResponseMessage(request);
			}
			catch (Exception e)
			{
				throw new DataMinerException(string.Format("Failed to set row on Element {0}/{1} table {2} key {3}", dmaId, elementId, tableId, key), e);
			}
		}

		/// <summary>
		/// Gets the values of a column from a <see cref="GetParameterResponseMessage"/> object.
		/// </summary>
		/// <param name="response"><see cref="GetParameterResponseMessage"/> object with the parameter values.</param>
		/// <param name="index">Index of the column.</param>
		/// <returns>An <see cref="object"/> array with the column values.</returns>
		private static object[] ColumnSelector(GetParameterResponseMessage response, uint index)
		{
			if (response.Value == null ||
				response.Value.ArrayValue == null)
			{
				return new object[0];
			}

			var column = new object[response.Value.ArrayValue[0].ArrayValue.Length];

			for (var i = 0; i < column.Length; i++)
			{
				try
				{
					column[i] = response.Value.ArrayValue[index].ArrayValue[i].ArrayValue[0].InteropValue;
				}
				catch (Exception)
				{
					column[i] = null;
				}
			}

			return column;
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, TReturn}(int, int, int, uint[], Func{T1, T2, T3, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, TReturn>(object[] firstColumn, object[] secondColumn, object[] thirdColumn, Func<T1, T2, T3, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			Func<T1, T2, T3, T4, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			Func<T1, T2, T3, T4, T5, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			Func<T1, T2, T3, T4, T5, T6, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="T3">Type of the third Column.</typeparam>
		/// <typeparam name="T4">Type of the fourth Column.</typeparam>
		/// <typeparam name="T5">Type of the fifth Column.</typeparam>
		/// <typeparam name="T6">Type of the sixth Column.</typeparam>
		/// <typeparam name="T7">Type of the seventh Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			Func<T1, T2, T3, T4, T5, T6, T7, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
			where T3 : IConvertible
			where T4 : IConvertible
			where T5 : IConvertible
			where T6 : IConvertible
			where T7 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, T8, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, T8, TReturn})"/>.
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
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="eighthColumn"><see cref="object"/> array with the eighth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			object[] eighthColumn,
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
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>(),
					eighthColumn[i].ChangeType<T8>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn})"/>.
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
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="eighthColumn"><see cref="object"/> array with the eighth column raw values.</param>
		/// <param name="ninthColumn"><see cref="object"/> array with the ninth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			object[] eighthColumn,
			object[] ninthColumn,
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
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>(),
					eighthColumn[i].ChangeType<T8>(),
					ninthColumn[i].ChangeType<T9>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn})"/>.
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
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="eighthColumn"><see cref="object"/> array with the eighth column raw values.</param>
		/// <param name="ninthColumn"><see cref="object"/> array with the ninth column raw values.</param>
		/// <param name="tenthColumn"><see cref="object"/> array with the tenth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			object[] eighthColumn,
			object[] ninthColumn,
			object[] tenthColumn,
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
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>(),
					eighthColumn[i].ChangeType<T8>(),
					ninthColumn[i].ChangeType<T9>(),
					tenthColumn[i].ChangeType<T10>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn})"/>.
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
		/// <typeparam name="T11">Type of the eleventh Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="eighthColumn"><see cref="object"/> array with the eighth column raw values.</param>
		/// <param name="ninthColumn"><see cref="object"/> array with the ninth column raw values.</param>
		/// <param name="tenthColumn"><see cref="object"/> array with the tenth column raw values.</param>
		/// <param name="eleventhColumn"><see cref="object"/> array with the eleventh column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			object[] eighthColumn,
			object[] ninthColumn,
			object[] tenthColumn,
			object[] eleventhColumn,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> returnSelector)
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
			where T11 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>(),
					eighthColumn[i].ChangeType<T8>(),
					ninthColumn[i].ChangeType<T9>(),
					tenthColumn[i].ChangeType<T10>(),
					eleventhColumn[i].ChangeType<T11>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn}(int, int, int, uint[], Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn})"/>.
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
		/// <typeparam name="T11">Type of the eleventh Column.</typeparam>
		/// <typeparam name="T12">Type of the twelfth Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="thirdColumn"><see cref="object"/> array with the third column raw values.</param>
		/// <param name="fourthColumn"><see cref="object"/> array with the fourth column raw values.</param>
		/// <param name="fifthColumn"><see cref="object"/> array with the fifth column raw values.</param>
		/// <param name="sixthColumn"><see cref="object"/> array with the sixth column raw values.</param>
		/// <param name="seventhColumn"><see cref="object"/> array with the seventh column raw values.</param>
		/// <param name="eighthColumn"><see cref="object"/> array with the eighth column raw values.</param>
		/// <param name="ninthColumn"><see cref="object"/> array with the ninth column raw values.</param>
		/// <param name="tenthColumn"><see cref="object"/> array with the tenth column raw values.</param>
		/// <param name="eleventhColumn"><see cref="object"/> array with the eleventh column raw values.</param>
		/// <param name="twelfthColumn"><see cref="object"/> array with the twelfth column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
			object[] firstColumn,
			object[] secondColumn,
			object[] thirdColumn,
			object[] fourthColumn,
			object[] fifthColumn,
			object[] sixthColumn,
			object[] seventhColumn,
			object[] eighthColumn,
			object[] ninthColumn,
			object[] tenthColumn,
			object[] eleventhColumn,
			object[] twelfthColumn,
			Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> returnSelector)
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
			where T11 : IConvertible
			where T12 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>(),
					thirdColumn[i].ChangeType<T3>(),
					fourthColumn[i].ChangeType<T4>(),
					fifthColumn[i].ChangeType<T5>(),
					sixthColumn[i].ChangeType<T6>(),
					seventhColumn[i].ChangeType<T7>(),
					eighthColumn[i].ChangeType<T8>(),
					ninthColumn[i].ChangeType<T9>(),
					tenthColumn[i].ChangeType<T10>(),
					eleventhColumn[i].ChangeType<T11>(),
					twelfthColumn[i].ChangeType<T12>());
			}
		}

		/// <summary>
		/// Iterator for method <see cref="GetColumns{T1, T2, TReturn}(int, int, int, uint[], Func{T1, T2, TReturn})"/>.
		/// </summary>
		/// <typeparam name="T1">Type of the first Column.</typeparam>
		/// <typeparam name="T2">Type of the second Column.</typeparam>
		/// <typeparam name="TReturn">Type of the return value.</typeparam>
		/// <param name="firstColumn"><see cref="object"/> array with the first column raw values.</param>
		/// <param name="secondColumn"><see cref="object"/> array with the second column raw values.</param>
		/// <param name="returnSelector">A function to map each column element to a return element.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired column.</returns>
		private static IEnumerable<TReturn> GetColumnsIterator<T1, T2, TReturn>(object[] firstColumn, object[] secondColumn, Func<T1, T2, TReturn> returnSelector)
			where T1 : IConvertible
			where T2 : IConvertible
		{
			for (var i = 0; i < firstColumn.Length; i++)
			{
				yield return returnSelector(
					firstColumn[i].ChangeType<T1>(),
					secondColumn[i].ChangeType<T2>());
			}
		}
	}
}
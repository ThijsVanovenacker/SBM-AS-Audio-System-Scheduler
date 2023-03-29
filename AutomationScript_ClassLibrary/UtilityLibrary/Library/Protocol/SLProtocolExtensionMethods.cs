namespace Skyline.DataMiner.Library.Protocol
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Skyline.DataMiner.Library.Common.Attributes;
    using Skyline.DataMiner.Library.Exceptions;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Scripting;

    /// <summary>
    /// Class with <see cref="SLProtocol"/> extension methods.
    /// </summary>
    [DllImport("QActionHelperBaseClasses.dll")]
    public static class SLProtocolExtensionMethods
    {
        /// <summary>
        /// Deletes all rows from a table.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">ID of the table.</param>
        /// <param name="keysColumnIdx">Index of the keys column.</param>
        public static void ClearTable(this SLProtocol protocol, int tableId, uint keysColumnIdx = 0)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            protocol.NotifyProtocol(156, tableId, protocol.GetColumn<string>(tableId, keysColumnIdx).ToArray());
        }

        /// <summary>
        /// Joins the values of a specific column in a ';' separated list, used for dependencyId drop-downs.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name = "tablePid">ID of the table.</param>
        /// <param name = "columnIdx">Index of the desired column.</param>
        /// <returns>A string with the values of a specific column separated by ';'.</returns>
        public static string ConvertColumnToString(this SLProtocol protocol, int tablePid, int columnIdx)
        {
            var column = protocol.GetColumn<string>(tablePid, (uint)columnIdx);
            return string.Join(";", column.Distinct());
        }

        /// <summary>
        /// Gets All Protocols from a DMS.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="GetProtocolsResponseMessage"/> with all Protocols on the DMS.
        /// </returns>
        public static IEnumerable<GetProtocolsResponseMessage> GetAllProtocols()
        {
            return DataMinerSystemInfo.GetAllProtocols();
        }

        /// <summary>
        /// Get the name of the available Vdx Files on the DMS.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <returns>A string array with the available Vdx Files.</returns>
        public static string[] GetAvailableVdxFiles(this SLProtocol protocol)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var message = new GetAvailableFilesMessage(7, string.Empty);

            var dmsMessage = protocol.SLNet.SendMessage(message);

            var response = (GetAvailableFilesResponseMessage)dmsMessage[0];

            return response.Sa.Sa;
        }

        /// <summary>
        /// Gets a buffer stored (by <see cref="SLProtocolExtensionMethods.SetBuffer{T}(SLProtocol, int, IEnumerable{T})"/>) in a DataMiner parameter using Serialization.
        /// </summary>
        /// <typeparam name="T">Type of the object to get.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramId">Id of the parameter where the object is stored.</param>
        /// <returns>An instance with the stored object.</returns>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
        /// The type being serialized does not conform to data contract rules.
        /// </exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// There is a problem with the instance being written.
        /// </exception>
        /// <exception cref="System.ServiceModel.QuotaExceededException">
        /// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
        /// </exception>
        public static T GetBuffer<T>(this SLProtocol protocol, int paramId)
            where T : IEnumerable, new()
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var content = protocol.GetParameter<string>(paramId);

            if (content == string.Empty)
            {
                return new T();
            }

            return content.DeserializeDataContractJsonObject<T>();
        }

        /// <summary>
        /// Gets a column with the desired format.
        /// </summary>
        /// <typeparam name="T">Type of the Column.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">ID of the table.</param>
        /// <param name="columnIdx">Index of the desired column.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with the desired column.</returns>
        public static IEnumerable<T> GetColumn<T>(this SLProtocol protocol, int tableId, uint columnIdx)
            where T : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var column = (object[])((object[])protocol.NotifyProtocol(321, tableId, new[] { columnIdx }))[0];

            for (var i = 0; i < column.Length; i++)
            {
                yield return column[i].ChangeType<T>();
            }
        }

        /// <summary>
        /// Gets a column with the desired format from any Element on the DMS.
        /// </summary>
        /// <typeparam name="T">Type of the Column.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
        /// <param name="elementId">Id of the Element.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnIdx">Index of the desired column.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with the desired column. <see cref="Enumerable.Empty{T}"/> is it was not possible to get the column values.</returns>
        public static IEnumerable<T> GetColumn<T>(
            this SLProtocol protocol,
            int dmaId,
            int elementId,
            int tableId,
            uint columnIdx)
            where T : IConvertible
        {
            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

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
        /// Gets a column as dictionary, with the table keys as keys for this dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys.</typeparam>
        /// <typeparam name="TValue">Type of the Values.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">ID of the Table.</param>
        /// <param name="keyIndex">Index of the table keys column.</param>
        /// <param name="columnIdx">Index of the column to retrieve.</param>
        /// <returns>A dictionary with the desired column.</returns>
        public static Dictionary<TKey, TValue> GetColumnAsDictionary<TKey, TValue>(
            this SLProtocol protocol,
            int tableId,
            uint keyIndex,
            uint columnIdx)
            where TKey : IConvertible
            where TValue : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var columns = (object[])protocol.NotifyProtocol(321, tableId, new[] { keyIndex, columnIdx });

            var keys = (object[])columns[0];
            var values = (object[])columns[1];

            var retrunValue = new Dictionary<TKey, TValue>();

            for (var i = 0; i < keys.Length; i++)
            {
                retrunValue.Add(
                    keys[i].ChangeType<TKey>(),
                    values[i].ChangeType<TValue>());
            }

            return retrunValue;
        }

        /// <summary>
        /// Gets two columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>());
            }
        }

        /// <summary>
        /// Gets three columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>());
            }
        }

        /// <summary>
        /// Gets four columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, T4, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>());
            }
        }

        /// <summary>
        /// Gets five columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, T4, T5, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where T5 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>());
            }
        }

        /// <summary>
        /// Gets six columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="T6">Type of the sixth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>());
            }
        }

        /// <summary>
        /// Gets seven columns from a table and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="T6">Type of the sixth Column.</typeparam>
        /// <typeparam name="T7">Type of the seventh Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>());
            }
        }

        /// <summary>
        /// Gets eight columns from a table and returns an array with the given selector.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>());
            }
        }

        /// <summary>
        /// Gets nine columns from a table and returns an array with the given selector.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>());
            }
        }

        /// <summary>
        /// Gets fourteen columns from a table and returns an array with the given selector.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>(),
                    ((object[])columns[9])[i].ChangeType<T10>());
            }
        }

        /// <summary>
        /// Gets fourteen columns from a table and returns an array with the given selector.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>(),
                    ((object[])columns[9])[i].ChangeType<T10>(),
                    ((object[])columns[10])[i].ChangeType<T11>());
            }
        }

        /// <summary>
        /// Gets fourteen columns from a table and returns an array with the given selector.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>(),
                    ((object[])columns[9])[i].ChangeType<T10>(),
                    ((object[])columns[10])[i].ChangeType<T11>(),
                    ((object[])columns[11])[i].ChangeType<T12>());
            }
        }

        /// <summary>
        /// Gets fourteen columns from a table and returns an array with the given selector.
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
        /// <typeparam name="T13">Type of the thirteenth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> returnSelector)
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
            where T13 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (columnsIdx == null)
            {
                throw new ArgumentNullException("columnsIdx");
            }

            if (returnSelector == null)
            {
                throw new ArgumentNullException("returnSelector");
            }

            if (columnsIdx.Length != 13)
            {
                throw new ArgumentOutOfRangeException(
                    "columnsIdx",
                    "Number of columns has to be 13");
            }

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>(),
                    ((object[])columns[9])[i].ChangeType<T10>(),
                    ((object[])columns[10])[i].ChangeType<T11>(),
                    ((object[])columns[11])[i].ChangeType<T12>(),
                    ((object[])columns[12])[i].ChangeType<T13>());
            }
        }

        /// <summary>
        /// Gets fourteen columns from a table and returns an array with the given selector.
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
        /// <typeparam name="T13">Type of the thirteenth Column.</typeparam>
        /// <typeparam name="T14">Type of the fourteenth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> returnSelector)
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
            where T13 : IConvertible
            where T14 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (columnsIdx == null)
            {
                throw new ArgumentNullException("columnsIdx");
            }

            if (returnSelector == null)
            {
                throw new ArgumentNullException("returnSelector");
            }

            if (columnsIdx.Length != 14)
            {
                throw new ArgumentOutOfRangeException(
                    "columnsIdx",
                    "Number of columns has to be 14");
            }

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                yield return returnSelector(
                    ((object[])columns[0])[i].ChangeType<T1>(),
                    ((object[])columns[1])[i].ChangeType<T2>(),
                    ((object[])columns[2])[i].ChangeType<T3>(),
                    ((object[])columns[3])[i].ChangeType<T4>(),
                    ((object[])columns[4])[i].ChangeType<T5>(),
                    ((object[])columns[5])[i].ChangeType<T6>(),
                    ((object[])columns[6])[i].ChangeType<T7>(),
                    ((object[])columns[7])[i].ChangeType<T8>(),
                    ((object[])columns[8])[i].ChangeType<T9>(),
                    ((object[])columns[9])[i].ChangeType<T10>(),
                    ((object[])columns[10])[i].ChangeType<T11>(),
                    ((object[])columns[11])[i].ChangeType<T12>(),
                    ((object[])columns[12])[i].ChangeType<T13>(),
                    ((object[])columns[13])[i].ChangeType<T14>());
            }
        }

        /// <summary>
        /// Gets two columns from any Element on the DMS and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
            int dmaId,
            int elementId,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);

            for (var i = 0; i < firstColumn.Length; i++)
            {
                yield return returnSelector(
                    firstColumn[i].ChangeType<T1>(),
                    secondColumn[i].ChangeType<T2>());
            }
        }

        /// <summary>
        /// Gets three columns from any Element on the DMS and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
            int dmaId,
            int elementId,
            int tableId,
            uint[] columnsIdx,
            Func<T1, T2, T3, TReturn> returnSelector)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);

            for (var i = 0; i < firstColumn.Length; i++)
            {
                yield return returnSelector(
                    firstColumn[i].ChangeType<T1>(),
                    secondColumn[i].ChangeType<T2>(),
                    thirdColumn[i].ChangeType<T3>());
            }
        }

        /// <summary>
        /// Gets four columns from any Element on the DMS and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);

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
        /// Gets five columns from any Element on the DMS and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);
            var fifthColumn = ColumnSelector(response, columnsIdx[4]);

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
        /// Gets six columns from any Element on the DMS and returns an array with the given selector.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="T6">Type of the sixth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);
            var fifthColumn = ColumnSelector(response, columnsIdx[4]);
            var sixthColumn = ColumnSelector(response, columnsIdx[5]);

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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);
            var fifthColumn = ColumnSelector(response, columnsIdx[4]);
            var sixthColumn = ColumnSelector(response, columnsIdx[5]);
            var seventhColumn = ColumnSelector(response, columnsIdx[6]);

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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);
            var fifthColumn = ColumnSelector(response, columnsIdx[4]);
            var sixthColumn = ColumnSelector(response, columnsIdx[5]);
            var seventhColumn = ColumnSelector(response, columnsIdx[6]);
            var eighthColumn = ColumnSelector(response, columnsIdx[7]);

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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
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
            this SLProtocol protocol,
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var response = protocol.GetParameterResponseMessage(dmaId, elementId, tableId);

            var firstColumn = ColumnSelector(response, columnsIdx[0]);
            var secondColumn = ColumnSelector(response, columnsIdx[1]);
            var thirdColumn = ColumnSelector(response, columnsIdx[2]);
            var fourthColumn = ColumnSelector(response, columnsIdx[3]);
            var fifthColumn = ColumnSelector(response, columnsIdx[4]);
            var sixthColumn = ColumnSelector(response, columnsIdx[5]);
            var seventhColumn = ColumnSelector(response, columnsIdx[6]);
            var eighthColumn = ColumnSelector(response, columnsIdx[7]);
            var ninthColumn = ColumnSelector(response, columnsIdx[8]);

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
        /// Gets the information of all DataMiner agents in a cluster.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="GetDataMinerInfoResponseMessage"/> with the information of all DataMiner agents in a cluster.
        /// </returns>
        public static IEnumerable<GetDataMinerInfoResponseMessage> GetDataMinerAgentsInfo(this SLProtocol protocol)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var getInfoMessage = new GetInfoMessage(InfoType.DataMinerInfo);

            var dmsMessage = protocol.SLNet.SendMessage(getInfoMessage);

            return dmsMessage.Cast<GetDataMinerInfoResponseMessage>();
        }

        /// <summary>
        /// Get the DVEs of an Element.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner Agent where the element is running.</param>
        /// <param name="elementId">Id of the element.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of GetElementLinksResponseMessage with the information about the given element DVEs.</returns>
        public static IEnumerable<GetElementLinksResponseMessage> GetElementDves(this SLProtocol protocol, int dmaId, int elementId)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var request = new GetElementLinksMessage(dmaId, elementId, ElementLinkInfoType.All);

            try
            {
                var response = (GetElementLinksResponseMessage)protocol.SLNet.SendMessage(request)[0];

                return response.DVEs;
            }
            catch (Exception)
            {
                return Enumerable.Empty<GetElementLinksResponseMessage>();
            }
        }

        /// <summary>
        /// Gets the table key based on a display key.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the table.</param>
        /// <param name="displayKey"><see cref="string"/> with the display key that should be searched.</param>
        /// <returns>A <see cref="string"/> with the corresponding key.</returns>
        /// <exception cref="KeyNotFoundException">If the table doesn't contains the given displayKey.</exception>
        /// <exception cref="InvalidOperationException">If there are multiple rows with the given displayKey.</exception>
        public static string GetKeyByDisplayKey(this SLProtocol protocol, int tableId, string displayKey)
        {
            var keysByDisplayKey = protocol.GetKeysToDisplayKeysMap(tableId).ToLookup(x => x.Value, x => x.Key);

            if (!keysByDisplayKey.Contains(displayKey))
            {
                throw new KeyNotFoundException(string.Format("Table {0} doesn't contain DisplayKey {1}", tableId, displayKey));
            }

            try
            {
                return keysByDisplayKey[displayKey].Single();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(string.Format("Table {0} contains multiple rows with DisplayKey {1}", tableId, displayKey), e);
            }
        }

        /// <summary>
        /// Gets a Dictionary with a table mapping between Keys and DisplayKeys of a table.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the table.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> with a table mapping between Keys and DisplayKeys.</returns>
        public static Dictionary<string, string> GetKeysToDisplayKeysMap(this SLProtocol protocol, int tableId)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            object[] result = (object[])protocol.NotifyProtocol(168, tableId, null);

            var keys = (object[])result[0];
            var displayKeys = (object[])result[1];

            var returnValue = new Dictionary<string, string>();

            for (int i = 0; i < keys.Length; i++)
            {
                returnValue.Add(keys[i].ChangeType<string>(), displayKeys[i].ChangeType<string>());
            }

            return returnValue;
        }

        /// <summary>
        ///     Gets a column with the desired format. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T">Type of the Column.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">ID of the table.</param>
        /// <param name="columnIdx">Index of the desired column.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with the desired column.</returns>
        public static IEnumerable<T?> GetNullableColumn<T>(this SLProtocol protocol, int tableId, uint columnIdx)
            where T : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var column = (object[])((object[])protocol.NotifyProtocol(321, tableId, new[] { columnIdx }))[0];

            for (var i = 0; i < column.Length; i++)
            {
                if (column[i] != null)
                {
                    yield return (T?)column[i].ChangeType<T>();
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Gets two columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null);
            }
        }

        /// <summary>
        /// Gets eight columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null);
            }
        }

        /// <summary>
        /// Gets four columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null);
            }
        }

        /// <summary>
        /// Gets five columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, T5, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, T5?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
            where T5 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];
                var fifthColumnValue = ((object[])columns[4])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null,
                    fifthColumnValue != null ? (T5?)secondColumnValue.ChangeType<T5>() : null);
            }
        }

        /// <summary>
        /// Gets six columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="T6">Type of the sixth Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, T5, T6, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, T5?, T6?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
            where T5 : struct, IConvertible
            where T6 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];
                var fifthColumnValue = ((object[])columns[4])[i];
                var sixthColumnValue = ((object[])columns[5])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null,
                    fifthColumnValue != null ? (T5?)secondColumnValue.ChangeType<T5>() : null,
                    sixthColumnValue != null ? (T6?)secondColumnValue.ChangeType<T6>() : null);
            }
        }

        /// <summary>
        /// Gets seven columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
        /// </summary>
        /// <typeparam name="T1">Type of the first Column.</typeparam>
        /// <typeparam name="T2">Type of the second Column.</typeparam>
        /// <typeparam name="T3">Type of the third Column.</typeparam>
        /// <typeparam name="T4">Type of the fourth Column.</typeparam>
        /// <typeparam name="T5">Type of the fifth Column.</typeparam>
        /// <typeparam name="T6">Type of the sixth Column.</typeparam>
        /// <typeparam name="T7">Type of the seventh Column.</typeparam>
        /// <typeparam name="TReturn">Type of the return value.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, T5, T6, T7, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
            where T5 : struct, IConvertible
            where T6 : struct, IConvertible
            where T7 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];
                var fifthColumnValue = ((object[])columns[4])[i];
                var sixthColumnValue = ((object[])columns[5])[i];
                var seventhColumnValue = ((object[])columns[6])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null,
                    fifthColumnValue != null ? (T5?)secondColumnValue.ChangeType<T5>() : null,
                    sixthColumnValue != null ? (T6?)secondColumnValue.ChangeType<T6>() : null,
                    seventhColumnValue != null ? (T7?)secondColumnValue.ChangeType<T7>() : null);
            }
        }

        /// <summary>
        /// Gets eight columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
            where T5 : struct, IConvertible
            where T6 : struct, IConvertible
            where T7 : struct, IConvertible
            where T8 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];
                var fifthColumnValue = ((object[])columns[4])[i];
                var sixthColumnValue = ((object[])columns[5])[i];
                var seventhColumnValue = ((object[])columns[6])[i];
                var eighthColumnValue = ((object[])columns[7])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null,
                    fifthColumnValue != null ? (T5?)secondColumnValue.ChangeType<T5>() : null,
                    sixthColumnValue != null ? (T6?)secondColumnValue.ChangeType<T6>() : null,
                    seventhColumnValue != null ? (T7?)secondColumnValue.ChangeType<T7>() : null,
                    eighthColumnValue != null ? (T8?)secondColumnValue.ChangeType<T8>() : null);
            }
        }

        /// <summary>
        /// Gets nine columns from a table and returns an array with the given selector. If values are Not Initialized returned values will be null.
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
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the Table.</param>
        /// <param name="columnsIdx">Array with the Columns Indexes.</param>
        /// <param name="returnSelector">A function to map each column element to a return element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TReturn"/> with the desired columns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Number of columns doesn't match the number of returned members.
        /// </exception>
        public static IEnumerable<TReturn> GetNullableColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
            this SLProtocol protocol,
            int tableId,
            uint[] columnsIdx,
            Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, TReturn> returnSelector)
            where T1 : struct, IConvertible
            where T2 : struct, IConvertible
            where T3 : struct, IConvertible
            where T4 : struct, IConvertible
            where T5 : struct, IConvertible
            where T6 : struct, IConvertible
            where T7 : struct, IConvertible
            where T8 : struct, IConvertible
            where T9 : struct, IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

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

            var columns = (object[])protocol.NotifyProtocol(321, tableId, columnsIdx);

            for (var i = 0; i < ((object[])columns[0]).Length; i++)
            {
                var firstColumnValue = ((object[])columns[0])[i];
                var secondColumnValue = ((object[])columns[1])[i];
                var thirdColumnValue = ((object[])columns[2])[i];
                var fourthColumnValue = ((object[])columns[3])[i];
                var fifthColumnValue = ((object[])columns[4])[i];
                var sixthColumnValue = ((object[])columns[5])[i];
                var seventhColumnValue = ((object[])columns[6])[i];
                var eighthColumnValue = ((object[])columns[7])[i];
                var ninethColumnValue = ((object[])columns[8])[i];

                yield return returnSelector(
                    firstColumnValue != null ? (T1?)firstColumnValue.ChangeType<T1>() : null,
                    secondColumnValue != null ? (T2?)secondColumnValue.ChangeType<T2>() : null,
                    thirdColumnValue != null ? (T3?)secondColumnValue.ChangeType<T3>() : null,
                    fourthColumnValue != null ? (T4?)secondColumnValue.ChangeType<T4>() : null,
                    fifthColumnValue != null ? (T5?)secondColumnValue.ChangeType<T5>() : null,
                    sixthColumnValue != null ? (T6?)secondColumnValue.ChangeType<T6>() : null,
                    seventhColumnValue != null ? (T7?)secondColumnValue.ChangeType<T7>() : null,
                    eighthColumnValue != null ? (T8?)secondColumnValue.ChangeType<T8>() : null,
                    ninethColumnValue != null ? (T9?)secondColumnValue.ChangeType<T9>() : null);
            }
        }

        /// <summary>
        /// Get the number of DVEs of a DataMiner Element.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner Agent where the element is running.</param>
        /// <param name="elementId">Id of the element.</param>
        /// <returns>An <see cref="int"/> with the number of DVEs.</returns>
        public static int GetNumberOfDves(this SLProtocol protocol, int dmaId, int elementId)
        {
            return protocol.GetElementDves(dmaId, elementId).Count();
        }

        /// <summary>
        /// Executes a <see cref="SLProtocol.GetParameter(int)"/> and return the value in the desired format.
        /// </summary>
        /// <typeparam name="T">Type of the Parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramId">Id of the parameter to retrieve.</param>
        /// <returns>The parameter value.</returns>
        public static T GetParameter<T>(this SLProtocol protocol, int paramId)
            where T : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            return protocol.GetParameter(paramId).ChangeType<T>();
        }

        /// <summary>
        /// Executes a <see cref="SLProtocol.GetParameterIndexByKey(int, string, int)"/> and return the value in the desired format.
        /// </summary>
        /// <typeparam name="T">Desired return Type.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="tableId">Id of the table.</param>
        /// <param name="key">Key of the desired row.</param>
        /// <param name="columnIdx">Index of the desired column (1 based index).</param>
        /// <returns>The value of the desired cell.</returns>
        public static T GetParameterIndexByKey<T>(this SLProtocol protocol, int tableId, string key, int columnIdx)
            where T : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            return protocol.GetParameterIndexByKey(tableId, key, columnIdx).ChangeType<T>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2)
            where T1 : IConvertible
            where T2 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 2)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2, out T3 param3)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 3)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2, out T3 param3, out T4 param4)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 4)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2, out T3 param3, out T4 param4, out T5 param5)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where T5 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 5)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2, out T3 param3, out T4 param4, out T5 param5, out T6 param6)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where T5 : IConvertible
            where T6 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 6)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <typeparam name="T7">Type of the seventh parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <param name="param7">Out variable with the seventh parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6, T7>(
            this SLProtocol protocol,
            uint[] paramIds,
            out T1 param1,
            out T2 param2,
            out T3 param3,
            out T4 param4,
            out T5 param5,
            out T6 param6,
            out T7 param7)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where T5 : IConvertible
            where T6 : IConvertible
            where T7 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 7)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
            param7 = parameters[6].ChangeType<T7>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <typeparam name="T7">Type of the seventh parameter.</typeparam>
        /// <typeparam name="T8">Type of the eighth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <param name="param7">Out variable with the seventh parameter value.</param>
        /// <param name="param8">Out variable with the eighth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6, T7, T8>(
            this SLProtocol protocol,
            uint[] paramIds,
            out T1 param1,
            out T2 param2,
            out T3 param3,
            out T4 param4,
            out T5 param5,
            out T6 param6,
            out T7 param7,
            out T8 param8)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where T5 : IConvertible
            where T6 : IConvertible
            where T7 : IConvertible
            where T8 : IConvertible
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 8)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
            param7 = parameters[6].ChangeType<T7>();
            param8 = parameters[7].ChangeType<T8>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <typeparam name="T7">Type of the seventh parameter.</typeparam>
        /// <typeparam name="T8">Type of the eighth parameter.</typeparam>
        /// <typeparam name="T9">Type of the ninth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <param name="param7">Out variable with the seventh parameter value.</param>
        /// <param name="param8">Out variable with the eighth parameter value.</param>
        /// <param name="param9">Out variable with the ninth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this SLProtocol protocol,
            uint[] paramIds,
            out T1 param1,
            out T2 param2,
            out T3 param3,
            out T4 param4,
            out T5 param5,
            out T6 param6,
            out T7 param7,
            out T8 param8,
            out T9 param9)
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 9)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
            param7 = parameters[6].ChangeType<T7>();
            param8 = parameters[7].ChangeType<T8>();
            param9 = parameters[8].ChangeType<T9>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <typeparam name="T7">Type of the seventh parameter.</typeparam>
        /// <typeparam name="T8">Type of the eighth parameter.</typeparam>
        /// <typeparam name="T9">Type of the ninth parameter.</typeparam>
        /// <typeparam name="T10">Type of the tenth parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <param name="param7">Out variable with the seventh parameter value.</param>
        /// <param name="param8">Out variable with the eighth parameter value.</param>
        /// <param name="param9">Out variable with the ninth parameter value.</param>
        /// <param name="param10">Out variable with the tenth parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this SLProtocol protocol,
            uint[] paramIds,
            out T1 param1,
            out T2 param2,
            out T3 param3,
            out T4 param4,
            out T5 param5,
            out T6 param6,
            out T7 param7,
            out T8 param8,
            out T9 param9,
            out T10 param10)
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 10)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
            param7 = parameters[6].ChangeType<T7>();
            param8 = parameters[7].ChangeType<T8>();
            param9 = parameters[8].ChangeType<T9>();
            param10 = parameters[9].ChangeType<T10>();
        }

        /// <summary>
        /// Gets the desired parameters and converts to the given types.
        /// </summary>
        /// <typeparam name="T1">Type of the fist parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <typeparam name="T3">Type of the third parameter.</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter.</typeparam>
        /// <typeparam name="T7">Type of the seventh parameter.</typeparam>
        /// <typeparam name="T8">Type of the eighth parameter.</typeparam>
        /// <typeparam name="T9">Type of the ninth parameter.</typeparam>
        /// <typeparam name="T10">Type of the tenth parameter.</typeparam>
        /// <typeparam name="T11">Type of the eleventh parameter.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
        /// <param name="param1">Out variable with the first parameter value.</param>
        /// <param name="param2">Out variable with the second parameter value.</param>
        /// <param name="param3">Out variable with the third parameter value.</param>
        /// <param name="param4">Out variable with the fourth parameter value.</param>
        /// <param name="param5">Out variable with the fifth parameter value.</param>
        /// <param name="param6">Out variable with the sixth parameter value.</param>
        /// <param name="param7">Out variable with the seventh parameter value.</param>
        /// <param name="param8">Out variable with the eighth parameter value.</param>
        /// <param name="param9">Out variable with the ninth parameter value.</param>
        /// <param name="param10">Out variable with the tenth parameter value.</param>
        /// <param name="param11">Out variable with the eleventh parameter value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the length of the paramIds is different from the number of out parameters.
        /// </exception>
        public static void GetParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this SLProtocol protocol,
            uint[] paramIds,
            out T1 param1,
            out T2 param2,
            out T3 param3,
            out T4 param4,
            out T5 param5,
            out T6 param6,
            out T7 param7,
            out T8 param8,
            out T9 param9,
            out T10 param10,
            out T11 param11)
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
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (paramIds == null)
            {
                throw new ArgumentNullException("paramIds");
            }

            if (paramIds.Length != 11)
            {
                throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
            }

            object[] parameters = (object[])protocol.GetParameters(paramIds);

            param1 = parameters[0].ChangeType<T1>();
            param2 = parameters[1].ChangeType<T2>();
            param3 = parameters[2].ChangeType<T3>();
            param4 = parameters[3].ChangeType<T4>();
            param5 = parameters[4].ChangeType<T5>();
            param6 = parameters[5].ChangeType<T6>();
            param7 = parameters[6].ChangeType<T7>();
            param8 = parameters[7].ChangeType<T8>();
            param9 = parameters[8].ChangeType<T9>();
            param10 = parameters[9].ChangeType<T10>();
            param11 = parameters[10].ChangeType<T11>();
        }

        /// <summary>
        /// Gets the information of the specified DataMiner Protocol.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="name">Name of the protocol.</param>
        /// <param name="version">Version of the protocol.</param>
        /// <param name="raw">Value indicating whether raw data should be fetched.</param>
        /// <returns>A <see cref="GetProtocolInfoResponseMessage"/> object with the protocol information.</returns>
        public static GetProtocolInfoResponseMessage GetProtocolInfo(
            this SLProtocol protocol,
            string name,
            string version = "Production",
            bool raw = true)
        {
            try
            {
                return ProtocolInfoCache.GetCacheProtocol(protocol.SLNet.RawConnection, name, version);
            }
            catch (Exception e)
            {
                throw new ProtocolNotFoundException(string.Format("Protocol: {0} Version: {1} doesn't exist", name, version), e);
            }
        }

        /// <summary>
        /// Gets all views where an Element/Service is included.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner agent where the Element/Service is running.</param>
        /// <param name="elementId">Id of the Element/Service.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="DataMinerObjectInfo"/> with all the Views where an Element/Service is included.</returns>
        public static IEnumerable<DataMinerObjectInfo> GetViewsForElement(this SLProtocol protocol, int dmaId, int elementId)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var getViewsForElementMessage = new GetViewsForElementMessage
            {
                DataMinerID = dmaId,
                ElementID = elementId
            };
            var dmsMessage = protocol.SLNet.SendMessage(getViewsForElementMessage);
            return ((GetViewsForElementResponse)dmsMessage[0]).Views;
        }

        /// <summary>
        /// Logs the time to run an action.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="action"><see cref="Action"/> to execute.</param>
        /// <param name="message">String with the message to display on the log.</param>
        /// <param name="logLevel">Set the log level. Default value = Level1.</param>
        public static void LogTimeToRun(this SLProtocol protocol, Action action, string message = null, Skyline.DataMiner.Scripting.LogLevel logLevel = Skyline.DataMiner.Scripting.LogLevel.Level1)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var start = DateTime.Now;

            action();

            var logMessage = message == null
                                ? string.Format(CultureInfo.InvariantCulture, "QA{1}|QAction {1} took {0}ms to Run", (DateTime.Now - start).TotalMilliseconds, protocol.QActionID)
                                : string.Format(CultureInfo.InvariantCulture, "QA{2}|{1} took {0}ms to Run", (DateTime.Now - start).TotalMilliseconds, message, protocol.QActionID);
            protocol.Log(logMessage, LogType.Information, logLevel);
        }

        /// <summary>
        /// Releases the Ownership of an Alarm.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner agent.</param>
        /// <param name="alarmId">Id of the alarm.</param>
        public static void ReleaseAlarmOwnership(this SLProtocol protocol, int dmaId, int alarmId)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var setAlarmStateMessage = new SetAlarmStateMessage(dmaId, alarmId, 3 /*Release Owner*/, string.Empty);
            protocol.SLNet.SendMessage(setAlarmStateMessage);
        }

        /// <summary>
        /// Renames the Root View.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="newName">Name to apply on the root View.</param>
        public static void RenameRootView(this SLProtocol protocol, string newName)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            var cdm = new GetInfoMessage(InfoType.GeneralInfoMessage)
            {
                DataMinerID = protocol.DataMinerID
            };

            var cim = new SetCustomerInfoMessage();
            var cdmresp = protocol.SLNet.SendMessage(cdm);

            foreach (var dmsMessage in cdmresp)
            {
                var giem = (GeneralInfoEventMessage)dmsMessage;
                cim.Address = giem.CustomerAddress;
                cim.City = giem.CustomerCity;
                cim.Contact1 = giem.SystemContact1;
                cim.Contact2 = giem.SystemContact2;
                cim.Contact3 = giem.SystemContact3;
                cim.Country = giem.CustomerCountry;
                cim.Email = giem.CustomerEmail;
                cim.Fax = giem.CustomerFax;
                cim.Name = newName; // here the magic happens
                cim.Tel = giem.CustomerTel;
                cim.Website = giem.CustomerWebsite;
            }

            protocol.SLNet.SendMessage(cim);
        }

        /// <summary>
        /// Stores an object into a DataMiner parameter using Serialization.
        /// </summary>
        /// <typeparam name="T">Type of the object to store.</typeparam>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="paramId">Id of the parameter where the object is stored.</param>
        /// <param name="data">Object to store.</param>
        /// <exception cref="ArgumentNullException">Data is null.</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
        /// The type being serialized does not conform to data contract rules.
        /// </exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// There is a problem with the instance being written.
        /// </exception>
        /// <exception cref="System.ServiceModel.QuotaExceededException">
        /// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
        /// </exception>
        public static void SetBuffer<T>(this SLProtocol protocol, int paramId, IEnumerable<T> data)
            where T : new()
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Any())
            {
                protocol.SetParameter(paramId, string.Empty);
            }

            protocol.SetParameter(paramId, data.SerializeDataContractJsonObject());
        }

        /// <summary>
        /// Sets multiple parameters at once.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol" /> instance used to communicate with DataMiner.</param>
        /// <param name="requests">
        /// Array of <see cref="SetParameterRequest" /> with the parameter ids and values to be set.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="protocol"/> or <paramref name="requests"/> is null.</exception>
        public static void SetParameters(this SLProtocol protocol, params SetParameterRequest[] requests)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (requests == null)
            {
                throw new ArgumentNullException("requests");
            }

            if (requests.Length == 0)
            {
                return;
            }

            var ids = new int[requests.Length];
            var values = new object[requests.Length];

            for (int i = 0; i < requests.Length; i++)
            {
                ids[i] = requests[i].Id;
                values[i] = requests[i].Value;
            }

            protocol.SetParameters(ids, values);
        }

        /// <summary>
        /// Gets a <see cref="GetParameterResponseMessage"/> with the Element's parameter info.
        /// </summary>
        /// <param name="protocol"><see cref="SLProtocol"/> instance used to communicate with DataMiner.</param>
        /// <param name="dmaId">Id of the DataMiner agent where the Element is running.</param>
        /// <param name="elementId">Id of the Element.</param>
        /// <param name="parameterId">Id of the desired Parameter.</param>
        /// <param name="displayKey">Display key to filter the desired row(s).</param>
        /// <returns>A <see cref="GetParameterResponseMessage"/>Element's parameter info.</returns>
        internal static GetParameterResponseMessage GetParameterResponseMessage(
            this SLProtocol protocol,
            int dmaId,
            int elementId,
            int parameterId,
            string displayKey = "")
        {
            var message = new GetParameterMessage(dmaId, elementId, parameterId, displayKey);
            var dmsMessages = protocol.SLNet.SendMessage(message);
            return (GetParameterResponseMessage)dmsMessages[0];
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
    }
}
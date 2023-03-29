namespace Skyline.DataMiner.Library
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Class used to store the retrieved protocols data Due to the time that we take to get this
	/// data, this will help to save some time.
	/// </summary>
	internal static class ProtocolInfoCache
	{
		/// <summary>
		/// IDictionary to hold the cached data.
		/// </summary>
		private static readonly IDictionary<string, GetProtocolInfoResponseMessage> Cache =
			new ConcurrentDictionary<string, GetProtocolInfoResponseMessage>();

		/// <summary>
		/// Object used to sync between threads.
		/// </summary>
		private static readonly object CacheLock = new object();

		/// <summary>
		/// Gets a GetProtocolInfoResponseMessage object from the desired Protocol with the
		/// cached data if it exists; otherwise will fetch the data from DataMiner and will store
		/// it in cache.
		/// </summary>
		/// <param name="connection">
		/// <see cref="Connection"/> instance used to communicate with DataMiner.
		/// </param>
		/// <param name="name">Name of the Protocol to get.</param>
		/// <param name="version">Version of the Protocol to get.</param>
		/// <returns>A GetProtocolInfoResponseMessage with the Protocol info.</returns>
		public static GetProtocolInfoResponseMessage GetCacheProtocol(Connection connection, string name, string version)
		{
			lock (CacheLock)
			{
				var cacheKey = GetCacheKey(name, version);

				if (Cache.ContainsKey(cacheKey))
				{
					return Cache[cacheKey];
				}

				return GetProtocolInfoFromDataMiner(connection, name, version);
			}
		}

		/// <summary>
		/// Gets the Element's parameter cache key.
		/// </summary>
		/// <param name="name">Name of the Protocol to get.</param>
		/// <param name="version">Version of the Protocol to get.</param>
		/// <returns>A string with the Element's parameter cache key.</returns>
		private static string GetCacheKey(string name, string version)
		{
			return string.Join(".", name, version);
		}

		/// <summary>
		/// Gets the GetProtocolInfoResponseMessage from DataMiner.
		/// </summary>
		/// <param name="connection">
		/// <see cref="Connection"/> instance used to communicate with DataMiner.
		/// </param>
		/// <param name="name">Name of the Protocol to get.</param>
		/// <param name="version">Version of the Protocol to get.</param>
		/// <param name="raw">Boolean to define if the information should be raw.</param>
		/// <returns>A GetProtocolInfoResponseMessage with the Protocol info.</returns>
		private static GetProtocolInfoResponseMessage GetProtocolInfoFromDataMiner(
			Connection connection,
			string name,
			string version,
			bool raw = true)
		{
			var response = connection.GetProtocol(name, version, raw);

			if (response == null)
			{
				throw new ProtocolNotFoundException(string.Format("Protocol: {0} Version: {1} doesn't exist", name, version));
			}

			Cache.Add(new KeyValuePair<string, GetProtocolInfoResponseMessage>(GetCacheKey(name, version), response));

			return response;
		}
	}
}
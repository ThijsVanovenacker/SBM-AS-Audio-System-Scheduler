namespace Skyline.DataMiner.Library.Common.InterAppCalls.CallBulk
{
    using Skyline.DataMiner.Library.Common;
    using Skyline.DataMiner.Library.Common.Serializing;
    using Skyline.DataMiner.Net;

    using System;

    /// <summary>
    /// Factory class that can create inter-app calls.
    /// </summary>
    public static class InterAppCallFactory
    {
		/// <summary>
		/// Creates an inter-app call from the specified string.
		/// </summary>
		/// <param name="rawData">The serialized raw data.</param>
		/// <returns>An inter-app call.</returns>
		/// <param name="serializer">Optional serializer to use. Leave empty to use default.</param>
		/// <exception cref="ArgumentNullException"><paramref name="rawData"/> is empty or null.</exception>
		/// <exception cref="ArgumentException">Format of <paramref name="rawData"/> is invalid and deserialization failed.</exception>
		public static IInterAppCall CreateFromRaw(string rawData, ISerializer serializer = null)
        {
            if (String.IsNullOrWhiteSpace(rawData)) throw new ArgumentNullException("rawData");
            if (serializer == null) serializer = SerializerFactory.CreateInterAppSerializer(typeof(InterAppCall));
            var returnedResult = serializer.DeserializeFromString<InterAppCall>(rawData);
            returnedResult.ReceivingTime = DateTime.Now;
            returnedResult.InternalSerializer = serializer;
            return returnedResult;
        }

		/// <summary>
		/// Creates an inter-app call from the contents of the specified parameter.
		/// </summary>
		/// <param name="connection">The raw SLNet connection.</param>
		/// <param name="agentId">The source DataMiner Agent ID.</param>
		/// <param name="elementId">The source element ID.</param>
		/// <param name="parameterId">The source parameter ID.</param>
		/// <param name="serializer">Optional serializer to use. Leave empty to use default.</param>
		/// <returns>An inter-app call.</returns>
		/// <exception cref="ArgumentException">The format of the content of the specified parameter is invalid and deserialization failed.</exception>
		public static IInterAppCall CreateFromRemote(IConnection connection, int agentId, int elementId, int parameterId, ISerializer serializer = null)
        {
            IDms thisDms = connection.GetDms();
            var element = thisDms.GetElement(new DmsElementId(agentId, elementId));
            var parameter = element.GetStandaloneParameter<string>(parameterId);
            var returnedResultRaw = parameter.GetValue();

            return CreateFromRaw(returnedResultRaw, serializer);
        }

		/// <summary>
		/// Creates a blank inter-app call.
		/// </summary>
		/// <returns>An inter-app call.</returns>
		public static IInterAppCall CreateNew()
        {
            return new InterAppCall();
        }
    }
}
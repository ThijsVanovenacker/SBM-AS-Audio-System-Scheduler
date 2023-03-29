namespace Skyline.DataMiner.Library
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	////using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using Skyline.DataMiner.Library.Common.Attributes;

	/// <summary>
	/// Class is Miscellaneous Extension methods.
	/// </summary>
	[DllImport("System.Runtime.Serialization.dll")]
	[DllImport("System.Xml.dll")]
	public static class MiscExtensions
	{
		private static readonly Type DateTimeType = typeof(DateTime);

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <typeparam name="T">Type of the result.</typeparam>
		/// <param name="obj">Object to convert.</param>
		/// <returns>The converted object.</returns>
		/// <exception cref="InvalidCastException">This conversion is not supported. Or <paramref name="obj"/> does not implement the <see cref="IConvertible"/> interface.</exception>
		/// <exception cref="FormatException"><paramref name="obj"/> is not in a format rmecognized by conversionType.</exception>
		/// <exception cref="OverflowException"><paramref name="obj"/> represents a number that is out of the range of conversionType.</exception>
		public static T ChangeType<T>(this object obj)
			where T : IConvertible
		{
			if (obj == null)
			{
				return default(T);
			}

			var type = typeof(T);

			if (type.IsEnum)
			{
				return (T)Enum.ToObject(type, obj.ChangeType<int>());
			}
			else if (type == DateTimeType)
			{
				var oadate = Convert.ToDouble(obj);

				if (!oadate.InRange(-657435.0, 2958465.99999999))
				{
					throw new OverflowException(string.Format("{0} is not a valid OA Date, supported range -657435.0 to 2958465.99999999", obj));
				}

				object date = DateTime.FromOADate(oadate);
				return (T)date;
			}
			else
			{
				return (T)Convert.ChangeType(obj, type);
			}
		}

		/// <summary>
		/// Deserializes, using DataContract deserializer, the JSON document contained by the
		/// specified string.
		/// </summary>
		/// <typeparam name="T">Type of object to deserialize.</typeparam>
		/// <param name="objectData">Input string.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static T DeserializeDataContractJsonObject<T>(this string objectData)
		{
			return (T)DeserializeDataContractJsonObject(objectData, typeof(T));
		}

		/// <summary>
		/// Deserializes, using DataContract deserializer, the JSON document contained by the
		/// specified string.
		/// </summary>
		/// <param name="objectData">Input string.</param>
		/// <param name="type">Type object with the type of object.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static object DeserializeDataContractJsonObject(this string objectData, Type type)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(objectData)))
			{
				var jsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
				var obj = jsonSerializer.ReadObject(stream);
				return obj;
			}
		}

		/// <summary>
		/// Deserializes, using DataContract deserializer, the XML document contained by the
		/// specified string.
		/// </summary>
		/// <typeparam name="T">Type of object to deserialize.</typeparam>
		/// <param name="objectData">Input string.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static T DeserializeDataContractObject<T>(this string objectData)
		{
			return (T)DeserializeDataContractObject(objectData, typeof(T));
		}

		/// <summary>
		/// Deserializes, using DataContract deserializer, the XML document contained by the
		/// specified string.
		/// </summary>
		/// <param name="objectData">Input string.</param>
		/// <param name="type">Type object with the type of object.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static object DeserializeDataContractObject(this string objectData, Type type)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(objectData)))
			{
				var xmlSerializer = new DataContractSerializer(type);
				return xmlSerializer.ReadObject(stream);
			}
		}

		/// <summary>
		/// Converts the given string into Title case.
		/// </summary>
		/// <param name="text">String  to convert.</param>
		/// <returns>The converted string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="text"/> is null.</exception>
		public static string FirstLetterToUpperCase(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new ArgumentNullException("text");
			}

			string result;

			if (text.Length > 1)
			{
				result = char.ToUpper(text[0]) + text.Substring(1).ToLower();
			}
			else
			{
				result = text.ToUpper();
			}

			return result;
		}

		/// <summary>
		/// Converts a <see cref="StringValueAttribute"/> into the corresponding enum value.
		/// </summary>
		/// <typeparam name="T">Type of the enum.</typeparam>
		/// <param name="value">String value of the enum.</param>
		/// <returns>The correct enum value if applicable.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		/// <exception cref="ArgumentException">If <typeparamref name="T"/> is not an enum.</exception>
		/// <exception cref="FormatException">If <paramref name="value"/> doesn't represent a <typeparamref name="T"/> valid <see cref="StringValueAttribute"/>.</exception>
		public static T FromStringValue<T>(this string value)
			where T : struct, IComparable, IFormattable, IConvertible
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			var type = typeof(T);

			if (!type.IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			foreach (var field in type.GetFields())
			{
				var stringValue = string.Empty;

				var attributes = field.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

				if (attributes != null && attributes.Length > 0)
				{
					stringValue = attributes[0].Value;
				}

				if (stringValue == value)
				{
					return (T)Enum.Parse(type, field.Name);
				}
			}

			throw new FormatException(string.Format("{0} is not a valued StringValue for Enum {1}", value, type.Name));
		}

		/// <summary>
		/// Gets the StringValue of an Enumeration.
		/// </summary>
		/// <typeparam name="T">Type of enumeration.</typeparam>
		/// <param name="value">Enumeration value.</param>
		/// <returns>The StringValue of an Enumeration if it exists; otherwise null.</returns>
		public static string GetStringValue<T>(this T value)
			where T : struct, IComparable, IFormattable, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			string output = null;
			var type = value.GetType();

			//Check first in our cached results...

			//Look for our 'StringValueAttribute'

			//in the field's custom attributes

			var field = type.GetField(value.ToString(CultureInfo.InvariantCulture));
			var attributes = field.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

			if (attributes != null && attributes.Length > 0)
			{
				output = attributes[0].Value;
			}

			return output;
		}

		/// <summary>
		/// Checks if a value belongs to the given values.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">Value to check.</param>
		/// <param name="range">Range to compare with.</param>
		/// <returns>True if the given value is present on the given range;otherwise false.</returns>
		public static bool In<T>(this T value, params T[] range)
		{
			return range.Contains(value);
		}

		/// <summary>
		/// Checks if a values is inside an interval.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">Value to check.</param>
		/// <param name="fromInclusive">Lower Range, inclusive value.</param>
		/// <param name="toInclusive">High Range, inclusive value.</param>
		/// <returns>True if the value is between the given interval; otherwise false.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		public static bool InRange<T>(this T value, T fromInclusive, T toInclusive)
			where T : IComparable
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			return value.CompareTo(fromInclusive) >= 0 && value.CompareTo(toInclusive) <= 0;
		}

		/// <summary>
		/// Checks if a character is an hexadecimal number.
		/// </summary>
		/// <param name="c">Character to be checked.</param>
		/// <returns>True if the character is an hexadecimal number;otherwise false.</returns>
		public static bool IsHex(this char c)
		{
			return c.InRange('0', '9') || c.InRange('a', 'f') || c.InRange('A', 'F');
		}

		/// <summary>
		/// Checks if an IP Address belongs to the local machine.
		/// </summary>
		/// <param name="host">IP Address to check.</param>
		/// <returns>true if the IP belong to the local machine; otherwise false.</returns>
		public static bool IsLocalIpAddress(this string host)
		{
			try
			{
				// get host IP addresses
				var hostIPs = Dns.GetHostAddresses(host);

				// get local IP addresses
				var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

				// test if any host IP equals to any local IP or to localhost
				foreach (var hostIP in hostIPs)
				{
					// is localhost
					if (IPAddress.IsLoopback(hostIP))
					{
						return true;
					}

					// is local address
					if (localIPs.Contains(hostIP))
					{
						return true;
					}
				}
			}
			catch (Exception)
			{
				// nothing to do
			}

			return false;
		}

		/// <summary>
		/// Checks if an <see cref="IEnumerable{T}"/> is null or empty.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="IEnumerable{T}"/> items.</typeparam>
		/// <param name="source">Source <see cref="IEnumerable{T}"/>.</param>
		/// <returns>True if <paramref name="source"/> is null or empty;otherwise false.</returns>
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
		{
			return source == null || !source.Any();
		}

		/// <summary>
		/// Check if a string is an integer.
		/// </summary>
		/// <param name="input">String to check.</param>
		/// <returns>True if the input string is an integer;otherwise false.</returns>
		public static bool IsNumber(this string input)
		{
			int n;
			return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out n);
		}

		/// <summary>
		/// Check valid IPv4.
		/// </summary>
		/// <param name="ipAddress">IPv4 to check.</param>
		/// <returns>True if the input string is an IPv4.</returns>
		public static bool IsValidIp(this string ipAddress)
		{
			IPAddress unused;
			var result = IPAddress.TryParse(ipAddress, out unused)
						 && (unused.AddressFamily != AddressFamily.InterNetwork || ipAddress.Count(c => c == '.') == 3);

			if (!result)
			{
				try
				{
					Dns.GetHostAddresses(ipAddress);
					result = true;
				}
				catch (Exception)
				{
					result = false;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks if an IPAddress object is a valid Subnet mask.
		/// </summary>
		/// <param name="ip">IPAddress instance to check.</param>
		/// <returns>True if the IPAddress instance is a valid subnet mask; otherwise false.</returns>
		public static bool IsValidSubnet(this IPAddress ip)
		{
			if (ip == null)
			{
				throw new ArgumentNullException("ip");
			}

			var octets = ip.GetAddressBytes();
			var restAreOnes = false;
			for (var i = 3; i >= 0; i--)
			{
				for (var j = 0; j < 8; j++)
				{
					var bitValue = (octets[i] >> j & 1) == 1;
					if (restAreOnes && !bitValue)
					{
						return false;
					}

					restAreOnes = bitValue;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if a string is a valid Subnet mask.
		/// </summary>
		/// <param name="ipAddr">String instance to check.</param>
		/// <returns>True if the string instance is a valid subnet mask; otherwise false.</returns>
		public static bool IsValidSubnet(this string ipAddr)
		{
			try
			{
				var ip = IPAddress.Parse(ipAddr);
				var octets = ip.GetAddressBytes();
				var restAreOnes = false;

				for (var i = 3; i >= 0; i--)
				{
					for (var j = 0; j < 8; j++)
					{
						var bitValue = (octets[i] >> j & 1) == 1;
						if (restAreOnes && !bitValue)
						{
							return false;
						}

						restAreOnes = bitValue;
					}
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if an <see cref="IEnumerable{T}"/> is not null neither empty.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="IEnumerable{T}"/> items.</typeparam>
		/// <param name="source">Source <see cref="IEnumerable{T}"/>.</param>
		/// <returns>True if <paramref name="source"/> is not null neither empty;otherwise false.</returns>
		public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
		{
			return source != null && source.Any();
		}

		/// <summary>
		/// Populates an array with a given value.
		/// </summary>
		/// <typeparam name="T">Type of the array.</typeparam>
		/// <param name="arr">Array to populate.</param>
		/// <param name="value">Value to populate the array.</param>
		public static void Populate<T>(this T[] arr, T value)
		{
			if (arr == null)
			{
				throw new ArgumentNullException("arr");
			}

			for (var i = 0; i < arr.Length; i++)
			{
				arr[i] = value;
			}
		}

		/// <summary>
		/// Tries to run a function until it succeeds or it reached the maximum number of retries.
		/// </summary>
		/// <param name="task"><see cref="Func{TResult}"/> object with the function to execute.</param>
		/// <param name="retries">Maximum number of retries (default value = 5).</param>
		/// <param name="sleepTime">
		/// Sleep time between retries in milliseconds (default value = 50).
		/// </param>
		/// <returns>True if the function was successfully executed; otherwise false.</returns>
		public static bool RetryUntilSuccessOrMaxRetries(this Func<bool> task, int retries = 5, int sleepTime = 50)
		{
			var attemps = 0;

			while (attemps < retries)
			{
				var success = task();

				if (success)
				{
					return true;
				}

				Thread.Sleep(sleepTime);
				attemps++;
			}

			return false;
		}

		/// <summary>
		/// Execute a function until it's successful or it times out.
		/// </summary>
		/// <param name="task">Function to execute.</param>
		/// <param name="timeout">Max time to wait (in milliseconds).</param>
		/// <param name="sleepTime">Time to sleep between retries (default value = 50).</param>
		/// <returns>True if the function is executed successfully; otherwise false.</returns>
		public static bool RetryUntilSuccessOrTimeout(this Func<bool> task, int timeout, int sleepTime = 50)
		{
			var elapsed = Stopwatch.StartNew();

			while (elapsed.ElapsedMilliseconds < timeout)
			{
				var success = task();

				if (success)
				{
					return true;
				}

				Thread.Sleep(sleepTime);
			}

			return false;
		}

		/// <summary>
		/// Execute a function until it's successful or it times out.
		/// </summary>
		/// <param name="task">Function to execute.</param>
		/// <param name="timeout">Max time to wait.</param>
		/// <param name="sleepTime">Time to sleep between retries (default value = 50).</param>
		/// <returns>True if the function is executed successfully; otherwise false.</returns>
		public static bool RetryUntilSuccessOrTimeout(this Func<bool> task, TimeSpan timeout, int sleepTime = 50)
		{
			var elapsed = Stopwatch.StartNew();

			while (elapsed.Elapsed < timeout)
			{
				var success = task();

				if (success)
				{
					return true;
				}

				Thread.Sleep(sleepTime);
			}

			return false;
		}

		/// <summary>
		/// Serializes, using DataContract serializer, the specified object and writes the JSON
		/// document to a string.
		/// </summary>
		/// <typeparam name="T">Type of the object to serialize.</typeparam>
		/// <param name="toSerialize">Object to serialize.</param>
		/// <returns>A string with the XML serialized object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="toSerialize"/> is null.</exception>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static string SerializeDataContractJsonObject<T>(this T toSerialize)
		{
			if (Equals(toSerialize, default(T)))
			{
				throw new ArgumentNullException("toSerialize");
			}

			using (var stream = new MemoryStream())
			{
				var jsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(toSerialize.GetType());
				jsonSerializer.WriteObject(stream, toSerialize);

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		/// <summary>
		/// Serializes, using DataContract serializer, the specified object and writes the XML
		/// document to a string.
		/// </summary>
		/// <typeparam name="T">Type of the object to serialize.</typeparam>
		/// <param name="toSerialize">Object to serialize.</param>
		/// <returns>A string with the XML serialized object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="toSerialize"/> is null.</exception>
		/// <exception cref="System.Runtime.Serialization.InvalidDataContractException">
		/// The type being serialized does not conform to data contract rules.
		/// </exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		/// There is a problem with the instance being written.
		/// </exception>
		/// <exception cref="System.ServiceModel.QuotaExceededException">
		/// The maximum number of objects to serialize has been exceeded. Check the System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph.
		/// </exception>
		public static string SerializeDataContractObject<T>(this T toSerialize)
		{
			if (Equals(toSerialize, default(T)))
			{
				throw new ArgumentNullException("toSerialize");
			}

			using (var stringWriter = new MemoryStream())
			{
				var xmlSerializer = new System.Runtime.Serialization.DataContractSerializer(toSerialize.GetType());
				xmlSerializer.WriteObject(stringWriter, toSerialize);

				return Encoding.UTF8.GetString(stringWriter.ToArray());
			}
		}

		/// <summary>
		/// Serializes the specified object and writes the XML document to a string.
		/// </summary>
		/// <typeparam name="T">Type of the object to serialize.</typeparam>
		/// <param name="toSerialize">Object to serialize.</param>
		/// <returns>A string with the XML serialized object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="toSerialize"/> is null.</exception>
		public static string SerializeObject<T>(this T toSerialize)
		{
			if (Equals(toSerialize, default(T)))
			{
				throw new ArgumentNullException("toSerialize");
			}

			using (var textWriter = new StringWriter())
			{
				var xmlSerializer = new System.Xml.Serialization.XmlSerializer(toSerialize.GetType());
				xmlSerializer.Serialize(textWriter, toSerialize);
				return textWriter.ToString();
			}
		}

		///// <summary>
		///// Convert a color to it's equivalent hexadecimal value.
		///// </summary>
		///// <param name="color">Color to be converted.</param>
		///// <returns>A string with the hexadecimal value.</returns>
		////[DllImport("System.Drawing.dll")]
		////public static string ToHexValue(this Color color)
		////{
		////	return "#" + color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
		////}

		/// <summary>
		/// Converts a string to it's Title Case equivalent, using en-GB culture info.
		/// </summary>
		/// <param name="self">String to put in title case.</param>
		/// <returns>A title case string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="self"/> is null.</exception>
		public static string ToTitleCase(this string self)
		{
			var cultureInfo = new CultureInfo("en-GB");
			return self.ToTitleCase(cultureInfo);
		}

		/// <summary>
		/// Converts a string to it's Title Case equivalent, using the given <paramref name="cultureInfo"/>.
		/// </summary>
		/// <param name="self">String to put in title case.</param>
		/// <param name="cultureInfo">Culture info to use.</param>
		/// <returns>A title case string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="self"/> or <paramref name="cultureInfo"/> are null.</exception>
		public static string ToTitleCase(this string self, CultureInfo cultureInfo)
		{
			if (self == null)
			{
				throw new ArgumentNullException("self");
			}

			if (cultureInfo == null)
			{
				throw new ArgumentNullException("cultureInfo");
			}

			return cultureInfo.TextInfo.ToTitleCase(self);
		}

		/// <summary>
		/// Trims a <see cref="DateTime"/> by the provided ticks.
		/// </summary>
		/// <param name="date"><see cref="DateTime"/> to trim.</param>
		/// <param name="ticks">Ticks from where to trim.</param>
		/// <returns>The <paramref name="date"/> trimmed by <paramref name="ticks"/>.</returns>
		/// <example>DateTime nowTrimmedToSeconds = DateTime.Now.Trim(TimeSpan.TicksPerSecond).</example>
		public static DateTime Trim(this DateTime date, long ticks)
		{
			return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
		}

		/// <summary>
		/// Tries to deserialize, using DataContract deserializer, the JSON document contained by the
		/// specified string.
		/// </summary>
		/// <typeparam name="T">Type of object to deserialize.</typeparam>
		/// <param name="objectData">Input string.</param>
		/// <param name="result">Out parameter that will contain the deserialized object if <paramref name="objectData"/> is a valid json;otherwise <code>default(T)</code>.</param>
		/// <returns>True if the deserialization was successful;otherwise false.</returns>
		public static bool TryDeserializeDataContractJsonObject<T>(this string objectData, out T result)
		{
			try
			{
				result = (T)DeserializeDataContractJsonObject(objectData, typeof(T));
				return true;
			}
			catch (Exception)
			{
				result = default(T);
				return false;
			}
		}

		/// <summary>
		/// Checks if a string matches a given wildcard.
		/// </summary>
		/// <param name="input">String to check.</param>
		/// <param name="wildcard">Wildcard value.</param>
		/// <param name="caseSensitive">
		/// Defines if the comparison should be case sensitive or not.
		/// </param>
		/// <returns>True if the string matches the given wildcard;otherwise false.</returns>
		public static bool WildcardMatch(this string input, string wildcard, bool caseSensitive = true)
		{
			// Replace the * with an .* and the ? with a dot. Put ^ at the beginning and a $ at the end
			var pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

			var regex = caseSensitive
							  ? new Regex(pattern)
							  : new Regex(pattern, RegexOptions.IgnoreCase);

			return regex.IsMatch(input);
		}

		/// <summary>
		/// Deserializes the XML document contained by the specified file.
		/// </summary>
		/// <typeparam name="T">Type of object to deserialize.</typeparam>
		/// <param name="path">Complete path to the file.</param>
		/// <returns>The deserialized object.</returns>
		public static T XmlDeserializeFromFile<T>(string path)
		{
			return (T)XmlDeserializeFromFile(path, typeof(T));
		}

		/// <summary>
		/// Deserializes the XML document contained by the specified string.
		/// </summary>
		/// <typeparam name="T">Type of object to deserialize.</typeparam>
		/// <param name="objectData">Input string.</param>
		/// <returns>The deserialized object.</returns>
		public static T XmlDeserializeFromString<T>(this string objectData)
		{
			return (T)XmlDeserializeFromString(objectData, typeof(T));
		}

		/// <summary>
		/// Deserializes the XML document contained by the specified string.
		/// </summary>
		/// <param name="objectData">Input string.</param>
		/// <param name="type">Type of object to deserialize.</param>
		/// <returns>The deserialized object.</returns>
		public static object XmlDeserializeFromString(this string objectData, Type type)
		{
			object result;

			using (TextReader reader = new StringReader(objectData))
			{
				var serializer = new System.Xml.Serialization.XmlSerializer(type);
				result = serializer.Deserialize(reader);
			}

			return result;
		}

		/// <summary>
		/// Deserializes the XML document contained by the specified file.
		/// </summary>
		/// <param name="path">Complete path to the file.</param>
		/// <param name="type">Type of object to deserialize.</param>
		/// <returns>The deserialized object.</returns>
		private static object XmlDeserializeFromFile(string path, Type type)
		{
			object result;

			using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var serializer = new System.Xml.Serialization.XmlSerializer(type);
				result = serializer.Deserialize(reader);
			}

			return result;
		}
	}
}
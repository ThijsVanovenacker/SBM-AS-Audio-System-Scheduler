namespace Skyline.DataMiner.Library.Snmp
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a DataMiner trap.
	/// </summary>
	public class DataMinerTrapInfo
	{
		/// <summary>
		/// Holds the OID of the Binding that will contain the Source IPAddress.
		/// </summary>
		public const string IPOID = "1.3.6.1.3.1057.1";

		/// <summary>
		/// Prevents a default instance of the <see cref="DataMinerTrapInfo"/> class from being created.
		/// </summary>
		private DataMinerTrapInfo()
		{
		}

		/// <summary>
		/// Gets the Bindings of the current trap.
		/// </summary>
		public TrapBindingInfo[] Bindings { get; private set; }

		/// <summary>
		/// Gets the Source IPAddress.
		/// </summary>
		public string IPAddress { get; private set; }

		/// <summary>
		/// Gets the OID of the Trap.
		/// </summary>
		public string OID { get; private set; }

		/// <summary>
		/// Gets the Ticks of the Trap.
		/// </summary>
		public long Ticks { get; private set; }

		/// <summary>
		/// Parses an object with the TrapInfo and returns a new DataMinerTrapInfo instance with that information.
		/// </summary>
		/// <param name="trapInfo"><see cref="object"/> with the trap information.</param>
		/// <returns>A DataMinerTrapInfo object with the trap information.</returns>
		/// <exception cref="ArgumentNullException">Argument trapInfo is null.</exception>
		/// <exception cref="InvalidTrapException">
		/// If the object is empty of doesn't have the required fields.
		/// </exception>
		public static DataMinerTrapInfo Parse(object trapInfo)
		{
			if (trapInfo == null)
			{
				throw new ArgumentNullException("trapInfo");
			}

			if (((object[])trapInfo).Length < 1)
			{
				throw new InvalidTrapException("Empty trap");
			}

			var generalTrapInfo = (object[])((object[])trapInfo)[0];

			if (generalTrapInfo.Length != 3)
			{
				throw new InvalidTrapException("Invalid general trap information");
			}

			return new DataMinerTrapInfo
			{
				OID = Convert.ToString(generalTrapInfo[0]),
				IPAddress = Convert.ToString(generalTrapInfo[1]),
				Ticks = Convert.ToInt64(generalTrapInfo[2]),
				Bindings = ParseTrapBindings(((object[])trapInfo).Skip(1)).ToArray()
			};
		}

		/// <summary>
		/// Tries to parse an object with the TrapInfo into a DataMinerTrapInfo object.
		/// </summary>
		/// <param name="trapInfo"><see cref="object"/> with the trap information.</param>
		/// <param name="trap">
		/// When this method returns, contains the DataMinerTrapInfo with the trap information, if
		/// the object is valid;otherwise it will contain an empty object.
		/// </param>
		/// <returns>True if the input object was correctly parsed;otherwise false.</returns>
		public static bool TryParse(object trapInfo, out DataMinerTrapInfo trap)
		{
			try
			{
				trap = Parse(trapInfo);
				return true;
			}
			catch (Exception)
			{
				trap = new DataMinerTrapInfo();
				return false;
			}
		}

		/// <summary>
		/// Converts the value of the current DataMinerTrapInfo object to its equivalent string representation.
		/// </summary>
		/// <returns>A string representation of the value of the current DataMinerTrapInfo object.</returns>
		public override string ToString()
		{
			return string.Format(
				"Trap OID: {0}\nSender IP: {1}\nTicks: {2}\nBindings:\n\t{3}",
				this.OID,
				this.IPAddress,
				this.Ticks,
				string.Join("\n\t", this.Bindings.Select(x => x.ToString())));
		}

		/// <summary>
		/// Parses the current trap bindings.
		/// </summary>
		/// <param name="trapBindings">
		/// <see cref="IEnumerable{T}"/> of <see cref="object"/> with the trap bindings information.
		/// </param>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="TrapBindingInfo"/> with the binding info parsed.</returns>
		private static IEnumerable<TrapBindingInfo> ParseTrapBindings(IEnumerable<object> trapBindings)
		{
			return trapBindings.Select(trapBinding => TrapBindingInfo.Parse((object[])trapBinding));
		}

		/// <summary>
		/// The exception that is thrown when trying to parse an invalid trap.
		/// </summary>
		[Serializable]
		public class InvalidTrapException : Exception
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="InvalidTrapException"/> class.
			/// </summary>
			public InvalidTrapException()
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="InvalidTrapException"/> class with a specified error message.
			/// </summary>
			/// <param name="message">The message that describes the error.</param>
			public InvalidTrapException(string message)
				: base(message)
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="InvalidTrapException"/> class with serialized data.
			/// </summary>
			/// <param name="info">The object that holds the serialized object data.</param>
			/// <param name="context">The contextual information about the source or destination.</param>
			protected InvalidTrapException(
				SerializationInfo info,
				StreamingContext context)
				: base(info, context)
			{
			}
		}

		/// <summary>
		/// Represent a DataMiner Trap Binding.
		/// </summary>
		public sealed class TrapBindingInfo
		{
			/// <summary>
			/// Prevents a default instance of the <see cref="TrapBindingInfo"/> class from being created.
			/// </summary>
			private TrapBindingInfo()
			{
			}

			/// <summary>
			/// Gets the OID of the Binding.
			/// </summary>
			public string OID { get; private set; }

			/// <summary>
			/// Gets the Value of the Binding.
			/// </summary>
			public string Value { get; private set; }

			/// <summary>
			/// Parses an object array with the BindingInfo and returns a new TrapBindingInfo
			/// instance with that information.
			/// </summary>
			/// <param name="bindingInfo"><see cref="object"/> array with the binding information.</param>
			/// <returns>A <see cref="TrapBindingInfo"/> object with the binding information.</returns>
			/// <exception cref="ArgumentNullException"><paramref name="bindingInfo"/> is null.</exception>
			/// <exception cref="ArgumentOutOfRangeException">
			/// <paramref name="bindingInfo"/> does not contain the necessary fields.
			/// </exception>
			public static TrapBindingInfo Parse(object[] bindingInfo)
			{
				if (bindingInfo == null)
				{
					throw new ArgumentNullException("bindingInfo");
				}

				if (bindingInfo.Length < 2)
				{
					throw new ArgumentOutOfRangeException("bindingInfo", "A binding array needs at least two fields, the OID and the Value");
				}

				return new TrapBindingInfo
				{
					OID = Convert.ToString(bindingInfo[0]),
					Value = Convert.ToString(bindingInfo[1])
				};
			}

			/// <summary>
			/// Tries to parse an object array with the BindingInfo into a <see cref="TrapBindingInfo"/> object.
			/// </summary>
			/// <param name="bindingInfo"><see cref="object"/> array with the binding information.</param>
			/// <param name="info">
			/// When this method returns, contains the TrapBindingInfo with the binding information,
			/// if the object is valid;otherwise it will contain an empty object.
			/// </param>
			/// <returns>True if the input object was correctly parsed;otherwise false.</returns>
			public static bool TryParse(object[] bindingInfo, out TrapBindingInfo info)
			{
				try
				{
					info = Parse(bindingInfo);
					return true;
				}
				catch (Exception)
				{
					info = new TrapBindingInfo();
					return false;
				}
			}

			/// <summary>
			/// Converts the value of the current TrapBindingInfo object to its equivalent string representation.
			/// </summary>
			/// <returns>
			/// A string representation of the value of the current TrapBindingInfo object.
			/// </returns>
			public override string ToString()
			{
				return string.Format("Binding OID: {0}\tBinding Value: {1}", this.OID, this.Value);
			}
		}
	}
}
namespace Skyline.DataMiner.Library.Automation
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a DataMiner Automation Script.
	/// </summary>
	public class AutomationScript
	{
		private readonly List<string> scriptArgs = new List<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="AutomationScript"/> class.
		/// Default Values:
		///		<see cref="Synchronous"/> = true,
		///		<see cref="PerformChecks"/> = true,
		///		<see cref="LockElements"/> = false,
		///		<see cref="ForceLockElements"/> = false,
		///		<see cref="WaitWhenLocked"/> = true.
		/// </summary>
		/// <param name="scriptName">String with the script name.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="scriptName"/> is null.</exception>
		public AutomationScript(string scriptName)
		{
			if (scriptName == null)
			{
				throw new ArgumentNullException("scriptName");
			}

			this.ScriptName = scriptName;
			this.Synchronous = true;
			this.PerformChecks = true;
			this.LockElements = false;
			this.ForceLockElements = false;
			this.WaitWhenLocked = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether lock elements will be forced.
		/// </summary>
		public bool ForceLockElements { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether elements will be locked.
		/// </summary>
		public bool LockElements { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether gets after sets will be performed.
		/// </summary>
		public bool PerformChecks { get; set; }

		/// <summary>
		/// Gets or sets the name of the script to execute.
		/// </summary>
		public string ScriptName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the script will be called for a different thread or in the current one.
		/// </summary>
		public bool Synchronous { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether wait when locked is enabled.
		/// </summary>
		public bool WaitWhenLocked { get; set; }

		/// <summary>
		/// Executes the script with the current settings.
		/// </summary>
		/// <param name="errorMessage">Out variable that will hold an error message if the script execution fails.</param>
		/// <returns>True if the script is executed successfully;otherwise false.</returns>
		public bool ExecuteScript(out string errorMessage)
		{
			errorMessage = string.Empty;

			var finalOptions = new List<string>();

			var scriptRunFlags = this.LockElements ? ScriptRunFlags.Lock : ScriptRunFlags.None;

			try
			{
				finalOptions.AddRange(this.scriptArgs);
				finalOptions.Add(string.Format("DEFER:{0}", !this.Synchronous ? "TRUE" : "FALSE"));
				finalOptions.Add(string.Format("CHECKSETS:{0}", !this.PerformChecks ? "FALSE" : "TRUE"));

				if (this.ForceLockElements)
				{
					scriptRunFlags |= ScriptRunFlags.ForceLock;
				}

				if (!this.WaitWhenLocked)
				{
					scriptRunFlags |= ScriptRunFlags.NoWait;
				}

				finalOptions.Add(string.Format("OPTIONS:{0}", scriptRunFlags));

				var scriptMessage = new ExecuteScriptMessage(this.ScriptName)
				{
					Options = new SA
					{
						Sa = finalOptions.ToArray()
					}
				};

				var response = Engine.SLNet.SendSingleResponseMessage(scriptMessage) as ExecuteScriptResponseMessage;

				if (response == null)
				{
					errorMessage = "Failed to execute Automation Script";
					return false;
				}

				if (response.HadError)
				{
					errorMessage = string.Join("\r\n", response.ErrorMessages);

					return false;
				}
				return true;
			}
			catch (Exception e)
			{
				errorMessage = e.ToString();
				return false;
			}
		}

		/// <summary>
		/// Selects a dummy argument for the automation script.
		/// </summary>
		/// <param name="dummyId">Id of the dummy in the script.</param>
		/// <param name="dmaId">Id of the DataMiner Agent.</param>
		/// <param name="elementId">Id of the DataMiner Element.</param>
		public void SelectDummy(int dummyId, int dmaId, int elementId)
		{
			this.scriptArgs.Add(string.Format("PROTOCOL:{0}:{1}:{2}", dummyId, dmaId, elementId));
			this.scriptArgs.Add(string.Format("FORCEDYNAMIC:{0}:{1}", dmaId, elementId));
		}

		/// <summary>
		/// Selects a dummy argument for the automation script.
		/// </summary>
		/// <param name="name">Name of the dummy in the script.</param>
		/// <param name="dmaId">Id of the DataMiner Agent.</param>
		/// <param name="elmentId">Id of the DataMiner Element.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
		public void SelectDummy(string name, int dmaId, int elmentId)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			this.scriptArgs.Add(string.Format("PROTOCOLBYNAME:{0}:{1}:{2}", name, dmaId, elmentId));
			this.scriptArgs.Add(string.Format("FORCEDYNAMIC:{0}:{1}", dmaId, elmentId));
		}

		/// <summary>
		/// Selects a memory argument for the automation script.
		/// </summary>
		/// <param name="name">Name of the memory file.</param>
		/// <param name="value">Value of the memory file.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="value"/> are null.</exception>
		public void SelectMemory(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			this.scriptArgs.Add(string.Format("MEMORYBYNAME:{0}:{1}", name, value));
		}

		/// <summary>
		/// Selects a memory argument for the automation script.
		/// </summary>
		/// <param name="memoryId">Id of the memory file.</param>
		/// <param name="value">Value of the memory file.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		public void SelectMemory(int memoryId, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			this.scriptArgs.Add(string.Format("MEMORY:{0}:{1}", memoryId, value));
		}

		/// <summary>
		/// Selects a parameter for the automation script.
		/// </summary>
		/// <param name="name">Name of the parameter.</param>
		/// <param name="value">Value of the parameter.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="value"/> are null.</exception>
		public void SelectScriptParam(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			this.scriptArgs.Add(string.Format("PARAMETERBYNAME:{0}:{1}", name, value));
		}

		/// <summary>
		/// Selects a parameter for the automation script.
		/// </summary>
		/// <param name="id">Id of the parameter.</param>
		/// <param name="value">Value of the parameter.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
		public void SelectScriptParam(int id, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			this.scriptArgs.Add(string.Format("PARAMETER:{0}:{1}", id, value));
		}
	}
}
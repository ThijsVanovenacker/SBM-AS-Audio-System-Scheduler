namespace Skyline.DataMiner.Library.Automation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Exceptions;

	public class SubScriptOptionsParser
	{
		/// <summary>
		/// Converts a script configuration string, like
		/// 'Script:ScriptName|DummyName=ElementName or DmaID/ElementID;...|ParameterName1=SingleValue;ParameterName2=#ValueFile;...| MemoryName=MemoryFileName;...|Tooltip|Options'
		/// to a <see cref="SubScriptOptions"/> object.
		/// </summary>
		/// <param name="engine"><see cref="Engine"/> instance used to communicate with DataMiner.</param>
		/// <param name="script"><see cref="string"/> with the script configuration.</param>
		/// <returns>A <see cref="SubScriptOptions"/> object with the script options.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="engine"/> or <paramref name="script"/> are null.</exception>
		/// <exception cref="DataMinerException">If <paramref name="script"/> is not convertible to <see cref="SubScriptOptions"/> object.</exception>
		public static SubScriptOptions GetSubScriptOptions(Engine engine, string script)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			if (script == null)
			{
				throw new ArgumentNullException("script");
			}

			var scriptConfigParts = script.Split('|');

			if (scriptConfigParts.Length == 0)
			{
				throw new DataMinerException(string.Format("Script configuration needs to be a '|' separated list.\r\n{0} is not valid", script));
			}

			var scriptName = scriptConfigParts[0].Substring(scriptConfigParts[0].IndexOf(':') + 1);

			var subScriptOptions = engine.PrepareSubScript(scriptName);

			if (!string.IsNullOrWhiteSpace(scriptConfigParts[1]))
			{
				AddSubScriptDummies(engine, scriptConfigParts[1], subScriptOptions);
			}

			if (!string.IsNullOrWhiteSpace(scriptConfigParts[2]))
			{
				AddSubScriptParameters(scriptConfigParts[2], subScriptOptions);
			}

			if (!string.IsNullOrWhiteSpace(scriptConfigParts[3]))
			{
				AddSubScriptMemoryFiles(scriptConfigParts[3], subScriptOptions);
			}

			if (!string.IsNullOrWhiteSpace(scriptConfigParts[5]))
			{
				AddSubScriptOptions(scriptConfigParts[5], subScriptOptions);
			}

			return subScriptOptions;
		}

		private static void AddSubScriptDummies(Engine engine, string dummiesConfig, SubScriptOptions subScriptOptions)
		{
			if (dummiesConfig == null)
			{
				throw new ArgumentNullException("dummiesConfig");
			}

			if (subScriptOptions == null)
			{
				throw new ArgumentNullException("subScriptOptions");
			}

			try
			{
				var dummies = dummiesConfig.Split(';', '=');

				for (int i = 0; i < dummies.Length; i += 2)
				{
					var elementConfig = dummies[i + 1];
					var element = elementConfig.Contains('/')
									? engine.FindElement((ElementID)elementConfig)
									: engine.FindElement(elementConfig);

					subScriptOptions.SelectDummy(dummies[i], element);
				}
			}
			catch (Exception e)
			{
				string message = string.Format(
					"{0} is not a valid dummies configuration. It should match the format 'DummyName=ElementName or DmaID/ElementID;...'",
					dummiesConfig);

				throw new DataMinerException(message, e);
			}
		}

		private static void AddSubScriptMemoryFiles(string memoryFilesConfig, SubScriptOptions subScriptOptions)
		{
			if (memoryFilesConfig == null)
			{
				throw new ArgumentNullException("memoryFilesConfig");
			}

			if (subScriptOptions == null)
			{
				throw new ArgumentNullException("subScriptOptions");
			}

			try
			{
				var memoryFileConfig = memoryFilesConfig.Split(';', '=');

				for (int i = 0; i < memoryFileConfig.Length; i += 2)
				{
					subScriptOptions.SelectMemory(memoryFileConfig[i], memoryFileConfig[i + 1]);
				}
			}
			catch (Exception e)
			{
				string message = string.Format(
					"{0} is not a valid memory file configuration. It should match the format 'ParameterName1=SingleValue;ParameterName2=#ValueFile;...'",
					memoryFilesConfig);

				throw new DataMinerException(message, e);
			}
		}

		private static void AddSubScriptOptions(string optionsConfig, SubScriptOptions subScriptOptions)
		{
			if (optionsConfig == null)
			{
				throw new ArgumentNullException("optionsConfig");
			}

			if (subScriptOptions == null)
			{
				throw new ArgumentNullException("subScriptOptions");
			}

			try
			{
				var options = new HashSet<string>(optionsConfig.Split(','), StringComparer.InvariantCultureIgnoreCase);

				subScriptOptions.ForceLockElements = options.Contains("ForceLock");
				subScriptOptions.LockElements = options.Contains("Lock");
				subScriptOptions.PerformChecks = !options.Contains("NoSetCheck");
				subScriptOptions.Synchronous = !options.Contains("Asynchronous");
				subScriptOptions.WaitWhenLocked = !options.Contains("NoWait");
			}
			catch (Exception e)
			{
				string message = string.Format(
					"{0} is not a valid options configuration. It should match the format 'Option1,Option2,...'",
					optionsConfig);

				throw new DataMinerException(message, e);
			}
		}

		private static void AddSubScriptParameters(string paramsConfig, SubScriptOptions subScriptOptions)
		{
			if (paramsConfig == null)
			{
				throw new ArgumentNullException("paramsConfig");
			}

			if (subScriptOptions == null)
			{
				throw new ArgumentNullException("subScriptOptions");
			}

			try
			{
				var parameters = paramsConfig.Split(';', '=');

				for (int i = 0; i < parameters.Length; i += 2)
				{
					subScriptOptions.SelectScriptParam(parameters[i], parameters[i + 1]);
				}
			}
			catch (Exception e)
			{
				string message = string.Format(
					"{0} is not a valid parameters configuration. It should match the format 'ParameterName1=SingleValue;ParameterName2=#ValueFile;...'",
					paramsConfig);

				throw new DataMinerException(message, e);
			}
		}
	}
}
using Skyline.DataMiner.Automation;

internal class Script
{
	public void Run(Engine engine)
	{
		string currentKey = engine.GetScriptParam("ID").Value;
		engine.GenerateInformation(currentKey);

		ScriptDummy dummyTable = engine.GetDummy("Element");

		string primaryKey = dummyTable.FindPrimaryKey(100, currentKey);
		engine.GenerateInformation(primaryKey);

		string startOrstop = engine.GetScriptParam("Start").Value;

		if (startOrstop == "End")
		{
			dummyTable.SetParameter(111, primaryKey, 6);
		}
		else if (startOrstop == "Start")
		{
			dummyTable.SetParameter(111, primaryKey, 2);
		}
	}
}
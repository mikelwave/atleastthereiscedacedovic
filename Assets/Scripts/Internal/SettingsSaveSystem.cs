using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
public static class SettingsSaveSystem
{
	public static void SaveSettings(SettingsScript settings)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath+"/settings.ceda";
		
		FileStream s = new FileStream(path,FileMode.Create);

		GameSettings gamesettings = new GameSettings(settings);
		try 
		{
			formatter.Serialize(s,gamesettings);
			Debug.Log("Settings saved to "+path);
		}
		catch(System.Runtime.Serialization.SerializationException)
		{
			Debug.LogError("Failed to save settings.");
		}
		s.Close();
	}
	public static GameSettings LoadSettings()
	{
		string path = Application.persistentDataPath+"/settings.ceda";
		if(File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path,FileMode.Open);

			GameSettings data = null;
			try 
			{
				//Debug.Log("Loading settings");
				data = formatter.Deserialize(stream) as GameSettings;
			}
			catch(System.Runtime.Serialization.SerializationException)
			{
				Debug.LogError("Failed to load settings. Loading defaults.");
			}
			stream.Close();
			//Debug.Log("Settings loaded from "+path);
			return data;
		}
		else
		{
			Debug.LogError("Settings not saved in "+path);
			return null;
		}
	}
}

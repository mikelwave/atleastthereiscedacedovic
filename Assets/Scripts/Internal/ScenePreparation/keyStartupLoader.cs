using UnityEngine;

public class keyStartupLoader : MonoBehaviour {

	// Use this for initialization
	void OnEnable ()
	{
		InputReader reader = GetComponent<InputReader>();
		//print("loading keys");
		GameSettings settings = SettingsSaveSystem.LoadSettings();
		if(settings!=null)
		{
		reader.registerInputsFromFile(settings.savedInputNameStrings);
		int type = Mathf.Clamp(settings.inputType,0,4);
		if(type==2)type = 1;
		reader.changeKeys(type,false);
		//print(reader.controllerType);
		//reader.changeKeys(5);
		}
		else reader.changeKeys(0,true);
		//print("keys loaded");
		Destroy(this);
	}
}

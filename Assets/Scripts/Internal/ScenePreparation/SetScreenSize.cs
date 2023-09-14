using UnityEngine;

public class SetScreenSize : MonoBehaviour {
	// Use this for initialization
	void Start () {
		if(Application.targetFrameRate!=60)
		{
			if(!Application.isEditor)
			Cursor.visible = false;
			GameSettings settings = SettingsSaveSystem.LoadSettings();
			Application.targetFrameRate = 60;
			if(settings==null||settings.ResolutionValue==0)
			Screen.SetResolution(768,432,Screen.fullScreen);
			else if(settings.ResolutionValue==1)
			Screen.SetResolution(1536,864,Screen.fullScreen);

			if(settings!=null)
			Screen.fullScreen = settings.fullscreen;
		}
	}
}

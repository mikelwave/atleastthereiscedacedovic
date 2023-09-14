[System.Serializable]
public class GameSettings
{
	public int musicVolume = 100;
	public int sfxVolume = 100;
	public bool musicEnabled = true;
	public bool sfxEnabled = true;
	public int HUDType = 0;
	public int ResolutionValue = 0;
	public bool fullscreen = false;
	public int rumble = 1;
	public string[] savedInputNameStrings;
	public int inputType = 0;
	public int padLayout = 1;
	
	public GameSettings (SettingsScript settings)
	{
		musicVolume = settings.musicVolume;
		sfxVolume = settings.sfxVolume;
		musicEnabled = settings.musicEnabled;
		sfxEnabled = settings.sfxEnabled;
		HUDType = settings.HUDType;
		ResolutionValue = settings.ResolutionValue;
		fullscreen = settings.fullscreen;
		if(settings.rumble==true) rumble = 1;
		else rumble = 0;
		inputType = settings.inputType;
		padLayout = settings.padLayout;

		savedInputNameStrings = new string[12];
		savedInputNameStrings = InputReader.inputStrings;
	}
}

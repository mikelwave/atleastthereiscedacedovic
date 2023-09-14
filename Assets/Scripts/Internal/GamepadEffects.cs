using UnityEngine;
using XInputDotNetPure;

public class GamepadEffects : MonoBehaviour {

	bool playerIndexSet = false;
    PlayerIndex playerIndex;
	GamePadState state;
    GamePadState prevState;
	Vector2 rumble = Vector2.zero,savedRumble = Vector2.zero;
	public bool usesController = false;
    GameSettings settings;
    bool rumbling = true;
    bool allowStoppedTimeRumble = false;
    void Start()
    {
        settings = SettingsSaveSystem.LoadSettings();
        if(settings==null||settings.rumble==0)
        rumbling = false;
    }
    public void toggleRumble(bool toggle)
    {
        rumbling = toggle;
        //if(rumbling)Debug.Log("Rumble: Enabled");
        //else Debug.Log("Rumble: Disbled");
    }
	// Update is called once per frame
	void Update ()
	{
		if (!playerIndexSet || !prevState.IsConnected)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected)
                {
                    //Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                    playerIndex = testPlayerIndex;
                    playerIndexSet = true;
                }
            }
        }

        prevState = state;
        state = GamePad.GetState(playerIndex);
		if(usesController)
        {
            if(Time.timeScale!=0||allowStoppedTimeRumble)
		    GamePad.SetVibration(playerIndex, rumble.x, rumble.y);
            else GamePad.SetVibration(playerIndex, 0, 0);
        }
	}
	public void setRumble(Vector2 rumbleValues)
	{
        if(rumbling)
        {
            if(rumbleValues==Vector2.zero)
                allowStoppedTimeRumble = false;
		    rumble = rumbleValues;
            //Debug.Log("Rumble: "+rumble);
        }
	}
    public void enableTimeStopRumble()
    {
        if(rumble!=Vector2.zero&&!allowStoppedTimeRumble)
            savedRumble = rumble;
        resetRumble();
        allowStoppedTimeRumble = true;
    }
    public void resetRumble()
    {
        rumble = Vector2.zero;
        allowStoppedTimeRumble = false;
    }
    public void loadSavedRumble()
    {
        if(savedRumble==Vector2.zero)
        rumble = Vector2.zero;
        else rumble = savedRumble;

        savedRumble = Vector2.zero;
        allowStoppedTimeRumble = false;
    }
}

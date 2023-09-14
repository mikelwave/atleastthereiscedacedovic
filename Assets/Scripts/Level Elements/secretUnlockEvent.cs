using UnityEngine;

//Unlocking the door to 7-7 in 7-6 after beaten last boss
public class secretUnlockEvent : MonoBehaviour
{
    dataShare DataS;

    void Start()
    {
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        if(dataShare.totalCompletedLevels>=41)
        {
            transform.GetChild(1).GetComponent<DoorScript>().eventLocked = false;
        }
    }
    public void loadSecret()
    {
        DataS.lastLoadedLevel = 41;
        DataS.loadSceneWithoutLoadScreen(51);
    }
}

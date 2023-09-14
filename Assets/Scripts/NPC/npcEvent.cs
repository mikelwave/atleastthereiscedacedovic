using UnityEngine;

public class npcEvent : MonoBehaviour
{
    dataShare DataS;
    // Start is called before the first frame update
    void Start()
    {
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        if(DataS.mode!=1&&dataShare.totalCompletedLevels>=35)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}

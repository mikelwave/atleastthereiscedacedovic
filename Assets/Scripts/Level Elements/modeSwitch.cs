using UnityEngine;

//Switches the characters at the end of 7-7
public class modeSwitch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(GameObject.Find("DataShare").GetComponent<dataShare>().mode==1)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}

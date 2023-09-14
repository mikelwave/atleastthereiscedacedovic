using System.Collections;
using UnityEngine;

public class hazeScript : MonoBehaviour
{
    PlayerScript pScript;
    // Start is called before the first frame update
    void Start()
    {
        pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        Destroy(transform.GetChild(0).gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.name.ToLower().Contains("playercollider"))
        {
            pScript.poison(true);
        }
        //print(other.name);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.name.ToLower().Contains("playercollider"))
        {
            pScript.poison(false);
        }
    }
    public void disableEvent()
    {
        StartCoroutine(mistClear());
    }
    IEnumerator mistClear()
    {
        yield return new WaitForSeconds(1.3f);
        GetComponent<Collider2D>().enabled = false;
        pScript.poison(false);
    }
}

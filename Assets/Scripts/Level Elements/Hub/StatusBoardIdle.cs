using UnityEngine;

public class StatusBoardIdle : MonoBehaviour
{
    StatusBoardScript boardMain;
    bool inZone = false;
    public string[] activateKeys = new string[]{"Up", "Alt Up",};
    public string[] deActivateKeys = new string[]{"Down", "Alt Down","Run","Jump","Start","Select"};
    public void setInZone(bool toSet)
    {
        inZone = toSet;
    }
    Animator pAnim;
    // Start is called before the first frame update
    void Start()
    {
        boardMain = GetComponent<StatusBoardScript>();
        boardMain.Initialize();
        if(boardMain!=null&&boardMain.pScript!=null)
        pAnim = boardMain.pScript.anim;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0)
        {
            if(inZone&&!boardMain.ready&&!boardMain.enabled&&pAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="Player_idle")
            {
                foreach(string s in activateKeys)
                {
                    if(SuperInput.GetKeyDown(s))
                    {
                        boardMain.toggleEnable(true);
                    }
                }
            }
            else if(boardMain.ready&&boardMain.enabled)
            {
                foreach(string s in deActivateKeys)
                {
                    if(SuperInput.GetKeyDown(s))
                    {
                        boardMain.toggleEnable(false);
                    }
                }
            }
        }
    }
}

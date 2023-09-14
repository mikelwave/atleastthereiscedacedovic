#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class levelInfoUpdate : MonoBehaviour
{
    static hitBlockStore hS;
    static bool updated = false;
    [SerializeField]
    static bool updateLoaded = false;
    static levelInfoUpdate()
    {
        if(!updateLoaded)
        {
            print("LevelInfoUpdate loaded.");
            EditorApplication.update+=Update;
            updateLoaded = true;
        }
    }
    public static void assignHitBlock(hitBlockStore hitBlock)
    {
        hS = hitBlock;
        print("Assigned HitBlock successfully.");
    }
    public static hitBlockStore GetHitBlock()
    {
        return hS;
    }
    static void Update ()
    {
        if(!EditorApplication.isPlaying&&!updateLoaded)
        {
            print("LevelInfoUpdate loaded.");
            EditorApplication.update+=Update;
            updateLoaded = true;
        }
        if(hS!=null&&EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying&&!updated)
        {
            updated = true;
            if(hS.checkOnStart)
            hS.UpdateGameTiles();
            Debug.Log("HitBlock updated successfully.");
            print("LevelInfoUpdate unloaded.");
            EditorApplication.update-=Update;
        }
        else if(hS==null||!EditorApplication.isPlaying&&!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if(updated)
            {
                updated = false;
                //Debug.Log("update allowed");
            }
        }
        //Debug.Log("Updating");
    }
}
#endif
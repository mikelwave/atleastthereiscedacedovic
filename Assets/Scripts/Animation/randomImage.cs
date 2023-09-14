using UnityEngine;
using UnityEngine.UI;

//Random loading screen icon
public class randomImage : MonoBehaviour
{
    public Sprite[] sprites;
    public Sprite[] spritesAlt;
    Image render;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<Image>();
        int e = GameObject.Find("DataShare").GetComponent<dataShare>().mode;
        if(e!=1) render.sprite = sprites[Random.Range(0,sprites.Length)];
        else render.sprite = spritesAlt[Random.Range(0,spritesAlt.Length)];
    }
}

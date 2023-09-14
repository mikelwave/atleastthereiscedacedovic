using System.Collections.Generic;
using UnityEngine;

//Script for applying Gold/Platinum trashbug skins
public class AltSkin : MonoBehaviour
{
    public Sprite[] animSprites;
    public Sprite deadSprite,impactSprite;

    // Start is called before the first frame update
    void Start()
    {
        //Check if save file is marked as all clear.
        if(dataShare.allClear)
        {
            //Replace dead sprite
            GetComponent<EnemyCorpseSpawner>().deadSprite = deadSprite;

            //Replace main sprites
            List <Sprite> s = transform.GetChild(0).GetComponent<SimpleAnim2>().sprites;
            s.Clear();
            for(int i = 0;i<animSprites.Length;i++)
            {
                s.Add(animSprites[i]);
            }

            //Replace impact sprite
            if(impactSprite!=null)
            GetComponent<bulletScript>().impact = impactSprite;
        }
    }
}

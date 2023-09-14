using UnityEngine;

// Script for the colorful overlay for the halva invincibility effect
public class halvaOverlay : MonoBehaviour
{
	Sprite[] halvaSpritesSmall,halvaSpritesBig,halvaSpritesAxe,halvaSpritesKnife; // Sprite arrays
	SpriteRenderer render; // Sprite renderer
	playerSprite pSprites; // Player sprite animation script

	// Load the overlay sprites, mode determines what character is used
	public void Load(int mode)
	{
		render = GetComponent<SpriteRenderer>();
		pSprites = transform.parent.transform.parent.GetComponent<playerSprite>();
		if(mode!=1)
		{
			halvaSpritesSmall = Resources.LoadAll<Sprite>("ceda_overlay_small");
			halvaSpritesBig = Resources.LoadAll<Sprite>("ceda_overlay_big");
			halvaSpritesAxe = Resources.LoadAll<Sprite>("ceda_overlay_axe");
			halvaSpritesKnife = Resources.LoadAll<Sprite>("ceda_overlay_csknife");
		}
		else
		{
			halvaSpritesSmall = Resources.LoadAll<Sprite>("playuh/playuh_overlay_small");
			halvaSpritesBig = Resources.LoadAll<Sprite>("playuh/playuh_overlay_big");
			halvaSpritesAxe = Resources.LoadAll<Sprite>("playuh/playuh_overlay_axe");
			halvaSpritesKnife = halvaSpritesBig;
		}
	}
	// Update is called once per frame after other update operations
	void LateUpdate ()
	{
		switch(pSprites.state)
		{
			default:render.sprite = halvaSpritesBig  [pSprites.currentSpriteInt]; break;
			case 0: render.sprite = halvaSpritesSmall[pSprites.currentSpriteInt]; break;
			case 3: render.sprite = halvaSpritesAxe  [pSprites.currentSpriteInt]; break;
			case 5: render.sprite = halvaSpritesKnife[pSprites.currentSpriteInt]; break;
		}
	}
}

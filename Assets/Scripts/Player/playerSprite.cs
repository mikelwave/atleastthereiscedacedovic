using UnityEngine;
using System;

public class playerSprite : MonoBehaviour {
	public bool dontLoadAlt = false;
	public int state = 0;
	public bool eternal = false;
	Sprite[] smallSprites,tallSprites,cheeseSprites,axeSprites,eterCeda,eterAxe,knifeSprites,lknifeSprites;
	public SpriteRenderer render;
	public SpriteRenderer[] extraRenders;
	public int currentSpriteInt;
	public PlayerMusicBounce musicBounce;
	public Animator playerAnim;
	public playerSprite pSprites;
	public bool dontUpdateManually = false;
	public String small = "ceda_small",
	tall = "ceda_tall",
	cheese = "ceda_cheese",
	axe = "ceda_axe",
	eternalAxe = "eternal_axe_ceda",
	eternalBurek = "eternal_burek_ceda",
	knife = "ceda_csknife",
	LKnife = "ceda_Lknife";
	public void loadPlayuh(bool overlay)
	{
		if(!overlay)
		{
			small = "playuh/playuh_small";
			tall = "playuh/playuh_tall";
			cheese = "playuh/playuh_cheese";
			axe = "playuh/playuh_axe";
			eternalAxe = "playuh/eternal_axe_playuh";
			eternalBurek = "playuh/eternal_burek_playuh";
			knife = "playuh/playuh_csknife";
			LKnife = "playuh/playuh_Lknife";
		}
		else
		{
			small = "playuh/playuh_small_eyeless";
			tall = "playuh/playuh_tall_eyeless";
			cheese = tall;
			axe = tall;
			eternalAxe = tall;
			eternalBurek = tall;
			knife = tall;
			LKnife = tall;
		}
	}
	public void playSound()
	{
		musicBounce.data.playSoundStatic(115);
	}
	// Use this for initialization
	void Start () {
		int mode = GameObject.Find("DataShare").GetComponent<dataShare>().mode;
		if(mode==1)
		{
			if(small=="ceda_small"&&!dontLoadAlt)
			{
				loadPlayuh(false);
				//load halva sprites
				if(transform.GetChild(0).childCount!=0)
				transform.GetChild(0).GetChild(0).GetComponent<halvaOverlay>().Load(mode);
			}
			else if(small=="eyeless_small_overlay")
			loadPlayuh(true);
		}
		else
		{
			//load halva sprites
			if(!dontUpdateManually&&transform.GetChild(0).childCount!=0)
			transform.GetChild(0).GetChild(0).GetComponent<halvaOverlay>().Load(mode);
		}
		if(transform.childCount>1 &&transform.GetChild(0).name=="Player")
		{
			if(playerAnim==null)
			playerAnim = transform.GetChild(0).GetComponent<Animator>();
			render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		}
		else
		{
			if(playerAnim==null)
			playerAnim = transform.GetComponent<Animator>();
			if(render == null)
			render = GetComponent<SpriteRenderer>();
		}
		smallSprites = Resources.LoadAll<Sprite>(small);
		tallSprites = Resources.LoadAll<Sprite>(tall);
		cheeseSprites = Resources.LoadAll<Sprite>(cheese);
		axeSprites = Resources.LoadAll<Sprite>(axe);
		eterCeda = Resources.LoadAll<Sprite>(eternalBurek);
		eterAxe = Resources.LoadAll<Sprite>(eternalAxe);
		knifeSprites = Resources.LoadAll<Sprite>(knife);
		lknifeSprites = Resources.LoadAll<Sprite>(LKnife);
		if(musicBounce==null)
		musicBounce = GetComponent<PlayerMusicBounce>();

	}
	void setSprite(int StreamState,int StreamSprite,bool StreamEternal,bool renderEnabled)
	{
		render.enabled = renderEnabled;
			switch(StreamState)
			{
				default:
				render.sprite = smallSprites[StreamSprite];
				break;
				case 1:
				case 4:
				if(StreamEternal&&StreamState==4)
					render.sprite = eterCeda[StreamSprite];
				else
					render.sprite = tallSprites[StreamSprite];
				break;
				case 2:
				render.sprite = cheeseSprites[StreamSprite];
				break;
				case 3:
				if(StreamEternal)
					render.sprite = eterAxe[StreamSprite];
				else
					render.sprite = axeSprites[StreamSprite];
				break;
				case 5:
				render.sprite = knifeSprites[StreamSprite];
				break;
				case 6:
				render.sprite = lknifeSprites[StreamSprite];
				break;
			}
	}
	void LateUpdate ()
	{
		if(!dontUpdateManually)
		{
			string spriteName = render.sprite.name;
			var newSpriteInt = Array.FindIndex(smallSprites, item => item.name == spriteName);
			currentSpriteInt = Mathf.Clamp(newSpriteInt,0,smallSprites.Length);
			if(musicBounce.frame==1&&render.sprite.name == smallSprites[0].name&&!playerAnim.GetBool("Talk"))
			{
				currentSpriteInt = 1;
			}
			switch(state)
			{
				default:
				render.sprite = smallSprites[currentSpriteInt];
				break;
				case 1:
				case 4:
				if(eternal&&state==4)
					render.sprite = eterCeda[currentSpriteInt];
				else
					render.sprite = tallSprites[currentSpriteInt];
				break;
				case 2:
				render.sprite = cheeseSprites[currentSpriteInt];
				break;
				case 3:
				if(eternal)
					render.sprite = eterAxe[currentSpriteInt];
				else
					render.sprite = axeSprites[currentSpriteInt];
				break;
				case 5:
				render.sprite = knifeSprites[currentSpriteInt];
				break;
				case 6:
				render.sprite = lknifeSprites[currentSpriteInt];
				break;
			}
			if(pSprites!=null) pSprites.setSprite(state,currentSpriteInt,eternal,render.enabled);
			if(extraRenders!=null)
			{
				foreach(SpriteRenderer Erender in extraRenders)
				{
					Erender.sprite = render.sprite;
				}
			}
		}
	}
}

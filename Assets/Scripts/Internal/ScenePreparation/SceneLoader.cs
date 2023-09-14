using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour {

	public bool debugDontLoadScene = false;
	private bool loadScene = false;
	public Sprite[] sprites;
	public StringArray[] loadTips;
	public TextMeshProUGUI loadText;
	int scene;
	void Start()
	{
		/*string s = "";
		for(int i = 0;i<loadTips.Length;i++)
		{
			for(int j = 0;j<loadTips[i].stringArr.Length;j++)
			{
				s+=(loadTips[i].stringArr[j])+'\n';
			}
			s+='\n';
		}
		print(s);*/
		dataShare DataS =  GameObject.Find ("DataShare").GetComponent<dataShare>();
		StringArray str = null;
		Sprite spr;
		switch(DataS.mode)
		{
			default:
				str = loadTips[DataS.currentWorld];
				if(DataS.worldProgression>=DataS.currentWorld)
				switch(DataS.currentWorld)
				{
					default: spr = sprites[DataS.currentWorld]; break;
					case 0: spr = sprites[0]; break;
				}
				else spr = sprites[0];
			break;
			case 1:
			str = loadTips[8];
			spr = sprites[8];
			break;
		}
		loadText.text = str.stringArr[Random.Range(0,str.stringArr.Length)];
		GameObject.Find("LoadingImage").GetComponent<SpriteRenderer>().sprite = spr;
		if(!loadScene&&!debugDontLoadScene)
		{
			loadScene = true;
			scene = DataS.levelToLoad;
			StartCoroutine(sceneLoad());
		}
	}
	IEnumerator sceneLoad()
	{
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);
		//AsyncOperation async1 = null;
		AsyncOperation reloadAssets = Resources.UnloadUnusedAssets();
		//if(unloadScene!=0)
		//async1 = SceneManager.UnloadSceneAsync(0);
        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone&&!reloadAssets.isDone||!reloadAssets.isDone&&!async.isDone)
		{
            yield return null;
        }
		AsyncOperation async2 = SceneManager.UnloadSceneAsync(0);
		while (!async2.isDone)
		{
            yield return null;
        }
	}
}

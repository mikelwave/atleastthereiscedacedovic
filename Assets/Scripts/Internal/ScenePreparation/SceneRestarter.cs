using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestarter : MonoBehaviour {
public bool debug = false;
	/*void Update ()
	{
		if(debug&&Input.GetKeyDown(KeyCode.R))
		{
			Time.timeScale = 1;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}*/
	public void restartScene()
	{
		Time.timeScale = 1;
		StartCoroutine(sceneLoad());
	}
	IEnumerator sceneLoad()
	{
		int index = SceneManager.GetActiveScene().buildIndex;
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
		AsyncOperation async = SceneManager.LoadSceneAsync(index);
		//AsyncOperation async1 = SceneManager.UnloadSceneAsync(index);
		AsyncOperation reloadAssets = Resources.UnloadUnusedAssets();
        while (!reloadAssets.isDone&&!async.isDone)
		{
            yield return null;
        }
	}
	public void beginDeathAnim()
	{
		Time.timeScale = 0;
	}
}

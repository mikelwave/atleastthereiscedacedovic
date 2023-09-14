using UnityEngine;

public class Dumbtest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		print("press keys now");
	}
	
	// Update is called once per frame
	void Update () {
		if(SuperInput.GetKeyDown("Jump"))
		{
			print("Jumping");
		}
		if(SuperInput.GetKeyUp("Up"))
		{
			print("Up Release");
		}
		if(SuperInput.GetKeyDown("Up"))
		{
			print("Up");
		}
		if(SuperInput.GetKeyUp("Down"))
		{
			print("Down Release");
		}
		if(SuperInput.GetKeyUp("Jump"))
		{
			print("rebind Up key by pressing a button");
			InputReader.rebindKey(0);
		}
		if(SuperInput.GetKeyUp("Run"))
		{
			print("rebind Down key by pressing a button");
			InputReader.rebindKey(1);
		}
	}
}

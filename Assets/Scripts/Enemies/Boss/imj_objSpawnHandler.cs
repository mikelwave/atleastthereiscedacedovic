using UnityEngine;

public class imj_objSpawnHandler : MonoBehaviour {
	imjBossScript main;
	ParticleSystem lazer;
	// Use this for initialization
	void Start () {
		main = transform.parent.GetComponent<imjBossScript>();
		lazer = transform.GetChild(3).GetComponent<ParticleSystem>();
	}
	
	public void spawnPlate()
	{
		main.spawnPlate();
	}
	public void razor()
	{
		main.spawnRazor();
	}
	public void toggleParticleLazer(int toggle)
	{
		if(toggle==0)
		{
			lazer.Play();
		}
		else lazer.Stop(false,ParticleSystemStopBehavior.StopEmitting);
	}
	public void  killAnimation()
	{
		main.StartCoroutine(main.explodeSoundLoop());
	}
	public void switchDir()
	{
		main.switchDir();
	}
	public void playSound(int i)
	{
		main.playSound(i);
	}
	public void playLoopSound(int i)
	{
		main.playLoopingSound(i);
	}
	public void killLoopSound()
	{
		main.killLoopSound();
	}
	public void playWarn()
	{
		main.summonWarning();
	}
	public void raisePhase()
	{
		main.newPhase();
	}

	public void fadeScene()
	{
		main.endScene();
	}
}

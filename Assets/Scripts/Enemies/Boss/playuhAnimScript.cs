using UnityEngine;

public class playuhAnimScript : MonoBehaviour
{
    playuhBossScript playuh;
    void Start()
    {
        playuh = transform.parent.GetComponent<playuhBossScript>();
    }
    public void powerUp()
    {
        playuh.powerUpEvent();
    }
    public void powerDown()
    {
        playuh.powerDownEvent();
    }
    public void superShoot()
    {
        playuh.superShoot();
    }
    public void playSound(int ID)
    {
        playuh.playSound(ID,true);
    }
    public void playSoundIntro(int ID)
    {
        if(playuh.midIntro)
        playuh.data.playUnlistedSound(playuh.sounds[ID]);
    }
    public void fakeFinalAttack()
    {
        playuh.StartCoroutine(playuh.fakeFinalAttack());
    }
    public void breakTiles(int ID)
    {
        playuh.breakTiles(ID);
        playuh.playSound(22,true);
    }
}

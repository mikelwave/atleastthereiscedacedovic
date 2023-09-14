using UnityEngine;

public class textBoxAssign : MonoBehaviour
{
    TextBox box;
    public bool noTextSound = false;
    public AudioClip letter_talk_alt;
    // Start is called before the first frame update
    void Start()
    {
        box = transform.GetChild(0).GetComponent<TextBox>();
        if(letter_talk_alt!=null)
        box.letter_type_angry = letter_talk_alt;
        if(noTextSound)
        {
            box.text_type = null;
            box.letter_type = null;
        }
    }
}

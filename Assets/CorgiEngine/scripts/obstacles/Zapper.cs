using UnityEngine;
using System.Collections;

public class Zapper : Switchable
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    override public IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);

        //if (OpenSoundEffect != null)
        //    SoundManager.Instance.PlaySound(OpenSoundEffect, transform.position);

        GetComponent<Animator>().SetBool("Disable", true);
        GetComponent<BoxCollider2D>().enabled = false;
    }


    override public IEnumerator Open(float duration)
    {
        yield return new WaitForSeconds(duration);

        //if (OpenSoundEffect != null)
        //    SoundManager.Instance.PlaySound(OpenSoundEffect, transform.position);

        GetComponent<Animator>().SetBool("Disable", true);
        GetComponent<BoxCollider2D>().enabled = false;
    }
}

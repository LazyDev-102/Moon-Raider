using UnityEngine;
using System.Collections;

public class IceBud : MonoBehaviour
{
    Popper popper;
    public AudioClip PuffSound;

    // Use this for initialization
    void Start()
    {
        popper = GetComponent<Popper>();
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        // we check that the object colliding with the water is actually a corgi controller and a character
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character == null)
            return;

        if(PuffSound != null)
            SoundManager.Instance.PlaySound(PuffSound, transform.position);

        popper.Pop();
        Destroy(gameObject, 0.025f);
    }
}
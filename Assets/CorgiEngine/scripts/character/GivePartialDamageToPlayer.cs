using UnityEngine;
using System.Collections;

public class GivePartialDamageToPlayer : MonoBehaviour
{
    public float PartialDamage = 0f;
    public Color DamageColor = Color.green;


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character == null)
            return;

        character.TakePartialDamage(PartialDamage, DamageColor);
    }


    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character == null)
            return;

        character.TakePartialDamage(-PartialDamage / 2, Color.white);
    }
}

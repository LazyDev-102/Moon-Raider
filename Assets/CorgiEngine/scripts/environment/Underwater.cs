using UnityEngine;
using System.Collections;

public class Underwater : MonoBehaviour
{
    public float WaterExitForce = 24f;
    public float PartialDamage = 0f;
    public Color DamageColor = Color.cyan;

    BoxCollider2D box;


    private void Start()
    {
        box = GetComponent<BoxCollider2D>();

        float offset = 1;

        box.size = new Vector2(box.size.x, box.size.y - offset);
        box.offset = new Vector2(0, -offset/2);
        
    }

    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        if (GameManager.Instance.Player != null)
        {
            if (collider.gameObject == GameManager.Instance.Player.gameObject)
            {
                if (!GameManager.Instance.Player.BehaviorState.Swimming)
                    GameManager.Instance.Player.EnterWater(PartialDamage, DamageColor);

                return;
            }
        }

        if (GameManager.Instance.PlayerTwo != null)
        {
            if (collider.gameObject == GameManager.Instance.PlayerTwo.gameObject)
            {
                if (!GameManager.Instance.PlayerTwo.BehaviorState.Swimming)
                    GameManager.Instance.PlayerTwo.EnterWater(PartialDamage, DamageColor);

                return;
            }
        }
    }

    /// <summary>
    /// Triggered when something exits the water
    /// </summary>
    /// <param name="collider">Something colliding with the water.</param>
    protected virtual void OnTriggerExit2D(Collider2D collider)
	{
        // we check that the object colliding with the water is actually a corgi controller and a character
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character != null)
        {
            if(character.Offscreen)
            {
                character.ExitWater(WaterExitForce);
                return;
            }

            CorgiController controller = collider.GetComponent<CorgiController>();
            if (controller == null)
                return;

            if (controller.Speed.y >= 0 && controller.transform.position.y > box.bounds.max.y)
            {
                character.ExitWater(WaterExitForce);
            }
        }
        else
        {
            if (collider.transform.position.y > box.bounds.max.y)
            {
                // Stop underwater critters from jumping out of the water
                Octopus octopus = collider.GetComponent<Octopus>();

                if (octopus != null)
                {
                    octopus.ExitWater();
                    return;
                }

                Eel eel = collider.GetComponent<Eel>();

                if (eel != null)
                {
                    eel.ExitWater();
                    return;
                }
            }
        }
    }
}

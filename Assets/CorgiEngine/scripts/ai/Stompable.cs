using UnityEngine;
using System.Collections;
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Health))]

/// <summary>
/// Add this class to an enemy (or whatever you want), to be able to stomp on it
/// </summary>
public class Stompable : MonoBehaviour 
{
	/// The number of vertical rays cast to detect stomping
	public int NumberOfRays=5;
	/// The force the hit will apply to the stomper
    public float KnockbackForce = 15f;
	/// The layer the player is on
    public LayerMask PlayerMask;
	/// The amount of damage each stomp causes to the stomped enemy
    public int DamagePerStomp;

	// private stuff
    protected BoxCollider2D _boxCollider;
    protected Health _health;

    /// <summary>
    /// On start, we get the various components
    /// </summary>
    protected virtual void Start ()
    {
        _boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
        _health = (Health)GetComponent<Health>();	
	}

    /// <summary>
    /// Each update, we cast rays above
    /// </summary>
    protected virtual void Update () 
	{
        CastRaysAbove();
	}

	/// <summary>
	/// Casts the rays above to detect stomping
	/// </summary>
    protected virtual void CastRaysAbove()
    {
        float rayLength = 0.5f;

        bool hitConnected = false;
        int hitConnectedIndex = 0;

        Vector2 verticalRayCastStart = new Vector2(_boxCollider.bounds.min.x,
                                                    _boxCollider.bounds.max.y);
        Vector2 verticalRayCastEnd = new Vector2(_boxCollider.bounds.max.x,
                                                   _boxCollider.bounds.max.y);

        RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfRays];

        for (int i = 0; i < NumberOfRays; i++)
        {
            Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(NumberOfRays - 1));
			hitsStorage[i] = CorgiTools.CorgiRayCast(rayOriginPoint, Vector2.up, rayLength, PlayerMask, true, Color.black);

            if (hitsStorage[i])
            {
                hitConnected = true;
                hitConnectedIndex = i;
                break;
            }
        }

        if (hitConnected)
        {
            CorgiController corgiController = hitsStorage[hitConnectedIndex].collider.GetComponent<CorgiController>();
			if (corgiController!=null)
            {
                corgiController.SetVerticalForce(KnockbackForce);
                _health.TakeDamage(DamagePerStomp, gameObject);
            }
        }
    }
}

using UnityEngine;
using System.Collections;
/// <summary>
/// A simple projectile behavior
/// </summary>
public class SimpleProjectile : Projectile, CanTakeDamage
{
	/// the amount of damage the projectile inflicts
	public int Damage;

    public AudioClip FireSfx;

    /// the effect to instantiate when the projectile gets destroyed
    public GameObject DestroyedEffect;
    public AudioClip DestroyedSfx;

    /// the amount of points to give the player when destroyed
    public int PointsToGiveToPlayer;	

	/// the lifetime of the projectile
	public float TimeToLive = 1;

    public float ShakeIntensity = 0;

    private float _pitch = 0;


    protected void Start()
    {
        var pc = GetComponent<PitchChanger>();
        if (pc != null)
            _pitch = pc.Pitch;

        if (RapidFire)
            _pitch += 0.1f;

        if (FireSfx != null)
            SoundManager.Instance.PlaySound(FireSfx, transform.position, false, 1f + _pitch);
    }

    /// <summary>
    /// Every frame, we check if the projectile has outlived its lifespan
    /// </summary>
    protected virtual void FixedUpdate () 
	{
		// if true, we destroy it
		if ((TimeToLive -= Time.deltaTime) <= 0)
		{
            DestroyProjectile(null);
            return;
		}
        // we move the projectile
        transform.Translate(Direction * ((Mathf.Abs(InitialVelocity.x) + Speed) * Time.deltaTime), Space.World);
    }	

	/// <summary>
	/// Called when the projectile takes damage
	/// </summary>
	/// <param name="damage">Damage.</param>
	/// <param name="instigator">Instigator.</param>
	public virtual void TakeDamage(int damage, GameObject instigator, bool melee = false)
	{
	/**	if (PointsToGiveToPlayer!=0)
		{
			var projectile = instigator.GetComponent<Projectile>();
			if (projectile != null && projectile.Owner.GetComponent<CharacterBehavior>() != null)
			{
				GameManager.Instance.AddPoints(PointsToGiveToPlayer);
			}
		}
		
		DestroyProjectile(null); **/
	}

	/// <summary>
	/// Triggered when the projectile collides with something
	/// </summary>
	/// <param name="collider">Collider.</param>
	protected override void OnCollideOther(Collider2D collider)
	{
       // Debug.Log("Collide other " + collider.gameObject.name);
		// Don't smudge platforms
		/*var layer = LayerMask.NameToLayer ("OneWayPlatforms");
		if (collider.gameObject.layer != layer)
        {
			var smudgePrefab = Resources.Load ("FX/Smudge") as GameObject;
			var smudge = Instantiate (smudgePrefab, collider.transform.position, Quaternion.identity);

			if (collider.transform.position.x > transform.position.x)
				smudge.transform.Rotate (Vector3.forward * 90);
			else
				smudge.transform.Rotate (Vector3.back * 90);

			smudge.transform.parent = transform;
		}*/

		DestroyProjectile(collider);
	}

	/// <summary>
	/// Raises the collide take damage event.
	/// </summary>
	/// <param name="collider">Collider.</param>
	/// <param name="takeDamage">Take damage.</param>
	protected override void OnCollideTakeDamage(Collider2D collider, CanTakeDamage takeDamage)
	{
        //Debug.Log("Collide damage " + collider.gameObject.name);

        takeDamage.TakeDamage(Damage, gameObject);
		DestroyProjectile(collider);
	}

    /// <summary>
    /// Destroys the projectile.
    /// </summary>
	protected virtual void DestroyProjectile(Collider2D collider)
	{
        if (DestroyedEffect != null)
        {
            var fx = Instantiate(DestroyedEffect, transform.position, transform.rotation);

            Animator fAnimator = fx.GetComponent<Animator>();

            if (fAnimator != null)
            {
                float l = fAnimator.GetCurrentAnimatorStateInfo(0).length;
                Destroy(fx, l);
            }

            if (DestroyedSfx != null)
                SoundManager.Instance.PlaySound(DestroyedSfx, transform.position, false, 1f + _pitch);

            if (ShakeIntensity > 0)
            {
                Vector3 ShakeParameters = new Vector3(ShakeIntensity, 0.5f, 1f);
                CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

                if (sceneCamera != null)
                    sceneCamera.Shake(ShakeParameters);
            }

            var sparklePrefab = Resources.Load("FX/Pebble") as GameObject;

            for (var o = 0; o < 4; o++)
            {
                GameObject sparkleObj = Instantiate(sparklePrefab, gameObject.transform.position, gameObject.transform.rotation);

                Rigidbody2D rigid2 = sparkleObj.GetComponent<Rigidbody2D>();

                if (Direction.x < 0)
                    rigid2.AddForce(new Vector2(Random.Range(1, 3), 0), ForceMode2D.Impulse);
                else
                    rigid2.AddForce(new Vector2(-Random.Range(1, 3), 0), ForceMode2D.Impulse);

                Destroy(sparkleObj, sparkleObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            }
        }

		Destroy(gameObject);
	}
}

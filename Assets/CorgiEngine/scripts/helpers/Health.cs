using UnityEngine;
using System.Collections;
/// <summary>
/// Adds this class to an object so it can have health (and lose it)
/// </summary>
public class Health : MonoBehaviour, CanTakeDamage
{
    /// the initial amount of health of the object
    public int InitialHealth;
	/// the current amount of health of the object
	public int CurrentHealth { get; protected set; }
	/// the points the player gets when the object's health reaches zero
//	public int PointsWhenDestroyed;
	public int MinDamageThreshold = 0;
	public int MinHurtDamage = 0;
    public bool MinTimeBetweenHurt = true;

	/// the effect to instantiate when the object takes damage
	public GameObject HurtEffect = null;

    [Header("Destroy")]
	public float DestroyDelay = 0.0f;
	public float ShakeIntensity = 1.0f;
	public float flickerSpeed = 0.02f;
	public int flickerCount = 10;
	public enum PopEnum{None, MiniGems, Gems, Shards, IceShards, PinkShards, PurpleShards, Health};

	//This is what you need to show in the inspector.
	public PopEnum popResult = PopEnum.MiniGems;

	public GameObject HurtBurstAnimation;
	public GameObject DestroyBurstAnimation;
	public GameObject DestroySmokeAnimation;
	public GameObject Remains;
	public bool RemainsBehind = false;
	public bool AlignDestroyToFloor = false;
	public int DamageNearby = 0;
    public LayerMask DamageMask;
	public float yOffset = 0;
	public float WalkDelay = 0;
    public bool Flickering = false;
    public bool StopsWalkOnHit = false;
    public bool ClearsParentColliders = true;

	[Header("Sounds")]
	// the sound to play when the player jumps
	public AudioClip HurtSfx;
	public AudioClip ReallyHurtSfx;
	// the sound to play when the player gets hit	
	public AudioClip DestroySfx;
	public AudioClip AltDestroySfx;
    public AudioClip DeflectSfx;

    //	private Color targetColor;
    private bool destroyed = false;
	private int _hurtDamage = 0;
	private bool _hurtEnable = false;
    private bool TakesDamage = true;

    // FIGHT ME!!!
    public bool PlayFightMusic = true;
	private FightMusic fightMusic;
	protected CharacterBehavior _characterBehavior;

	private SpriteRenderer _renderer;
	private Shader _shaderGUItext;
	private Shader _shaderSpritesDefault;

	private BoxCollider2D _collider;
    private Animator animator;
    private float _pitch = 0;

    public int DeflectionCount = 0;


    protected void Start()
    {
        CurrentHealth = InitialHealth;
//		targetColor = Color.white;
		var gameManagers = GameObject.Find ("GameManagers");
		BackgroundMusic bmusic = gameManagers.GetComponent<BackgroundMusic> ();
		fightMusic = bmusic.fightMusic;
		_characterBehavior = GetComponent<CharacterBehavior>();

        animator = GetComponentInParent<Animator>();
        if (animator == null)
            animator = GetComponent<Animator>();

        _collider = GetComponent<BoxCollider2D> ();

		var pc = GetComponentInParent<PitchChanger> ();
		if (pc != null)
			_pitch = pc.Pitch;

        _renderer = GetComponent<SpriteRenderer>();

        _shaderGUItext = Shader.Find("GUI/Text Shader");
		_shaderSpritesDefault = Shader.Find("Sprites/Default");
    }

    protected void OnEnable()
    {
        CurrentHealth = InitialHealth;
    }

	void Update ()
	{

	}

	public void PlayReallyHurt()
	{
		if (AltDestroySfx != null) {
			if (Random.Range (0, 9) < 5) {
				if (ReallyHurtSfx != null)
					SoundManager.Instance.PlaySound (ReallyHurtSfx, transform.position, false, 1f + _pitch);
			} else {
				SoundManager.Instance.PlaySound (AltDestroySfx, transform.position, false, 1f + _pitch);
			}
		} else {
			if (ReallyHurtSfx != null)
				SoundManager.Instance.PlaySound (ReallyHurtSfx, transform.position, false, 1f + _pitch);
		}
	}

	/// <summary>
	/// What happens when the object takes damage
	/// </summary>
	/// <param name="damage">Damage.</param>
	/// <param name="instigator">Instigator.</param>
	public virtual void TakeDamage(int damage, GameObject instigator, bool melee = false)
	{
        if (destroyed || !TakesDamage)
            return;

        if(damage < MinDamageThreshold)
        {
            if (animator != null)
            {
                if (animator.HasParameterOfType("Deflect", AnimatorControllerParameterType.Bool))
                {
                    if (!animator.GetBool("Deflect"))
                    {
                        animator.SetBool("Deflect", true);

                        if (DeflectSfx != null)
                            SoundManager.Instance.PlaySound(DeflectSfx, transform.position, false);

                        DeflectionCount++;
                        StartCoroutine(StopDeflect(0.3f));
                    }
                }
                else
                {
                    // We make the character's sprite flicker
                    StartCoroutine(Flicker());
                }
            }
            return;
        }

        if (MinTimeBetweenHurt)
        {
            TakesDamage = false;
            StartCoroutine(ResetLayerCollision(0.05f));
        }

        Vector3 ShakeParameters = new Vector3(ShakeIntensity/4,0.5f,1f);
		CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

		if (sceneCamera != null)
			sceneCamera.Shake(ShakeParameters);

		// Only take damange every _hurtDamage'th time
		bool playHurt = true;
		_hurtDamage += damage;

        if (_hurtDamage >= MinHurtDamage)
        {
            if (MinHurtDamage > 0)
                CurrentHealth -= 1;
            else
			    CurrentHealth -= damage;

            if (CurrentHealth < 0)
                CurrentHealth = 0;

            _hurtDamage = 0;
			_hurtEnable = true;

            if (animator != null)
            {
                if (animator.HasParameterOfType("Hurt", AnimatorControllerParameterType.Bool))
                {
                    if(!animator.GetBool("Hurt"))
                        animator.SetBool("Hurt", true);
                }
            }

            // Toggle off any AI
            if (StopsWalkOnHit)
            {
                AISimpleWalk walkAI = GetComponentInParent<AISimpleWalk>();
                if (walkAI != null)
                    walkAI.Disable();
            }

			playHurt = false;
            if(MinHurtDamage > 0 || CurrentHealth == 0)
			    PlayReallyHurt ();
		}

		// when the object takes damage, we instantiate its hurt effect
		if (HurtEffect != null) {
			Instantiate (HurtEffect, instigator.transform.position, transform.rotation);
		}

		// Let's handle the fight music. If this thing is hurt, crank it up!
		if (fightMusic != null && PlayFightMusic) {
			fightMusic.Play ();
		}

		// if the object doesn't have health anymore, we destroy it
		if (CurrentHealth <= 0)
        {
            if (melee && GetComponent<Optimize>() != null)
                popResult = PopEnum.Health;

            DestroyObject ();
			destroyed = true;
		}
        else
        {
			if (HurtSfx != null && playHurt)
				SoundManager.Instance.PlaySound(HurtSfx,transform.position, false, 1f + _pitch);

            // We make the character's sprite flicker
            StartCoroutine(Flicker());
        }	
	}


    protected virtual IEnumerator StopDeflect(float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.SetBool("Deflect", false);
    }


    protected virtual IEnumerator ResetLayerCollision(float delay)
    {
        yield return new WaitForSeconds(delay);

        TakesDamage = true;
    }


    /// <summary>
    /// Coroutine used to make the character's sprite flicker (when hurt for example).
    /// </summary>
    public IEnumerator Flicker()
    {
        Flickering = true;

        for (var n = 0; n < flickerCount; n++)
        {
			_renderer.material.shader = _shaderGUItext;
			yield return new WaitForSeconds(flickerSpeed);
			_renderer.material.shader = _shaderSpritesDefault;
			yield return new WaitForSeconds(flickerSpeed);
        }

		_renderer.color = Color.white;
        Flickering = false;

        if (_hurtEnable) 
        {
			_hurtEnable = false;

			yield return new WaitForSeconds(0.8f);

			Animator animator = GetComponentInParent<Animator> ();

            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator != null)
            {
                if (animator.HasParameterOfType("Hurt", AnimatorControllerParameterType.Bool))
                    animator.SetBool("Hurt", false);
            }

			if(StopsWalkOnHit)
			    StartCoroutine (Walk ());
		}

        // makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
//        Physics2D.IgnoreLayerCollision(9, 12, false);
//        Physics2D.IgnoreLayerCollision(9, 13, false);
    }

	public IEnumerator Walk()
	{
		yield return new WaitForSeconds(WalkDelay);

		// Toggle off any AI
		AISimpleWalk walkAI = GetComponentInParent<AISimpleWalk> ();
		if (walkAI != null)
			walkAI.Walk ();
	}


    /// <summary>
    /// Destroys the object
    /// </summary>
    protected virtual void DestroyObject()
	{
		EnemyController controller = GetComponent<EnemyController>();

		if (controller != null) {
			controller.enabled = false;
		}

		AIShootOnSight shootOnSight = GetComponent<AIShootOnSight> ();

        if (shootOnSight == null)
            shootOnSight = GetComponent<AIShootOnSight>();

        if (shootOnSight != null) {
			shootOnSight.CancelShoot ();
			shootOnSight.enabled = false;
		}

		Animator animator = GetComponentInParent<Animator>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            if (animator.HasParameterOfType("Dying", AnimatorControllerParameterType.Bool))
                animator.SetBool("Dying", true);
        }

        if (ClearsParentColliders)
        {
            BoxCollider2D[] boxes = GetComponentsInParent<BoxCollider2D>();

            if (boxes == null)
                boxes = GetComponents<BoxCollider2D>();

            for (int i = 0; i < boxes.Length; i++)
            {
                BoxCollider2D box = boxes[i];
                if (box != null)
                {
                    if (box.enabled)
                    {
                        box.enabled = false;
                        box.isTrigger = false;
                    }
                }
            }

            Rigidbody2D rigid = GetComponentInParent<Rigidbody2D>();

            if (rigid == null)
                rigid = GetComponent<Rigidbody2D>();

            if (rigid != null)
            {
                if (rigid.bodyType != RigidbodyType2D.Static)
                {
                    rigid.velocity = Vector3.zero;
                    rigid.gravityScale = 0;
                }
            }
        }

		GiveDamageToPlayer damage = GetComponentInParent<GiveDamageToPlayer> ();

        if (damage == null)
            damage = GetComponent<GiveDamageToPlayer>();

        if (damage != null) {
			damage.DamageToGive = 0;
			damage.enabled = false;
		}

		AIReact react = GetComponentInParent<AIReact> ();

        if (react == null)
            react = GetComponent<AIReact>();

        if (react != null) {
			react.enabled = false;
		}
			
		gameObject.layer = LayerMask.NameToLayer ("Foreground");

		if (HurtBurstAnimation != null) {
			var burstEffect = Instantiate (HurtBurstAnimation, transform.position + yOffset*Vector3.up, transform.rotation);

			Destroy (burstEffect, burstEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
		}

		// Pop into gems
		if (popResult == PopEnum.Gems)
			Gem.popIntoGems (gameObject, Random.Range(3, 7));
		else if (popResult == PopEnum.MiniGems)
			Gem.popIntoMiniGems (gameObject, Random.Range(3, 7));
		else if (popResult == PopEnum.Shards)
			Gem.popIntoShards (gameObject, Random.Range (2, 4));
        else if (popResult == PopEnum.IceShards)
            Gem.popIntoIceShards(gameObject, Random.Range(2, 4));
        else if (popResult == PopEnum.PinkShards)
            Gem.popIntoPinkShards(gameObject, Random.Range(2, 4));
        else if (popResult == PopEnum.PurpleShards)
            Gem.popIntoPurpleShards(gameObject, Random.Range(2, 4));
        else if (popResult == PopEnum.Health)
			Gem.popIntoHealth (gameObject, 1);

		StartCoroutine (Deactivate (DestroyDelay + 0.001f));
	}


	public virtual IEnumerator Deactivate(float duration)
	{
		yield return new WaitForSeconds (duration);

		if (_characterBehavior != null)
			_characterBehavior.BehaviorState.IsDead = true;

		// If this doesn't animate, then it should just be hidden
		if (GetComponent<Animator>() == null && GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = false;

		/*Vector2 raycastOriginS = new Vector2(transform.position.x, transform.position.y - 1);
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOriginS, Vector2.down, 1, (1 << LayerMask.NameToLayer("Platforms")), true, Color.yellow);

		// if the raycast hits anything
		if (raycast)
		{
			if (raycast.collider.gameObject.GetComponent<Health> () == null && raycast.collider.gameObject.GetComponent<BossWall>() == null)
            {
				var smudgePrefab = Resources.Load ("FX/Smudge") as GameObject;
				var smudge = Instantiate (smudgePrefab, raycast.point + Vector2.down, Quaternion.identity);
				smudge.transform.parent = raycast.collider.gameObject.transform.parent;
			}
		}*/

		Vector3 ShakeParameters = new Vector3(ShakeIntensity,0.5f,1f);
		CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

		if (sceneCamera != null)
			sceneCamera.Shake(ShakeParameters);

		if (DestroySfx != null)
			SoundManager.Instance.PlaySound (DestroySfx, transform.position);

		if (DestroyBurstAnimation != null)
        {
			var burstEffect = Instantiate (DestroyBurstAnimation, transform.position + yOffset*Vector3.up, transform.rotation);
			Destroy (burstEffect, burstEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
		}

		if (DestroySmokeAnimation != null)
        {
			var smokeEffect = Instantiate(DestroySmokeAnimation,transform.position,transform.rotation);

			if (AlignDestroyToFloor)
            {
				float newY = transform.position.y - GetComponent<SpriteRenderer> ().bounds.size.y / 2 + smokeEffect.GetComponent<SpriteRenderer> ().bounds.size.y / 2;
				smokeEffect.transform.position = new Vector3 (transform.position.x, newY, transform.position.z);
			}

			Destroy (smokeEffect, smokeEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
		}

		if (Remains != null)
        {
			var r = Instantiate (Remains, transform.position, transform.rotation);
			SpriteRenderer rSprite = r.GetComponent<SpriteRenderer>();
            
            //if(rSprite)
			    //rSprite.transform.localScale = transform.localScale;
			r.transform.parent = transform.parent;

            if (RemainsBehind)
                r.transform.position += Vector3.forward * 10;

            // Support fuse switches
            Fuse fuse = GetComponentInParent<Fuse>();
            if(fuse != null)
            {
                Plug plug = r.GetComponent<Plug>();
                if (plug != null)
                    plug.SaveIndex = fuse.SaveIndex;
            }

		}

		if (DamageNearby != 0)
        {
			Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);

            RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, 3f, Vector2.right, 0.0f, DamageMask);

            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];

                if (circle.collider != _collider)
                {
                    var neighborHealth = circle.collider.GetComponent<Health>();

                    if (neighborHealth)
                    {
                        neighborHealth.TakeDamage(DamageNearby, gameObject);
                    }

                    var neighborRigid = circle.collider.GetComponent<Rigidbody2D>();
                    if(neighborRigid)
                    {
                        neighborRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
                        neighborRigid.AddForce(new Vector2(0, -0.1f));
                    }
                }
            }
		}

		Animator animator = GetComponentInParent<Animator>();
        if (animator != null)
        {
            if (animator.HasParameterOfType("Dead", AnimatorControllerParameterType.Bool))
                animator.SetBool("Dead", true);
        }

		SpriteExploder exploder = gameObject.GetComponent<SpriteExploder>();
		if (exploder != null)
		{
            //Debug.Log("boom");
            exploder.transform.parent = GameManager.Instance.Grid.transform;
            exploder.ExplodeSprite();
		}
		else
		{
			yield return new WaitForSeconds(0.1f);

			gameObject.SetActive(false);
		}
	}
}

using UnityEngine;
using System.Collections;

public class BossBeetle : Boss
{
	public GameObject MouthSplash;

	private bool _started = false;
	private bool _wasHurt = false;
	private bool _wasDead = false;
    private bool _wasShooting = false;

	private Animator _animator;
//	private BoxCollider2D _collider;
	private Health _health;
	private AIShootOnSight _shoot;
    private EnemyController _controller;

	private CameraController sceneCamera;

	private int BossStage = 0;

	BossPillar pillar1;
	BossPillar pillar2;
	BossPillar pillar3;
	public float speed = 2f;

	// Use this for initialization
	void Start ()
	{
        if (CheckDefeated())
            return;

        _animator = GetComponent<Animator> ();
//		_collider = GetComponent<BoxCollider2D> ();
		_health = GetComponent<Health> ();
		_shoot = GetComponent<AIShootOnSight> ();
		_shoot.enabled = false;
        _controller = GetComponent<EnemyController>();

        sceneCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraController> ();

        StartCoroutine (FindPillars (0.125f));
	}
	
	// Update is called once per frame
	void Update ()
	{
		bool reacting = _animator.GetBool ("Reacting");

		if (reacting && !_started) 
		{
			_started = true;

			CloseWalls ();
			SoundManager.Instance.PlayBossMusic();

			sceneCamera.FreezeAt (pillar2.transform.position + 4*Vector3.up);

            StartCoroutine (RiseUp (0.5f, pillar3, speed));
            StartCoroutine(GUIManager.Instance.SlideBarsOut(3f));
            StartCoroutine (Attack (4.5f));
			return;
		}

        if (_shoot.isShooting && !_wasShooting)
        {
			if (MouthSplash != null)
			{
				var splash = Instantiate(MouthSplash, transform.position + 2.5f * transform.localScale.x * Vector3.right + 0.5f* Vector3.down, Quaternion.Euler(0, 0, -90 * transform.localScale.x));
				splash.transform.parent = transform.parent;

				Destroy(splash, 0.583f);
			}
        }

        _wasShooting = _shoot.isShooting;

        bool hurt = _animator.GetBool ("Hurt");
		bool dying = _animator.GetBool ("Dying");

		if (hurt && !_wasHurt && !dying) 
		{
			_wasHurt = true;

			_shoot.enabled = false;

			if (BossStage == 0) {

                //_health.MinDamageThreshold = 100;

                StartCoroutine (DropDown (0.1f, pillar1, speed / 2));
				StartCoroutine (DropDown (0.1f, pillar3, speed / 2));

				StartCoroutine (SwitchToPillar (4f, pillar1));

				StartCoroutine (RiseUp (6f, pillar1, speed));
				StartCoroutine (RiseUp (6f, pillar3, speed));
				StartCoroutine (Attack (9f));
			} 
			else if (BossStage == 1) {

                //_health.MinDamageThreshold = 100;

                StartCoroutine (DropDown (0.1f, pillar1, speed / 2));
				StartCoroutine (DropDown (0.1f, pillar2, speed / 2));

				StartCoroutine (SwitchToPillar (4f, pillar2));

				StartCoroutine (RiseUp (6f, pillar1, speed));
				StartCoroutine (RiseUp (6f, pillar2, speed));
				StartCoroutine (Attack (9f));
			} 
			else if (BossStage == 2) {
				StartCoroutine (DropDown (0.1f, pillar3, speed / 2));
				StartCoroutine (DropDown (4f, pillar1, speed / 2));
				StartCoroutine (RiseUp (5f, pillar3, speed));
				StartCoroutine (Attack (6.5f));
			}
			BossStage++;
			return;
		}

		if (dying && !_wasDead) {
			_wasDead = true;
			StartCoroutine (Dead(1f));
		}

        bool shouldTurn = false;
        if (GameManager.Instance.Player.transform != null)
        {
            if (GameManager.Instance.Player.transform.position.x > transform.position.x && transform.localScale.x < 0 ||
               GameManager.Instance.Player.transform.position.x < transform.position.x && transform.localScale.x > 0)
            {
                shouldTurn = true;
            }
        }

		if (shouldTurn && _animator.GetBool ("Turning") == false) {
			_animator.SetBool ("Turning", true);
			StartCoroutine(Flip(0.417f));
		}
	}


	public virtual IEnumerator Dead(float duration)
	{
		yield return new WaitForSeconds (duration);

		Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
		sceneCamera.Shake(ShakeParameters);

        OpenWalls ();

		StartCoroutine (RiseUp (2f, pillar1, speed));
        StartCoroutine(RiseUp(2f, pillar2, speed));
        StartCoroutine(RiseUp(2f, pillar3, speed));

        SoundManager.Instance.PlayRegularMusic ();
	}

	public virtual IEnumerator Flip(float duration)
	{
		yield return new WaitForSeconds (duration);

		_animator.SetBool ("Turning", false);
		_shoot.enabled = true;
		transform.localScale = new Vector3 (-transform.localScale.x, 1, 1);
	}

	public virtual IEnumerator RiseUp(float duration, BossPillar pillar, float s)
	{
		yield return new WaitForSeconds (duration);

		//_health.MinDamageThreshold = 1;

		pillar.Rise (s);
		_wasHurt = false;

        if(BossStage == 0)
        {
            // temp
            SuckUpGems();
        }

        yield return new WaitForSeconds(duration);

        GameManager.Instance.ThawCharacter();

    }

	public virtual IEnumerator DropDown(float duration, BossPillar pillar, float s)
	{
		yield return new WaitForSeconds (duration);

		pillar.Sink (s);
	}

	public virtual IEnumerator Attack(float duration)
	{
		yield return new WaitForSeconds (duration);

		_shoot.enabled = true;
	}


	public virtual IEnumerator SwitchToPillar(float duration, BossPillar pillar)
	{
		yield return new WaitForSeconds (duration);

        if (pillar.transform.localPosition.x <= pillar2.transform.localPosition.x)
		    transform.localScale = new Vector3 (1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
        transform.localPosition = new Vector3 (pillar.transform.position.x, transform.localPosition.y, transform.localPosition.z);
	}

	public virtual IEnumerator FindPillars(float duration)
	{
		yield return new WaitForSeconds (duration);

        _controller.Parameters.Gravity = -50;

        var maskLayer = 1 << LayerMask.NameToLayer ("Platforms");
		RaycastHit2D[] circles = Physics2D.CircleCastAll (transform.localPosition, 400.0f, Vector2.right, 0.0f, maskLayer);

		for(int i = 0; i < circles.Length; i++)
		{
			var circle = circles [i];

			var pillar = circle.collider.gameObject.GetComponent<BossPillar> ();

			if (pillar != null) {
				if (pillar.transform.position.x == -5)
					pillar2 = pillar;
				else if (pillar.transform.position.x == -13)
					pillar1 = pillar;
				else
					pillar3 = pillar;
			}
		}

		StartCoroutine (DropDown (0.1f, pillar3, 10f));
	}
}


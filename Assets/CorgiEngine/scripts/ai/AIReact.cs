using UnityEngine;
using System.Collections;

public class AIReact : MonoBehaviour
{
	protected Animator _animator;
	protected bool updated = false;

	public Vector2 point; 
	public bool Reacting = false;
	public float radius;
	LayerMask maskLayer = 0;
	public float SFXDelay = 1.0f;
	public enum ModeEnum{Radial, Linear, Both};
	public ModeEnum mode = ModeEnum.Radial;
    public bool JustPlayerOne = false;

    [Header("Health")]
	public AudioClip ReactSfx;
	public AudioClip AltReactSfx;
	public AudioClip TertiaryReactSfx;

	public float ShootAngle = 0.0f;
	private float nextSFX = 0.0f;

	private float _pitch = 0;

	public GameObject Target;

	// Use this for initialization
	void Start ()
	{
		_animator = GetComponentInParent<Animator>();

        if(_animator == null)
			_animator = GetComponentInChildren<Animator>();

		nextSFX = Time.time;

		maskLayer = 1 << LayerMask.NameToLayer ("Player");

		var pc = GetComponentInParent<PitchChanger> ();
		if (pc != null)
			_pitch = pc.Pitch;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		if (mode == ModeEnum.Radial)
			SearchRadial ();
		else if (mode == ModeEnum.Linear)
			SearchLinear ();
		else {
			SearchRadial ();

			if (!Reacting)
				SearchLinear ();
		}

		// updates the animator if it's not null
		if (_animator != null)
		{
			if (_animator.HasParameterOfType("Reacting", AnimatorControllerParameterType.Bool))
				_animator.SetBool("Reacting", Reacting);
		}
	}

	void SearchLinear()
	{
		Vector2 _direction = new Vector2 (-transform.localScale.x * (float)Mathf.Cos (Mathf.Deg2Rad * ShootAngle), transform.localScale.y * -(float)Mathf.Sin (Mathf.Deg2Rad * ShootAngle));

		// we cast a ray in front of the agent to check for a Player
		var dist = radius;

		if (mode == ModeEnum.Both)
			dist *= 2;
            
        RaycastHit2D raycast = CorgiTools.CorgiRayCast(transform.localPosition, _direction, dist, maskLayer, true, Color.blue);

		if (raycast)
        {
            bool r = raycast.collider.gameObject.tag == "Player";

            if(GlobalVariables.TwoPlayer && !JustPlayerOne)
                r = (raycast.collider.gameObject.tag == "Player" || raycast.collider.gameObject.tag == "PlayerTwo");

            Reacting = r;
			Target = raycast.collider.gameObject;

			React (raycast.point);
		} else {
			Reacting = false;
		}
	}

	void SearchRadial()
	{
        RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, radius, Vector2.right, 0.0f, maskLayer);

		Reacting = false;

		for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];

            if (circle)
            {
                bool r = circle.collider.gameObject.tag == "Player";

                if (GlobalVariables.TwoPlayer && !JustPlayerOne)
                    r = (circle.collider.gameObject.tag == "Player" || circle.collider.gameObject.tag == "PlayerTwo");

                Reacting = r;
                Target = circle.collider.gameObject;

                React(circle.point);
            }
        }
	}

	void React(Vector2 p)
	{
		point = p;
		if (!updated)
		{
			updated = true;

			if (Time.time > nextSFX) 
			{
				if (AltReactSfx == null) {
					if (ReactSfx != null)
						SoundManager.Instance.PlaySound (ReactSfx, transform.position, false, 1f + _pitch);
				} else {
					int rand = Random.Range (0, 10);
					if (rand < 5) {
						if (ReactSfx != null)
							SoundManager.Instance.PlaySound (ReactSfx, transform.position, false, 1f + _pitch);
					} else if (rand >= 4 && rand < 8) {
						if (AltReactSfx != null)
							SoundManager.Instance.PlaySound (AltReactSfx, transform.position, false, 1f + _pitch);
					} else if (rand == 2) {
						if (TertiaryReactSfx != null)
							SoundManager.Instance.PlaySound (TertiaryReactSfx, transform.position, false, 1f + _pitch);
					}
				}

				nextSFX = Time.time + SFXDelay;
			}
		}
	}

	public virtual IEnumerator Reset(float duration)
	{
		yield return new WaitForSeconds (duration);
		updated = false;
		AISimpleWalk walk = GetComponentInParent<AISimpleWalk> ();

		if (walk != null) {
			walk.Speed = walk.Speed / 1.5f;
		}
	}
}

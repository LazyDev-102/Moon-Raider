using UnityEngine;
using System.Collections;

public class Guard : MonoBehaviour
{
    public bool StopToAttack = false;
    public bool SpeedUpToAttack = false;
	public LayerMask WallMask;

    protected EnemyController _controller;
	protected Animator _animator;
	protected AISimpleWalk _walk;
	protected AIReact _react;
	protected Rigidbody2D _rigid;
	protected SpriteRenderer _renderer;

    float orgSpeed = 0;

	// Use this for initialization
	void Start ()
	{
		_controller = GetComponent<EnemyController>();
		_renderer = GetComponent<SpriteRenderer>();
		_animator = GetComponent<Animator> ();
		_walk = GetComponent<AISimpleWalk> ();
		_react = GetComponent<AIReact> ();
		_rigid = GetComponent<Rigidbody2D> ();

        orgSpeed = _walk.Speed;

        // Random variant
        for (int i = 0; i < 3; i++)
        {
            _animator.SetLayerWeight(i, 0);
        }

        int show = (int)Random.Range(0, 3);
        _animator.SetLayerWeight(show, 1);
    }
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (_animator != null) 
		{ 
			CorgiTools.UpdateAnimatorBool (_animator, "Grounded", _controller.State.IsGrounded);
			CorgiTools.UpdateAnimatorFloat(_animator,"vSpeed",_controller.Speed.y);
		}

		if (_react != null && _walk != null) 
		{
			_walk.AvoidFalling = !_react.Reacting;

            if (SpeedUpToAttack)
            {
                if (_react.Reacting)
                    _walk.Speed = 1.5f * orgSpeed;
                else
                    _walk.Speed = orgSpeed;
            }
        }
	}


	private bool SeesHighWall()
	{
		// See if it's just a 1-high,  otherwise say it's false
		RaycastHit2D highWall = CorgiTools.CorgiRayCast (transform.position + 1.5f*Vector3.up, _walk.Direction, 4, WallMask, true, Color.black);

		if (highWall) 
		{
			return true;
		}

		return false;
	}

	private bool SeesWall()
	{
		RaycastHit2D wall = CorgiTools.CorgiRayCast (transform.position, 1.5f* _walk.Direction, 4, WallMask, true, Color.yellow);

		if (wall) 
		{
			return _controller.State.IsGrounded;
		}

		return false;
	}

    private void OnTriggerEnter(Collider other)
    {
        if (_animator.GetBool("Reacting"))
        {
            CorgiTools.UpdateAnimatorBool(_animator, "Colliding", true);

            if (StopToAttack)
            {
                _walk.Disable();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CorgiTools.UpdateAnimatorBool(_animator, "Colliding", false);

        if (StopToAttack)
        {
            _walk.Start();
        }
    }
}


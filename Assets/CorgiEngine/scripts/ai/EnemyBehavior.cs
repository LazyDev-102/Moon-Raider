using UnityEngine;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
	protected Animator _animator;
	protected EnemyController _controller;
    protected SpriteRenderer _renderer;

    // Use this for initialization
    void Start ()
	{
		_animator = GetComponent<Animator> ();
		_controller = GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update ()
	{
		CorgiTools.UpdateAnimatorFloat(_animator,"Speed",Mathf.Abs(_controller.Speed.x));
	}
}


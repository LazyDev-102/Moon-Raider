using UnityEngine;
using System.Collections;
/// <summary>
/// Add this class to a platform to make it a jumping platform, a trampoline or whatever.
/// It will automatically push any character that touches it up in the air.
/// </summary>
public class Jumper : MonoBehaviour 
{
	/// the force of the jump induced by the platform
	public float JumpPlatformBoost = 40;
	public AudioClip SoundEffect = null;

	private Animator _animator;

	void Start()
	{
		_animator = GetComponent<Animator> ();
	}

	/// <summary>
	/// Triggered when a CorgiController touches the platform, applys a vertical force to it, propulsing it in the air.
	/// </summary>
	/// <param name="controller">The corgi controller that collides with the platform.</param>
		
	public virtual void OnTriggerEnter2D(Collider2D collider)
	{
		CorgiController controller = collider.GetComponent<CorgiController>();
		if (controller == null)
			return;

		if (SoundEffect != null)
			SoundManager.Instance.PlaySound (SoundEffect, transform.position);

        controller.NoFall();
        controller.SetVerticalForce(JumpPlatformBoost);

		Animator pAnimator = collider.GetComponentInChildren<Animator>();

		if (pAnimator != null)
			pAnimator.SetBool ("SpinJumping", true);
		//else
		//	Debug.Log ("No pAnimator");

		if (_animator != null) {
			_animator.SetBool ("On", true);
			StartCoroutine (Off (0.25f, controller));
		}
	}

	public virtual IEnumerator Off(float duration, CorgiController controller)
	{
		yield return new WaitForSeconds (duration);

		if (_animator != null) {
			_animator.SetBool ("On", false);
		}

        yield return new WaitForSeconds(1.125f);
        controller.ResetGravity();
    }
}

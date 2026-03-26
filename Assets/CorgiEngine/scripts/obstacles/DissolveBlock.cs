using UnityEngine;
using System.Collections;

public class DissolveBlock : MonoBehaviour
{
	Animator _animator;

	public AudioClip TriggerSfx;
	public AudioClip ExplodeSfx;

    public int SaveIndex = 0;


	// Use this for initialization
	void Start ()
	{
		_animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (_animator.GetBool("Started"))
            return;

		var mask = 1 << LayerMask.NameToLayer ("Player");
		RaycastHit2D playerL = CorgiTools.CorgiRayCast (transform.position + Vector3.up + Vector3.left, Vector3.up, 2f, mask, true, Color.yellow);
        RaycastHit2D playerM = CorgiTools.CorgiRayCast(transform.position + Vector3.up, Vector3.up, 2f, mask, true, Color.yellow);
        RaycastHit2D playerR = CorgiTools.CorgiRayCast(transform.position + Vector3.up + Vector3.right, Vector3.up, 2f, mask, true, Color.yellow);

        if (playerL || playerM || playerR) 
		{
            TriggerDissolve();
		}
	}


    void TriggerDissolve()
    {
        _animator.SetBool("Started", true);

        if (TriggerSfx != null)
            SoundManager.Instance.PlaySound(TriggerSfx, transform.position);

        StartCoroutine(DisableCollision(0.667f));
        StartCoroutine(Disable(1.167f));
    }

	public virtual IEnumerator DisableCollision(float duration)
	{
		yield return new WaitForSeconds (duration);

		GetComponent<BoxCollider2D> ().enabled = false;

		if (ExplodeSfx != null)
			SoundManager.Instance.PlaySound(ExplodeSfx, transform.position);
	}

	public virtual IEnumerator Disable(float duration)
	{
		yield return new WaitForSeconds (duration);

		enabled = false;
        LevelVariables.blockState[SaveIndex] = true;
        GetComponent<SpriteRenderer>().enabled = false;
    }
}


using UnityEngine;
using System.Collections;

public class PopupSpike : MonoBehaviour
{
	public AudioClip PopSfx;
	public float Cap = 1;

	Vector3 targetPos;
	Vector3 orgPos;

	// Use this for initialization
	void Start ()
	{
		orgPos = transform.position;
		targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = Vector3.Lerp (transform.position, targetPos, 5*Time.deltaTime);
	}

	public virtual IEnumerator Popup(float duration)
	{
		yield return new WaitForSeconds (duration);

		targetPos = transform.position + 1.5f*Vector3.up;

		if (PopSfx != null && Random.Range(0,3) < 1)
			SoundManager.Instance.PlaySound(PopSfx, transform.position);

		StartCoroutine (Drop (0.75f));
	}

	public virtual IEnumerator Drop(float duration)
	{
		yield return new WaitForSeconds (duration);

		targetPos = orgPos;
	}
}


using UnityEngine;
using System.Collections;

public class BossPillar : MonoBehaviour
{
	public AudioClip RiseFallSfx;

	Vector3 targetPosition;
	bool up = false;
	float speed = 1f;
	public int Number = 0;
	private CameraController sceneCamera;

	//private BoxCollider2D lavaCollider;

    public float Bottom = 4f;

	// Use this for initialization
	void Start ()
	{
		targetPosition = transform.position;
		sceneCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraController> ();

		//Debug.Log ("Pillar at " + transform.localPosition.x);
	}

	void ClearCollider()
	{
		// Clear out lava collider
		/*var mask = 1 << LayerMask.NameToLayer ("Platforms");
		RaycastHit2D[] pillars = CorgiTools.CorgiRaycastAll (transform.position, Vector3.down, 2f, mask, true, Color.yellow);

		for(int i = 0; i < pillars.Length; i++)
		{ 
			var pillar = pillars [i];
			if (pillar.collider.GetComponent<GiveDamageToPlayer> ())
			{
				lavaCollider = pillar.collider.GetComponent<BoxCollider2D> ();
				lavaCollider.enabled = false;
			}
		}*/
	}

	// Update is called once per frame
	void Update ()
	{
		if (transform.position.y < targetPosition.y) {

			if (targetPosition.y - transform.localPosition.y <= 0.1f) {
				transform.position = targetPosition;
				//lavaCollider.enabled = true;
			}
			else {
				Vector3 ShakeParameters = new Vector3(0.25f, 0.25f, 1f);
				sceneCamera.Shake(ShakeParameters);

				Vector3 newPosition = speed * Time.deltaTime * Vector3.up;
				transform.Translate (newPosition, Space.World);
			}
		}
		else if (transform.localPosition.y > targetPosition.y) {

			if (transform.position.y - targetPosition.y <= 0.1f) {
				transform.position = targetPosition;
			}
			else {

				Vector3 ShakeParameters = new Vector3(0.25f, 0.25f, 1f);
				sceneCamera.Shake(ShakeParameters);

				Vector3 newPosition = speed * Time.deltaTime * Vector3.down;
				transform.Translate (newPosition, Space.World);
			}
		}
	}

	public void Rise(float s)
	{
		if (RiseFallSfx != null)
			SoundManager.Instance.PlaySound(RiseFallSfx, transform.position);	
		
		speed = s;
		targetPosition = transform.position + Bottom * Vector3.up;
	}

	public void Sink(float s)
	{
		if (RiseFallSfx != null)
			SoundManager.Instance.PlaySound(RiseFallSfx, transform.position);	

		//ClearCollider ();
		speed = s;
		targetPosition = transform.position + Bottom * Vector3.down;
	}
}


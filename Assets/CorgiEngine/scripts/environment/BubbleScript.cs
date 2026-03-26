using UnityEngine;
using System.Collections;

public class BubbleScript : MonoBehaviour
{
	private Health _health;
	public GameObject StartDead;
	public GameObject StopDead;

	public virtual void Start()
	{
		_health = GetComponentInParent<Health> ();

		if (_health == null) {
			//Debug.Log ("No health!");
			return;
		}

		float sizeX = GetComponentInParent<SpriteRenderer> ().bounds.size.x;
		float sizeY = GetComponentInParent<SpriteRenderer> ().bounds.size.y;

		Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);

		float rotation = transform.rotation.eulerAngles.z;

		if (rotation == 90f) 
		{
			RaycastHit2D[] raycastL = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.left, 1.1f*sizeX, (1 << LayerMask.NameToLayer ("Platforms")), true, Color.red);

			if (raycastL.Length > 0) {
				for (int i = 0; i < raycastL.Length; i++) {
					if (raycastL[i].collider.gameObject.GetComponent<BubbleScript> () == null)
						_health.Remains = Instantiate(StartDead, transform.position, transform.rotation);
				}
			}

			RaycastHit2D[] raycastR = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.right, 1.1f*sizeX, (1 << LayerMask.NameToLayer ("Platforms")), true, Color.red);

			if (raycastR.Length > 0) {
				for (int i = 0; i < raycastR.Length; i++) {
					if(raycastR[i].collider.gameObject.GetComponent<BubbleScript>() == null)
						_health.Remains = Instantiate(StopDead, transform.position, transform.rotation);
				}
			}
		}
		else if (rotation == 0f) 
		{
			RaycastHit2D[] raycastL = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.down, 1.1f*sizeY, (1 << LayerMask.NameToLayer ("Platforms")), true, Color.red);


			if (raycastL.Length > 0) {
				for (int i = 0; i < raycastL.Length; i++) {
					if(raycastL[i].collider.gameObject.GetComponent<BubbleScript>() == null)
						_health.Remains = Instantiate(StopDead, transform.position, transform.rotation);
				}
			}

			RaycastHit2D[] raycastR = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.up, 1.1f*sizeY, (1 << LayerMask.NameToLayer ("Platforms")), true, Color.red);

			if (raycastR.Length > 0) {
				for (int i = 0; i < raycastR.Length; i++) {
					if(raycastR[i].collider.gameObject.GetComponent<BubbleScript>() == null)
						_health.Remains = Instantiate(StartDead, transform.position, transform.rotation);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}


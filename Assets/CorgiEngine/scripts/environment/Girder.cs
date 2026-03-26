using UnityEngine;
using System.Collections;

public class Girder : MonoBehaviour
{
	public virtual void Awake()
	{
        if (GlobalVariables.WorldIndex != 1 && GlobalVariables.WorldIndex != 5)
            return;

		int show =  Random.Range (1, 10);

		if (show == 1) {
			var vinePrefab = Resources.Load ("Environment/VineA") as GameObject;
			var vine = Instantiate (vinePrefab, gameObject.transform.position, Quaternion.identity);
			vine.transform.parent = transform;
		} else if (show == 2) {
			var vinePrefab = Resources.Load ("Environment/VineB") as GameObject;
			var vine = Instantiate (vinePrefab, gameObject.transform.position, Quaternion.identity);
			vine.transform.parent = transform;
		} else if (show == 3) {
			var vinePrefab = Resources.Load ("Environment/VineC") as GameObject;
			var vine = Instantiate (vinePrefab, gameObject.transform.position, Quaternion.identity);
			vine.transform.parent = transform;
		} 
	}
}


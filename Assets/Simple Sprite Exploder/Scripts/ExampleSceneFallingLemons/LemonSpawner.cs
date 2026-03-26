using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LemonSpawner : MonoBehaviour {

    [SerializeField]
    private Transform lemonPrefab;

    private float t = 3;
    
	void Update ()
    {
        t += Time.deltaTime;
        if (t > 2.5f)
        {
            Transform temp = Instantiate(lemonPrefab, transform.position, Quaternion.identity, transform);
            temp.position = new Vector3(Random.Range(-4.99f, 5.0f), temp.position.y, temp.position.z);
            temp.Rotate(transform.forward, Random.Range(-180, 180));
            t = 0;
        }
	}
}

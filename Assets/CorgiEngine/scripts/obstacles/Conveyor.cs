using UnityEngine;
using System.Collections;

public class Conveyor : MonoBehaviour
{
	public float AddedSpeedX = 1.0f;
	public bool Reverse = false;

	// Use this for initialization
	void Start ()
	{
		Reverse = transform.localScale.x < 0;
		if (Reverse) {
			AddedSpeedX = -AddedSpeedX;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}


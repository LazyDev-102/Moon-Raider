using UnityEngine;
using System.Collections;

public class PitchChanger : MonoBehaviour
{
    public float MinShift = 0;
    public float MaxShift = 10;
    public float Scalar = 50;

    public float Pitch
    {
        get
        {
            return _pitch;
        }
    }


	private float _pitch = 0f;

	// Use this for initialization
	void Awake ()
	{
		_pitch = (float)Random.Range (MinShift, MaxShift) / Scalar;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}


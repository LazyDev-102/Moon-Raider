using UnityEngine;
using System.Collections;

public class AILook : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (GameManager.Instance.Player == null)
			return;

		if (GameManager.Instance.Player.transform.position.x > transform.position.x)
			transform.localScale = new Vector3(-1, 1, 1);
		else
			transform.localScale = Vector3.one;
    }
}

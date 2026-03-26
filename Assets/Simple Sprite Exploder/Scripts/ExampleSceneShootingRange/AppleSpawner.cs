using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleSpawner : MonoBehaviour {

    [SerializeField]
    private Sprite crossahir;
    [SerializeField]
    private GameObject applePrefab;
    [SerializeField]
    private bool slowMoOnClick;

    private bool shot;
    private float fastT;
    private bool speedUp;

    // Use this for initialization
    void Start () {
        Cursor.SetCursor(crossahir.texture, new Vector2(crossahir.texture.width / 2, crossahir.texture.height / 2), CursorMode.Auto);
	}
	
	// Update is called once per frame
	void Update () {
		if (slowMoOnClick)
        {
            if (shot && !speedUp)
            {
                Time.timeScale = 0.35f;
            }
            else if (!shot && speedUp)
            {
                Time.timeScale = Mathf.Lerp(0.35f, 1, fastT);
                fastT += 2 * Time.deltaTime;

                if (Time.timeScale > 0.99)
                    speedUp = false;
            }
            else
            {
                Time.timeScale = 1;
                fastT = 0;
            }
        }
	}

    public void SpawnNewApple(Vector2 pos)
    {
        shot = true;
        StartCoroutine(WaitThenSpawn(pos));
        StartCoroutine(WaitThenSpeed());
    }

    IEnumerator WaitThenSpawn(Vector2 pos)
    {
        yield return new WaitForSeconds(2.0f);
        Instantiate(applePrefab, pos, Quaternion.identity);
    }

    IEnumerator WaitThenSpeed()
    {
        yield return new WaitForSeconds(0.25f);
        shot = false;
        speedUp = true;
    }
}

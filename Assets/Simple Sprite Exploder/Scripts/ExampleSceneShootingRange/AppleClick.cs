using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleClick : MonoBehaviour {

    private void OnMouseDown()
    {
        AppleSpawner appleSpawner = GameObject.Find("AppleSpawner").GetComponent<AppleSpawner>();
        appleSpawner.SpawnNewApple(transform.position);
    }
}

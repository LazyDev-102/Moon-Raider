using UnityEngine;
using System.Collections;

public class Waterfall : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        StartCoroutine(Splash(0.1f));
    }

    public virtual IEnumerator Splash(float duration)
    {
        // we wait for a few seconds
        yield return new WaitForSeconds(duration);

        var maskLayer = (1 << LayerMask.NameToLayer("Platforms")) | (1 << LayerMask.NameToLayer("Water"));
        RaycastHit2D raycast = Physics2D.Raycast(transform.localPosition, Vector2.down, 2, maskLayer);

        if (raycast)
        {
            if (GlobalVariables.WorldIndex == 9)
            {
                var waterfallSplashPrefab = Resources.Load("Environment/SewerFallSplash") as GameObject;
                Instantiate(waterfallSplashPrefab, new Vector3(transform.localPosition.x, transform.localPosition.y - 1, -5), Quaternion.identity);
            }
            else
            {
                var waterfallSplashPrefab = Resources.Load("Environment/WaterFallSplash") as GameObject;
                Instantiate(waterfallSplashPrefab, new Vector3(transform.localPosition.x, transform.localPosition.y - 1, -5), Quaternion.identity);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}

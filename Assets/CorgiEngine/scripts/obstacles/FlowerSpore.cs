using UnityEngine;
using System.Collections;

public class FlowerSpore : MonoBehaviour
{

    bool canStick = false;

    // Use this for initialization
    void Start()
    {
        float time = (float)Random.Range(5, 10) / 10.0f;
        StartCoroutine(EnableStick(time));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator EnableStick(float duration)
    {
        yield return new WaitForSeconds(duration);

        canStick = true;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (!canStick)
            return;

        SpriteExploder exploder = gameObject.GetComponent<SpriteExploder>();
        if (exploder != null)
        {
            //Debug.Log("boom");
            exploder.transform.parent = GameManager.Instance.Grid.transform;
            exploder.ExplodeSprite();

            canStick = false;

            Destroy(gameObject, 0.1f);
        }
    }
}

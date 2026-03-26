using UnityEngine;
using System.Collections;
using JulienFoucher;

public class GemsInOut : MonoBehaviour
{
    public bool In = true;

    BoxCollider2D _box;


    // Use this for initialization
    void Start()
    {
        if(!In)
        {
            _box = GetComponent<BoxCollider2D>();

            StartCoroutine(Generate(2.25f));
        }
    }


    // Update is called once per frame
    void Update()
    {


    }


    public virtual IEnumerator Generate(float delay)
    {
        yield return new WaitForSeconds(delay);

        var gemPrefab = Resources.Load("Items/MiniGem") as GameObject;
        var gemObj = Instantiate(gemPrefab, transform.position + Vector3.right + 0.25f*Vector3.up, gameObject.transform.rotation);
        gemObj.transform.parent = gameObject.transform.parent;

        var gem = gemObj.GetComponent<Gem>();
        gem.Init(Random.Range(1, 3));
        gem.Tracks = false;
        gemObj.GetComponent<SpriteTrail>().enabled = false;
        gem.StartCoroutine(gem.Collect(0.05f));

        StartCoroutine(Generate(delay));
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (!In)
            return;

        Gem gem = collider.GetComponent<Gem>();

        if (gem == null)
            return;

        Destroy(gem.gameObject, 1.5f);
    }
}

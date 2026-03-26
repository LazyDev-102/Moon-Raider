using UnityEngine;
using System.Collections;
[RequireComponent(typeof(SpriteRenderer))]

public class RandomSprite : MonoBehaviour
{

    public Sprite[] SpriteCollection;

    protected virtual void Start()
    {
        //sprites = new SpriteCollection("Spritesheet");
        GetComponent<SpriteRenderer>().sprite = SpriteCollection[Random.Range(0, SpriteCollection.Length)];
    }
}

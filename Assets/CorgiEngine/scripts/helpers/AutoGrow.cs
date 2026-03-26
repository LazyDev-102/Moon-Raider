using UnityEngine;
using System.Collections;

public class AutoGrow : MonoBehaviour
{
    public float Rate = 1.1f;
    public float Max = 10f;

    private float _scale = 1;

    private SpriteRenderer _sprite;

    // Use this for initialization
    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (_scale > Max)
        {
            Destroy(gameObject);
            return;
        }

        _scale *= Rate;
        transform.localScale = _scale * Vector3.one;

        float a = (Max - _scale) / Max;

        _sprite.color = new Color(1, 1, 1, a);
    }
}

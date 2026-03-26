using UnityEngine;
using System.Collections;

public class Fuse : Plug
{
    public GameObject Remains;

    override public void Pull(bool instant = false)
    {
        base.Pull(instant);

        if (Remains != null)
        {
            var r = Instantiate(Remains, transform.position, transform.rotation);
            SpriteRenderer rSprite = r.GetComponent<SpriteRenderer>();
            r.transform.parent = transform.parent;
        }

        GetComponentInChildren<SpriteRenderer>().enabled = false;
        GetComponentInChildren<BoxCollider2D>().enabled = false;
    }
}

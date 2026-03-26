using UnityEngine;
using System.Collections;

public class ChildRenderer : MonoBehaviour
{
    CharacterBehavior _parent;

    // Use this for initialization
    void Start()
    {
        _parent = GetComponentInParent<CharacterBehavior>();

        if (_parent == null)
            Debug.Log("No parent behavior!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnBecameVisible()
    {
        //Debug.Log("Visible!");
        if (_parent != null)
            _parent.OnVisible();
    }

    void OnBecameInvisible()
    {
        //Debug.Log("Invisible!");
        if (_parent != null)
        {
            if(_parent.isActiveAndEnabled)
                _parent.OnInvisible();
        }
    }
}

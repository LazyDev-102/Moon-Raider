using UnityEngine;
using System.Collections;

public class Celebrator : MonoBehaviour
{
    Animator _animator;

    public static int Index = 0;
    int _index = 0;

    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();

        _index = Celebrator.Index;
        Celebrator.Index++;

        SetAnimatorLayer(_index);

        StartCoroutine(SetLayerAgain(0.1f));
    }

    public virtual IEnumerator SetLayerAgain(float duration)
    {
        yield return new WaitForSeconds(duration);

        SetAnimatorLayer(_index);
    }

        // Update is called once per frame
    void Update()
    {

    }

    public void SetAnimatorLayer(int show)
    {
        for (int i = 0; i < 9; i++)
        {
            _animator.SetLayerWeight(i, 0);
        }
        _animator.SetLayerWeight(show, 1);
    }
}

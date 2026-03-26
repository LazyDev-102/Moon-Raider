using UnityEngine;
using System.Collections;

public class Switchable : MonoBehaviour
{
    public bool KeepScanning = false;
    public CameraController cam;


    public virtual IEnumerator Open(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Override me
    }

    public virtual IEnumerator Close(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Override me
    }

    public virtual void Flicker()
    {
        // Override me
    }

    public virtual IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Override me}
    }


    public IEnumerator Freeze(float duration, Transform t)
    {
        yield return new WaitForSeconds(duration);
        cam.FreezeAt(t.position);
    }

    public IEnumerator Thaw(float duration)
    {
        yield return new WaitForSeconds(duration);

        cam.SetTarget(GameManager.Instance.Player.transform);
        cam.FollowsPlayer = true;

        //Debug.Log("Actually thawing");
    }
}


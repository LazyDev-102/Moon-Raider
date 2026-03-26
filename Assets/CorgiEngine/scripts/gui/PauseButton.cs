using UnityEngine;
using System.Collections;

public class PauseButton : MonoBehaviour
{
    public virtual void PauseButtonAction()
    {
        GameManager.Instance.Pause();
        //admanager.instance.ShowGenericVideoAd();

    }
}

using UnityEngine;
using System.Collections;

public class GoogleChecker : PersistentSingleton<GoogleChecker>
{
    // Use this for initialization
    void Start()
    {
        CheckForLicense.CheckLicense((bool hasLicense) =>
        {
            GlobalVariables.GameLocked = !hasLicense;
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}

using UnityEngine;
using System.Collections;

public class LevelSelector : MonoBehaviour
{
	public int WorldNumber;
	public int LevelIndex;
	public string NameOverride;

	public virtual void GoToLevelByName(string name)
	{
		LevelManager.Instance.GotoLevel(name);
	}

    public virtual void GoToLevel()
    {
		if (NameOverride != "" && NameOverride != null) {
			Debug.Log ("Loading level: " + NameOverride);
			GoToLevelByName (NameOverride);
			return;
		}

        GlobalVariables.LoadSaved = false;
        GlobalVariables.SavedLevelIndex = -1;
        GlobalVariables.WorldIndex = WorldNumber;
		GlobalVariables.LevelIndex = LevelIndex;

		LevelManager.Instance.GotoLevel("World" + WorldNumber);
    }

    public virtual void RestartLevel()
    {
      
       GameManager.Instance.UnPause();
       Application.LoadLevel(Application.loadedLevel);

    }


    public void BackToMainMenu()
    {
        LevelManager.Instance.BackToMain();
        //admanager.instance.ShowGenericVideoAd();

    }

}

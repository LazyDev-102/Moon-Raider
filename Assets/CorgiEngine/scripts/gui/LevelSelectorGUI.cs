using UnityEngine;
using System.Collections;

public class LevelSelectorGUI : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
       GUIManager.Instance.SetHUDActive(false);
	   Cursor.visible = true;
    }
}

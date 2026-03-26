using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class SlotMenu : MonoBehaviour
{
    public GameObject Warning;
    public GameObject Selector;
    public AudioClip WarnSfx;

    public Image[] Button;
    public Text[] Name;
    public Text[] Description;
    public Text CloseText;

    public int HighlightedSlot = 0;

    //private Sprite[] frames;

    // Use this for initialization
    void Start()
    {
        //frames = Resources.LoadAll<Sprite>("buttons");
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void HighlightSlotByIndex(int slot)
    {
        HighlightedSlot = slot;
        HighlightSlot();
    }


    public void HighlightSlot()
    {
        // default color
        for (int i = 0; i < Button.Length; i++)
        {
            Button[i].color = new Color(Button[i].color.r, Button[i].color.g, Button[i].color.b, 186.0f / 255.0f);
        }

        // highlighted color
        Button[HighlightedSlot].color = new Color(Button[HighlightedSlot].color.r,
                                                  Button[HighlightedSlot].color.g,
                                                  Button[HighlightedSlot].color.b, 1);

        Selector.transform.position = Button[HighlightedSlot].transform.position;
    }


    public void ShowWarning()
    {
        if (WarnSfx != null)
            SoundManager.Instance.PlaySound(WarnSfx, transform.position);

        Warning.SetActive(true);
    }


    public void Refresh()
    {
        CloseText.color = Color.white;
        Warning.SetActive(false);

        for (int i = 0; i < Name.Length; i++)
        {
            Name[i].text = "Slot " + (i + 1);

#if UNITY_TVOS
            String saveData = PlayerPrefs.GetString("SavedGame" + i, "");
                
            if(saveData != "")
            {
                PickerSave savedGame = JsonUtility.FromJson<PickerSave>(saveData);
#else
            var filePath = Application.persistentDataPath + "/savedgame" + i + ".json";
    		if (File.Exists(filePath))
    		{
                PickerSave savedGame = JsonUtility.FromJson<PickerSave>(File.ReadAllText(filePath));
#endif
                if (savedGame == null)
    		    {
    			    Description[i].text = "empty";
    		    }
    		    else
    		    {
				    string levelString = "Zone " + savedGame.SavedWorldIndex + "-" + (savedGame.SavedLevelIndex + 1);

                    if (savedGame.SavedWorldIndex > 10)
					    levelString = "Complete";

				    Description[i].text = levelString;
    		    }
    		}
			else
			{
				Description[i].text = "empty";
			}
		}
	}
}

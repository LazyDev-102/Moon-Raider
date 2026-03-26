using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class ModeMenu : MonoBehaviour
{
    public int HighlightedSlot = 0;

    public GameObject Selector;

    public Image[] Button;

    // Use this for initialization
    void Start()
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
}

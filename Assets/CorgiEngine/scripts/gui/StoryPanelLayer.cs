using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[Serializable]
public class StoryPanelLayer
{
    public Image Image;
    public Vector3 StartScale { get; set; }
    public Vector3 StartPosition { get; set; }
    public Vector3 TargetScale;
    public Vector3 TargetPosition;
}

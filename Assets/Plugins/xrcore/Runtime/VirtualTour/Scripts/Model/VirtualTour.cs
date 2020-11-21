using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VirtualTour
{
    public int startState;
    public string leaveScene;

    public Dictionary<int, VirtualState> states;
}

[Serializable]
public class VirtualState
{
    public string img;
    public string img2;
    public string video;
    public string video_af;
    public List<VirtualMarker> markers;
}

[Serializable]
public class VirtualMarker
{
    public string reference;
    public string texture;
    public string text;
    public Vector3 scale;
    public Vector3 position;
}

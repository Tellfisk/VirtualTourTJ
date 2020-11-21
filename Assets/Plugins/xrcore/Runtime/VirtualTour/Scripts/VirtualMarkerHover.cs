using UnityEngine;
using HTC.UnityPlugin.Pointer3D;

public class VirtualMarkerHover : MonoBehaviour, IPointer3DHoverHandler
{
    private bool isHovered = false;
    public Outline outlineToHover;
    public Color onHoverColor = new Color(0.0f, 1.0f, 0.0f);
    public Color defaultColor = new Color(1.0f, 0.0f, 0.0f);

    public void OnPointer3DHover(Pointer3DEventData eventData)
    {
        isHovered = true;
    }

    public void Update()
    {
        if (isHovered)
        {
            outlineToHover.OutlineColor = onHoverColor;
        }
        else
        {
            outlineToHover.OutlineColor = defaultColor;
        }
        isHovered = false;
    }
}

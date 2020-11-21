using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OfflineAvatar : MonoBehaviour
{
    private static List<XRNodeState> _nodeStates = new List<XRNodeState>();

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    void FixedUpdate()
    {
        UpdateAvatarTransformsForLocalPlayer();
    }

    void Update()
    {
        UpdateAvatarTransformsForLocalPlayer();
    }

    void LateUpdate()
    {
        UpdateAvatarTransformsForLocalPlayer();
    }

    void UpdateAvatarTransformsForLocalPlayer()
    {
        _nodeStates.Clear();
        InputTracking.GetNodeStates(_nodeStates);

        bool headActive = false;
        bool leftHandActive = false;
        bool rightHandActive = false;

        foreach (XRNodeState nodeState in _nodeStates)
        {
            if (nodeState.nodeType == XRNode.Head)
            {
                headActive = nodeState.tracked;

                Vector3 position;
                if (nodeState.TryGetPosition(out position))
                {
                    head.localPosition = position;
                }

                Quaternion rotation;
                if (nodeState.TryGetRotation(out rotation))
                {
                    head.localRotation = rotation;
                }
            }
            else if (nodeState.nodeType == XRNode.LeftHand)
            {
                leftHandActive = nodeState.tracked;

                Vector3 position;
                if (nodeState.TryGetPosition(out position))
                {
                    leftHand.localPosition = position;
                }

                Quaternion rotation;
                if (nodeState.TryGetRotation(out rotation))
                {
                    leftHand.localRotation = rotation;
                }
            }
            else if (nodeState.nodeType == XRNode.RightHand)
            {
                rightHandActive = nodeState.tracked;

                Vector3 position;
                if (nodeState.TryGetPosition(out position))
                {
                    rightHand.localPosition = position;
                }

                Quaternion rotation;
                if (nodeState.TryGetRotation(out rotation))
                {
                    rightHand.localRotation = rotation;
                }
            }
        }
    }
}

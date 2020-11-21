using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using HTC.UnityPlugin.Pointer3D;
using TMPro;

public class VirtualMarkerComponent : MonoBehaviour, IPointer3DPressExitHandler
{
    public string reference;
    public string texture;
    public string text;

    public MeshRenderer sphereMR;
    public TextMeshPro textTMP;

    private VirtualTourManager vtm;

    public void Initialize(VirtualMarker vm, VirtualTourManager newVtm)
    {
        reference = vm.reference;
        transform.localScale = vm.scale;
        transform.position = vm.position;

        vtm = newVtm;

        if (vm.texture != null)
        {
            texture = vm.texture;
            StartCoroutine(LoadImage(texture));
        }

        if (vm.text != null)
        {
            text = vm.text;
            textTMP.text = text;
        }
    }

    IEnumerator LoadImage(string imgPath)
    {
        string url = Path.Combine(Application.streamingAssetsPath, imgPath);

        Texture2D tex = new Texture2D(2, 2);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                tex = DownloadHandlerTexture.GetContent(uwr);
            }
        }

        sphereMR.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        sphereMR.GetComponent<MeshRenderer>().material.mainTexture = tex;
    }

    public void OnPointer3DPressExit(Pointer3DEventData eventData)
    {
        int index = reference.IndexOf(':');
        if (index != -1)
        {
            string refTarget = reference.Substring(0, index);

            switch (refTarget)
            {
                case "state":
                    string refNumberStr = reference.Substring(index + 1, reference.Length - index - 1);
                    int refNumber = Convert.ToInt32(refNumberStr);
                    vtm.PrepareNewState(refNumber);
                    break;

                case "scene":
                    string refSceneStr = reference.Substring(index + 1, reference.Length - index - 1);
                    SceneManager.LoadSceneAsync(refSceneStr);
                    break;
            }
        }
    }

    public void Update()
    {
        transform.LookAt(Camera.main.transform, Vector3.up);
    }
}

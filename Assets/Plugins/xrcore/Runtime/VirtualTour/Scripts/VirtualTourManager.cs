using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class VirtualTourManager : MonoBehaviour
{
    private string tourName;
    public VirtualTour vt;

    public Shader imgStereoShader;
    public GameObject backgroundImageSphere;
    public GameObject markersPrefab;
    public GameObject markers;

    private Texture2D tempMainTexture;
    private Texture2D tempSecTexture;

    public float alpha = 0.0f;
    public float fadeDamp = 0.0f;
    public Color fadeColor;

    public CanvasGroup myCanvas;
    public Image bg;
    float lastTime = 0;
    bool loadingInProgress = false;

    public Color onHoverColor = new Color(0.0f, 1.0f, 0.0f);
    public Color defaultColor = new Color(1.0f, 0.0f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        tourName = TourSelect.selectedTourName;
        //Getting the visual elements 
        bg.color = fadeColor;
        myCanvas.alpha = 0;

        string tourFilePath = Path.Combine(Application.streamingAssetsPath, tourName, "tour.json");

        StartCoroutine(LoadVirtualTour(tourFilePath));
    }

    IEnumerator LoadVirtualTour(string jsonPath)
    {
        string dataAsJson = "";
        vt = null;

        UnityWebRequest www = UnityWebRequest.Get(jsonPath);
        yield return www.SendWebRequest();
        dataAsJson = www.downloadHandler.text;

        try
        {
            vt = JsonConvert.DeserializeObject<VirtualTour>(dataAsJson);
        }
        catch (JsonException je)
        {
            // do nothing, try to continue
            Debug.Log("QUESTDEB: error with deserialization : " + je.Message);
        }

        if (vt != null)
        {
            PrepareNewState(vt.startState);
        }
        else
        {
            // We do not have info about scene, what to do?
            Debug.Log("QUESTDEB: we are missing states");
        }
    }

    public void PrepareNewState(int state)
    {
        if (!loadingInProgress)
        {
            string tourImgsPath = Path.Combine(Application.streamingAssetsPath, tourName);
            VirtualState currState = vt.states[state];

            if (!string.IsNullOrEmpty(Path.Combine(tourImgsPath, currState.img)))
            {
                loadingInProgress = true;
                StartCoroutine(
                    FadeInOutNewState(
                        LoadImageAndMarkers(
                            Path.Combine(tourImgsPath, currState.img),
                            Path.Combine(tourImgsPath, currState.img2), 
                            currState.markers
                             )
                        )
                    );
            }
        }
    }


    public IEnumerator FadeInOutNewState(IEnumerator loadFunc)
    {
        yield return StartCoroutine(FadeOut());
        yield return StartCoroutine(loadFunc);
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        lastTime = Time.time;
        float coDelta = lastTime;
        bool hasFadedOut = false;

        while (!hasFadedOut)
        {
            coDelta = Time.time - lastTime;
            alpha = NewAlpha(coDelta, 1, alpha);

            if (alpha == 1)
            {
                hasFadedOut = true;
            }

            lastTime = Time.time;
            myCanvas.alpha = alpha;
            yield return null;
        }

        yield return null;
    }

    IEnumerator FadeIn()
    {
        lastTime = Time.time;
        float coDelta = lastTime;
        bool hasFadedIn = false;

        while (!hasFadedIn)
        {
            coDelta = Time.time - lastTime;

            alpha = NewAlpha(coDelta, 0, alpha);
            if (alpha == 0)
            {
                hasFadedIn = true;
            }

            lastTime = Time.time;
            myCanvas.alpha = alpha;
            yield return null;
        }

        loadingInProgress = false;
        yield return null;
    }

    IEnumerator LoadImageAndMarkers(string imgPath, string imgPath2, List<VirtualMarker> newMarkers)
    {
        foreach (Transform child in markers.transform)
        {
            Destroy(child.gameObject);
        }

        string url = Path.Combine(Application.streamingAssetsPath, imgPath);

        if (imgPath2 != null)
        {
            backgroundImageSphere.GetComponent<MeshRenderer>().material = new Material(imgStereoShader);
        }
        else
        {
            backgroundImageSphere.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Texture"));
        }

        Destroy(tempMainTexture);
        tempMainTexture = null;
        Destroy(tempSecTexture);
        tempSecTexture = null;
        Resources.UnloadUnusedAssets();

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                tempMainTexture = new Texture2D(2, 2);
                tempMainTexture = DownloadHandlerTexture.GetContent(uwr);
            }
        }

        if (imgPath2 != null)
        {
            string urlSec = Path.Combine(Application.streamingAssetsPath, imgPath2);

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(urlSec))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    tempSecTexture = new Texture2D(2, 2);
                    tempSecTexture = DownloadHandlerTexture.GetContent(uwr);

                    backgroundImageSphere.GetComponent<MeshRenderer>().material.mainTexture = tempMainTexture;
                    backgroundImageSphere.GetComponent<MeshRenderer>().material.SetTexture(Shader.PropertyToID("_SecTex"), tempSecTexture);
                }
            }
        } else
        {
            backgroundImageSphere.GetComponent<MeshRenderer>().material.mainTexture = tempMainTexture;
        }

        if (newMarkers != null)
        {
            foreach (VirtualMarker vm in newMarkers)
            {
                GameObject newMarker = GameObject.Instantiate(markersPrefab, markers.transform);
                newMarker.GetComponent<VirtualMarkerComponent>().Initialize(vm, this);
                newMarker.GetComponent<VirtualMarkerHover>().defaultColor = defaultColor;
                newMarker.GetComponent<VirtualMarkerHover>().onHoverColor = onHoverColor;
            }
        }
    }

    float NewAlpha(float delta, int to, float currAlpha)
    {
        switch (to)
        {
            case 0:
                currAlpha -= fadeDamp * delta;
                if (currAlpha <= 0)
                    currAlpha = 0;

                break;
            case 1:
                currAlpha += fadeDamp * delta;
                if (currAlpha >= 1)
                    currAlpha = 1;

                break;
        }

        return currAlpha;
    }
}

public class TourSelect
{
    public static string selectedTourName;

}
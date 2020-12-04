using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class DoorClass : MonoBehaviour
{
    public string tourName;  //The name of the folder with the tour this door links to
    public string tourPath;
    private VirtualTour vt;


    private void Start()
    {
        StartCoroutine(LoadDoorImage());
    }

    public IEnumerator LoadDoorImage()
    {
        string jsonPath = Path.Combine(tourPath, "tour.json");
        UnityWebRequest www = UnityWebRequest.Get(jsonPath);
        yield return www.SendWebRequest();
        string dataAsJson = www.downloadHandler.text;

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
            string imageName = vt.states[vt.startState].img;
            string imagePath = Path.Combine(tourPath, imageName);

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    Texture2D imageTexture = new Texture2D(2, 2);
                    imageTexture = DownloadHandlerTexture.GetContent(uwr);

                    GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Texture"));
                    GetComponent<MeshRenderer>().material.mainTexture = imageTexture;
                }
            }

        }
        else
        {
            Debug.Log("Error: Did not load VirtualTour object -- " + jsonPath);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using System.IO;

public class ButtonController : MonoBehaviour
{ 
    public GameObject buttonPrefab;
    private float staticPos = -30;
    public string tourFolder;

    void Start()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
        DirectoryInfo[] paths = dir.GetDirectories();

        foreach (DirectoryInfo path in paths)
        {
            string tourPath = path.ToString();
            string[] words = tourPath.Split('/');
            string tourFolder = words[words.Length - 1];  //Get the folder name from the path
            GameObject go = GameObject.Instantiate(buttonPrefab);
            go.transform.position = new Vector3(staticPos, 0, 20);
            go.GetComponent<ButtonClass>().folder = tourFolder;
            staticPos += 20;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform != null)
                {
                    GameObject button = hit.collider.gameObject;
                    string folder = button.GetComponent<ButtonClass>().folder;
                    button.GetComponent<SceneChange>().ChangeSceneAndTour("VirtualTour", folder);
                }
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO;
using TMPro;

public class DoorController : MonoBehaviour
{ 
    public GameObject doorPrefab;
    private float staticPos = -5;   // TODO Has to be dynamic, changing based on the number of available tours

    void Start()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
        DirectoryInfo[] paths = dir.GetDirectories();

        foreach (DirectoryInfo path in paths)
        {
            string tourPath = path.ToString();
            
            string[] words = tourPath.Split('\\');
            string tourFolder = words[words.Length - 1];  //Get the folder name from the path
            Debug.Log(path);
            Debug.Log(tourFolder);
            GameObject door = GameObject.Instantiate(doorPrefab, new Vector3(staticPos, 0, 0), Quaternion.Euler(-90,0,0));
            door.GetComponentInChildren<TextMeshPro>().text = tourFolder;
            staticPos += 3;
            door.GetComponent<DoorClass>().folder = tourFolder;
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
                    string folder = button.GetComponent<DoorClass>().folder;
                    SceneChange.ChangeSceneAndTour("VirtualTour", folder);
                }
            }
        }
    }

}

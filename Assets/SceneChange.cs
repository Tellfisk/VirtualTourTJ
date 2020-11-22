﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeSceneAndTour(string sceneName, string folder)
    {
        TourSelectxxx.selectedTour = folder;
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + "   " + folder);
    }

}
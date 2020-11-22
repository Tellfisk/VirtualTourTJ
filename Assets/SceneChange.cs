using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    public void ChangeSceneToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public static void ChangeSceneAndTour(string sceneName, string folder)
    {
        TourSelect.selectedTour = folder;
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + "   " + folder);
    }

}

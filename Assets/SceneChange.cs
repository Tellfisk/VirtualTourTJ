using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    public static void ChangeSceneToLobby()
    {
        //VirtualTourManager vt = new VirtualTourManager();
        //StartCoroutine(vt.FadeInOutNewState(LoadEnumTest()));
        Debug.Log("test");
        SceneManager.LoadScene("Lobby");
    }

    public static void ChangeSceneAndTour(string sceneName, string tourName)
    {
        TourSelect.selectedTour = tourName;
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + "   " + tourName);
    }

    public IEnumerator LoadEnumTest()
    {
        SceneManager.LoadScene("Lobby");
        yield return null;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    public void ChangeSceneToLobby()
    {
        //VirtualTourManager vt = new VirtualTourManager();
        //StartCoroutine(vt.FadeInOutNewState(LoadEnumTest()));
        SceneManager.LoadScene("Lobby");
    }

    public static void ChangeSceneAndTour(string sceneName, string folder)
    {
        TourSelect.selectedTour = folder;
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + "   " + folder);
    }

    public IEnumerator LoadEnumTest()
    {
        SceneManager.LoadScene("Lobby");
        yield return null;
    }

}

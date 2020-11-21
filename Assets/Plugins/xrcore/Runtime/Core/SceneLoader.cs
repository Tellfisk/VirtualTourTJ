using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Rainfall.Xrcore
{
    public class SceneLoader : MonoBehaviour
    {
        public GameObject loadingScreen;
        public CanvasGroup canvasGroup;
        public Slider loadingSlider;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(StartLoad(sceneName));
        }

        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(StartLoadAsync(sceneName));
        }

        IEnumerator StartLoad(string sceneName)
        {
            loadingScreen.SetActive(true);
            yield return StartCoroutine(FadeLoadingScreen(1, 1));

            SceneManager.LoadScene(sceneName);

            yield return StartCoroutine(FadeLoadingScreen(0, 1));
            loadingScreen.SetActive(false);
        }

        IEnumerator StartLoadAsync(string sceneName)
        {
            loadingScreen.SetActive(true);
            yield return StartCoroutine(FadeLoadingScreen(1, 1));

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
                loadingSlider.value = progressValue;
                yield return null;
            }

            yield return StartCoroutine(FadeLoadingScreen(0, 1));
            loadingScreen.SetActive(false);
        }

        IEnumerator FadeLoadingScreen(float targetValue, float duration)
        {
            float startValue = canvasGroup.alpha;
            float time = 0;

            while (time < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = targetValue;
        }
    }
}


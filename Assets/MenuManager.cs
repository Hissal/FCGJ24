using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject canvas;

    public void PlayVideo(int SceneIndexToLoadAfterVideo)
    {
        videoPlayer.Play();
        StartCoroutine(SwitchScene(SceneIndexToLoadAfterVideo));
    }

    private IEnumerator SwitchScene(int sceneIndex)
    {
        print("Playeing Video " + videoPlayer.clip.length);
        yield return new WaitForSeconds(0.1f);
        canvas.SetActive(false);
        yield return new WaitForSeconds((float)videoPlayer.clip.length);
        print("Loading Scene");
        LoadScene(sceneIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

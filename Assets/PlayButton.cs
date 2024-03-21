using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayButton : MonoBehaviour
{

    [SerializeField] VideoPlayer videoPlayer;

    public void PlayVideo(int SceneIndexToLoadAfterVideo)
    {
        videoPlayer.Play();
        StartCoroutine(SwitchScene(SceneIndexToLoadAfterVideo));
        transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator SwitchScene(int sceneIndex)
    {
        yield return new WaitForSeconds((float)videoPlayer.clip.length);
        LoadScene(sceneIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

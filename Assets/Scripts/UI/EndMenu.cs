using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadPreviousSceneAfterDelay(3f));
    }

    IEnumerator LoadPreviousSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // go back to menu
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.LogWarning("No previous scene available to load.");
        }
    }
}

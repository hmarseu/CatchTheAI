using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    [SerializeField] private GameObject pauseMenuUI;
    private SFXManager _sfxManager;

    private void Awake()
    {
        _sfxManager = GameObject.Find("VFX_List").GetComponent<SFXManager>();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void Resume()
    {
        _sfxManager?.PlaySoundEffect(3);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        _sfxManager.PlaySoundEffect(3);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void QuitGame()
    {
        _sfxManager?.PlaySoundEffect(3);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

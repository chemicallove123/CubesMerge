using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Popup Panels")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        Debug.Log("[MainMenu] Main Menu started.");

        // Make sure Controls popup starts closed
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
            Debug.Log("[MainMenu] Controls Panel initialized and hidden.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Controls Panel has NOT been assigned in the Inspector!");
        }

        // Make sure Credits popup starts closed
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
            Debug.Log("[MainMenu] Credits Panel initialized and hidden.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Credits Panel has NOT been assigned in the Inspector!");
        }
    }

    //PLAY
    public void PlayGame()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        Debug.Log("[MainMenu] Play button pressed.");
        Debug.Log("[MainMenu] Loading scene with build index: " + nextSceneIndex);

        SceneManager.LoadScene(nextSceneIndex);
    }

    //CONTROLS
    public void OpenControls()
    {
        Debug.Log("[MainMenu] Controls button pressed.");

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            Debug.Log("[MainMenu] Controls Panel opened.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cannot open Controls Panel because it has not been assigned!");
        }

        // Close Credits if it is currently open
        if (creditsPanel != null && creditsPanel.activeSelf)
        {
            creditsPanel.SetActive(false);
            Debug.Log("[MainMenu] Credits Panel closed because Controls Panel was opened.");
        }
    }

    public void CloseControls()
    {
        Debug.Log("[MainMenu] Controls close button (X) pressed.");

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
            Debug.Log("[MainMenu] Controls Panel closed.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cannot close Controls Panel because it has not been assigned!");
        }
    }

    //CREDITS

    public void OpenCredits()
    {
        Debug.Log("[MainMenu] Credits button pressed.");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            Debug.Log("[MainMenu] Credits Panel opened.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cannot open Credits Panel because it has not been assigned!");
        }

        // Close Controls if it is currently open
        if (controlsPanel != null && controlsPanel.activeSelf)
        {
            controlsPanel.SetActive(false);
            Debug.Log("[MainMenu] Controls Panel closed because Credits Panel was opened.");
        }
    }

    public void CloseCredits()
    {
        Debug.Log("[MainMenu] Credits close button (X) pressed.");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
            Debug.Log("[MainMenu] Credits Panel closed.");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cannot close Credits Panel because it has not been assigned!");
        }
    }

    //QUIT

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quit button pressed.");
        Debug.Log("[MainMenu] Quitting game...");

        Application.Quit();

        // Application.Quit() does nothing inside the Unity Editor,
        // so this confirms that the button itself is working.
#if UNITY_EDITOR
        Debug.Log("[MainMenu] Application.Quit() ignored because the game is running inside the Unity Editor.");
#endif
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void ReloadScene() {
        if(SettingsMenu.menuOpen) { return; }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

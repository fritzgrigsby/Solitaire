using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public static bool menuOpen;

    void Start() {
        gameObject.SetActive(false);
        menuOpen = false;
    }

    public void OpenMenu() {
        gameObject.SetActive(true);
        menuOpen = true;
    }

    public void CloseMenu() {
        gameObject.SetActive(false);
        menuOpen = false;
    }

    public void SetCardBacks(Sprite card_back) {
        var list = FindObjectsOfType<UpdateSprite>();
        foreach(var s in list) {
            s.SetCardback(card_back);
        }
    }
}

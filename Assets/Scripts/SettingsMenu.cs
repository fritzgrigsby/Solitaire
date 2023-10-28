using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    // Globally accessable bool 
    public static bool menuOpen;

    [Header("Card Back Buttons")]
    [SerializeField] List<CardButton_SO> cardButton_SOs;
    [SerializeField] GameObject cardButtonPrefab;
    [SerializeField] GameObject buttonLayout;

    void Start() {
        InitCardBackButtons();
        gameObject.SetActive(false);
        menuOpen = false;
    }

    void InitCardBackButtons() {
        foreach(var so in cardButton_SOs) {
            // Instantiate new button and assign card sprite 
            GameObject new_button = Instantiate(cardButtonPrefab, Vector3.zero, Quaternion.identity, buttonLayout.transform);
            new_button.GetComponent<Image>().sprite = so.cardSprite;

            // Set selected sprite in sprite state, then assign to button 
            var ss = new SpriteState();
            ss.selectedSprite = so.hilightedSprite;
            new_button.GetComponent<Button>().spriteState = ss;

            // Set OnClick Event
            new_button.GetComponent<Button>().onClick.AddListener(() => SetCardBacks(so.cardSprite));
        }
    }

    public void OpenMenu() {
        gameObject.SetActive(true);
        menuOpen = true;
    }

    public void CloseMenu() {
        gameObject.SetActive(false);
        menuOpen = false;
    }

    void SetCardBacks(Sprite sprite) {
        var list = FindObjectsOfType<UpdateSprite>();
        foreach(var s in list) {
            s.SetCardback(sprite);
        }
    }
}

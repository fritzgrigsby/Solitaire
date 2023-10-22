using UnityEngine;

public class UserInput : MonoBehaviour
{
    Solitaire solitaire;

    void Start() {
        solitaire = FindObjectOfType<Solitaire>();        
    }

    void Update() {
        GetMouseClick();
    }

    void GetMouseClick() {
        if (Input.GetMouseButtonDown(0)) {

            Vector3 mousePostion = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);

            if(hit) {
                if (hit.collider.CompareTag("Deck")) {
                    DeckClick();
                }
                if (hit.collider.CompareTag("Card")) {
                    CardClick(hit.collider.gameObject);
                }
            }
        }
    }

    void DeckClick() {
        solitaire.CallShowCards();
    }

    void CardClick(GameObject selected) {
        solitaire.CardClick(selected);
    }
}

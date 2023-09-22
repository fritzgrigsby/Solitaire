using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    [SerializeField]
    Sprite cardBack;
    Sprite cardFront;
    Selectable selectable;
    SpriteRenderer spriteRenderer;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectable = GetComponent<Selectable>();
    }

    void Start() {
        cardFront = FindObjectOfType<Solitaire>().GetCardFace(gameObject.name);
    }

    void Update() {
        spriteRenderer.sprite = selectable.faceUp ? cardFront : cardBack;
    }
}

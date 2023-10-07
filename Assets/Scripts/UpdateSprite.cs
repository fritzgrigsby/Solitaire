using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float moveHeight = 2;
    [SerializeField] Sprite cardBack;

    Sprite cardFront;
    Selectable selectable;
    SpriteRenderer spriteRenderer;

    bool moving = false;

    Vector3 targetPosition = new Vector3();

    public void SetMovePosition(Vector3 target_position) {
        targetPosition = target_position;
    }

    public void SetPostion(Vector3 position) {
        targetPosition = position;
        transform.position = position;
    }

    public void SetCardFront(Sprite front) {
        cardFront = front;
    }

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectable = GetComponent<Selectable>();
    }

    void Start() {
        targetPosition = gameObject.transform.position;
    }

    void Update() {
        // Set sprite to render
        spriteRenderer.sprite = selectable.faceUp ? cardFront : cardBack;

        // Move to target
        MoveToTarget();
    }

    void MoveToTarget() {
        // On edge change, trigger movement and set card to movement height 
        if(transform.position != targetPosition && moving == false) {
            transform.position = new Vector3(transform.position.x, transform.position.y, -moveHeight);
            moving = true;
        }

        // Move when movement is triggered
        if(moving) {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            moving = !(transform.position == targetPosition); 
        }
    }
}

using System.Collections;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float moveHeight = 2;
    [SerializeField] float shakeAmount = 15f;
    [SerializeField] float shakeLength = .25f;
    [SerializeField] Sprite cardBack;

    Sprite cardFront;
    Selectable selectable;
    SpriteRenderer spriteRenderer;

    bool moving = false;
    bool shaking = false;

    Vector3 targetPosition = new Vector3();


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

        // Shake
        Shake();

        // Move to target
        MoveToTarget();
    }

    public void SetCardFront(Sprite front) {
        cardFront = front;
    }

    public void SetCardback(Sprite back) {
        cardBack = back;
    }

    public void SetMovePosition(Vector3 target_position) {
        targetPosition = target_position;
    }

    public void SetPostion(Vector3 position) {
        targetPosition = position;
        transform.position = position;
    }

    public void SetShake() {
        shaking = true;
        StartCoroutine(TimeShake());
    }

    void MoveToTarget() {
        // On edge change, trigger movement and set card to movement height 
        if(transform.position != targetPosition && moving == false && shaking == false) {
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

    void Shake() {
        if(shaking) {
            Vector3 new_pos = Random.insideUnitSphere * Time.deltaTime * shakeAmount;
            new_pos.y = 0;
            new_pos.z = 0;
            transform.position = targetPosition + new_pos;
        }
    }

    IEnumerator TimeShake() {
        Vector3 og_pos = transform.position;

        yield return new WaitForSeconds(shakeLength);

        shaking = false;
        transform.position = og_pos;
    }

}

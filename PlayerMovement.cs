using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using System;

public class PlayerMovement : MonoBehaviour
{
    public float modifier = 22; //sling power
    public float maxSpeed = 600; //max rb velocity magnitute is 60
    public float friction = 0.99f; //friction
    public float minDistToMove = 3f;  //min distance between inital mouse pos and final mouse pos that would make the player move

    public Vector3 DesiredDir { get; private set; } //Desired Movement Direction.
    public float RealDist { get; private set; }

    private Rigidbody2D rb;
    private Vector3 initialMousePos = new Vector3();
    private Vector3 initalPlayerPos = new Vector3();
    public float MaxRbVelocity { get => maxSpeed / 10; private set { }}
    public float CurrentRbVelocity { get => rb.velocity.magnitude; private set { } }

    public static Action PullOrRelease = delegate { };

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        initalPlayerPos = rb.transform.position;

        LevelStateManager.OnLevelReset += ResetPosition;
    }

    private void OnDestroy()
    {
        LevelStateManager.OnLevelReset -= ResetPosition;
    }

    private void ResetPosition() => transform.position = initalPlayerPos;

    void Update() => ProcessInputs();
    private void FixedUpdate() =>  ApplyFriction();

    void ProcessInputs() 
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            initialMousePos = new Vector3(temp.x, temp.y, 0);
            initalPlayerPos = transform.position;
            PullOrRelease();
        }

        Vector3 temp2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos = new Vector3(temp2.x, temp2.y, 0);

        DesiredDir = initialMousePos - mousePos;


        float mouseDist = Vector3.Distance(initialMousePos, mousePos);
        float playerDist = Vector3.Distance(initalPlayerPos, transform.position);

        RealDist = mouseDist - playerDist;


        if (Input.GetKeyUp(KeyCode.Space))
        {
            Vector2 force = new Vector2(DesiredDir.x, DesiredDir.y);

            float angle = Vector2.SignedAngle(Vector2.up, -force);
            transform.eulerAngles = new Vector3(0, 0, angle);

            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.AddForce(Vector3.ClampMagnitude(force * modifier, maxSpeed), ForceMode2D.Impulse);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            //This part below makes the player stop if they tapped spacebar and havent moved their mouse.           
            if (Mathf.Abs(RealDist) < minDistToMove) rb.velocity = Vector2.zero;
            PullOrRelease();
        }
    }

    void ApplyFriction() 
    {
        rb.velocity *= friction; 
    }
}

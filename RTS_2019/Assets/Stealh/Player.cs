using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event System.Action OnReachedEndOfLevel;
    private float inputHorizontal;
    private float inputVertical;
    private Rigidbody rb;
    public int speed = 8;


    bool disabled;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Guard.OnGuardHasSpottedPlayer += Disable;
    }

    // Update is called once per frame
    void Update()
    {
       // Debug.Log(inputHorizontal);
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        
    }
    void Disable()
    {
        disabled = true;
    }
    void FixedUpdate()
    {
        Vector3 targetPosition = Vector3.zero;
        if (!disabled)
        {
            targetPosition = new Vector3(inputHorizontal, 0, inputVertical);

            rb.velocity = (targetPosition * speed);
            if (inputHorizontal != 0 || inputVertical != 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetPosition);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 320 * Time.deltaTime);
            }
        }
    }

    void OnDestroy()
    {
        Guard.OnGuardHasSpottedPlayer -= Disable;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Finish")
        {
            Disable();
            if(OnReachedEndOfLevel != null)
            {
                OnReachedEndOfLevel();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;
    public ParticleSystem dirtParticle;

    private bool isRunning = false;

    private const float LANE_DISTANCE = 2.0f;
    private const float TURN_SPEED = 0.05f;

    private float jumpForce = 4.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;

    private int desiredLane = 1; // 0 = Moveleft, 1 = middle, 2 = right

    // Speed Modifier
    private float originalSpeed = 7.0f;
    public float speed = 7.0f;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning)
            return;

        if (Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            // Change the modifier text
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }

        // Gather the input on which lane we should be
        if(MobileInput.Instance.SwipeLeft)
            MoveLane(false);
        if (MobileInput.Instance.SwipeRight)
            MoveLane(true);

        // Calculate where we should be
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (desiredLane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;

        // Let's calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        bool isGrounded = IsGrounded();

        // Calculate Y
        if (isGrounded) // Grounded
        {
            verticalVelocity = -0.1f;
            anim.SetBool("Grounded", isGrounded);

            if (MobileInput.Instance.SwipeUp)
            {
                // Jump
                anim.SetTrigger("Jump_trig");
                verticalVelocity = jumpForce;
            }
            else if (MobileInput.Instance.SwipeDown)
            {
                // Slide
                StartSliding();
                Invoke("StopSliding", 1.0f);
            }
        }
        else
        {
            verticalVelocity -= (gravity * Time.deltaTime);

            // Fast fAll mechanic
            if (MobileInput.Instance.SwipeDown)
            {
                verticalVelocity = -jumpForce;
            }
        }

        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        // Move the player
        controller.Move(moveVector * Time.deltaTime);

        // rotate the player to direction
        Vector3 direction = controller.velocity;
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, direction, TURN_SPEED);
        }
    }

    private void StartSliding()
    {
        anim.SetBool("Sliding", true);
        dirtParticle.Play();
        controller.height /= 2;
        controller.center = new Vector3(controller.center.x, controller.center.y / 2, controller.center.z);
    }

    private void StopSliding()
    {
        anim.SetBool("Sliding", false);
        dirtParticle.Stop();
        controller.height *= 2;
        controller.center = new Vector3(controller.center.x, controller.center.y * 2, controller.center.z);
    }

    private void MoveLane(bool goingRight)
    {
        // left
        if (!goingRight)
        {
            desiredLane--;
            if (desiredLane == -1)
                desiredLane = 0;
        }
        else
        {
            desiredLane++;
            if(desiredLane == 3)
                desiredLane = 2;
        }
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f, controller.bounds.center.z), Vector3.down);

        return Physics.Raycast(groundRay, 0.2f + 0.1f);
    }

    public void StartRunning()
    {
        isRunning = true;
        anim.SetTrigger("StartRunning");
    }

    private void Crash()
    {
        anim.SetTrigger("Death_b");
        isRunning = false;
        GameManager.Instance.isDead = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
                break;

        }
    }
}

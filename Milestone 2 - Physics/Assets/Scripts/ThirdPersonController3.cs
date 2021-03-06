using UnityEngine;
using System.Collections;

public class ThirdPersonController3 : MonoBehaviour
{
	public Rigidbody target;
		// The object we're steering

	public float walkSpeed = 15.0f;
	public float backWalkSpeed = 7.0f;
	public float runSpeed = 30.0f;

	public float jumpSpeed = 20.0f;

	public float drag = 1.0f;

	//public float speed = 1.0f, speedChangeScale = 1.5f, turnSpeed = 2.0f, mouseTurnSpeed = 2.0f, jumpSpeed = 2.0f, drag = 1.0f;
		// Tweak to ajust character responsiveness


	public int status = 0;
		// 0: walk; 1: back walk; 2: run

	private const float inputThreshold = 0.01f,
		directionalJumpFactor = 0.0f,
		groundOffset = 0.1f;

    public bool canJump = true;

	public float inAirControl = 0.2f;

	public bool canRun = true;

    private float jumpRepeatTime = 0.05f;
    private float jumpTimeout = 0.15f;
    private float lastJumpButtonTime = -10.0f;
    private float lastJumpTime = -1.0f;
	

    private float groundedTimeout = 0.25f;
	private bool grounded;

	private float distToGround;
	
	public bool Grounded
	// Make our grounded status available for other components
	{
		get
		{
			return grounded;
		}
	}

	void Start ()
	// Verify setup, configure rigidbody
	{
		if (target == null)
		{
			target = GetComponent<Rigidbody> ();
		}

		
		if (target == null)
		{
			Debug.LogError ("No target assigned. Please correct and restart.");
			enabled = false;
			return;
		}
		
		target.drag = drag;

		distToGround = collider.bounds.extents.y;

		//target.freezeRotation = true;
			// We will be controlling the rotation of the target, so we tell the physics system to leave it be
		status = 0;

		GameObject.Find ("Game Controller").GetComponent<GameController>().UpdateWeightText(Mathf.Round(target.mass));
	}
	
	
	void Update ()
	// Handle rotation here to ensure smooth application.
	{

	}
	

	void FixedUpdate ()
	// Handle movement here since physics will only be calculated in fixed frames anyway
	{
        if (Input.GetButton("Jump"))
        // Handle jumping
        {
            lastJumpButtonTime = Time.time;
        }

        ApplyJumping();


		Vector3 moveForward = Camera.main.transform.TransformDirection(Vector3.forward);
		moveForward.y = 0;
		moveForward = moveForward.normalized;
		Vector3 moveRight = Camera.main.transform.TransformDirection(Vector3.right);
		moveRight.y = 0;
		moveRight = moveRight.normalized;
		
		Vector3 movement = Input.GetAxis ("Vertical") * moveForward +
			Input.GetAxis ("Horizontal") * moveRight;
		
		//Debug.Log(SidestepAxisInput.ToString());
		
		if (movement.magnitude > inputThreshold)
			// Only apply movement if we have sufficient input
		{
			
			
			float appliedSpeed = walkSpeed;
			// Scale down applied speed if in walk mode
			status = 0;
			
			
			
			if (Input.GetAxis ("Vertical") < 0.0f)
				// Scale down applied speed if walking backwards
			{
				appliedSpeed = backWalkSpeed;
				status = 1;
			}
			else if (canRun && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
			{
				appliedSpeed = runSpeed;
				status = 2;
			}
			
			if (isGrounded())
				target.AddForce (movement.normalized * appliedSpeed, ForceMode.Force);
			else
				target.AddForce (movement.normalized * appliedSpeed * inAirControl , ForceMode.Force);
			
		}

	}

	bool isGrounded () 
	{
		return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
	}


	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Pickup")
		{
			Destroy(collision.gameObject);
			target.mass += 1.0f;

			GameObject.Find ("Game Controller").GetComponent<GameController>().UpdateWeightText(Mathf.Round(target.mass));

		}
		
	}


    void ApplyJumping()
    {
        // Prevent jumping too fast after each other
        if (lastJumpTime + jumpRepeatTime > Time.time)
            return;

        if (isGrounded())
        {
            // Jump
            // - Only when pressing the button down
            // - With a timeout so you can press the button slightly before landing		
            if (canJump && Time.time < lastJumpButtonTime + jumpTimeout)
            {
				Vector3 curV = target.velocity;
				curV.y = 0;
				target.velocity = curV;
                target.AddForce(
                    jumpSpeed * new Vector3(0, 1, 0) +
                        target.velocity.normalized * directionalJumpFactor,
                    ForceMode.Impulse
                );
                lastJumpTime = Time.time;
                lastJumpButtonTime = -10.0f;
            }

        }

    }

}

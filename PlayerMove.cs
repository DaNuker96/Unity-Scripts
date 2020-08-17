using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private string horizontalInputName;
    [SerializeField] private string verticalInputName;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float defaultmovementSpeed;
    [SerializeField] public float sprintSpeed;

    private CharacterController charController;

    [SerializeField] private AnimationCurve jumpFallOff;
    [SerializeField] private float jumpMultiplier;
    [SerializeField] private KeyCode jumpKey;

    private bool isJumping;
    public bool isCrouching;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        float horizInput = Input.GetAxis(horizontalInputName);
        float vertInput = Input.GetAxis(verticalInputName);

        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;

        charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed);

        sprint();
        JumpInput();
        crouch();

    }

    private void sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching) //sprint
        {
            movementSpeed = sprintSpeed;
        }
        else
        {
            movementSpeed = defaultmovementSpeed;
        }
    }

    private void crouch() //changes the "player height" on character controller so change accordingly if the values are different
    {
        if (Input.GetKey(KeyCode.C))
        {
            charController.height = 2f;

            isCrouching = true;

            movementSpeed = 2;
        }
        else
        {
            charController.height = 3f;

            isCrouching = false;

        }

    }


    private void JumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        charController.slopeLimit = 90.0f;

        float timeInAir = 0.0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;

            yield return null;
        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);
        charController.slopeLimit = 45.0f;
        isJumping = false;
    }
}

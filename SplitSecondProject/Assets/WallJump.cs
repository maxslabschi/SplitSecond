using UnityEngine;

public class WallJump : MonoBehaviour
{
    public float wallJumpForce = 5f;
    public LayerMask wallMask;
    public Transform wallCheck;
    public float wallDistance = 0.5f;

    private bool isTouchingWall;
    private Vector3 velocity;

    private void Update()
    {
        WallJumpMechanic();
    }

    private void WallJumpMechanic()
    {
        isTouchingWall = Physics.CheckSphere(wallCheck.position, wallDistance, wallMask);

        if (isTouchingWall && Input.GetButtonDown("Jump"))
        {
            Vector3 wallJumpDirection = transform.forward + transform.up;
            GetComponent<CharacterController>().Move(wallJumpDirection * wallJumpForce * Time.deltaTime);
        }
    }
}

/*
Hook To:
Attach this script to the Player GameObject.
Create an empty child named WallCheck and position it at the edge of the player where walls might be.
Assign WallCheck to the wallCheck field.
Set the wallMask to the appropriate layer for walls (e.g., "Wall").
*/
using UnityEngine;

public class WallJump : MonoBehaviour
{
    public float wallJumpForce = 5f;
    public LayerMask wallMask;
    public Transform wallCheck;
    public float wallDistance = 2f;

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
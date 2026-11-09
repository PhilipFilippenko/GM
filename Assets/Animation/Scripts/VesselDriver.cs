using UnityEngine;
using UnityEngine.InputSystem;

public class VesselDriver : MonoBehaviour
{
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.5f;
    public float sprintSpeed = 7f;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
        bool attacking = current.IsName("Attack") || next.IsName("Attack");

        float inputX = 0f;
        float inputZ = 0f;
        if (!attacking)
        {
            if (keyboard.wKey.isPressed) inputZ += 1f;
            if (keyboard.sKey.isPressed) inputZ -= 1f;
            if (keyboard.dKey.isPressed) inputX += 1f;
            if (keyboard.aKey.isPressed) inputX -= 1f;
        }

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = cameraForward * inputZ + cameraRight * inputX;
        if (move.magnitude > 1f) move.Normalize();
        bool moving = move != Vector3.zero;

        bool run = keyboard.leftShiftKey.isPressed;
        bool sprint = run && keyboard.leftCtrlKey.isPressed;

        float animSpeed = 0f;
        float moveSpeed = 0f;
        if (moving)
        {
            if (sprint)
            {
                animSpeed = 2f;
                moveSpeed = sprintSpeed;
            }
            else if (run)
            {
                animSpeed = 1f;
                moveSpeed = runSpeed;
            }
            else
            {
                animSpeed = 0.15f;
                moveSpeed = walkSpeed;
            }
        }
        animator.SetFloat("Speed", animSpeed, 0.15f, Time.deltaTime);

        if (moving)
        {
            Quaternion lookDirection = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, 720f * Time.deltaTime);
            transform.position += move * moveSpeed * Time.deltaTime;
        }

        Vector3 rayStart = transform.position + Vector3.up * 0.6f;
        RaycastHit ground;
        if (Physics.Raycast(rayStart, Vector3.down, out ground, 2f))
        {
            Vector3 position = transform.position;
            position.y = Mathf.MoveTowards(position.y, ground.point.y, 4f * Time.deltaTime);
            transform.position = position;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !attacking)
            animator.SetTrigger("Attack");

        if (keyboard.fKey.wasPressedThisFrame)
            animator.SetTrigger("Cast");
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

// Drives the boss Animator from player input.
// WASD moves (local space), Shift jogs, Shift+Ctrl sprints, Q/E turn,
// LMB plays the next attack of a three-hit combo, RMB holds block.
// An attack, once started, plays to the end: input is ignored until it is over.
[RequireComponent(typeof(Animator))]
public class BossAnimationDriver : MonoBehaviour
{
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveZHash = Animator.StringToHash("MoveZ");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
    static readonly int BlockHash = Animator.StringToHash("Block");

    [Header("Movement (metres per second at the model's scale)")]
    public float walkSpeed = 4f;
    public float jogSpeed = 9f;
    public float sprintSpeed = 14f;
    public float turnSpeed = 120f;

    [Header("Animator")]
    [Tooltip("Smoothing time for blend tree parameters, so blends ease in rather than snap.")]
    public float paramDampTime = 0.15f;

    Animator animator;
    int comboStep;
    float comboResetTime;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null) return;

        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Attack states are tagged "Attack" in the controller; also cover the blend into one.
        bool attacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")
            || animator.GetNextAnimatorStateInfo(0).IsTag("Attack");
        if (attacking)
        {
            input = Vector2.zero;
            animator.ResetTrigger(AttackHash);
            // The combo window starts when the swing ends, not when it was clicked.
            comboResetTime = Time.time + 1.5f;
        }

        bool blocking = !attacking && mouse != null && mouse.rightButton.isPressed;
        animator.SetBool(BlockHash, blocking);

        // Jog and sprint only apply when moving forward; the blend tree has them on +Z only.
        float gait = 1f;
        if (input.y > 0.5f && keyboard.leftShiftKey.isPressed)
            gait = keyboard.leftCtrlKey.isPressed ? 3f : 2f;
        if (blocking) input = Vector2.zero;

        // Blend tree positions: walk = 1, jog = 2, sprint = 3 along Z.
        Vector2 blend = input * gait;
        animator.SetFloat(MoveXHash, blend.x, paramDampTime, Time.deltaTime);
        animator.SetFloat(MoveZHash, blend.y, paramDampTime, Time.deltaTime);

        // Move by the damped values so the body speed follows what the legs are showing.
        Vector2 shown = new Vector2(animator.GetFloat(MoveXHash), animator.GetFloat(MoveZHash));
        float speed = SpeedForGait(shown.magnitude);
        Vector3 dir = transform.right * shown.x + transform.forward * shown.y;
        if (dir.sqrMagnitude > 0.0001f)
            transform.position += dir.normalized * speed * Time.deltaTime;

        float turn = 0f;
        if (!attacking && keyboard.qKey.isPressed) turn -= turnSpeed;
        if (!attacking && keyboard.eKey.isPressed) turn += turnSpeed;
        transform.Rotate(0f, turn * Time.deltaTime, 0f);

        if (mouse != null && mouse.leftButton.wasPressedThisFrame && !blocking && !attacking)
            TriggerAttack();
        if (Time.time > comboResetTime) comboStep = 0;
    }

    void TriggerAttack()
    {
        animator.SetInteger(AttackIndexHash, comboStep);
        animator.SetTrigger(AttackHash);
        comboStep = (comboStep + 1) % 3;
    }

    float SpeedForGait(float g)
    {
        if (g <= 1f) return walkSpeed * g;
        if (g <= 2f) return Mathf.Lerp(walkSpeed, jogSpeed, g - 1f);
        return Mathf.Lerp(jogSpeed, sprintSpeed, Mathf.Clamp01(g - 2f));
    }
}

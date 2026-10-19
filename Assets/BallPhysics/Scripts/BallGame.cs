using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BallGame : MonoBehaviour
{
    public SpherePhysics spherePrefab;
    public PhysicsManager manager;
    public Transform cannon;
    public SpherePhysics[] targets;
    public float shotSpeed = 14f;
    public float shotLifeTime = 6f;
    public int maxShots = 8;

    int shots;
    int knocked;
    bool[] isKnocked;

    void Start()
    {
        isKnocked = new bool[targets.Length];
        foreach (SpherePhysics target in targets)
            target.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Mouse.current.leftButton.wasPressedThisFrame && shots < maxShots && knocked < targets.Length)
            Shoot();

        // only direct hits from the cannon count
        for (int i = 0; i < targets.Length; i++)
        {
            SpherePhysics hitBy = targets[i].lastTouched;
            if (!isKnocked[i] && hitBy != null && System.Array.IndexOf(targets, hitBy) < 0)
            {
                isKnocked[i] = true;
                knocked++;
                targets[i].GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 aim = ray.GetPoint(-ray.origin.z / ray.direction.z);

        SpherePhysics ball = Instantiate(spherePrefab, cannon.position, Quaternion.identity);
        ball.velocity = (aim - cannon.position).normalized * shotSpeed;
        manager.Register(ball);
        Destroy(ball.gameObject, shotLifeTime);
        shots++;
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 32;
        string text = "Hit every red ball with the cannon   Shots left: " + (maxShots - shots) + "   Hit: " + knocked + "/" + targets.Length;
        if (knocked == targets.Length)
            text = "You win! Shots used: " + shots + "   R to play again";
        else if (shots == maxShots && FindObjectsByType<SpherePhysics>().Length == targets.Length)
            text = "Out of shots! Hit: " + knocked + "/" + targets.Length + "   R to try again";
        GUI.Label(new Rect(20, 20, 1400, 60), text);
    }
}

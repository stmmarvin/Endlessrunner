using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    bool alive = true;
    [SerializeField] TextMeshProUGUI gameOverText;
    public float speed = 5;
    [SerializeField] Rigidbody rb;

    float horizontalInput;
    [SerializeField] float horizontalMultiplier = 2f;

    public float speedIncreasePerPoint = 0.1f;

    [SerializeField] float jumpFource = 400f;
    [SerializeField] LayerMask groundMask;
    public GameObject Restartbutton;

    [Header("Mobile Touch")]
    [SerializeField] bool enableTouchInput = true;
    [SerializeField] float touchHorizontalSensitivity = 2.5f; // hoger = sneller naar links/rechts
    [SerializeField] float touchDeadZonePixels = 8f; // kleine bewegingen negeren

    int activeFingerId = -1;
    Vector2 lastTouchPos;
    bool jumpRequested;

    private void FixedUpdate()
    {
        if (!alive) return;

        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        Vector3 horizontalMove = transform.right * horizontalInput * speed * Time.fixedDeltaTime * horizontalMultiplier;
        rb.MovePosition(rb.position + forwardMove + horizontalMove);
    }

    void Update()
    {
        if (!alive) return;

        jumpRequested = false;

        // PC input blijft werken
        float pcHorizontal = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }

        // Mobiel input (alleen als enabled + device touch ondersteunt)
        float touchHorizontal = 0f;
        if (enableTouchInput && Input.touchSupported)
        {
            touchHorizontal = ReadTouchHorizontal();
            if (ReadTouchJump())
            {
                jumpRequested = true;
            }
        }

        // Combineer: als er touch is, gebruik touch; anders PC.
        // (Als je liever wilt optellen/clampen kan dat ook.)
        horizontalInput = Mathf.Abs(touchHorizontal) > 0f ? touchHorizontal : pcHorizontal;

        if (jumpRequested)
        {
            Jump();
        }

        if (transform.position.y < -5f)
        {
            Die();
        }
    }

    float ReadTouchHorizontal()
    {
        if (Input.touchCount <= 0)
        {
            activeFingerId = -1;
            return 0f;
        }

        // Kies 1 vinger om te "sturen"
        Touch touch = default;
        bool hasTouch = false;

        if (activeFingerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == activeFingerId)
                {
                    touch = Input.GetTouch(i);
                    hasTouch = true;
                    break;
                }
            }
        }

        if (!hasTouch)
        {
            touch = Input.GetTouch(0);
            activeFingerId = touch.fingerId;
        }

        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPos = touch.position;
            return 0f;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            activeFingerId = -1;
            return 0f;
        }

        Vector2 delta = touch.position - lastTouchPos;

        // Alleen horizontale beweging
        if (Mathf.Abs(delta.x) < touchDeadZonePixels)
        {
            return 0f;
        }

        lastTouchPos = touch.position;

        // Map pixels per frame naar -1..1-ish
        float axis = Mathf.Clamp(delta.x / (Screen.width * 0.25f) * touchHorizontalSensitivity, -1f, 1f);
        return axis;
    }

    bool ReadTouchJump()
    {
        if (Input.touchCount <= 0)
        {
            return false;
        }

        // Tap = touch began
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                return true;
            }
        }

        return false;
    }

    public void Die()
    {
        alive = false;
        gameOverText.gameObject.SetActive(true);
        Restartbutton.SetActive(true);
    }

    public void Restart(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void Jump()
    {
        float height = GetComponent<Collider>().bounds.size.y;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, (height / 2f) + 0.1f, groundMask);
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpFource);
    }
}

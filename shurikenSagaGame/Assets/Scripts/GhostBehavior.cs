using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostBehavior : MonoBehaviour
{
    [SerializeField]
    Transform player;
    public float detRange;
    private float originalDR;
    public float aggroDetRange;
    public float sideAmp = 0.5f; // Side amplitude for horizontal floating
    public float verticalAmp = 0.3f; // Vertical amplitude for up-down floating
    public float sideFrequency = 1f; // Frequency of horizontal floating
    public float verticalFrequency = 0.8f; // Frequency of vertical floating
    private Vector2 randomTarget;
    private float timePassedInAggro = 0f;
    public float cooldown = 2f; // Time for each movement segment
    public float floatForce = 0.3f; // Reduced force for slower movement
    public float randomOffsetRadius = 1f; // Radius for random offset around player direction
    private bool pickFloatDir = true;
    private bool outOfAggro = false;
    public float dragAmount;

    //private bool charge = false;
    private float timeUntilCharge = 0;
    public float chargeInterval;
    public float chargeAmp;

    public Color blinkColor = Color.red;
    private SpriteRenderer spriteRenderer;
    private float blinkTimer = 0f;
    private float nextBlinkTime = 0f;
    private Color originalColor;
    public float timeBeforeBlinking;

    public float overallAmpSide;
    public float overallAmpVert;

    private Rigidbody2D rb;
    Vector2 basePosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalDR = detRange;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Ensure the ghost is unaffected by gravity
        rb.drag = dragAmount; // Adjust drag for smooth deceleration
        basePosition = transform.position;
    }

    private void Update()
    {
        float pDistance = Vector2.Distance(transform.position, player.position);
        if (pDistance > detRange)
        {
            spriteRenderer.color = originalColor;
            detRange = originalDR;
            if (outOfAggro)
            {
                outOfAggro = false;
                basePosition = transform.position;
            }

            Float();
        }
        else
        {
            Aggro();
        }
    }

    private void Float()
    {
        // Add oscillation in both X and Y axes for "floaty" effect
        float sideMovement = Mathf.Sin(Time.time * sideFrequency) * sideAmp * overallAmpSide;
        float verticalMovement = Mathf.Sin(Time.time * verticalFrequency) * verticalAmp * overallAmpVert;
        rb.AddForce(new Vector2(sideMovement, verticalMovement));
    }

    private void Aggro()
    {
        detRange = aggroDetRange;

        outOfAggro = true;
        timePassedInAggro += Time.deltaTime;
        timeUntilCharge += Time.deltaTime;

        // Pick a new random target position in the direction of the player when cooldown resets
        if (timePassedInAggro >= cooldown || pickFloatDir)
        {
            // Direction vector from ghost to player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // Set the target in the general direction of the player with a random offset
            randomTarget = (Vector2)transform.position + directionToPlayer * 2f + Random.insideUnitCircle * randomOffsetRadius;
            pickFloatDir = false;
            timePassedInAggro = 0f;

            // Randomize oscillation frequency and amplitude slightly for a natural effect
            sideFrequency = Random.Range(0.8f, 1.2f);
            verticalFrequency = Random.Range(0.6f, 1.0f);
            sideAmp = Random.Range(0.4f, 0.6f);
            verticalAmp = Random.Range(0.2f, 0.4f);
        }

        // Calculate direction and apply force for smooth movement
        Vector2 direction = (randomTarget - (Vector2)transform.position).normalized;
        rb.AddForce(direction * floatForce, ForceMode2D.Force);

        if (timeUntilCharge >= chargeInterval)
        {
            spriteRenderer.color = originalColor;
            timeUntilCharge = 0f;
            blinkTimer = 0f;
            Vector2 chargeDir = (player.position - transform.position).normalized;
            rb.AddForce(chargeDir * chargeAmp, ForceMode2D.Force);
        } else if (timeUntilCharge >= chargeInterval - timeBeforeBlinking)
        {
            // Increment the blinkTimer, reset when this block is first entered
            blinkTimer += Time.deltaTime;

            // Calculate the blink interval based on the normalized blinkTimer
            float blinkInterval = Mathf.Lerp(0.2f, 0.04f, blinkTimer / timeBeforeBlinking);

            // Check if it's time to blink
            if (Time.time >= nextBlinkTime)
            {
                // Toggle between blinkColor and normalColor
                spriteRenderer.color = spriteRenderer.color == originalColor ? blinkColor : originalColor;

                // Set the next blink time based on the calculated interval
                nextBlinkTime = Time.time + blinkInterval;
            }

            // Ensure the color resets to normal after the entire `timeBeforeBlinking` duration
            if (blinkTimer >= timeBeforeBlinking)
            {
                spriteRenderer.color = originalColor; // Reset to normal color at the end
                blinkTimer = 0; // Reset blinkTimer for the next charge
            }
        }

        // Check if ghost reached the random target, reset pickFloatDir to pick a new position
        if (Vector2.Distance(transform.position, randomTarget) < 0.5f)
        {
            pickFloatDir = true;
        }
    }
}
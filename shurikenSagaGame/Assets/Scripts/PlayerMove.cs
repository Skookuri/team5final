using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour {
    //public AudioSource WalkSFX;
    public Rigidbody2D rb2D;
    private bool FaceLeft = false; // determine which way player is facing.
    public static float runSpeed = 10f;
    public float startSpeed = 10f;
    public bool isAlive = true;
    public bool isShoot = false;
    
    public Transform player;

    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component in Player_Art
    private Animator animator; // Reference to the Animator component in Player_Art

    public Sprite defaultSprite;
    public Sprite sideSprite;
    public Sprite backSprite;
    public Sprite shuriSprite;

    public Transform firePoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    // dashing vars (Massimo)
    [SerializeField]
    public float dashSpeed = 25f; // Speed during dash
    [SerializeField]
    public float dashDuration = .5f; // Duration of the dash
    [SerializeField]
    public float dashCooldown = .5f; // Cooldown time between dashes
    [SerializeField]
    public float dashLength = 10f; // Distance the player dashes

    public float doubleTapTime = 0.22f; // Time window for double-tap detection
    private float lastTapTime = 0f; // Last time a movement key was tapped
    private string lastTappedKey = ""; // Track the last tapped movement key

    private bool isDashing = false;

    public float timePassed = 0;
    public bool isVisible = true;

    private Vector3 lastSavedPosition;
    private bool fell = false;

    void Start(){
        lastSavedPosition = transform.position;

        rb2D = transform.GetComponent<Rigidbody2D>();

        // Get the SpriteRenderer component from the player_art child
        spriteRenderer = transform.Find("player_art").GetComponent<SpriteRenderer>();

        // Get the Animator component from the player_art child
        animator = transform.Find("player_art").GetComponent<Animator>();

        InvokeRepeating("SavePosition", 1, 1);
    }

    private void SavePosition()
    {
        if (!fell)
        {
            lastSavedPosition = transform.position;
        }
    }

    void Update(){
        //NOTE: Horizontal axis: [a] / left arrow is -1, [d] / right arrow is 1
        //NOTE: Vertical axis: [w] / up arrow, [s] / down arrow
        Vector3 hvMove = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

        // Update vertical movement for the Animator
        animator.SetFloat("Vertical", hvMove.y);  // Send the vertical movement to the Animator
        bool MovingHoriz = hvMove.x != 0;  // Check if horizontal movement is happening
        animator.SetBool("MovingHoriz", MovingHoriz);  // Send the horizontal movement != checker to the Animator
        
        if (isAlive == true){
            //transform.position = transform.position + hvMove * runSpeed * Time.deltaTime;
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Massimo changes start*******
            // Check for double-tap on movement keys
            if (moveVertical > 0 && Input.GetKeyDown(KeyCode.W))
            {
                HandleDoubleTap("W");
            }
            else if (moveVertical < 0 && Input.GetKeyDown(KeyCode.S))
            {
                HandleDoubleTap("S");
            }
            else if (moveHorizontal < 0 && Input.GetKeyDown(KeyCode.A))
            {
                HandleDoubleTap("A");
            }
            else if (moveHorizontal > 0 && Input.GetKeyDown(KeyCode.D))
            {
                HandleDoubleTap("D");
            }

            // Handle normal movement
            //if (!isDashing)
            //{
            //    Vector2 movement = new Vector2(moveHorizontal, moveVertical) * runSpeed;
            //    rb2D.velocity = movement;
            //}
            //Massimo changes end*******

            if (Time.time >= nextAttackTime){
                if (Input.GetAxis("AttackYea") > 0){
                    playerFire();
                    nextAttackTime = Time.time + 1f / attackRate;
                    isShoot = true;
                } else {
                    isShoot = false;
                }
            }

            if (isShoot == true) {
                spriteRenderer.sprite = shuriSprite; //Hide non- moving
            } else if (!isDashing){ //disabled for when dashing (Massimo)
                if (hvMove.y < 0) {
                    animator.enabled = true; // Disable Animator for front view
                    //spriteRenderer.sprite = defaultSprite; //Show non-moving default sprite
                } else if (hvMove.y > 0) {
                    animator.enabled = false; // Disable Animator for back view
                    //animator.enabled = true;
                    spriteRenderer.sprite = backSprite; //Show non-moving default sprite
                } else if (hvMove.x != 0) {
                    animator.enabled = false; // Disable Animator for side view
                    spriteRenderer.sprite = sideSprite; //Show non-moving default sprite
                } else {
                    animator.enabled = true; // Enable Animator for Idle front view animation
                    //spriteRenderer.sprite = defaultSprite; //Show non-moving default sprite
                }
            }
            // Turning. Reverse if input is moving the Player right and Player faces left.
            if ((hvMove.x < 0 && !FaceLeft) || (hvMove.x > 0 && FaceLeft)){
                playerTurn();
            }
        }

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hole") && !isDashing)
        {
            //LANCE - ACCOUNT FOR DAMAGE HERE
            fell = true;
            StartCoroutine(Wait());
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2); // Wait for 2 seconds
        Debug.Log("2 seconds have passed!");
        Respawn();
        // Add any actions you want to perform after the wait here
    }

    public void Respawn()
    {
        fell = false;
        transform.position = lastSavedPosition;
    }

    private void FixedUpdate()
    { 
        if (isAlive && !isDashing && !fell)
        {
            Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * runSpeed * Time.fixedDeltaTime * 33;
            rb2D.velocity = movement;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        } else if ( isAlive && isDashing && !fell)
        {
            //Debug.Log(Time.fixedDeltaTime);
            if (Time.time > timePassed + 0.037)
            {
                timePassed = Time.time;
                ToggleVis();
            }
        } else if (fell)
        {
            Vector2 movement = new Vector2(0, 0);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        }
    }

    private void ToggleVis() { 
        if(isVisible)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .6f);
        } else
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .8f);
        }
        isVisible = !isVisible;

    }

    //Massimo changes start*******
    private void HandleDoubleTap(string key)
    {
        if (key == lastTappedKey && Time.time - lastTapTime <= doubleTapTime)
        {
            // If the same key is tapped twice within the time window, dash
            StartCoroutine(Dash(key));
        }

        // Update last tapped key and time
        lastTappedKey = key;
        lastTapTime = Time.time;
    }

    IEnumerator Dash(string directionKey)
    {
        isDashing = true;

        // Determine the dash direction based on the key
        Vector2 direction = Vector2.zero;
        if (directionKey == "W")
        {
            direction = Vector2.up;
            spriteRenderer.sprite = backSprite;

        }
        else if (directionKey == "S")
        {
            direction = Vector2.down;
            spriteRenderer.sprite = defaultSprite;
        }
        else if (directionKey == "A") 
        {
            direction = Vector2.left;
            spriteRenderer.sprite = sideSprite;
        }
        else if (directionKey == "D")
        {
            direction = Vector2.right;
            spriteRenderer.sprite = sideSprite;
        }

        float elapsedTime = 0f;

        // Store the initial position to calculate the distance
        Vector2 startPosition = rb2D.position;
        while (elapsedTime < dashDuration)
        {
            // Calculate the new position based on dash length and speed
            Vector2 dashPosition = startPosition + direction * dashLength;

            // Move to the dash position instantly
            rb2D.MovePosition(Vector2.MoveTowards(rb2D.position, dashPosition, dashSpeed * Time.fixedDeltaTime / 10));

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Stop the dash
        rb2D.velocity = Vector2.zero;

        // Allow for immediate resumption of regular movement
        isDashing = false;
    }
    //Massimo changes end*******

    private void playerTurn(){
        // NOTE: Switch player facing label
        FaceLeft = !FaceLeft;

        // NOTE: Multiply player's x local scale by -1.
        Vector3 theScale = spriteRenderer.transform.localScale;
        theScale.x *= -1;
        spriteRenderer.transform.localScale = theScale;
    }

    void playerFire(){
        //animator.SetTrigger ("Fire");
        //Vector2 fwd = (firePoint.position - this.transform.position).normalized;
        //Vector2 fwd = 0.normalized;
        //GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        //projectile.GetComponent<Rigidbody2D>().AddForce(fwd * projectileSpeed, ForceMode2D.Impulse);
        //spriteRenderer.sprite = shuriSprite; //Show non-moving default sprite


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Set z to 0 since we're working in 2D

        // Calculate the direction from the firing point to the mouse position
        Vector2 fwd = (mousePosition - firePoint.position).normalized;

        // Instantiate the projectile and apply force towards the mouse position
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().AddForce(fwd * projectileSpeed, ForceMode2D.Impulse);

        // Set the sprite to the default sprite
        spriteRenderer.sprite = shuriSprite; // Show non-moving default sprite
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float minSlideSpeed = 0.05f;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.red;
    
    [Header("Attack Settings")]
    [SerializeField] private float maxAttackDuration = 3f;
    [SerializeField] private Color cooldownColor = Color.blue;
    
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private InputSystem_Actions inputActions;
    private bool isJumpPressed = false;
    
    // Attack system variables
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private float attackStartTime = 0f;
    private float attackDuration = 0f;
    private float cooldownEndTime = 0f;

    void Awake()
    {
        // Inicializa o Input System
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        
        // Subscribe to jump action (space key)
        inputActions.Player.Jump.started += OnJumpStarted;
        inputActions.Player.Jump.canceled += OnJumpCanceled;
    }

    void OnDisable()
    {
        // Unsubscribe from jump action
        inputActions.Player.Jump.started -= OnJumpStarted;
        inputActions.Player.Jump.canceled -= OnJumpCanceled;
        
        inputActions.Disable();
    }

    void Start()
    {
        // Verifica se o Rigidbody2D foi atribuído
        if (playerBody == null)
        {
            Debug.LogError("Player Body (Rigidbody2D) não foi atribuído! Por favor, arraste o Rigidbody2D do player no Inspector.");
        }
        
        // Verifica se o SpriteRenderer foi atribuído
        if (spriteRenderer == null)
        {
            Debug.LogError("Sprite Renderer não foi atribuído! Por favor, arraste o SpriteRenderer do player no Inspector.");
        }
        
        // Define a cor inicial
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    void Update()
    {
        // Captura o input WASD
        GetInput();
        
        // Aplica o movimento
        Move();
        
        // Atualiza o sistema de ataque
        UpdateAttackSystem();
        
        // Atualiza a cor do sprite
        UpdateSpriteColor();
    }
    
    private void GetInput()
    {
        // Lê o input usando o novo Input System
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        // Normaliza o vetor para movimento diagonal consistente
        moveInput = moveInput.normalized;
    }
    
    private void Move()
    {
        if (playerBody == null) return;
        
        Vector2 targetVelocity;
        
        // Se há input, define a velocidade alvo
        if (moveInput != Vector2.zero)
        {
            targetVelocity = moveInput * moveSpeed;
            
            // Acelera suavemente em direção à velocidade alvo
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Quando não há input, desacelera suavemente até zero
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
            
            // Para completamente quando a velocidade fica muito baixa (evita tremulação)
            if (currentVelocity.magnitude < minSlideSpeed)
            {
                currentVelocity = Vector2.zero;
            }
        }
        
        // Aplica a velocidade ao Rigidbody2D
        playerBody.linearVelocity = currentVelocity;
    }
    
    private void UpdateSpriteColor()
    {
        if (spriteRenderer == null) return;
        
        // Muda a cor baseado no estado atual
        if (isOnCooldown)
        {
            spriteRenderer.color = cooldownColor;
        }
        else if (isAttacking)
        {
            spriteRenderer.color = activeColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        // Only start attack if not on cooldown
        if (!isOnCooldown)
        {
            isJumpPressed = true;
            isAttacking = true;
            attackStartTime = Time.time;
        }
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumpPressed = false;
        
        // Only end attack if we were actually attacking
        if (isAttacking)
        {
            EndAttack();
        }
    }
    
    // Método público para verificar se o jump está pressionado
    public bool IsJumpPressed()
    {
        return isAttacking; // Return attacking state instead of just button press
    }
    
    // Public method to check if player is currently attacking
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    // Private method to update attack system
    private void UpdateAttackSystem()
    {
        // Check cooldown
        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            isOnCooldown = false;
        }
        
        // Check max attack duration
        if (isAttacking && (Time.time - attackStartTime) >= maxAttackDuration)
        {
            // Force end attack after max duration
            isJumpPressed = false;
            EndAttack();
        }
    }
    
    // Private method to handle attack ending
    private void EndAttack()
    {
        if (!isAttacking) return;
        
        isAttacking = false;
        attackDuration = Time.time - attackStartTime;
        
        // Calculate cooldown based on hold time
        float cooldownTime = attackDuration;
        
        // Add penalty if held over max duration
        if (attackDuration >= maxAttackDuration)
        {
            cooldownTime += 1f; // Add 1 second penalty
        }
        
        // Start cooldown
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownTime;
        
        Debug.Log($"Attack ended. Duration: {attackDuration:F2}s, Cooldown: {cooldownTime:F2}s");
    }
    
    // Method called when player should die
    private void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }
    
    // Collision detection for enemies
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            
            // Player dies if hit by enemy while not attacking
            if (!isAttacking && enemyScript != null && !enemyScript.IsPinballMode())
            {
                Die();
            }
        }
    }
    
    // Collision detection for pinball mode enemies (using OnCollisionEnter2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            
            // Player dies if hit by enemy in pinball mode while not attacking
            if (!isAttacking && enemyScript != null && enemyScript.IsPinballMode())
            {
                Die();
            }
        }
    }
}

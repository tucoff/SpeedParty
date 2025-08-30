using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private float moveSpeed = 3f; // Reduzido de 5f para 3f
    [SerializeField] private float acceleration = 8f; // Reduzido de 10f para 8f
    [SerializeField] private float deceleration = 4f; // Reduzido de 5f para 4f
    [SerializeField] private float minSlideSpeed = 0.05f;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.red;
    
    [Header("Attack Settings")]
    [SerializeField] private float maxAttackDuration = 3f;
    [SerializeField] private Color cooldownColor = Color.blue;
    
    [Header("Pinball Settings")]
    [SerializeField] private float pinballDuration = 5f;
    [SerializeField] private float minPinballSpeed = 1.5f; // Reduzido para ser mais lento
    [SerializeField] private Color pinballColor = Color.magenta;
    [SerializeField] private PhysicsMaterial2D pinballMaterial;
    
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
    
    // Pinball system variables
    private bool isPinballMode = false;
    private float pinballEndTime = 0f;
    private bool canTakeDamage = true;

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
        
        // Cria material de física para pinball se não foi atribuído
        if (pinballMaterial == null)
        {
            CreatePinballMaterial();
        }
    }

    void Update()
    {
        // Atualiza o sistema de pinball primeiro
        UpdatePinballSystem();
        
        // Só permite movimento e ataque se não estiver em modo pinball
        if (!isPinballMode)
        {
            // Captura o input WASD
            GetInput();
            
            // Aplica o movimento
            Move();
            
            // Atualiza o sistema de ataque
            UpdateAttackSystem();
        }
        else
        {
            // No modo pinball, mantém velocidade mínima
            MaintainPinballSpeed();
        }
        
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
        if (isPinballMode)
        {
            spriteRenderer.color = pinballColor;
        }
        else if (isOnCooldown)
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
        return isJumpPressed; // Return actual jump pressed state
    }
    
    // Public method to check if player is currently attacking
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    // Public method to check if player is in pinball mode
    public bool IsPinballMode()
    {
        return isPinballMode;
    }
    
    // Public method to check if player can take damage
    public bool CanTakeDamage()
    {
        return canTakeDamage && !isPinballMode;
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
    
    // Pinball system methods
    private void UpdatePinballSystem()
    {
        if (isPinballMode && Time.time >= pinballEndTime)
        {
            ExitPinballMode();
        }
    }
    
    private void MaintainPinballSpeed()
    {
        if (playerBody.linearVelocity.magnitude < minPinballSpeed)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            playerBody.linearVelocity = randomDirection * minPinballSpeed;
        }
    }
    
    public void EnterPinballMode(Vector2 initialVelocity)
    {
        if (isPinballMode) return;
        
        isPinballMode = true;
        canTakeDamage = false;
        pinballEndTime = Time.time + pinballDuration;
        
        // Aplicar velocity inicial se fornecida
        if (initialVelocity != Vector2.zero)
        {
            playerBody.linearVelocity = initialVelocity;
        }
        
        // Trocar para material pinball para ricochete
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = pinballMaterial;
        }
        
        Debug.Log($"Player entered pinball mode for {pinballDuration} seconds!");
    }
    
    private void ExitPinballMode()
    {
        if (!isPinballMode) return;
        
        isPinballMode = false;
        canTakeDamage = true;
        
        // Remover material pinball
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = null;
        }
        
        Debug.Log("Player exited pinball mode!");
    }
    
    private void CreatePinballMaterial()
    {
        pinballMaterial = new PhysicsMaterial2D("Player Pinball Material");
        pinballMaterial.friction = 0f;
        pinballMaterial.bounciness = 0.8f; // Reduzido de 1f para 0.8f para menor velocidade
    }
    
    // Method called when player should die
    private void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }
    
    // Collision detection for enemies and other players
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            
            // Player dies if hit by enemy while not attacking and can take damage
            if (CanTakeDamage() && !isAttacking && enemyScript != null && !enemyScript.IsPinballMode())
            {
                Die();
            }
        }
        else if (other.CompareTag("Player"))
        {
            Player otherPlayer = other.GetComponent<Player>();
            
            // Se este player está atacando e o outro não está em pinball, transformar o outro em pinball
            if (isAttacking && otherPlayer != null && !otherPlayer.IsPinballMode())
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                Vector2 knockbackForce = knockbackDirection * minPinballSpeed * 1.5f;
                otherPlayer.EnterPinballMode(knockbackForce);
                
                Debug.Log($"{gameObject.name} attacked {other.name} and put them in pinball mode!");
            }
        }
    }
    
    // Collision detection for pinball mode enemies and players (using OnCollisionEnter2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            
            // Player dies if hit by enemy in pinball mode while not attacking and can take damage
            if (CanTakeDamage() && !isAttacking && enemyScript != null && enemyScript.IsPinballMode())
            {
                Die();
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Player otherPlayer = collision.gameObject.GetComponent<Player>();
            
            // Se este player está atacando e colidiu com outro player em pinball, o outro morre
            if (isAttacking && otherPlayer != null && otherPlayer.IsPinballMode())
            {
                otherPlayer.Die();
                Debug.Log($"{gameObject.name} destroyed pinball player {collision.gameObject.name}!");
            }
            // Se ambos estão em pinball mode, eles ricocheteiam
            else if (isPinballMode && otherPlayer != null && otherPlayer.IsPinballMode())
            {
                // O Unity Physics já cuida do ricochete, só logamos
                Debug.Log($"Pinball collision between {gameObject.name} and {collision.gameObject.name}!");
            }
        }
    }
}

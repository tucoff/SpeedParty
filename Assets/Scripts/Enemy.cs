using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float updateTargetInterval = 0.2f;
    
    [Header("Physics Settings")]
    [SerializeField] private Rigidbody2D enemyRigidbody;
    [SerializeField] private PhysicsMaterial2D pinballMaterial;
    
    [Header("Attack Response Settings")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float pinballDuration = 5f;
    [SerializeField] private float minPinballSpeed = 2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pinballColor = Color.yellow;
    
    private Transform targetPlayer;
    private GameObject[] players;
    private float lastTargetUpdateTime;
    private bool isPinballMode = false;
    private float pinballEndTime;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    
    // Estados do inimigo
    private enum EnemyState
    {
        Following,
        Pinball
    }
    private EnemyState currentState = EnemyState.Following;
    
    void Start()
    {
        // Componentes necessários
        if (enemyRigidbody == null)
            enemyRigidbody = GetComponent<Rigidbody2D>();
        
        if (enemyRigidbody == null)
        {
            Debug.LogError("Enemy precisa de um Rigidbody2D!");
            return;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        
        // Configura o trigger do collider
        if (enemyCollider != null)
        {
            enemyCollider.isTrigger = true;
        }
        
        // Cria material de física para pinball se não foi atribuído
        if (pinballMaterial == null)
        {
            CreatePinballMaterial();
        }
        
        // Configuração inicial do Rigidbody
        enemyRigidbody.gravityScale = 0f;
        enemyRigidbody.linearDamping = 0.5f; // Drag normal para movimento de seguir
        
        FindNearestPlayer();
    }
    
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Following:
                FollowingBehavior();
                break;
                
            case EnemyState.Pinball:
                PinballBehavior();
                break;
        }
    }
    
    private void FollowingBehavior()
    {
        // Atualiza o alvo periodicamente
        if (Time.time - lastTargetUpdateTime >= updateTargetInterval)
        {
            FindNearestPlayer();
            lastTargetUpdateTime = Time.time;
        }
        
        // Move em direção ao player mais próximo
        MoveTowardsTarget();
    }
    
    private void PinballBehavior()
    {
        // Verifica se o modo pinball deve terminar
        if (Time.time >= pinballEndTime)
        {
            ExitPinballMode();
            
            // NEW: Disappear after pinball time ends and notify wave manager
            EnemyDeathHandler deathHandler = GetComponent<EnemyDeathHandler>();
            if (deathHandler != null)
            {
                deathHandler.NotifyEnemyDisappear();
            }
            else
            {
                // Fallback: destroy directly and try to notify wave manager
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.OnEnemyDisappear(gameObject);
                }
                Destroy(gameObject);
            }
            return;
        }
        
        // Mantém uma velocidade mínima no modo pinball
        if (enemyRigidbody.linearVelocity.magnitude < minPinballSpeed)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            enemyRigidbody.linearVelocity = randomDirection * minPinballSpeed;
        }
    }
    
    private void FindNearestPlayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            targetPlayer = null;
            return;
        }
        
        float nearestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;
        
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                if (distance <= detectionRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = player.transform;
                }
            }
        }
        
        targetPlayer = nearestPlayer;
    }
    
    private void MoveTowardsTarget()
    {
        if (targetPlayer == null || currentState != EnemyState.Following) return;
        
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        
        // Usa Rigidbody para movimento mais suave
        enemyRigidbody.linearVelocity = direction * moveSpeed;
        
        // Rotaciona para olhar na direção do movimento
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), 5f * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se colidiu com um player
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            
            // Se o player está com jump pressionado, ativa o modo pinball
            if (playerScript != null && playerScript.IsJumpPressed() && currentState == EnemyState.Following)
            {
                EnterPinballMode(other.transform);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // No modo pinball, pode colidir com players e causar knockback
        if (currentState == EnemyState.Pinball && collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (playerRb != null)
            {
                // Calcula direção do knockback para o player
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        // No modo pinball, mata outros inimigos ao colidir com eles
        if (currentState == EnemyState.Pinball && collision.gameObject.CompareTag("Enemy"))
        {
            Enemy otherEnemy = collision.gameObject.GetComponent<Enemy>();
            
            // Coloca o outro inimigo em modo pinball se não estiver já
            if (otherEnemy != null && !otherEnemy.IsPinballMode())
            {
                Debug.Log("Pinball enemy hit another enemy - putting it in pinball mode!");
                otherEnemy.EnterPinballMode(transform);
            }
        }
    }
    
    public void EnterPinballMode(Transform attackingEntity)
    {
        currentState = EnemyState.Pinball;
        isPinballMode = true;
        pinballEndTime = Time.time + pinballDuration;
        
        // Calcula direção oposta à entidade que causou o pinball (player ou outro inimigo)
        Vector2 knockbackDirection = (transform.position - attackingEntity.position).normalized;
        
        // Aplica força de knockback
        enemyRigidbody.linearVelocity = Vector2.zero;
        enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        
        // Configura física para pinball
        enemyRigidbody.linearDamping = 0.01f; // Fricção mínima
        enemyCollider.isTrigger = false; // Permite colisões físicas
        
        if (pinballMaterial != null)
        {
            enemyCollider.sharedMaterial = pinballMaterial;
        }
        
        // Muda cor visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = pinballColor;
        }
        
        Debug.Log("Enemy entrou em modo pinball!");
    }
    
    private void ExitPinballMode()
    {
        currentState = EnemyState.Following;
        isPinballMode = false;
        
        // Restaura configurações normais
        enemyRigidbody.linearDamping = 0.5f;
        enemyRigidbody.linearVelocity = Vector2.zero;
        enemyCollider.isTrigger = true;
        enemyCollider.sharedMaterial = null;
        
        // Restaura cor normal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        Debug.Log("Enemy saiu do modo pinball!");
    }
    
    private void CreatePinballMaterial()
    {
        pinballMaterial = new PhysicsMaterial2D("PinballMaterial");
        pinballMaterial.bounciness = 1.5f; // Bounce gigantesco
        pinballMaterial.friction = 0.01f;   // Fricção mínima
    }
    
    // Métodos públicos
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }
    
    public void SetPinballDuration(float duration)
    {
        pinballDuration = duration;
    }
    
    public bool IsPinballMode()
    {
        return isPinballMode;
    }
    
    public float GetRemainingPinballTime()
    {
        if (isPinballMode)
        {
            return Mathf.Max(0f, pinballEndTime - Time.time);
        }
        return 0f;
    }
    
    // Method to manually destroy enemy (for wave management)
    public void DestroyEnemy()
    {
        EnemyDeathHandler deathHandler = GetComponent<EnemyDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.NotifyEnemyDeath();
        }
        
        Destroy(gameObject);
    }
    
    // Gizmos para debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (targetPlayer != null && currentState == EnemyState.Following)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
        }
        
        if (isPinballMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}

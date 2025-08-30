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
    
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private InputSystem_Actions inputActions;
    private bool isJumpPressed = false;

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
        
        // Muda a cor baseado no estado do botão space
        spriteRenderer.color = isJumpPressed ? activeColor : normalColor;
    }
    
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        isJumpPressed = true;
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumpPressed = false;
    }
    
    // Método público para verificar se o jump está pressionado
    public bool IsJumpPressed()
    {
        return isJumpPressed;
    }
}

using UnityEngine;

/// <summary>
/// Controlador simples de customização de cores do jogador
/// </summary>
public class PlayerCustomizationController : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Current Colors")]
    [SerializeField] private Color currentShirtColor = Color.red;
    [SerializeField] private Color currentPantsColor = Color.green;
    [SerializeField] private Color currentHatColor = Color.blue;
    [SerializeField] private Color currentSkinColor = Color.yellow;
    
    // Material instance para evitar alterações no material original
    private Material materialInstance;
    
    // Property IDs para otimização
    private static readonly int ShirtColorProperty = Shader.PropertyToID("_ShirtColor");
    private static readonly int PantsColorProperty = Shader.PropertyToID("_PantsColor");
    private static readonly int HatColorProperty = Shader.PropertyToID("_HatColor");
    private static readonly int SkinColorProperty = Shader.PropertyToID("_SkinColor");
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        SetupMaterial();
        ApplyCurrentColors();
    }
    
    private void InitializeComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError($"SpriteRenderer não encontrado no GameObject {gameObject.name}");
            }
        }
    }
    
    private void SetupMaterial()
    {
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
        }
    }
    
    private void ApplyCurrentColors()
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor(ShirtColorProperty, currentShirtColor);
            materialInstance.SetColor(PantsColorProperty, currentPantsColor);
            materialInstance.SetColor(HatColorProperty, currentHatColor);
            materialInstance.SetColor(SkinColorProperty, currentSkinColor);
        }
    }
    
    public void SetShirtColor(Color newColor)
    {
        currentShirtColor = newColor;
        if (materialInstance != null)
        {
            materialInstance.SetColor(ShirtColorProperty, newColor);
        }
    }
    
    public void SetPantsColor(Color newColor)
    {
        currentPantsColor = newColor;
        if (materialInstance != null)
        {
            materialInstance.SetColor(PantsColorProperty, newColor);
        }
    }
    
    public void SetHatColor(Color newColor)
    {
        currentHatColor = newColor;
        if (materialInstance != null)
        {
            materialInstance.SetColor(HatColorProperty, newColor);
        }
    }
    
    public void SetSkinColor(Color newColor)
    {
        currentSkinColor = newColor;
        if (materialInstance != null)
        {
            materialInstance.SetColor(SkinColorProperty, newColor);
        }
    }
    
    public Color GetShirtColor() => currentShirtColor;
    public Color GetPantsColor() => currentPantsColor;
    public Color GetHatColor() => currentHatColor;
    public Color GetSkinColor() => currentSkinColor;
    
    public void SetAllColors(Color shirt, Color pants, Color hat, Color skin)
    {
        SetShirtColor(shirt);
        SetPantsColor(pants);
        SetHatColor(hat);
        SetSkinColor(skin);
    }
    
    public void ResetToDefaultColors()
    {
        SetAllColors(Color.red, Color.green, Color.blue, Color.yellow);
    }
    
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }
    }
}

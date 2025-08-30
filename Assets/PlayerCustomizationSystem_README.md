# Sistema de Customização de Cores do Jogador

Este sistema permite que os jogadores personalizem as cores da camisa, calças, chapéu e pele do personagem utilizando um shader customizado.

## Arquivos Criados

### Shader
- **PlayerColorSwapShader.shader** - Shader Unlit que substitui cores específicas da spritesheet

### Material
- **PlayerCustomizationMaterial.mat** - Material que utiliza o shader de customização

### Scripts
- **PlayerCustomizationController.cs** - Controlador principal para alterar as cores
- **PlayerCustomizationUI.cs** - Interface de usuário para testes (opcional)

## Como Usar

### 1. Configuração da Spritesheet

1. Selecione a spritesheet `RotateAnim(1).png` no Project Window
2. No Inspector, altere o **Texture Type** para **Sprite (2D and UI)**
3. Clique em **Sprite Editor** para fatiar a spritesheet se necessário
4. Clique em **Apply** para salvar as mudanças

### 2. Criação do Prefab do Personagem

1. Crie um novo GameObject na cena (GameObject > Create Empty)
2. Renomeie para "Player"
3. Adicione um componente **Sprite Renderer**
4. No Sprite Renderer:
   - Defina o **Sprite** como um dos sprites da spritesheet RotateAnim(1)
   - Defina o **Material** como PlayerCustomizationMaterial
5. Adicione o script **PlayerCustomizationController**
6. Drag & drop este GameObject para a pasta Prefabs para criar um Prefab

### 3. Configuração do Script

O script `PlayerCustomizationController` oferece os seguintes métodos públicos:

```csharp
// Alterar cores individualmente
SetShirtColor(Color newColor)    // Altera a cor da camisa
SetPantsColor(Color newColor)    // Altera a cor das calças
SetHatColor(Color newColor)      // Altera a cor do chapéu
SetSkinColor(Color newColor)     // Altera a cor da pele

// Alterar todas as cores de uma vez
SetAllColors(Color shirt, Color pants, Color hat, Color skin)

// Resetar para cores padrão
ResetToDefaultColors()

// Obter cores atuais
GetShirtColor()
GetPantsColor()
GetHatColor()
GetSkinColor()
```

### 4. Usando o Sistema via Script

```csharp
// Exemplo de uso
public class ExampleUsage : MonoBehaviour
{
    public PlayerCustomizationController playerCustomization;
    
    void Start()
    {
        // Alterar para cores personalizadas
        playerCustomization.SetShirtColor(Color.blue);
        playerCustomization.SetPantsColor(Color.yellow);
        playerCustomization.SetHatColor(Color.red);
        playerCustomization.SetSkinColor(new Color(1f, 0.8f, 0.6f)); // Tom de pele
    }
}
```

### 5. Interface de Usuário (Opcional)

Para testar o sistema, você pode criar uma UI simples:

1. Crie um Canvas (UI > Canvas)
2. Crie um GameObject vazio e adicione o script `PlayerCustomizationUI`
3. Configure as referências:
   - **Player Customization**: Arraste o GameObject com PlayerCustomizationController
   - **Color Buttons**: Crie botões UI para cada cor e configure os arrays
   - **Reset Button**: Botão para resetar as cores
   - **Current Colors Text**: Text UI para mostrar as cores atuais

## Propriedades do Shader

O shader possui as seguintes propriedades configuráveis:

- **_ShirtColor**: Cor da camisa (padrão: vermelho)
- **_PantsColor**: Cor das calças (padrão: verde)
- **_HatColor**: Cor do chapéu (padrão: azul)
- **_SkinColor**: Cor da pele (padrão: amarelo)
- **_ColorTolerance**: Tolerância para detecção de cores (padrão: 0.1)

## Cores Originais Mapeadas

O shader substitui estas cores da spritesheet original:
- **Vermelho puro (1,0,0)** → Cor da camisa
- **Verde puro (0,1,0)** → Cor das calças
- **Azul puro (0,0,1)** → Cor do chapéu
- **Amarelo puro (1,1,0)** → Cor da pele

## Troubleshooting

### Cores não estão mudando:
1. Verifique se o material está aplicado ao Sprite Renderer
2. Confirme se as cores originais na spritesheet correspondem aos valores esperados
3. Ajuste a propriedade `_ColorTolerance` se necessário

### Performance:
- O sistema usa Property IDs para otimização
- Cria instâncias de material automaticamente para evitar modificações globais
- Remove instâncias de material automaticamente quando objetos são destruídos

### Compatibilidade:
- Compatível com Unity 2019.4 LTS ou superior
- Funciona com o Universal Render Pipeline (URP)
- Otimizado para sprites 2D

## Customização Avançada

Para adicionar mais partes do corpo ou cores:

1. Edite o shader para adicionar novas propriedades Color
2. Defina as cores originais correspondentes no shader
3. Atualize o script PlayerCustomizationController com novos métodos
4. Configure o material com as novas propriedades

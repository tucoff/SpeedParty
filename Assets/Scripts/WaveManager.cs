using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private float timeBetweenWaves = 5f;
    
    [Header("Wave Settings")]
    [SerializeField] private int initialEnemiesPerWave = 3;
    [SerializeField] private int enemyIncreasePerWave = 2;
    [SerializeField] private int maxEnemiesPerWave = 20;
    
    //[SerializeField] private float difficultyIncreaseRate = 1.1f;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Color intervalColor = Color.yellow;
    [SerializeField] private Color activeWaveColor = Color.white;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debug = false;
    
    [Header("Current Wave Info")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private int enemiesInCurrentWave = 0;
    [SerializeField] private int enemiesAliveCount = 0;
    [SerializeField] private int totalDeaths = 0;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool waveInProgress = false;
    private bool gameStarted = false;

    // Singleton instance for easy access from other scripts
    public static WaveManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Validate spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: Nenhum spawn point foi atribuído!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("WaveManager: Enemy prefab não foi atribuído!");
            return;
        }

        if (waveText == null)
        {
            Debug.LogError("WaveManager: Wave Text UI não foi atribuído!");
        }

        UpdateWaveText();
        StartGame();
    }

    void Update()
    {
        // Debug mode: press N to start next wave
        if (debug && Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (waveInProgress)
            {
                ForceNextWave();
            }
            else
            {
                StartNewWave();
            }
        }

        // Check if all enemies in current wave are dead
        if (waveInProgress && enemiesAliveCount <= 0 && activeEnemies.Count == 0)
        {
            EndCurrentWave();
        }

        // Clean up null references in active enemies list
        CleanUpActiveEnemiesList();
    }

    public void StartGame()
    {
        if (gameStarted) return;
        
        gameStarted = true;
        Debug.Log("Jogo iniciado! Começando primeira wave...");
        
        if (debug)
        {
            Debug.Log("Modo DEBUG ativado! Pressione N para iniciar as waves.");
            UpdateWaveText(); // Update text but don't start wave
        }
        else
        {
            StartCoroutine(StartWaveCoroutine());
        }
    }

    private IEnumerator StartWaveCoroutine()
    {
        yield return new WaitForSeconds(1f); // Small delay before first wave
        StartNewWave();
    }

    private void StartNewWave()
    {
        if (waveInProgress) return;

        waveInProgress = true;
        enemiesInCurrentWave = Mathf.Min(initialEnemiesPerWave + (currentWave - 1) * enemyIncreasePerWave, maxEnemiesPerWave);
        enemiesAliveCount = enemiesInCurrentWave;
        
        Debug.Log($"Iniciando Wave {currentWave} com {enemiesInCurrentWave} inimigos!");
        
        UpdateWaveText();
        
        StartCoroutine(SpawnWaveEnemies());
    }

    private IEnumerator SpawnWaveEnemies()
    {
        for (int i = 0; i < enemiesInCurrentWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
        
        Debug.Log($"Todos os {enemiesInCurrentWave} inimigos da Wave {currentWave} foram spawnadoss!");
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        // Choose random spawn point
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[randomSpawnIndex].transform.position;

        // Instantiate enemy
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Add enemy to active list
        activeEnemies.Add(newEnemy);

        // Get Enemy component and apply difficulty scaling
        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // Increase enemy stats based on current wave
            //float speedMultiplier = Mathf.Pow(difficultyIncreaseRate, currentWave - 1);
            //enemyComponent.SetMoveSpeed(3f * speedMultiplier);
            // Manter detectionRange padrão de 100f definido no Enemy prefab
            //enemyComponent.SetDetectionRange(10f + (currentWave * 0.5f));
        }

        // Subscribe to enemy events (we'll modify Enemy.cs to support this)
        EnemyDeathHandler deathHandler = newEnemy.GetComponent<EnemyDeathHandler>();
        if (deathHandler == null)
        {
            deathHandler = newEnemy.AddComponent<EnemyDeathHandler>();
        }
        deathHandler.Initialize(this);

        Debug.Log($"Inimigo spawnado na posição {spawnPosition} para Wave {currentWave}");
    }

    private void EndCurrentWave()
    {
        waveInProgress = false;
        currentWave++;
        
        Debug.Log($"Wave {currentWave - 1} completada! Total de mortes: {totalDeaths}");
        Debug.Log($"Próxima wave ({currentWave}) começará em {timeBetweenWaves} segundos...");
        
        UpdateWaveText();
        
        StartCoroutine(WaitForNextWave());
    }

    private IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        
        if (debug)
        {
            Debug.Log($"Wave {currentWave} pronta! Pressione N para iniciar.");
        }
        else
        {
            StartNewWave();
        }
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        // Remove from active enemies list
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        // Decrease alive count and increase death count
        enemiesAliveCount--;
        totalDeaths++;

        Debug.Log($"Inimigo morreu! Restantes na wave: {enemiesAliveCount}, Total de mortes: {totalDeaths}");
    }

    public void OnEnemyDisappear(GameObject enemy)
    {
        // Called when enemy disappears after pinball time ends
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemiesAliveCount--;
            totalDeaths++; // Count disappearance as death
            
            Debug.Log($"Inimigo desapareceu após pinball! Restantes na wave: {enemiesAliveCount}, Total de mortes: {totalDeaths}");
        }
    }

    private void CleanUpActiveEnemiesList()
    {
        // Remove null references (destroyed enemies)
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                // Don't decrease enemiesAliveCount here as it should be handled by OnEnemyDeath/OnEnemyDisappear
            }
        }
    }

    // Public getters for UI or other systems
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAliveCount;
    public int GetTotalDeaths() => totalDeaths;
    public int GetEnemiesInCurrentWave() => enemiesInCurrentWave;
    public bool IsWaveInProgress() => waveInProgress;

    // Method to manually start next wave (for testing)
    public void ForceNextWave()
    {
        if (waveInProgress)
        {
            // Kill all active enemies
            foreach (GameObject enemy in activeEnemies.ToArray())
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
            enemiesAliveCount = 0;
        }
    }

    private void UpdateWaveText()
    {
        if (waveText == null) return;

        waveText.text = $"Wave Nº{currentWave}";
        
        // Set color based on wave state
        if (waveInProgress)
        {
            waveText.color = activeWaveColor;
        }
        else
        {
            waveText.color = intervalColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn points in editor
        if (spawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.transform.position, 0.5f);
                    Gizmos.DrawIcon(spawnPoint.transform.position, "sv_label_1", true);
                }
            }
        }
    }
}

using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    private WaveManager waveManager;
    private Enemy enemyComponent;
    private bool hasNotifiedDeath = false;

    public void Initialize(WaveManager manager)
    {
        waveManager = manager;
        enemyComponent = GetComponent<Enemy>();
    }

    void Update()
    {
        // Check if enemy should disappear after pinball time ends
        if (enemyComponent != null && !hasNotifiedDeath)
        {
            CheckEnemyDisappearConditions();
        }
    }

    private void CheckEnemyDisappearConditions()
    {
        // You can add conditions here for when the enemy should disappear
        // For now, we'll let the Enemy script handle its own lifecycle
        // This is a placeholder for future disappear conditions
    }

    public void NotifyEnemyDeath()
    {
        if (hasNotifiedDeath) return;
        
        hasNotifiedDeath = true;
        
        if (waveManager != null)
        {
            waveManager.OnEnemyDeath(gameObject);
        }
    }

    public void NotifyEnemyDisappear()
    {
        if (hasNotifiedDeath) return;
        
        hasNotifiedDeath = true;
        
        if (waveManager != null)
        {
            waveManager.OnEnemyDisappear(gameObject);
        }
        
        // Destroy the enemy
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Make sure we notify the wave manager when enemy is destroyed
        if (!hasNotifiedDeath && waveManager != null)
        {
            waveManager.OnEnemyDeath(gameObject);
        }
    }
}

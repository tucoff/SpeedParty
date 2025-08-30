using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesAliveText;
    [SerializeField] private TextMeshProUGUI totalDeathsText;
    [SerializeField] private TextMeshProUGUI waveProgressText;
    
    [Header("Settings")]
    [SerializeField] private bool showUI = true;
    
    void Update()
    {
        if (!showUI || WaveManager.Instance == null) return;
        
        UpdateUIElements();
    }
    
    private void UpdateUIElements()
    {
        WaveManager waveManager = WaveManager.Instance;
        
        // Update wave text
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveManager.GetCurrentWave()}";
        }
        
        // Update enemies alive text
        if (enemiesAliveText != null)
        {
            enemiesAliveText.text = $"Enemies Alive: {waveManager.GetEnemiesAlive()}";
        }
        
        // Update total deaths text
        if (totalDeathsText != null)
        {
            totalDeathsText.text = $"Total Deaths: {waveManager.GetTotalDeaths()}";
        }
        
        // Update wave progress text
        if (waveProgressText != null)
        {
            int enemiesInWave = waveManager.GetEnemiesInCurrentWave();
            int enemiesAlive = waveManager.GetEnemiesAlive();
            int enemiesDefeated = enemiesInWave - enemiesAlive;
            
            waveProgressText.text = $"Progress: {enemiesDefeated}/{enemiesInWave}";
        }
    }
    
    // Method to toggle UI visibility
    public void ToggleUI()
    {
        showUI = !showUI;
        
        // Hide/show all UI elements
        if (waveText != null) waveText.gameObject.SetActive(showUI);
        if (enemiesAliveText != null) enemiesAliveText.gameObject.SetActive(showUI);
        if (totalDeathsText != null) totalDeathsText.gameObject.SetActive(showUI);
        if (waveProgressText != null) waveProgressText.gameObject.SetActive(showUI);
    }
}

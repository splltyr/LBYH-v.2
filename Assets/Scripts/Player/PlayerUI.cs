using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public Slider healthSlider;
    public Image fillImage;
    public GameObject deathMenu; // Drag the 'DeathPanel' here!
    public Color highHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lerpSpeed = 5f;

    private float targetHealth;

    public void Initialize(float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            targetHealth = maxHealth;
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        targetHealth = currentHealth;
    }

    void Update()
    {
        if (healthSlider == null) return;
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * lerpSpeed);

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthSlider.value / healthSlider.maxValue);
        }
    }

    public void ShowDeathMenu()
    {
        if (deathMenu != null)
        {
            deathMenu.SetActive(true);
            
            // Ensure time is paused
            Time.timeScale = 0f;

            // --- AUTOMATIC FIX: Hook up the button in code just in case ---
            Button btn = deathMenu.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartGame);
                Debug.Log("<color=green>PlayerUI: Button listener assigned automatically.</color>");
            }
            
            // Force cursor visibility
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Debug.LogError("PlayerUI: deathMenu is NOT ASSIGNED! Drag 'DeathPanel' into the slot.");
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting to Main Menu...");
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Start Menu 1");
    }
}

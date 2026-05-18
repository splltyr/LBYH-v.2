using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar; 
    public bool autoDestroy = true; // Set this to false for enemies with custom death animations!

    private DamonFinale finaleScript;

    void Start()
    {
        currentHealth = maxHealth;
        finaleScript = GetComponent<DamonFinale>();
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        ProcessDamage(damage);
    }

    public void TakeDamage(float damage, Vector2 direction)
    {
        ProcessDamage(damage);
    }

    private void ProcessDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        
        // Update the slider!
        if (healthBar != null) healthBar.value = currentHealth;
        
        if (finaleScript != null)
        {
            finaleScript.TakeDamage(damage);
        }
        
        if (currentHealth <= 0)
        {
            SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            if (finaleScript == null && autoDestroy) Destroy(gameObject, 0.2f);
        }
        
        Debug.Log(gameObject.name + " took " + damage + " damage! HP: " + currentHealth);
    }
}
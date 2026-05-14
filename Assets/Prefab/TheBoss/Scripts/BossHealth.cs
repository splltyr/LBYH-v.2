using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{

	public int health = 500;
	public GameObject deathEffect;
	public bool isInvulnerable = false;

	[Header("UI References")]
	public Slider healthSlider;
	public GameObject healthCanvas;

	private Animator anim;
	private SpriteRenderer spriteRenderer;
	private Color originalColor;

	void Awake()
	{
		// Ensure only one AudioListener exists in the scene
		AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
		if (listeners.Length > 1)
		{
			bool keptOne = false;
			foreach (var listener in listeners)
			{
				if (listener.gameObject.CompareTag("MainCamera") && !keptOne)
				{
					listener.enabled = true;
					keptOne = true;
				}
				else
				{
					listener.enabled = false;
				}
			}
			if (!keptOne && listeners.Length > 0) listeners[0].enabled = true;
		}
	}

	void Start()
	{
		anim = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		
		if (spriteRenderer != null) 
			originalColor = spriteRenderer.color;

		// Try to find UI components if not linked
		if (healthSlider == null) 
			healthSlider = GetComponentInChildren<Slider>();
		
		if (healthCanvas == null && healthSlider != null)
		{
			Canvas c = healthSlider.GetComponentInParent<Canvas>();
			if (c != null) healthCanvas = c.gameObject;
		}

		if (healthSlider != null)
		{
			healthSlider.maxValue = health;
			healthSlider.value = health;
		}
	}

	public void TakeDamage(int damage)
	{
		if (isDead() || isInvulnerable)
			return;

		health -= damage;

		if (healthSlider != null)
		{
			healthSlider.value = health;
		}

		// Trigger Hit animation and flash
		if (anim != null)
		{
			anim.SetTrigger("Hit");
		}
		StartCoroutine(HitFlash());

		if (health <= 200)
		{
			if (anim != null) anim.SetBool("IsEnraged", true);
		}

		if (health <= 0)
		{
			Die();
		}
	}

	// Overload for float damage/knockback if called by other scripts
	public void TakeDamage(float damage, Vector2 knockback)
	{
		TakeDamage((int)damage);
	}

	public void OnHit()
	{
		if (anim != null) anim.SetTrigger("Hit");
	}

	private IEnumerator HitFlash()
	{
		if (spriteRenderer == null) yield break;
		spriteRenderer.color = Color.red; 
		yield return new WaitForSeconds(0.1f);
		spriteRenderer.color = originalColor;
	}

	private bool isDead()
	{
		return health <= 0;
	}

	void Die()
	{
		if (deathEffect != null)
			Instantiate(deathEffect, transform.position, Quaternion.identity);
		
		Destroy(gameObject);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	public BossHealth bossHealth;
	public Slider slider;

	void Start()
	{
		if (bossHealth == null)
			bossHealth = FindAnyObjectByType<BossHealth>();
		
		if (slider == null)
			slider = GetComponent<Slider>();

		if (bossHealth != null && slider != null)
			slider.maxValue = bossHealth.health;
	}

	// Update is called once per frame
	void Update()
    {
		slider.value = bossHealth.health;
    }
}

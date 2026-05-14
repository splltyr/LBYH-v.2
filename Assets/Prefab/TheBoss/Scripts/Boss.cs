using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{

	public Transform player;

	public bool isFlipped = false;

	public void LookAtPlayer()
	{
		if (transform.position.x > player.position.x && isFlipped)
		{
			Flip();
			isFlipped = false;
		}
		else if (transform.position.x < player.position.x && !isFlipped)
		{
			Flip();
			isFlipped = true;
		}
	}

	private void Flip()
	{
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

}

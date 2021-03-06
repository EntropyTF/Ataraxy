﻿using UnityEngine;
using System.Collections;

public class ShockBall : Projectile
{
	public Detonator explosive;
	private float shockBallGrowthRate = 3f;
	private float maxShockBallSize = 4f;
	public float blastRadius;
	public float explosiveDamage;

	void Start()
	{
		Damage = 5;
		ProjVel = 1;
		explosiveDamage = 3;
		blastRadius = 5;
		explosive = transform.FindChild("Detonator-Tiny").GetComponent<Detonator>();
	}

	void Update()
	{
		if (((SphereCollider)collider).radius < 4)
		{
			((SphereCollider)collider).radius += shockBallGrowthRate * Time.deltaTime / 2;
			blastRadius += shockBallGrowthRate * Time.deltaTime;
			explosiveDamage += shockBallGrowthRate * Time.deltaTime / 2;
		}
		if (particleSystem.startSize < 4)
		{
			//particleSystem.startLifetime += shockBallGrowthRate * Time.deltaTime;
			particleSystem.startSize += shockBallGrowthRate * Time.deltaTime;
		}
	}

	public void Shocked()
	{
		//DO THE THING!
		explosive.Explode();
		gameObject.particleSystem.enableEmission = false;
		gameObject.collider.enabled = false;
		Destroy(rigidbody);
		enabled = false;

		Collider[] hitColliders = Physics.OverlapSphere(explosive.transform.position, blastRadius);
		int i = 0;
		while (i < hitColliders.Length)
		{
			float distFromBlast = Vector3.Distance(hitColliders[i].transform.position, explosive.transform.position);
			float parameterForMessage = -(explosiveDamage * blastRadius / distFromBlast);

			hitColliders[i].gameObject.SendMessage("AdjustHealth", parameterForMessage, SendMessageOptions.DontRequireReceiver);
			i++;
		} 


		Destroy(gameObject, 5.0f);
	}
}

﻿using UnityEngine;
using System.Collections;

public class Enemy : NPC
{
	/// <summary>
	/// A Vector3 that tracks the muzzle to the player (for firing projectiles)
	/// </summary>
	private Vector3 dirToTarget;
	#region Combat State Information
	public bool CanSeePlayer = false;
	public enum EnemyState { Idle, Searching, Preparing, Attacking };
	public EnemyState state;
	public float stateTimer = 0;
	#endregion

	#region Experience Values
	/// <summary>
	/// The value this enemy provides when slain.
	/// </summary>
	public float XpReward = 5;

	//private float xpRateOverTime = .5f;
	private float xpRateOverTime = 5.5f;

	private bool gainXpWhenHit = false;
	private float xpGainWhenHit = .5f;

	private float expGainPerShot = 5;
	#endregion

	#region Projectile Attack Variables
	public GameObject projectilePrefab;
	private GameObject gunMuzzle;

	private float firingTimer;
	public float FiringTimer
	{
		get { return firingTimer; }
		set { firingTimer = value; }
	}

	private float firingCooldown;
	public float FiringCooldown
	{
		get { return firingCooldown; }
		set { firingCooldown = value; }
	}

	public bool firing = false;
	#endregion

	#region Override Methods - Faction, AdjustHealth, KillEntity
	public override Allegiance Faction
	{
		get { return Allegiance.Enemy; }
	}

	public override void AdjustHealth(float amount)
	{
		base.AdjustHealth(amount);
		if (gainXpWhenHit)
		{
			GainExperience(xpGainWhenHit);
		}
	}

	public override void KillEntity()
	{
		Untarget();

		//Entity sets IsDead to true but nothing else.
		base.KillEntity();

		AwardExperience();

		//Spawn a token
		ThrowToken(SpawnToken());

		//Give nearby enemies experience.

		//And after all that, destroy ourself.
		Destroy(gameObject);
	}

	public virtual void AwardExperience()
	{
		//Give the player experience.
		GameManager.Instance.player.GainExperience(XpReward, Level);
	}

	public virtual GameObject SpawnToken()
	{
		//Drop a token.
		GameObject newToken = (GameObject)GameObject.Instantiate(GameManager.Instance.tokenPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
		return newToken;	
	}

	public virtual void ThrowToken(GameObject newToken)
	{
		float randBound = 4000;
		newToken.rigidbody.AddForce(newToken.rigidbody.mass * (Vector3.up * Random.Range(.8f, 1.5f) * randBound / 8) + new Vector3(Random.Range(-randBound, randBound), 0, Random.Range(-randBound, randBound)));
	}

	#endregion

	#region Core Functions - Start, Update
	public override void Start()
	{
		base.Start();
		gameObject.tag = "Enemy";
		FindFiringPositions();
	}

	public virtual void FindFiringPositions()
	{
		gunMuzzle = transform.FindChild("Gun Muzzle").gameObject;
		FiringCooldown = 6;
	}

	public override void Update()
	{
		if (!UIManager.Instance.paused)
		{
			firingTimer -= Time.deltaTime;
			//If we know where the player is, update them as the target.

			HandleExperience();

			HandleKnowledge();

			HandleAggression();

			base.Update();
		}
	}
	#endregion

	#region Enemy Behavior Functions
	public void HandleExperience()
	{
		if (CanSeePlayer)
		{
			GainExperience(Time.deltaTime * xpRateOverTime);
		}

		
	}

	public override void GainLevel()
	{
		XpReward += 5;
		int randomBonuses = Random.Range(0, 3);
		if(randomBonuses == 1)
		{
			if(FiringCooldown > 3)
			{
				FiringCooldown -= 3f;
			}
		}
		else if (randomBonuses == 2)
		{
			projectilePrefab.GetComponent<Projectile>().Damage += 2;
		}
		else
		{
			Health += 5;
			AdjustHealth(5);
		}
		base.GainLevel();
	}

	/// <summary>
	/// A method for handling movement
	/// </summary>
	public virtual void HandleMovement()
	{

	}

	/// <summary>
	/// A method for handling if the enemy can see the player.
	/// </summary>
	public virtual void HandleKnowledge()
	{
		//Set the direction to the player, can be updated to lead the player's movement.
		dirToTarget = (GameManager.Instance.player.transform.position - gunMuzzle.transform.position);// +charMotor.movement.velocity * percentagePlayerVelLeading;

		dirToTarget.Normalize();
	}

	public virtual void HandleAggression()
	{
		if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) < 50)
		{
			CanSeePlayer = true;
		}
		else
		{
			CanSeePlayer = false;
		}

		firing = CanSeePlayer;

		if (firing)
		{
			//Raycast to the player, if it is a clear shot, fire in that direction.
			if (firingTimer <= 0)
			{
				//Fire at the player
				AttackPlayer();

				//Set ourselves on cooldown
				firingTimer = FiringCooldown;

				//Learn from the experience. Leveling up is checked earlier in the update loop.
				GainExperience(expGainPerShot);
			}
			else
			{
				//Debug.Log("Cooldown Remaining: " + Counter + "\n");
			}
		}
	}

	/// <summary>
	/// Basic projectile attack. Creates, pushes and initializes projectile.
	/// </summary>
	public void AttackPlayer()
	{
		GameObject projectile = (GameObject)GameObject.Instantiate(projectilePrefab, gunMuzzle.transform.position, Quaternion.identity);

		Projectile proj = projectile.GetComponent<Projectile>();

		if (proj != null)
		{
			proj.Faction = Faction;
			proj.Shooter = this;

			projectile.rigidbody.AddForce(dirToTarget * proj.ProjVel * projectile.rigidbody.mass);
			Destroy(projectile, proj.ProjLife);
		}
	}
	#endregion

}

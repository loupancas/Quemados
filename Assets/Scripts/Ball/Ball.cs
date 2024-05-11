using Fusion;
using UnityEngine;
using UnityEngine.Serialization;


	public enum EWeapon
	{
		None,
		Ball,
		
	}
	
	public class Ball : NetworkBehaviour
	{
		public EWeapon Type;

		[Header("Shoot Setup")]
		public float     Damage = 10f;
		public int       FireRate = 100;
		[Range(1, 20)]
		public int       BallsPerShot = 1;
		public float     Dispersion = 0f;
		public LayerMask HitMask;
		public float     MaxHitDistance = 100f;

		
		[Header("Visuals")]
		public Sprite     Icon;
		public string     Name;
		public Animator   Animator;
		[FormerlySerializedAs("BallVisual")]
		public GameObject ThirdPersonVisual;

		[Header("Fire Effect")]
		[FormerlySerializedAs("MuzzleTransform")]
		public Transform        ThirdPersonMuzzleTransform;
		public GameObject       MuzzleEffectPrefab;
		public BallVisual       BallVisualPrefab;

		
		public bool HasBall => RemainingShoot > 0;

		[Networked]
		public NetworkBool IsCollected { get; set; }
				
		[Networked]
		public int RemainingShoot { get; set; }

		//[Networked]
		private int _fireCount { get; set; }
		[Networked]
		private TickTimer _fireCooldown { get; set; }
		[Networked, Capacity(32)]
		private NetworkArray<ProjectileData> _ShootData { get; }

		private int _fireTicks;
		private int _visibleFireCount;
		private bool _reloadingVisible;
		private GameObject _muzzleEffectInstance;
		private SceneObjects _sceneObjects;

		public void Fire(Vector3 firePosition, Vector3 fireDirection, bool justPressed)
		{
			if (IsCollected == false)
				return;
			if (justPressed == false )
				return;			
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false)
				return;
			

			// Random needs to be initialized with same seed on both input and
			// state authority to ensure the projectiles are fired in the same direction on both.
			Random.InitState(Runner.Tick * unchecked((int)Object.Id.Raw));

			for (int i = 0; i < BallsPerShot; i++)
			{
				var BallDirection = fireDirection;

				if (Dispersion > 0f)
				{
					// We use unit sphere on purpose -> non-uniform distribution (more projectiles in the center).
					var dispersionRotation = Quaternion.Euler(Random.insideUnitSphere * Dispersion);
					BallDirection = dispersionRotation * fireDirection;
				}

				FireProjectile(firePosition, BallDirection);
			}

			_fireCooldown = TickTimer.CreateFromTicks(Runner, _fireTicks);
			
		}

	

		public void AddShoot(int amount)
		{
			RemainingShoot += amount;
		}

		public void ToggleVisibility(bool isVisible)
		{
			
			ThirdPersonVisual.SetActive(isVisible);

			if (_muzzleEffectInstance != null)
			{
				_muzzleEffectInstance.SetActive(false);
			}
		}

	

		public override void Spawned()
		{
			
			_sceneObjects = Runner.GetSingleton<SceneObjects>();
		}

		public override void FixedUpdateNetwork()
		{
			if (IsCollected == false)
				return;

			
		}

	

		private void FireProjectile(Vector3 firePosition, Vector3 fireDirection)
		{
			var projectileData = new ProjectileData();

			var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

			// Whole projectile path and effects are immediately processed (= hitscan projectile).
			if (Runner.LagCompensation.Raycast(firePosition, fireDirection, MaxHitDistance,
				    Object.InputAuthority, out var hit, HitMask, hitOptions))
			{
				projectileData.HitPosition = hit.Point;
				projectileData.HitNormal = hit.Normal;

				if (hit.Hitbox != null)
				{
					ApplyDamage(hit.Hitbox, hit.Point, fireDirection);
				}
				else
				{
					// Hit effect is shown only when player hits solid object.
					projectileData.ShowHitEffect = true;
				}
			}

		}

		

		private void ApplyDamage(Hitbox enemyHitbox, Vector3 position, Vector3 direction)
		{
			//var enemyHealth = enemyHitbox.Root.GetComponent<Health>();
			//if (enemyHealth == null || enemyHealth.IsAlive == false)
			return;
			
		}

		

		private struct ProjectileData : INetworkStruct
		{
			public Vector3     HitPosition;
			public Vector3     HitNormal;
			public NetworkBool ShowHitEffect;
		}
	}


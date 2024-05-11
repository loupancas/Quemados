using Fusion;
using UnityEngine;


	public class Balls : NetworkBehaviour
	{
		//public Animator    Animator;
	    public Transform   FireTransform;
	    //public float       WeaponSwitchTime = 1f;
	    //public AudioSource SwitchSound;

	    public bool IsSwitching => _switchTimer.ExpiredOrNotRunning(Runner) == false;

	    [Networked, HideInInspector]
	    public Ball CurrentWeapon { get; set; }
	    [HideInInspector]
	    public Ball[] BallWeapon;

	    [Networked]
	    private TickTimer _switchTimer { get; set; }
	    //[Networked]
	    //private Ball _pendingWeapon { get; set; }

	    private Ball _visibleWeapon;

	    public void Fire(bool justPressed)
		{
			if (CurrentWeapon == null)
				return;

			CurrentWeapon.Fire(FireTransform.position, FireTransform.forward, justPressed);
		}

	   


	    public bool PickupBall(EWeapon Type)
	    {
		    var weapon = GetWeapon(Type);
			
		if (weapon.IsCollected)
		{
			return true;
		}
		else return false;
		
			
	    }

	    public Ball GetWeapon(EWeapon weaponType)
	    {
			for (int i = 0; i < BallWeapon.Length; ++i)
			{
				if (BallWeapon[i].Type == weaponType)
					return BallWeapon[i];
			}

			return default;
	    }

	    public override void Spawned()
	    {
		    if (HasStateAuthority)
		    {
			    CurrentWeapon = BallWeapon[0];
			    CurrentWeapon.IsCollected = true;
		    }
	    }

	    public override void FixedUpdateNetwork()
	    {
	    }

	    public override void Render()
	    {
		    if (_visibleWeapon == CurrentWeapon) 
			    return;

		    // Update weapon visibility
		    for (int i = 0; i < BallWeapon.Length; i++)
		    {
			    var weapon = BallWeapon[i];
			    if (weapon == CurrentWeapon)
			    {
					weapon.ToggleVisibility(true);
			    }
				else
				{
					weapon.ToggleVisibility(false);
				}		    

		      _visibleWeapon = CurrentWeapon;
             }

        }

	    private void Awake()
	    {
		    // Ball are already present inside Player prefab.
		    BallWeapon = GetComponentsInChildren<Ball>();
	    }

	
	}


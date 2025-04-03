using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using System.Resources;
using UnityEditor;
using Fusion.Addons.Physics;
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; set; }
    //public static bool ControlsEnabled = false;
    [Header("Stats")]
    [SerializeField] public float _speed = 3;
    [SerializeField] public float _jumpForce = 5;
    [SerializeField] private float _groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _shootDamage = 25f;
    [SerializeField] private LayerMask _shootLayer;
    [SerializeField] private LayerMask _ballLayer;
    private Rigidbody _rgbd;
    public Animator _animator;
    private bool _isGrounded = true;
    //private int fadeTime = 5;
    public float _xAxi;
    public float _yAxi;
    public bool _jumpPressed;
    private bool _shootPressed;
    public float _defaultSpeed;
    public float _defaultJump; 
    public Camera Camera;
    [SerializeField] private Ball2 _currentBall;
    [Networked] public bool HasBall { get; set; }
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private GameObject ballPrefab;
    [field: Header("Stats")]
 
    [Networked, OnChangedRender(nameof(ChangeColor))] public Color _teamColor { get; set; }
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    private bool hasTeam = false;
    #region Networked Color Change

    [Networked, OnChangedRender(nameof(OnNetColorChanged))]
    Color NetworkedColor { get; set; }


    void OnNetColorChanged() => GetComponentInChildren<Renderer>().material.color = NetworkedColor;
   
    #endregion

    #region Networked Health Change

    [Networked, OnChangedRender(nameof(OnNetHealthChanged))]
    private float NetworkedHealth { get; set; } = 3;
    void OnNetHealthChanged() => Debug.Log($"Life = {NetworkedHealth}");

    #endregion

    public event Action<float> OnMovement = delegate {  };
    public event Action OnShooting = delegate {  };

 
    private void Awake()
    {
        //if (_ballPrefab != null)
        //{
        //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = false;
        //}

     

    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            LocalPlayer = this;
            NetworkedColor = GetComponentInChildren<Renderer>().material.color;
            Debug.Log("Local Player");
            Camera = Camera.main;
            Camera.GetComponent<ThirdPersonCamera>().Target = transform;
            _rgbd = GetComponent<Rigidbody>();
            _defaultJump = _jumpForce;
            _defaultSpeed = _speed;
            //ball.GetComponent<MeshRenderer>().enabled = false;

            HasBall = false;

            //inventory.OnItemAdded += InventoryChanged;
            //inventory.OnItemRemoved += InventoryChanged;
            //_weaponHandler = GetComponent<WeaponHandler>();

            //if (_weaponHandler == null)
            //{
            //    Debug.LogError("WeaponHandler component is missing.");
            //}
            // Asignar autoridad de entrada
            if (!Object.HasInputAuthority)
            {
                Object.AssignInputAuthority(Runner.LocalPlayer);
            }

            StartCoroutine(WaitInit());

        }
        else
        {
            Debug.Log("No State Authority");
            //SynchronizeProperties();
            //GameManager.Instance.AddNewPlayer(Runner.ActivePlayers.Count(), this);
            //Debug.Log("index" + Runner.ActivePlayers.Count());
            //if (GameManager.Instance.Runner.ActivePlayers.Count() == 2)
            //{
            //    Debug.Log("Start Countdown");
            //    //StartCoroutine(UIManager.instance.StartCountdown(3));

            //}
        }
    }

   
    void SynchronizeProperties()
    {
        OnNetColorChanged();
    }

    public static void EnablePlayerControls()
    {
        //ControlsEnabled = true;
    }
    
    void Update()
    {
        if (!HasStateAuthority ) return;

       //heckGrounded();
   
       _xAxi = Input.GetAxis("Horizontal");
        _yAxi = Input.GetAxis("Vertical");

       //Debug.Log(moveInputVector);

        if (Input.GetKeyDown(KeyCode.Space) )
        {
            _jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupBall();
            Debug.Log("E key pressed");
        

        }

        if (Input.GetMouseButtonDown(0) && HasBall)
        {
            RPC_FireAndDropBall();
            //ball.GetComponent<MeshRenderer>().enabled = false;
           
        }
         
    }




    #region
    //private void TryPickupBall()
    //{
    //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
    //    foreach (var hitCollider in hitColliders)
    //    {
    //        if (hitCollider.CompareTag("Ball"))
    //        {
    //            NetworkObject ballNetworkObject = hitCollider.GetComponent<NetworkObject>();
    //            BallPickUp ballPickUp = hitCollider.GetComponent<BallPickUp>();
    //            if (ballNetworkObject && !ballNetworkObject.HasInputAuthority && ballPickUp)
    //            {
    //                ballPickUp.RPC_PickUp(this);
    //                ballNetworkObject.AssignInputAuthority(Runner.LocalPlayer);
    //                HeldBall = ballNetworkObject;
    //                //if (_ballPrefab != null && !_ballPrefab.GetComponentInChildren<MeshRenderer>().enabled)
    //                //{
    //                //    HasBall = true;
    //                //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = true;
    //                //}
    //                break;
    //            }
    //        }
    //    }
    //}

    //private void ThrowBall()
    //{
    //    if (HeldBall.IsValid)
    //    {
    //        Vector3 throwDirection = transform.forward;
    //        Ball2 ball = HeldBall.GetComponent<Ball2>();
    //        if (ball != null)
    //        {
    //            ball.RpcThrow(throwDirection);
    //            HeldBall.GetComponent<NetworkObject>().RemoveInputAuthority();
    //            HeldBall = default;
    //            //if (_ballPrefab != null && _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled)
    //            //{
    //            //    HasBall = false;
    //            //    _ballPrefab.GetComponentInChildren<MeshRenderer>().enabled = false;
    //            //}
    //        }
    //    }
    //}
    #endregion


    private void TryPickupBall()
    {
        Debug.Log("TryPickupBall called");
        if (HasBall)
        {
            Debug.Log("Player already has a ball");
            return;
        }

        if (!Object.HasInputAuthority)
        {
            Debug.Log("Player does not have input authority");
            return;
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, _ballLayer);
        Debug.Log($"Number of colliders found: {hitColliders.Length}");

        foreach (var hitCollider in hitColliders)
        {
            BallPickUp ballPickUp = hitCollider.GetComponent<BallPickUp>();
            if (ballPickUp != null)
            {
                Debug.Log("BallPickUp component found, calling RPC_PickUpBall");
                RPC_PickUpBall(ballPickUp.Object);
                break;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickUpBall(NetworkObject ballObject)
    {
        Debug.Log("RPC_PickUpBall called");
        BallPickUp ballPickUp = ballObject.GetComponent<BallPickUp>();
        if (ballPickUp != null)
        {
            ballPickUp.PickUp(this);
            HasBall = true;
            MeshRenderer ballRenderer = ballObject.GetComponent<MeshRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.enabled = false;
            }
            _currentBall = Instantiate(ballPrefab).GetComponent<Ball2>();
            if (_currentBall != null)
            {
                Debug.Log("Ball2 component assigned to _currentBall");
            }
            else
            {
                Debug.LogError("Failed to assign Ball2 component to _currentBall");
            }
        }
    }

 

   

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_FireAndDropBall()
    {
        if (!HasInputAuthority)
        {
            Debug.LogError("Player does not have input authority to fire and drop the ball.");
            return;
        }
        Debug.Log($"Attempting to throw ball. Ball valid: {_currentBall?.Object?.IsValid ?? false}, Authority: {HasInputAuthority}");
        // Lógica para lanzar la pelota
        if (_currentBall != null)
        {
            Vector3 throwDirection = ballSpawnPoint.forward; // Dirección de lanzamiento desde ballSpawnPoint
            _currentBall.transform.position = ballSpawnPoint.position; // Posición de lanzamiento desde ballSpawnPoint
            _currentBall.RpcThrow(throwDirection);
            HasBall = false;
            _currentBall = null; // Limpiar la referencia a la pelota
        }
        else
        {
            Debug.LogError("No ball found in the scene to throw.");
        }
    }


    



    private void ChangeColor()
    {
        _meshRenderer.material.color = _teamColor;
    }



    public override void FixedUpdateNetwork()
    {
       
        if (!HasStateAuthority) return;

        //MOVIMIENTO
        Movement();


      

    }

   

    public void Movement()
    {
        Vector3 camForward = Camera.transform.forward;
        Vector3 camRight = Camera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = (camForward * _yAxi + camRight * _xAxi).normalized;

        // SALTO
        if (_jumpPressed)
        {
            Jump();
            _jumpPressed = false;
        }

        // DISPARO
        //if (_shootPressed)
        //{
        //    //RaycastShoot();
        //    _shootPressed = false;
        //}

        // MOVIMIENTO
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
            _rgbd.velocity += direction * (_speed * 10 * Runner.DeltaTime);

            var velocity = Vector3.ClampMagnitude(_rgbd.velocity, _speed);
            velocity.y = _rgbd.velocity.y;
            _rgbd.velocity = velocity;
            OnMovement(_rgbd.velocity.magnitude);
            _animator.SetFloat("Speed", _rgbd.velocity.magnitude);
        }
        else
        {
            var velocity = _rgbd.velocity;
            velocity.x = 0;
            velocity.z = 0;
            _rgbd.velocity = velocity;
            _animator.SetFloat("Speed", 0);
        }
    }

    void Jump()
    {
        _rgbd.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _animator.SetBool("Jumping", true);
        // playerView.isRunning(true);
    }

   
    #region POWER UP
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<JumpPower>())
        {
            Debug.Log("PowerUp Jump");
            JumpPower jumpPower = other.gameObject.GetComponent<JumpPower>();
            jumpPower.GetPower();
            _jumpForce = jumpPower.modifier;
            ChangeColorRecursively(transform, Color.red);
            StartCoroutine(NormalizeStats(jumpPower.fadeTime));
        }
        if (other.gameObject.GetComponent<SpeedPower>())
        {
            Debug.Log("PowerUp Speed");
            SpeedPower speedPower = other.gameObject.GetComponent<SpeedPower>();
            speedPower.GetPower();
            _speed = speedPower.modifier;
            ChangeColorRecursively(transform, Color.blue);
            StartCoroutine(NormalizeStats(speedPower.fadeTime));
        }
    }   

    IEnumerator NormalizeStats(float fadeTime)
    {
        yield return new WaitForSeconds(fadeTime);
        _speed = _defaultSpeed;
        _jumpForce = _defaultJump;
        ChangeColorRecursively(transform, Color.white);
    }
    #endregion
    void ChangeColorRecursively(Transform parent, Color color)
    {
        foreach (Transform child in parent)
        {
            SkinnedMeshRenderer renderer = child.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = color;
                }
            }
            
            ChangeColorRecursively(child, color);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float dmg)
    {
        Local_TakeDamage(dmg);
    }

    public void Local_TakeDamage(float dmg)
    {
        NetworkedHealth -= dmg;

        if (NetworkedHealth <= 0)
        {
            //Dead();
            //SetLoseScreenRPC();
            //UIManager.instance.SetLoseScreen();
            RoomM.Instance.RPC_PlayerWin(Runner.LocalPlayer);
        }
        else
        {
            Debug.Log("Player Hit");
        }

    }

    void Dead()
    {
        Runner.Despawn(Object);
    }

    //private void SetLoseScreenRPC()
    //{
    //    UIManager.instance.SetLoseScreen();
    //}

    private IEnumerator WaitInit()
    {
        yield return new WaitForSeconds(1);

        switch (Runner.LocalPlayer.PlayerId)
        {
            case 1:
                _teamColor = Color.red;
                break;
            case 2:
                _teamColor = Color.blue;
                break;
            default:
                _teamColor = Color.yellow;
                break;
        }
    }
}

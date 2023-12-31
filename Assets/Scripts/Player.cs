using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : Actor
{
    [SerializeField]
    GameObject restartLevel;
    [SerializeField]
    bool isMelee = true;
    [SerializeField]
    private bool isInCombat;
    [SerializeField]
    private bool isInMenu;

    [SerializeField]
    private Transform lastChkPointCoord;
    [SerializeField]
    private float defModifier;
    [SerializeField]
    private bool isAllowedDodge;
    [SerializeField]
    private Vector2 aimDir;

    [SerializeField]
    private short dashCount = 0, maxDashCount;
    [SerializeField]
    private float dashCoolDownTime;



    public AudioSource audioSource;
    public AudioSource shogunCocking;
    public Transform mousePos;

    
    [SerializeField] GameObject projectile;
    [SerializeField] float projectileSpeed = 1;

    [SerializeField] float atkCooldown = 3f;

    private bool inCoolDown;
    private bool faceRight = true;

    public bool IsInCombat { get => isInCombat; set => isInCombat = value; }
    public bool IsInMenu { get => isInMenu; set => isInMenu = value; }

    public Transform LastChkPointCoord { get => lastChkPointCoord; set => lastChkPointCoord = value; }
    public float DefModifier { get => defModifier; set => defModifier = value; }
    public bool IsAllowedDodge { get => isAllowedDodge; set => isAllowedDodge = value; }
    public Vector2 AimDir { get => aimDir; set => aimDir = value; }
    public short MaxDashCount { get => maxDashCount; set => maxDashCount = value; }

    public float iFrameDuration;

    IEnumerator DashFeedback()
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        yield return new WaitForSeconds(.5f);
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    IEnumerator ShootBurst()
    {
        inCoolDown = true;
        Vector3 Position = new Vector3(transform.position.x,
                                       transform.position.y ,
                                       0);

        
            var projectileInstance = Instantiate(projectile,
                        Position,
                        Quaternion.identity);

            projectileInstance.gameObject.GetComponent<Rigidbody2D>().velocity = aimDir.normalized * projectileSpeed;

        shogunCocking.Play();
        yield return new WaitForSeconds(atkCooldown);
        inCoolDown = false;
    }

    public new void BecomeVulnerable()
    {
        IsInvulnerable = false;
        CancelInvoke("BecomeVulnerable");
    }

    public IEnumerator MakeInvulnerableAfterDamaged()
    {
        IsInvulnerable = true;
        yield return new WaitForSeconds(iFrameDuration);
        IsInvulnerable = false;
    }

    void Start()
    {
        inCoolDown = false;
        isMelee = true;
        IsInvulnerable = false;
        //audioSource = GetComponent<AudioSource>();



    }

    //limit frame rate
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Application.targetFrameRate != 60)
            Application.targetFrameRate = 60;

        if (Defeated)
            restartLevel.GetComponent<restartLevel>().Restart();


        //userInput
        AimDir = mousePos.position - transform.position;

        if (Input.GetKeyDown(KeyCode.R)) 
        {
            isMelee = !isMelee;
        }


        if (Input.GetKeyDown(KeyCode.Mouse0) && !IsInvulnerable && isMelee)
        {
            Attack(aimDir);

        }else if (Input.GetKeyDown(KeyCode.Mouse0) && !isMelee && !inCoolDown)
            Attack(aimDir);


        if (Attacking && isMelee)
        {
            Timer += Time.deltaTime;

            if (Timer >= TimeToAttack)
            {
                Timer = 0;
                Attacking = false;
                AttackArea.SetActive(Attacking);
            }

        }

        if (Input.GetKeyDown(KeyCode.Space) && (dashCount < MaxDashCount) && IsInvulnerable == false)
        {

            CancelInvoke("StartDashCD");

            Dash(MoveDir);
            StartCoroutine(DashFeedback());
            //play sound
            audioSource.Play();
            Debug.Log("Player Dashed");
            //play animation
            Animator.SetTrigger("Dash");

            dashCount++;
            Debug.Log(dashCount);
            //reset and start dash cooldown timer
            Invoke("StartDashCD", dashCoolDownTime);

            return;
        }

        if (IsInvulnerable)
        {
            Invoke("BecomeVulnerable", 0.5f);
            return;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Move();


    }




    public override void Move()
    {
        if (IsStunned)
        {

            Invoke("UnStunned", 0.5f);
            return;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");





        //if looking right and clicked left(flip to the left)
        //if (faceRight && horizontalInput < 0)
        if (AimDir.x < 0)
        {
            transform.localScale = new Vector3(-(Mathf.Abs(transform.localScale.x)), transform.localScale.y, transform.localScale.z);

            faceRight = false;


        }
        //if looking left and click right(flip to the right)
        //else if (!faceRight && horizontalInput > 0)
        else if (AimDir.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            faceRight = true;


        }

        //line 69 also got animation variable
        Animator.SetFloat("playerDir", (int)Math.Ceiling(AimDir.x));
        Animator.SetInteger("xVelocity", (int)Rb.velocity.magnitude);

        //Debug.Log(AimDir.x);

        MoveDir = new Vector2(horizontalInput, verticalInput).normalized;
        Rb.velocity = MoveDir * MoveSpeed;

    }



    public override void Attack(Vector2 aimDir)
    {
        if (isMelee)
        {
            //if using range weapons then need to use attackDr
            Animator.SetTrigger("Attack");

            Debug.Log("Attack Anim triggered");

            //List<Collider2D> enemyColliders = equippedWeapon.GetEnemyCollider(equippedWeapon.AttackCollider);




            Attacking = true;
            AttackArea.SetActive(Attacking);
            Debug.Log("Attacking");
            //Debug.Log("Enemy layermask = "+LayerMask.NameToLayer("Enemy"));
        }
        else
        {
            StartCoroutine(ShootBurst());
        }

    }

    public void StartDashCD()
    {
        dashCount = 0;
        Debug.Log(dashCount);
    }

    public void HitFeedback()
    {
        StartCoroutine(StartBlinking());
    }

    IEnumerator StartBlinking()
    {
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(iFrameDuration / 9f);
        this.GetComponent<SpriteRenderer>().color = Color.white;
    }
}

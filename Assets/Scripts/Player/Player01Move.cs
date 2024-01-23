using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;
using Unity.Collections;

public class Player01Move : NetworkBehaviour
{
    public static Animator anim;
    private Rigidbody2D body;
    public static NetworkAnimator netanim;

    public AnimatorStateInfo Player01Layer0;

    private ulong ownerClientId;
    public static float speed;
    public static float fps;
    public static float jump;
    public static float moveHorizontal;
    public static float moveVertical;
    private float lastPressFire2 = 0.0f;
    private float lastPressFire3 = 0.0f;
    private float timeAllowedToChain = 0.5f;
    public static bool Hits = false;
    public bool isGrounded;
    public static bool isBlocking = false;
    public static bool isParrying = false;

    private bool SlideAttacking = false;
    public float SlideAttackAmt = 50f;

    private bool KamikazeeAttacking = false;
    public float KamikazeeAttackAmt = 0.1f;

    private bool KickDownAttacking = false;

    private bool isFacingRight;
    private bool isFacingLeft;

    public static bool isMovingForward;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    // Start is called before the first frame update
    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = 
                HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            PlayerName.Value = userData.userName;
        }

        if (!IsOwner) return;
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        netanim = GetComponent<NetworkAnimator>();

        speed = 0.1f;
        fps = 60f;
        jump = 0.3f;
        isMovingForward = false;
        isFacingRight = true;
        timeAllowedToChain = 0.5f;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
        GameObject closestTarget = FindClosestTarget(networkObjects);
        if (closestTarget != null)
        {
            Vector2 targetPos = closestTarget.transform.position;

            if (targetPos.x > transform.position.x)
            {
                //Debug.Log("Player is facing Right");
                StartCoroutine(RightisTrue());
                isFacingRight = true;
                isFacingLeft = false;
            }
            if (targetPos.x < transform.position.x)
            {
                //Debug.Log("Player is facing left");
                StartCoroutine(LeftisTrue());
                isFacingLeft = true;
                isFacingRight = false;
            }
        }
        else
        {
            Debug.Log("no client spawned");
        }
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        Player01Layer0 = anim.GetCurrentAnimatorStateInfo(0);

        P1Actions();
    }

    GameObject FindClosestTarget(NetworkObject[] networkObjects)
    {
        GameObject closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var networkObject in networkObjects)
        {
            if (networkObject != null && networkObject != GetComponent<NetworkObject>())
            {
                float distance = Vector2.Distance(transform.position, networkObject.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = networkObject.gameObject;
                }
            }
        }

        return closestTarget;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        P1Move();
    }

    public void P1MoveForward()
    {
        isMovingForward = true;
        Debug.Log(isMovingForward);
    }

    public void P1Move()
    {
        if (Player01Layer0.IsTag("Motion") || Player01Layer0.IsTag("MotionJumping"))
        {

            if (moveHorizontal > 0 || HealthDisplay.isMovingForward == true)
            {
                moveHorizontal = 1;
                anim.SetBool("RunForward", true);
                if (isFacingRight == true)
                {
                    transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                }
                if (isFacingLeft == true)
                {
                    transform.Translate(fps * new Vector2(-speed, 0f) * Time.deltaTime);
                    isBlocking = true;
                }

            }

            if (moveHorizontal < 0 || HealthDisplay.isMovingBackward == true)
            {
                moveHorizontal = -1;
                anim.SetBool("RunBackward", true);
                if (isFacingRight == true)
                {
                    transform.Translate(fps * new Vector2(-speed, 0f) * Time.deltaTime);
                    isBlocking = true;
                }
                if (isFacingLeft == true)
                {
                    transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                }


            }

            if (moveHorizontal == 0)
            {
                moveHorizontal = 0;
                anim.SetBool("RunForward", false);
                anim.SetBool("RunBackward", false);
                isBlocking = false;
            }

            if (isGrounded == true)
            {
                if (moveVertical > 0 || HealthDisplay.isMovingUp == true)
                {
                    netanim.SetTrigger("Jump");
                    body.AddForce(new Vector2(body.velocity.x, fps * jump), ForceMode2D.Impulse);
                    isGrounded = false;
                    Debug.Log("Grounded is " + isGrounded);
                }
            }

            if (moveVertical < 0 || HealthDisplay.isMovingDown == true)
            {
                anim.SetBool("Dodge", true);
            }
        }

        if (Player01Layer0.IsTag("Dodge"))
        {
            if (moveVertical < 0 || HealthDisplay.isMovingDown == true)
            {
                anim.SetBool("Dodge", true);
            }
            if (moveVertical == 0 && HealthDisplay.isMovingDown != true)
            {
                anim.SetBool("Dodge", false);
                HealthDisplay.isMovingDown = false;
            }
        }

    }

    public void P1Actions()
    {
        if (Health.isDead == true)
        {
            anim.SetBool("IsDead", true);
        }
        if (SlideAttacking == true)
        {
            body.transform.Translate(SlideAttackAmt * Time.deltaTime, 0, 0);
        }
        if (KamikazeeAttacking == true)
        {
            body.transform.Translate(fps * speed * Time.deltaTime, -KamikazeeAttackAmt * Time.deltaTime, 0);
        }

        if (KickDownAttacking == true)
        {
            body.transform.Translate(0, -KamikazeeAttackAmt * Time.deltaTime, 0);
        }
        if (Player01Layer0.IsTag("Motion") || Player01Layer0.IsTag("MotionJumping"))
        {
            if (Input.GetButtonDown("Fire1") || HealthDisplay.isLeftJab == true)
            {
                lastPressFire2 = Time.time;
                netanim.SetTrigger("LeftJab");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
                HealthDisplay.isLeftJab = false;
                StartCoroutine(LeftJabFalse());
            }

            if (Input.GetButtonDown("Fire2") || HealthDisplay.isRightJab == true)
            {
                netanim.SetTrigger("RightJab");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
                HealthDisplay.isRightJab = false;
                StartCoroutine(RightJabFalse());
            }

            if (Input.GetButtonDown("Fire3") || HealthDisplay.isLeftKick == true)
            {
                lastPressFire3 = Time.time;
                netanim.SetTrigger("LeftKick");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
                HealthDisplay.isLeftKick = false;
                StartCoroutine(LeftKickFalse());
            }

            if (Input.GetButtonDown("Jump") || HealthDisplay.isRightKick == true)
            {
                netanim.SetTrigger("RightKick");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
                HealthDisplay.isRightKick = false;
                StartCoroutine(RightKickFalse());
            }

            if (Input.GetButtonDown("Parry") || HealthDisplay.isParry == true)
            {
                netanim.SetTrigger("Parry");
                isParrying = true;
                HealthDisplay.isParry = false;
                StartCoroutine(ParryFalse());
            }
            else
            {
                isParrying = false;
            }

            if (Input.GetButtonDown("Combo") || HealthDisplay.isCombo == true)
            {
                netanim.SetTrigger("Combo");
                Hits = false;
                HealthDisplay.isCombo = false;
            }

            if ((Time.time <= (lastPressFire3 + timeAllowedToChain)))
            {
                if (Input.GetButtonDown("Jump") || HealthDisplay.isRightKick == true && (Time.time <= (lastPressFire3 + timeAllowedToChain)))
                {
                    netanim.SetTrigger("RandLFeetAttack");
                    Hits = false;
                    HealthDisplay.isRightKick = false;
                }
            }
            
        }

        if (Player01Layer0.IsTag("Attack"))
        {
            if (Input.GetButtonDown("Fire2"))
            {
                netanim.SetTrigger("RightJab");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
            }

            if (Input.GetButtonDown("Jump"))
            {
                netanim.SetTrigger("RightKick");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
            }

            if (Input.GetButtonDown("Jump") || HealthDisplay.isRightJab == true && (Time.time <= (lastPressFire3 + timeAllowedToChain)))
            {
                netanim.SetTrigger("RandLFeetAttack");
                Hits = false;
                HealthDisplay.isRightJab = false;
            }

            if (Input.GetButtonDown("Fire2") && (Time.time <= (lastPressFire2 + timeAllowedToChain)))
            {
                netanim.SetTrigger("GrabFeetAttack");
                Hits = false;
            }
        }

        if (Player01Layer0.IsTag("Dodge"))
        {
            if (Input.GetButtonDown("Fire1") || HealthDisplay.isLeftJab == true)
            {
                netanim.SetTrigger("LeftJab");
                HealthDisplay.isLeftJab = false;
                if (isFacingRight == true)
                {
                    body.AddForce(new Vector2(fps * 0.1f, fps * jump), ForceMode2D.Impulse);
                }
                if (isFacingLeft == true)
                {
                    body.AddForce(new Vector2(fps * -0.1f, fps * jump), ForceMode2D.Impulse);
                }

                Hits = false;
            }

            if (Input.GetButtonDown("Fire2") || HealthDisplay.isRightJab == true)
            {
                netanim.SetTrigger("RightJab");
                HealthDisplay.isRightJab = false;
                if (isFacingRight == true)
                {
                    body.AddForce(new Vector2(fps * 0.1f, fps * jump), ForceMode2D.Impulse);
                }
                if (isFacingLeft == true)
                {
                    body.AddForce(new Vector2(fps * -0.1f, fps * jump), ForceMode2D.Impulse);
                }

                Hits = false;
            }

            if (Input.GetButtonDown("Fire3") || HealthDisplay.isLeftKick == true)
            {
                netanim.SetTrigger("LeftKick");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
                HealthDisplay.isLeftKick = false;
            }
        }

        if (Player01Layer0.IsTag("MotionJumping"))
        {
            if (Input.GetButtonDown("Fire1") || HealthDisplay.isLeftJab == true)
            {
                netanim.SetTrigger("LeftJab");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
            }

            if (Input.GetButtonDown("Fire3") || HealthDisplay.isLeftKick == true)
            {
                netanim.SetTrigger("LeftKick");
                body.transform.Translate(fps * new Vector2(speed, 0f) * Time.deltaTime);
                Hits = false;
            }
        }

    }

    public void SlideAttack()
    {
        StartCoroutine(AttackSlide());
    }
    public void KamikazeeAttack()
    {
        StartCoroutine(KamikazeeDown());
    }
    public void KickDownAttack()
    {
        StartCoroutine(KickDowns());
    }

    IEnumerator AttackSlide()
    {
        SlideAttacking = true;
        yield return new WaitForSeconds(0.1f);
        SlideAttacking = false;
    }
    IEnumerator KamikazeeDown()
    {
        KamikazeeAttacking = true;
        yield return new WaitForSeconds(0.1f);
        KamikazeeAttacking = false;
    }
    IEnumerator KickDowns()
    {
        KickDownAttacking = true;
        yield return new WaitForSeconds(0.1f);
        KickDownAttacking = false;
    }

    IEnumerator LeftJabFalse()
    {
        yield return new WaitForSeconds(0.1f);
        HealthDisplay.isLeftJab = false;
    }

    IEnumerator RightJabFalse()
    {
        yield return new WaitForSeconds(0.1f);
        HealthDisplay.isRightJab = false;
    }

    IEnumerator LeftKickFalse()
    {
        yield return new WaitForSeconds(0.1f);
        HealthDisplay.isLeftKick = false;
    }

    IEnumerator RightKickFalse()
    {
        yield return new WaitForSeconds(0.1f);
        HealthDisplay.isRightKick = false;
    }

    IEnumerator ParryFalse()
    {
        yield return new WaitForSeconds(0.1f);
        HealthDisplay.isParry = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Grounded is " + isGrounded);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.attachedRigidbody == null) return;
        if (collision.gameObject.CompareTag("P1LeftJab"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hits");
            }
        }

        if (collision.gameObject.CompareTag("P1RightJab"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
            }

        }

        if (collision.gameObject.CompareTag("P1LeftKick"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hits");
            }

        }

        if (collision.gameObject.CompareTag("P1RightKick"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
            }

        }

        if (collision.gameObject.CompareTag("P1SlideAttack"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
            }

        }

        if (collision.gameObject.CompareTag("P1Kamikazee"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
                StartCoroutine(KamikazeeGrab());
            }
        }

        if (collision.gameObject.CompareTag("P1Kickdown"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
            }

        }

        if (collision.gameObject.CompareTag("P1UppercutLeft"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
                body.AddForce(new Vector2(fps * 0.1f, fps * jump), ForceMode2D.Impulse);
            }

        }

        if (collision.gameObject.CompareTag("P1UppercutRight"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hitdown");
                body.AddForce(new Vector2(fps * 0.1f, fps * jump), ForceMode2D.Impulse);
            }

        }

        if (collision.gameObject.CompareTag("P1GrabAttack"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hits");
            }

        }

        if (collision.gameObject.CompareTag("P1Combo"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hits");
                StartCoroutine(Takedown());
            }

        }

        if (collision.gameObject.CompareTag("P1RandLFeetAttack"))
        {
            if (isBlocking == true)
            {
                netanim.SetTrigger("Block");
            }
            else
            {
                netanim.SetTrigger("Hits");
                StartCoroutine(Takedown());
            }

        }
    }
    IEnumerator KamikazeeGrab()
    {
        yield return new WaitForSeconds(0.2f);
        netanim.SetTrigger("KamikazeeGrab");
    }

    IEnumerator Takedown()
    {
        yield return new WaitForSeconds(0.2f);
        netanim.SetTrigger("Hitdown");
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator LeftisTrue()
    {
        yield return new WaitForSeconds(0.15f);
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    IEnumerator RightisTrue()
    {
        yield return new WaitForSeconds(0.15f);
        transform.eulerAngles = new Vector3(0, 0, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;

public class Player : MonoBehaviour
{   
    public float speed;
    public GameObject[] weapon; 
    public bool[] hasWeapon;
    public GameObject[] grenades;
    public GameObject grenadeObject;
    public GameObject bar;
    public GameManager manager;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int hasGrenades;
    public Camera followCamara;
    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool altDown;

    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool cant;
    bool isDamage;
    bool isOn = true;
    public bool isShop;
    bool isDead;
    bool isDot;


    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;

    int equipWeaponIndex=-1;
    float fireDelay;

    void Awake()
    {
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        //PlayerPrefs.SetInt("MaxScore", 112500);
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();
        Ontab();
        
        
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        altDown = Input.GetButtonDown("Cancel");

    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || (isReload && !cant) || isDead)
            moveVec = Vector3.zero;
        
        if(!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.2f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn()
    {
        
        transform.LookAt(transform.position + moveVec);
           
            if (fDown && !isDead)
            {
                Ray ray = followCamara.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
                {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; 
                transform.LookAt(transform.position + nextVec);
                }
            }
        
        
    }

    void Jump()
    {
        if (jDown && !isJump && moveVec == Vector3.zero && !isDodge && !isSwap && !isShop && !isDead)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            if (isReload)
            {

                cant = true;
            }
            isJump = true;
            
        }
    }
    void Grenade()
    {
        if (hasGrenades == 0)
            return;
        if(gDown && !isReload && !isSwap && !isShop && !isDead)
        {
            Ray ray = followCamara.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObject, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;

        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isJump && !isShop && !isDead)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
     }
    void Reload()
    {
        
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && equipWeapon.curAmmo != equipWeapon.maxAmmo && !isReload && !isShop && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            
            Invoke("ReloadOut", 1f);
            
        }
    }
    void ReloadOut()
    {
        if (cant)
        {
            isReload = false;
            return;
        }
        int reAmmo = ammo < equipWeapon.maxAmmo - equipWeapon.curAmmo ? ammo : equipWeapon.maxAmmo - equipWeapon.curAmmo;
            equipWeapon.curAmmo = equipWeapon.maxAmmo;
            ammo -= reAmmo;
            isReload = false;
        
    }
    void Dodge()
    {
        if (jDown && !isJump && moveVec != Vector3.zero && !isDodge && !isSwap && !isShop && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        cant = false;
        isDodge = false;
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapon[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapon[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapon[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isShop && !isDead)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapon[weaponIndex].GetComponent<Weapon>() ;
            weapon[weaponIndex].gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }
    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge && !isShop && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapon[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            { 
                
                Shop shop = nearObject.GetComponent<Shop>();
                
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
       // rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
       
    }
    void StoptoWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 3, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StoptoWall();
        if (isDamage)
        {
            
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            cant = false;
            isJump = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                        ammo = maxammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                        coin = maxcoin;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxhasGrenades)
                        hasGrenades = maxhasGrenades -1;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                        hasGrenades = maxhasGrenades;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                        health = maxhealth;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            bool isBossAtk = other.name == "BossMeleeArea";
            
            if (!isDamage)
            {
            Bullet enemyBullet = other.GetComponent<Bullet>();
                
            health -= enemyBullet.damage;
            StartCoroutine(OnDamage(isBossAtk));
            }
            else
            {
                if (isBossAtk)
                {
                    moveVec = Vector3.zero;
                    rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
                    Invoke("BossOut", 1f);
                }
            }
            
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);

        }
       
    }

    

    void BossOut()
    {
        rigid.velocity = Vector3.zero;

    }

    IEnumerator DotDamage()
    {
        isDot = true;
        health -= 2;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.5f);
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        isDot = false;
    }

    IEnumerator OnDamage(bool isbossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        if (isbossAtk)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        if (isbossAtk)
        {
           
            rigid.velocity= Vector3.zero;
        }
        if(health <= 0 && !isDead)
        {
            OnDie();
        }
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon"|| other.tag == "Shop")
            nearObject = other.gameObject;

        if(other.tag == "DotDamage" && !isDot)
        {
            StartCoroutine(DotDamage());
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            nearObject = null;
            isShop = false;
        }

    }
    void Ontab()
    {
        if (altDown)
        {
            if (isOn)
            {
                bar.SetActive(false);
                isOn = false;
            }
            else
            {
                bar.SetActive(true);
                isOn = true;
            }
        }
    }
}

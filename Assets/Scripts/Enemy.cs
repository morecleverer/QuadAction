using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    public enum Type { A, B, C, D };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;

    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;

     void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
               nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
            
    }
    void Targeting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0f;
            float targetRange = 0f;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(0.7f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2f);

                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);

                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);

    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }
    void FreezeVelocity()
    {
        if (isChase)
        {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            Vector3 reactVec = transform.position - other.transform.position;
            curHealth -= weapon.damage;

            StartCoroutine(OnDamage(reactVec, false));

        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            Vector3 reactVec = transform.position - other.transform.position;
            curHealth -= bullet.damage;
            StartCoroutine(OnDamage(reactVec, false));
            Destroy(other.gameObject);
            
        }
    }

    public void HiBybyGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));

    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        if (isGrenade)
        {
            reactVec = reactVec.normalized;
            reactVec += Vector3.up*2;

            rigid.AddForce(reactVec * 5, ForceMode.Impulse);
        }
        

        if(curHealth > 0)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            if(!isDead)
                Die();
            isDead = true;

            if (isGrenade && enemyType != Type.D)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up *3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 10, ForceMode.Impulse);
            }
            else if(enemyType != Type.D)
            {
            reactVec = reactVec.normalized;
            reactVec += Vector3.up;

            rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            else
            {
                rigid.velocity = Vector3.zero;
            }
            if(manager.isBattle = false || enemyType == Type.D)
            {
                Object[] objects = GameObject.FindGameObjectsWithTag("EnemyBullet");
                foreach (GameObject obj in objects)
                    Destroy(obj);
            }

            
                Destroy(gameObject, 4);
        }
    }
    void Die()
    {
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.gray;
        Debug.Log("1");
        gameObject.layer = 14;
        nav.enabled = false;
        isChase = false;

        anim.SetTrigger("doDie");
        Player player = target.GetComponent<Player>();
        player.score += score;

        if(enemyType != Type.D)
        {
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);
        }
        else
        {
            for(int i=0; i< manager.stage/5 + 1; i++)
            {
                Instantiate(coins[1], transform.position, Quaternion.identity);

            }
            Instantiate(coins[2], transform.position, Quaternion.identity);
            Instantiate(coins[2], transform.position, Quaternion.identity);
            Instantiate(coins[2], transform.position, Quaternion.identity);

        }

        switch (enemyType)
        {
            case Type.A:
                manager.enemyCntA--;
                Debug.Log("a");

                break;
            case Type.B:
                manager.enemyCntB--;
                Debug.Log("b");

                break;
            case Type.C:
                manager.enemyCntC--;
                Debug.Log("c");

                break;
            case Type.D:
                manager.enemyCntD--;
                Debug.Log("a");

                break;
        }
    }
}

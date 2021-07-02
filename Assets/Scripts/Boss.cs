using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public GameObject Fire;
    public GameObject boom;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook;

    Vector3 lookVec;
    Vector3 tauntVec;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            return;
        }
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            Vector3 tarvec = new Vector3(target.position.x, 0, target.position.z);
            transform.LookAt(tarvec + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }
    IEnumerator Think()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);
        switch (ranAction)
        {
            case 0:
                
            case 1:
               StartCoroutine(MissileShot());
               break;
            case 2:

            case 3:
                StartCoroutine(RockShot());

                break;
            case 4:
                StartCoroutine(Taunt());

                break;
        }
    }
    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantmissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantmissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;
        yield return new WaitForSeconds(0.3f);
        GameObject instantmissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantmissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;
        rigid.velocity = Vector3.zero;
        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }
    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
       rigid.velocity = Vector3.zero;

        yield return new WaitForSeconds(3f);
        isLook = true;
        StartCoroutine(Think());

    }
    IEnumerator Taunt()
    {
        Fire.SetActive(false);
        tauntVec = target.position + lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");
        rigid.velocity = Vector3.zero;

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        rigid.velocity = Vector3.zero;
        boom.SetActive(true);
        yield return new WaitForSeconds(0.4f);

        boxCollider.enabled = true;
        
        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = false;
        rigid.velocity = Vector3.zero;
        Fire.SetActive(true);

        yield return new WaitForSeconds(1f);
        rigid.velocity = Vector3.zero;
        boom.SetActive(false);
        isLook = true;
        nav.isStopped = true;


        StartCoroutine(Think());

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG;
using DG.Tweening;

public class Shop : MonoBehaviour
{

    public GameObject uiGroup;
    public Animator anim;

    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public string[] talkData;
    public Text talkText;

    Player enterPlayer;

    // Start is called before the first frame update
    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.transform.DOLocalMoveY(0, 0.5f, false).SetEase(Ease.OutBack);
        
    }

    // Update is called once per frame
    public void Exit()
    {
        if (uiGroup.transform.localPosition != new Vector3(0, 0))
            return;
        anim.SetTrigger("doHello");
        uiGroup.transform.DOLocalMoveY(-1000, 0.5f, false).SetEase(Ease.InBack);
        //enterPlayer.isShop = false;

    }
    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.coin)
        {
            Debug.Log("a");
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }
        else
        {
            Debug.Log("b");
        }

        enterPlayer.coin -= price;

        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);

        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
    }
    IEnumerator Talk()
    {
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];

    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletYellow : Bullet
{
    public BoxCollider2D boxCollider;

    private int moveAble = 1;

    // Start is called before the first frame update
    void Start()
    {
        base.damage = 10;
        base.speed= 5;

        Destroy(gameObject, 10);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(base.dir * speed * Time.deltaTime * moveAble);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") changeToMine(); 
        if (!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerScript>().Hit(damage, base.player);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    void changeToMine()
    {
        base.damage = 35;

        moveAble = 0;

        transform.localScale= Vector2.one * 1.5f;
        boxCollider.size = boxCollider.size * 1.6f; 
    }
}

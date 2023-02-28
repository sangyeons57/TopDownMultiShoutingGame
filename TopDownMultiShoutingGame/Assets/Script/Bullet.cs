using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks , IPunObservable
{
    public PhotonView pv;
    Vector2 dir;

    private int damage = 10;
    private int speed = 7;

    private GameObject player;

    // Start is called before the first frame update
    private void Start() => Destroy(gameObject, 3.5f);

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * 7 * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        if (!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerScript>().Hit(damage, player);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    [PunRPC]
    public void settingRPC(string nickName)
    {
        if (pv.IsMine) dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        this.player = GameObject.Find(nickName);
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (pv.IsMine)
        {
            stream.SendNext(dir);
        }
        else
        {
            dir = (Vector2)stream.ReceiveNext();
        }
    }
}

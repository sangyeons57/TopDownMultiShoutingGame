using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour, IPunObservable
{
    public PhotonView pv;
    public SpriteRenderer sr;
    public TMP_Text nicknameText;
    public Image healthImage;
    public Rigidbody2D rb;

    public bool isGround;
    private Vector3 curPos;

    public int playerPoint { get; set; } = 0;

    private void Awake()
    {
        //자기 자신이면 photonNetwork에 저장했던 닉넴임 아니면 해당 플레이어의 닉네임
        nicknameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;

        nicknameText.color = pv.IsMine ? Color.green : Color.red;

    }

    private void Start()
    {
        if (pv.IsMine)
            transform.tag = "Player";
        else
            transform.tag = "Enemy";
        if (pv.IsMine) foreach (GameObject go in GameObject.FindGameObjectsWithTag("Bullet")) Destroy(go);
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            //좌우이동
            float axis = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);

            //점프
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            if (isGround && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))) pv.RPC("JumpRPC", RpcTarget.All);

            //총알 발사
            if (Input.GetMouseButtonDown(0))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.identity)
                    .GetComponent<PhotonView>().RPC("settingRPC", RpcTarget.All);
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);


    }

    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }

    public void Hit(int damage, GameObject hittedBy)
    {
        Debug.Log($"Hit : {(float)damage / 100} ");
        healthImage.fillAmount -= (float)damage / 100;
        if (healthImage.fillAmount <=0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPannel").gameObject.SetActive(true);

            hittedBy.GetComponent<PhotonView>().RPC("playerPointPlusRPC",RpcTarget.All, 1);
            GameObject.Find("Canvas").transform.Find("GamePlayPanel").gameObject.SetActive(false);


            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void playerPointPlusRPC(int addnum)
    {
        if (pv.IsMine) playerPoint += addnum;
        Debug.Log(playerPoint);
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
            stream.SendNext(playerPoint);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
            playerPoint = (int)stream.ReceiveNext();
        }
    }
}

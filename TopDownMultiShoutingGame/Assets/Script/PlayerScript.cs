using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour, IPunObservable
{
    private CinemachineVirtualCamera m_Camera;

    private PhotonView pv;
    private TMP_Text nicknameText;
    private Image healthImage;
    private Rigidbody2D rb;

    private bool isGround;
    private Vector3 curPos;

    public float playerSpeed = 4;
    public int jumpSize = 300;
    public int health = 100;
    public float reloadingSpeed = 0.0f;
    public string bulletName = "Bullet";
    public int bulletNumber = 1;

    private bool isReload = true;

    private void Awake()
    {
        gameObject.transform.localScale= Vector3.one * 0.6f;
    }

    private void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
        nicknameText = gameObject.transform.Find("playerInfoCanvas").transform.Find("NickName").GetComponent<TMP_Text>();
        healthImage = gameObject.transform.Find("playerInfoCanvas").transform.Find("health").GetComponent<Image>();
        rb = gameObject.GetComponent<Rigidbody2D>();

        if (pv.IsMine)
        {
            m_Camera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            m_Camera.LookAt = transform;
            m_Camera.Follow = transform;
        }

        setName(PhotonNetwork.NickName);
    }

    private void OnEnable()
    {
        Invoke("bulletDelete", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            //움직임
            moveSetting(playerSpeed);

            //총알 발사
            if (Input.GetMouseButtonDown(0) && isReload) shooting();
        }
        else if (isOverDistance(100)) overDistanceCorrection();
        else posCorrection();
    }

    private bool isOverDistance(int distance) => (transform.position - curPos).sqrMagnitude >= distance;
    private void overDistanceCorrection() => transform.position = curPos;

    private void posCorrection() => transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);

    private void shooting()
    {
        isReload= false;
        for (int i = 0;i < bulletNumber; i++) 
            PhotonNetwork.Instantiate(bulletName, transform.position, Quaternion.identity)
                .GetComponent<PhotonView>().RPC("settingRPC", RpcTarget.All, PhotonNetwork.NickName, i);

        Invoke("reload", reloadingSpeed);
    }

    private void reload() => isReload= true;

    private void moveSetting(float speed)
    {
        //좌우이동
        float axis = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(speed * axis, rb.velocity.y);

        //점프
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
        if (isGround && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))) pv.RPC("JumpRPC", RpcTarget.All);
    }


    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpSize);
    }

    public float Hit(int damage, GameObject hittedBy)
    {
        healthImage.fillAmount -= (float)damage / health;
        if (healthImage.fillAmount <=0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPannel").gameObject.SetActive(true);

            hittedBy.GetComponent<PhotonView>().RPC("playerPointPlusRPC",RpcTarget.AllBuffered, 1);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }

        return healthImage.fillAmount;
    }

    [PunRPC]
    public void playerPointPlusRPC (int point)
    {
        if (pv.IsMine) GameObject.Find("Canvas").transform.Find("GamePlayPanel").GetComponent<GamePlayPanel>()
                .addPoint(PhotonNetwork.NickName, point);
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        }
        else 
        {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    private void bulletDelete()
    {
        if (pv.IsMine) foreach (GameObject deleteBullet in GameObject.FindGameObjectsWithTag("Bullet")) Destroy(deleteBullet);
    }

    private void setName(string name)
    {
        //자기 자신이면 photonNetwork에 저장했던 닉넴임 아니면 해당 플레이어의 닉네임
        nicknameText.text = pv.IsMine ? name : pv.Owner.NickName;
        transform.name = nicknameText.text;
        nicknameText.color = pv.IsMine ? Color.green : Color.red;
    }
}

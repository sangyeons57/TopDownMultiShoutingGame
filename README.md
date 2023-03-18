TopDownMultiShoutingGame
=========================

<span style = "font-size:18px">
1멀티플레이 네트워킹 연습
</span>

---
Class PlayerScript
------------------

- 플레이어의 기본조작을 구현한 함수

### Start() 

#### 카메라 Fllow설정

``` CSharp
if (pv.IsMine)
{
    m_Camera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
    m_Camera.LookAt = transform;
    m_Camera.Follow = transform;
}
```

- plyaer에 부여된 photonview가 내거인경우
  - camera가 따라다니게 설정

### void: moveSetting(float) 
``` CSharp
private void moveSetting(float speed)
{
    //좌우이동
    float axis = Input.GetAxisRaw("Horizontal");
    rb.velocity = new Vector2(speed * axis, rb.velocity.y);

    //점프
    isGround = Physics2D.OverlapCircle((Vector2)transform.position 
        + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
    if (isGround && (Input.GetKeyDown(KeyCode.W) ||
        Input.GetKeyDown(KeyCode.UpArrow))) 
        pv.RPC("JumpRPC", RpcTarget.All);
}
```
- 좌우이동
- physics2D.OverlapCircle로 충돌체를 만들어서 
  - (위치, 크기, 조건레이어) 입력해서 
  - 조건레이어에 적합하면 실행
    - 조건레이어는 1 2 4 8 16 32등 2의 배수만 가능
    - 사용방법도 1 "<<" 이걸로 비트연산자를 사용하기 때문이다. 



### void: shooting() 

```CSharp
private void shooting()
{
    isReload= false;
    for (int i = 0;i < bulletNumber; i++) 
        PhotonNetwork.Instantiate(bulletName, transform.position, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("settingRPC", RpcTarget.All, PhotonNetwork.NickName, i);

    Invoke("reload", reloadingSpeed);
}
```
- 장전상태를 비활성화 시키고
- 총알수 만큼 모든 클라이언트에 총알을 생성한다
  - 그러고나서 총알에 플레이어닉네임과 한번에 발사되는 몇번째 총알인지 설정한다
- reolaodSpeed초 후 재장전 한다.


### void: posCorrection()

위치보정
```CSharp
private void posCorrection() => transform.position =
    Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
```


### float: Hit(int, GameObject)

플레이거 맞았는지 확인하는 함수
``` CSharp
public float Hit(int damage, GameObject hittedBy)
{
    healthImage.fillAmount -= (float)damage / health;
    if (healthImage.fillAmount <=0)
    {
        GameObject.Find("Canvas").transform
            .Find("RespawnPannel").gameObject.SetActive(true);

        hittedBy.GetComponent<PhotonView>().RPC("playerPointPlusRPC",RpcTarget.AllBuffered, 1);
        pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    return healthImage.fillAmount;
}
```

- 체력바를 damage만큼 감소시킨다
- 체력바가 0보다 작으면 리스폰상태로 가고
- hittedBy로 들어온 "죽인" 플레이어에게 점수를 준다
- 죽임을 당한 오브젝트를 모든 클라이언트에서 제거한다.

<br><br>

---
Class Bullet
------------------

### void: OnTriggerEnter2D(Collider2D)
```CSharp
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
    if (!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
    {
        collision.GetComponent<PlayerScript>().Hit(damage, player);
        pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }
}
```
- 무언가 부딛친경우 불리는 함수
- plaform인 경우 총알 오브젝트 파괴
- 자기자신의 총알아니다 && 부딛친것이 플레이어이다 && photonView가 존재한다면
  - PlayerScript에 Hit를 발생시킨다
  - 그리고 총알을 모든 클라이언트에서 삭제

<br>
BulletGreen, BulletYellow, BulletWhite은 Bullet에서 큰변화가없는 것은 문서로 안 만들었다


Class BulletBlue : Bullet
----------------

산탄총으로 Bullet을 상속받음

``` CSharp
angle = Mathf.Atan2(base.dir.y, base.dir.x) * Mathf.Rad2Deg + shotgunDir[base.bulletNum];
angle *= Mathf.Deg2Rad;

base.dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
pv.RPC("syncDir", RpcTarget.AllBuffered , base.dir);
```

- 플레이어에서 플레이어가 클릭한 방향에대한 방향값을 변형시킨다
- 방향의 좌표값을 vector2 > radian > degree 로 변형시킨다
- 이미 설정되있는 변형값을 angle(degree) 에 더한다
- 다시 radian > vector2.nomalized로 변형시킨다
- 방향값을 다른 크라이언트와 싱크시킨다

Class BulletGreen : Bullet
----------------

### Void: Update()
```CSharp
void Update()
{
    if ( target != null)
    {
        Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rb.angularVelocity = -rotateAmount * rotateSpeed;
    }
    rb.velocity = transform.up * speed;
}
```
- 타겟이 존재하는경우
    - 타겟의 상대좌표를 구한고 nomalized한다
    - 타겟의 방향으로 회전시킨다
- 직진한다

### void: setDetect()
```CSharp
private void setDetect()
{
    detect.Clear();
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        if (obj.transform.name != playerNickname)
            detect.Add(obj, Vector2.Distance(transform.position, obj.transform.position));

    if (detect.Count == 0) return ;

    detect = (from pair in detect
                orderby pair.Key descending
                select pair)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

    setTarget(detectDistance);
}
```
- Dictionary detect를 클리어한다
- player 오브젝를 전부 반복시킨다 
  - 자신의 총알이 인경우를 빼고
    - (오브젝트, 위치값) 으로 저장한다 
- 아무것도 detect되지 않은 경우 이함수에서 나간다 
- key값을 기준으로 내림차순으로 정렬한다
- 가장가까운 타겟으로 방향으로 설정한다



class BulletMit : Bullet >> class explosionCircle
---------------------------

### void: OnTriggerEnter2D(Collider2D)
```CSharp
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
    {
        collision.GetComponent<PlayerScript>().Hit(damage, player);
        Debug.Log("collision pv.isMine");
    }
    else if (pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
    {
        Debug.Log("collision pv.isMine");
    }
}
```

- 폭팔범위에 닿은경우
- 닿은것이 player인 경우 && 워본 클라이언트에서 발생한경우(중복발생 제거)
  - hit 발생
- 원보 클라이언트 && 닿은거싱 Player인 경우 &&  닿은것의  해당 플레이어가 내가조정하고 있는 클라이언트에 캐릭터인 경우
  - (현재사용안함)



<br><br>

class GamePlayPanel
---------------------


### void: updateScore()
```CSharp
//플레이어에게 보여주는 score창 업데이트
void updateScore()
{
    //score구 데이터 삭제
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Score")) 
        Destroy(obj);

    //플레이어수가 server에 수와 현제 표현하고 있는 수가 같은지 확인하고
    //아닐경우 수정함
    checkPlayerNumber();
    //사전 정렬
    var sortingDict = from pair in playerPointNote
                orderby pair.Value descending
                select pair;

    //정렬된거가지고 표시
    int counter = 0;
    foreach (KeyValuePair<string,int> pair in sortingDict)
    {
        GameObject obj = Instantiate(scoreInstance, gameObject.transform);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-75, -25 + (-20) * counter);
        obj.GetComponent<TMP_Text>().text = $"{(int)counter + 1}. {pair.Key} {pair.Value}";

        counter++;
    }
}
```

- 구데이터 제거
- 플레이어수 싱크확인
- 플레이어 점수리스트 점수순으로 정렬
- 위부터하나씩 저기


### void: checkPlayerNumber()
```CSharp
public void checkPlayerNumber()
{
    if (PhotonNetwork.PlayerList.Length != playerPointNote.Count)
    {
        List<string> checkingList = playerPointNote.Keys.ToList();
        //추가된 사람 데이터에 추가
        foreach (var player in PhotonNetwork.PlayerList)
        {
            //확인되 플레이어는list에서 하나씩 빠짐
            checkingList.Remove(player.NickName);
            //있는데 없는경우 추가로 보정
            if (!playerPointNote.ContainsKey(player.NickName)) setPoint(player.NickName, 0);
        }
        //없어진사람 삭제
        foreach (string checkingPerson in checkingList)
        {
            //없는데 있는경우 제거로 보정
            removePointDictEntry(checkingPerson);
        }
    }
}
```

- 서버에있는 사람과 GamePlayPanel이 알고있는 사람수가 다른경우
  - panel이 알고있는 player이름을 모음 (추가, 사람이 더있는경우)
    - 모은것을 하나식 대조
    - 가지고있지 않은 값이 나온 경우 그값을 추가로 저장
  - 확인하지 못값 값이 남아있는경우 제거 (제거, 사람이 없어진 경우)


<br><br>

class NetworkingManager
---------------------


### void: OnJoinedRoom()
```CSharp
public override void OnJoinedRoom()
{
    roomcount = 0;
    //1.사용 불가능한 닉네임을 사용한다
    if (!isableUseThisNickName())
    {
        PhotonNetwork.Disconnect();
        setStausText("NICKNAME is already used \n or NICKNAME is an imposible");
        return;
    }
    //2. 플레이어가 캐릭터을  안하거나
    else if (DisconnectPanel.transform.Find("PlayerSelecter").GetComponent<Playerselecter>()
        .selectedColor == "")
    {
        PhotonNetwork.Disconnect();
        setStausText("select COLOR");
        return;
    }
    // --중 하나라도 안하면 다시하게 한다
    //정상 접속시 텍스트 삭제
    else setStausText("");

    //플레이어 color 세팅
    playerColor = DisconnectPanel.transform.Find("PlayerSelecter").GetComponent<Playerselecter>().selectedColor;

    //페널세팅
    DisconnectPanel.SetActive(false);
    GamePlayPanel.SetActive(true);
    Spawn();
    }

```

- 방찾기초기화
- 사용가능한 닉네임이 아닌 경우
  - disconnect후 상태매세지 리턴
- 캐릭터선택을 안 한 경우
  - discconnect후 상태메시지 리턴
- 정상적일경우 상태메시지 제거
- 캐릭터와 pnael세팅



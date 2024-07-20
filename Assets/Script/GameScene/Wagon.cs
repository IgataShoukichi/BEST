using System.Collections;
using UnityEngine;

public class Wagon : MonoBehaviour
{
    [SerializeField] Transform foodPosition;
    [SerializeField] Transform leftPlayerPosition;
    [SerializeField] Transform rightPlayerPosition;

    Transform thisTransform;
    Rigidbody thisRigidbody;
    PlayerRPC connectedLeftPlayer = null;
    PlayerRPC connectedRightPlayer = null;
    bool moveflag = false;
    bool isCompleted = false;

    void Awake()
    {
        thisTransform = this.transform;
        thisRigidbody = GetComponent<Rigidbody>();
        thisRigidbody.isKinematic = true;

        isCompleted = false;
    }




    void Update()
    {
        if (moveflag && connectedLeftPlayer != null)//左プレイヤーを基準に動く
        {
            thisTransform.position = Vector3.Lerp(connectedLeftPlayer.gameObject.transform.position,
                connectedRightPlayer.gameObject.transform.position, 0.5f);

            Vector3 targetRotation = connectedLeftPlayer.gameObject.transform.position - thisTransform.position;
            float rotation = Mathf.Atan2(targetRotation.x, targetRotation.z) * Mathf.Rad2Deg + 90f;
            thisTransform.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }

    public string SetPossible()
    {
        if(!moveflag && !isCompleted && (connectedLeftPlayer == null || connectedRightPlayer == null))
        {
            return this.gameObject.name;
        }
        else
        {
            return null;
        }
    }

    public void SetFood(Food food)
    {
        GameObject foodPrefab = food.GetFoodModel();//モデルをデータベースから取得
        GameObject createFoodObject = Instantiate(foodPrefab);
        createFoodObject.name = food.GetFoodNumber().ToString();//フード番号
        createFoodObject.transform.SetParent(foodPosition.transform, true);
        createFoodObject.transform.localScale = foodPrefab.transform.localScale / 2;
        createFoodObject.transform.localPosition = Vector3.zero;

        
    }
    
    public void SetTable(string tableName)
    {
        GameObject tableObject = GameVariable.tableList.Find(n => n.name == tableName);
        tableObject.GetComponent<Table>().WagonReady(this.gameObject);
    }

    public bool SetJointPlayer(PlayerRPC playerRPC)
    {
        GameObject playerObject = playerRPC.gameObject;
        //左右判別
        bool leftTarget = Vector3.Distance(playerObject.gameObject.transform.position, leftPlayerPosition.position) < 
            Vector3.Distance(playerObject.transform.position, rightPlayerPosition.position) ? true : false;

        if(leftTarget && connectedLeftPlayer == null)
        {
            playerObject.transform.position = 
                new Vector3(leftPlayerPosition.position.x, playerObject.transform.position.y, leftPlayerPosition.position.z);
            playerObject.transform.rotation = leftPlayerPosition.rotation;
            playerRPC.SetHinge(thisRigidbody);
            connectedLeftPlayer = playerRPC;
            //CheckKinematic();
            //return true;
        }
        else if(!leftTarget && connectedRightPlayer == null)
        {
            playerObject.transform.position =
                new Vector3(rightPlayerPosition.position.x, playerObject.transform.position.y, rightPlayerPosition.position.z);
            playerObject.transform.rotation = rightPlayerPosition.rotation;
            playerRPC.SetHinge(thisRigidbody);
            connectedRightPlayer = playerRPC;
            //CheckKinematic();
            //return true;
        }
        else
        {
            return false;
        }

        if (connectedLeftPlayer != null && connectedRightPlayer != null)
        {
            connectedRightPlayer.SetHinge(connectedLeftPlayer.gameObject.GetComponent<Rigidbody>());
            connectedLeftPlayer.SetHinge(connectedRightPlayer.gameObject.GetComponent<Rigidbody>());
            CheckMoveChange();
            //StartCoroutine(JointDelay());
            return true;
        }
        else
        {
            return false;
        }

    }

    IEnumerator JointDelay()
    {
        yield return new WaitForSeconds(0.2f);
        connectedRightPlayer.SetHinge(connectedLeftPlayer.gameObject.GetComponent<Rigidbody>());
        connectedLeftPlayer.SetHinge(connectedRightPlayer.gameObject.GetComponent<Rigidbody>());
        CheckMoveChange();
    }

    public bool UnsetJointPlayer(PlayerRPC playerRPC)
    {
        if (connectedLeftPlayer == playerRPC && connectedLeftPlayer != null)
        {
            if(connectedRightPlayer != null)
            {
                connectedRightPlayer.SetHinge(thisRigidbody);
            }
            playerRPC.SetHinge(null);
            connectedLeftPlayer = null;
            CheckMoveChange();
            return true;
        }
        else if (connectedRightPlayer == playerRPC && connectedRightPlayer != null)
        {
            if (connectedLeftPlayer != null)
            {
                connectedLeftPlayer.SetHinge(thisRigidbody);
            }
            playerRPC.SetHinge(null);
            connectedRightPlayer = null;
            CheckMoveChange();
            return true;
        }
        else
        {
            return true;
        }
    }

    void CheckMoveChange()
    {
        if(connectedLeftPlayer != null && connectedRightPlayer != null)
        {
            //thisRigidbody.isKinematic = false;
            moveflag = true;
        }
        else
        {
            //thisRigidbody.isKinematic = true;
            moveflag = false;
        }
    }

    public IEnumerator Complete()
    {
        moveflag = false;
        isCompleted = true;
        connectedLeftPlayer?.ChangePoint(connectedLeftPlayer.playerPlusPoint[PlayerRPC.PlayerPlusCategory.PutFood]);
        connectedRightPlayer?.ChangePoint(connectedRightPlayer.playerPlusPoint[PlayerRPC.PlayerPlusCategory.PutFood]);
        connectedLeftPlayer?.TackCompleteMark();
        connectedRightPlayer?.TackCompleteMark();

        connectedLeftPlayer?.UnsetWagon();
        connectedRightPlayer?.UnsetWagon();
        yield return new WaitUntil(() => UnsetJointPlayer(connectedLeftPlayer));
        yield return new WaitUntil(() => UnsetJointPlayer(connectedRightPlayer));
        yield return true;
    }

}

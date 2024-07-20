using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class FoodStand : MonoBehaviour
{
    [System.NonSerialized]public UnityEvent onVacantSpace = new UnityEvent();//料理が置けるようになったら
    [System.NonSerialized]public UnityEvent onFoodUpdate = new UnityEvent();
    string foodName = null;
    GameObject bigFoodObject = null;
    [SerializeField] GameObject foodContent;//料理が入るオブジェクト
    [SerializeField] Transform bigFoodWagonCreatePosition;//でかい料理用のワゴン生成ポイント
    [System.NonSerialized] public GameObject obonPrefab;//料理を乗せるようのお盆
    [System.NonSerialized] public GameObject wagonPrefab;//でかい料理を乗せる用のワゴン

    [Header("大きい料理用")]public bool bigFoodExclusive = false;
    [System.NonSerialized]public bool foodFlag = false;//料理が乗っているか
    float bigFoodDistance = 3f;//取ったと判定するまでの距離

    [SerializeField][Header("Effect")] ParticleSystem spawnEffect;

    void Start()
    {

    }

    void Update()
    {
        //if (foodContent.transform.childCount <= 0 && oneFlag && !bigFoodExclusive)
        //{
        //    foodFlag = false;
        //    oneFlag = false;
        //    foodName = null;
        //    onVacantSpace.Invoke();
        //    Debug.Log("onVacantSpace");
        //}
        //if(bigFoodExclusive && bigFoodObject != null)//一定以上離れたら取った
        //{
        //    if (bigFoodDistance < Vector3.Distance(bigFoodWagonCreatePosition.position, bigFoodObject.transform.position))
        //    {
        //        bigFoodObject = null;
        //        foodFlag = false;
        //        foodName = null;
        //        onVacantSpace.Invoke();
        //        Debug.Log("BigCakeSpaceOK");
        //    }
        //}
    }

    public void GetFood()
    {
        foodFlag = false;
        foodName = null;
        onVacantSpace.Invoke();
        Debug.Log("onVacantSpace");
    }

    public string FoodGetInformation()
    {
        if (foodFlag && !bigFoodExclusive)
        {
            return foodName;
        }
        else
        {
            return null;
        }
    }

    public void SetFood(int tableNumber,List<Food> foodList)
    {
        if(obonPrefab != null)
        {
            GameObject prefab = obonPrefab;//モデルをデータベースから取得
            GameObject createObject = Instantiate(prefab);
            createObject.name = tableNumber + "," + GameVariable.foodCount;//テーブル番号,生成番号
            createObject.transform.SetParent(foodContent.transform, true);
            createObject.transform.localScale = prefab.transform.localScale;
            createObject.transform.localPosition = Vector3.zero + Vector3.up * (createObject.transform.localScale.y / 2);
            
            //料理生成
            foreach (Food food in foodList)
            {
                foreach (Transform trans in createObject.transform.GetChild(1))
                {
                    if (trans.childCount <= 0)
                    {
                        GameObject foodPrefab = food.GetFoodModel();//モデルをデータベースから取得
                        GameObject createFoodObject = Instantiate(foodPrefab);
                        createFoodObject.name = food.GetFoodNumber().ToString();//フード番号
                        createFoodObject.transform.SetParent(trans.transform, true);
                        createFoodObject.transform.localScale = foodPrefab.transform.localScale / 2;
                        createFoodObject.transform.localPosition = Vector3.zero;
                        break;
                    }
                }
            }
            createObject.transform.localScale = Vector3.zero;
            createObject.transform.DOScale(prefab.transform.localScale, 0.2f).SetEase(Ease.OutCirc);
            foodName = tableNumber + "," + GameVariable.foodCount;
            GameVariable.foodObjectList.Add(createObject);
            GameVariable.foodCount++;
            spawnEffect?.Play();
            onFoodUpdate.Invoke();
        }
    }

    public void SetBigFood(int tableNumber, List<Food> foodList)
    {
        if (wagonPrefab != null)
        {
            GameObject prefab = wagonPrefab;//モデルをデータベースから取得
            GameObject createObject = Instantiate(prefab);
            createObject.name = tableNumber + "," + GameVariable.foodCount;//テーブル番号,生成番号
            //createObject.transform.localScale = prefab.transform.localScale;
            createObject.transform.localScale = Vector3.zero;
            createObject.transform.DOScale(prefab.transform.localScale, 0.3f).SetEase(Ease.OutCirc);
            createObject.transform.position = bigFoodWagonCreatePosition.position;

            //料理生成
            foreach (Food food in foodList)
            {
                createObject.GetComponent<Wagon>().SetFood(food);
            }
            createObject.GetComponent<Wagon>().SetTable(tableNumber.ToString());
            foodName = tableNumber + "," + GameVariable.foodCount;
            bigFoodObject = createObject;
            GameVariable.foodObjectList.Add(createObject);
            GameVariable.foodCount++;
            spawnEffect?.Play();
            onFoodUpdate.Invoke();
            StartCoroutine(CheckDistance());
        }
    }

    IEnumerator CheckDistance()
    {
        while (true)
        {
            if(bigFoodObject == null)
            {
                bigFoodObject = null;
                foodFlag = false;
                foodName = null;
                onVacantSpace.Invoke();
                Debug.Log("BigFoodSpaceOK");
                break;
            }
            else if (bigFoodDistance < Vector3.Distance(bigFoodWagonCreatePosition.position, bigFoodObject.transform.position))
            {
                bigFoodObject = null;
                foodFlag = false;
                foodName = null;
                onVacantSpace.Invoke();
                Debug.Log("BigFoodSpaceOK");
                break;
            }
            yield return null;
        }
    }
}

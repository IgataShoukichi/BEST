using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class FoodStand : MonoBehaviour
{
    [System.NonSerialized]public UnityEvent onVacantSpace = new UnityEvent();//�������u����悤�ɂȂ�����
    [System.NonSerialized]public UnityEvent onFoodUpdate = new UnityEvent();
    string foodName = null;
    GameObject bigFoodObject = null;
    [SerializeField] GameObject foodContent;//����������I�u�W�F�N�g
    [SerializeField] Transform bigFoodWagonCreatePosition;//�ł��������p�̃��S�������|�C���g
    [System.NonSerialized] public GameObject obonPrefab;//�������悹��悤�̂��~
    [System.NonSerialized] public GameObject wagonPrefab;//�ł����������悹��p�̃��S��

    [Header("�傫�������p")]public bool bigFoodExclusive = false;
    [System.NonSerialized]public bool foodFlag = false;//����������Ă��邩
    float bigFoodDistance = 3f;//������Ɣ��肷��܂ł̋���

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
        //if(bigFoodExclusive && bigFoodObject != null)//���ȏ㗣�ꂽ������
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
            GameObject prefab = obonPrefab;//���f�����f�[�^�x�[�X����擾
            GameObject createObject = Instantiate(prefab);
            createObject.name = tableNumber + "," + GameVariable.foodCount;//�e�[�u���ԍ�,�����ԍ�
            createObject.transform.SetParent(foodContent.transform, true);
            createObject.transform.localScale = prefab.transform.localScale;
            createObject.transform.localPosition = Vector3.zero + Vector3.up * (createObject.transform.localScale.y / 2);
            
            //��������
            foreach (Food food in foodList)
            {
                foreach (Transform trans in createObject.transform.GetChild(1))
                {
                    if (trans.childCount <= 0)
                    {
                        GameObject foodPrefab = food.GetFoodModel();//���f�����f�[�^�x�[�X����擾
                        GameObject createFoodObject = Instantiate(foodPrefab);
                        createFoodObject.name = food.GetFoodNumber().ToString();//�t�[�h�ԍ�
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
            GameObject prefab = wagonPrefab;//���f�����f�[�^�x�[�X����擾
            GameObject createObject = Instantiate(prefab);
            createObject.name = tableNumber + "," + GameVariable.foodCount;//�e�[�u���ԍ�,�����ԍ�
            //createObject.transform.localScale = prefab.transform.localScale;
            createObject.transform.localScale = Vector3.zero;
            createObject.transform.DOScale(prefab.transform.localScale, 0.3f).SetEase(Ease.OutCirc);
            createObject.transform.position = bigFoodWagonCreatePosition.position;

            //��������
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

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class FoodStandManager : MonoBehaviour
{
    Dictionary<int,List<Food>> standbyNormalFoods = new Dictionary<int, List<Food>>();
    Dictionary<int,List<Food>> standbyBigFoods = new Dictionary<int, List<Food>>();

    List<FoodStand> normalStand = new List<FoodStand>();
    List<FoodStand> bigStand = new List<FoodStand>();
    float createDelayTime = 1f;
    void Start()
    {
        
    }

    public void FoodStandSetting(GameObject obonPrefab,GameObject wagonPrefab)
    {
        foreach(GameObject stand in GameVariable.foodStandList)
        {
            if(stand.GetComponent<FoodStand>().bigFoodExclusive)
            {
                bigStand.Add(stand.GetComponent<FoodStand>());
            }
            else
            {
                normalStand.Add(stand.GetComponent<FoodStand>());
            }
            stand.GetComponent<FoodStand>().obonPrefab = obonPrefab;
            stand.GetComponent<FoodStand>().wagonPrefab = wagonPrefab;
            stand.GetComponent<FoodStand>().onVacantSpace.RemoveAllListeners();
            stand.GetComponent<FoodStand>().onVacantSpace.AddListener(() => FoodGeneration());
        }
    }

    public void FoodCreate(int tableNumber, List<Food> orderFood)
    {
        StartCoroutine(FoodPreparation());
        IEnumerator FoodPreparation()
        {
            yield return orderFood[0].GetFoodTime();//‰¼
            if (orderFood[0].GetFoodBig())
            {
                standbyBigFoods.Add(tableNumber, orderFood);
            }
            else
            {
                standbyNormalFoods.Add(tableNumber, orderFood);
            }
            FoodGeneration();

            //yield return new WaitForSeconds(foodList.Count);
        }
            /*
            foreach(int number in orderFood)
            {
                StartCoroutine(FoodPreparation(GameVariable.foodDataBase.GetFood(number)));
            }
            */
        }

    //IEnumerator FoodPreparation(List<int> foodList)
    //{
        //yield return new WaitForSeconds(food.GetFoodTime());
        //standbyFood.Add(food);
        //FoodGeneration();
        //StartCoroutine(FoodGenerationDelay());
    //}

    void FoodGeneration()
    {
        foreach(FoodStand stand in bigStand)
        {
            if (!stand.foodFlag && standbyBigFoods.Count > 0)
            {
                foreach (KeyValuePair<int, List<Food>> tableFood in standbyBigFoods)
                {
                    if (tableFood.Value[0].GetFoodBig())
                    {
                        stand.foodFlag = true;
                        StartCoroutine(BigFoodGenerationDelay(stand, tableFood.Key, tableFood.Value));
                        standbyBigFoods.Remove(tableFood.Key);
                        break;
                    }
                }
            }
        }
        foreach (FoodStand stand in normalStand)
        {
            if (!stand.foodFlag && standbyNormalFoods.Count > 0)
            {
                foreach (KeyValuePair<int, List<Food>> tableFood in standbyNormalFoods)
                {
                    if (!tableFood.Value[0].GetFoodBig())
                    {
                        stand.foodFlag = true;
                        StartCoroutine(NormalFoodGenerationDelay(stand, tableFood.Key, tableFood.Value));
                        standbyNormalFoods.Remove(tableFood.Key);
                        break;
                    }
                }
            }
        }
        /*
        foreach (GameObject stand in GameVariable.foodStandList)
        {
            if (!stand.GetComponent<FoodStand>().foodFlag && standByFoods.Count > 0)
            {
                if (!standByFoods[0][0].GetFoodBig() && !stand.GetComponent<FoodStand>().bigFoodExclusive)
                {
                    stand.GetComponent<FoodStand>().foodFlag = true;
                    StartCoroutine(FoodGenerationDelay(stand, standbyFood[0]));
                    standbyFood.RemoveAt(0);
                }
                else if (standbyFood[0].GetFoodBig() && stand.GetComponent<FoodStand>().bigFoodExclusive)
                {
                    stand.GetComponent<FoodStand>().foodFlag = true;
                    StartCoroutine(FoodGenerationDelay(stand, standbyFood[0]));
                    standbyFood.RemoveAt(0);
                }
            }
            if (!stand.GetComponent<FoodStand>().foodFlag && standbyFood.Count > 0)
            {
                if (!standbyFood[0].GetFoodBig() && !stand.GetComponent<FoodStand>().bigFoodExclusive)
                {
                    stand.GetComponent<FoodStand>().foodFlag = true;
                    StartCoroutine(FoodGenerationDelay(stand, standbyFood[0]));
                    standbyFood.RemoveAt(0);
                }
                else if(standbyFood[0].GetFoodBig() && stand.GetComponent<FoodStand>().bigFoodExclusive)
                {
                    stand.GetComponent<FoodStand>().foodFlag = true;
                    StartCoroutine(FoodGenerationDelay(stand, standbyFood[0]));
                    standbyFood.RemoveAt(0);
                }
            }
            
        }
        */
    }

    IEnumerator NormalFoodGenerationDelay(FoodStand stand,int tableNumber,List<Food> food)
    {
        yield return new WaitForSeconds(createDelayTime);
        stand.SetFood(tableNumber,food);
    }
    IEnumerator BigFoodGenerationDelay(FoodStand stand, int tableNumber, List<Food> food)
    {
        yield return new WaitForSeconds(createDelayTime);
        stand.SetBigFood(tableNumber, food);
    }
}

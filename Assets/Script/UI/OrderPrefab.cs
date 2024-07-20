using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OrderPrefab : MonoBehaviour
{
    [SerializeField] Text tableNumber;
    //[SerializeField] GameObject[] orderPanels;
    [SerializeField] List<GameObject> orderPanels;
    
    public int orderCount = 0;

    public void OrderSetting(int tNumber,List<Food> orderList)
    {
        tableNumber.text = tNumber.ToString("");
        for(int i = 0; i < orderPanels.Count; i++) 
        { 
            if(i < orderList.Count)
            {
                orderPanels[i].gameObject.SetActive(true);
                if(orderList[i].GetFoodSprite() != null)
                {
                    orderPanels[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = orderList[i].GetFoodSprite();
                }
                orderPanels[i].transform.GetChild(1).gameObject.GetComponent<Text>().text = orderList[i].GetFoodName();
            }
            else
            {
                //orderPanels[i].gameObject.SetActive(false);
                Destroy(orderPanels[i].gameObject);
            }
        }
        orderCount = orderList.Count;
    }
    /*
    public void UpdateOrder(int orderNumber)
    {
        //orderPanels[orderNumber].gameObject.SetActive(false);
        Destroy(orderPanels[orderNumber].gameObject);
        orderPanels.RemoveAt(orderNumber);
        orderCount--;
    }
    */
}

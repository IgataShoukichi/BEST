using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomerPrefab : MonoBehaviour,IPointerClickHandler
{
    [SerializeField][Header("‘Ò‚Á‚Ä‚¢‚é”Ô†")] Text waitingNumberText;
    [SerializeField][Header("‚¨‹q‚³‚ñ‚Ì–¼‘O")] Text customerNameText;
    [SerializeField][Header("‚¨‹q‚³‚ñ‚Ìl”")] Text customerCountText;

    [System.NonSerialized]public CustomerList customerList;
    CustomerGroup customerGroup = null;
    int number = -1;

    public void CustomerSetting(CustomerList customerList, int number, CustomerGroup customerGroup)
    {
        this.customerGroup = customerGroup;
        this.number = number;
        this.customerList = customerList;
        this.waitingNumberText.text = number.ToString();
        customerNameText.text = customerGroup.GetCustomerName();
        customerCountText.text = customerGroup.GetCustomerDetail().Count.ToString();
        //EventTrigger.Entry entry1 = new EventTrigger.Entry();
        //entry1.eventID = EventTriggerType.PointerClick;
        //entry1.callback.AddListener((eventDate) => { ClickPanel(); });
        //this.GetComponent<EventTrigger>().triggers.Add(entry1);
    }

    public void SelectPanel(out int outNumber,out CustomerGroup outCustomerGroup)
    {
        //if (customerGroup != null && this.number >= 0)
        //{
            outNumber = this.number;
            outCustomerGroup = this.customerGroup;
            //return customerGroup;
        //}
        //else
        //{
            //return null;
        //}
    }

    public void ClickPanel()
    {
        if(customerGroup != null && number >= 0)
        {
            customerList.TapGuideOK(number, customerGroup,this.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ClickPanel();
    }
}

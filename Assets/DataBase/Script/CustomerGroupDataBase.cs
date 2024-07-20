using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CustomerGroupDataBase", menuName = "CreateCustomerGroupDataBase")]
public class CustomerGroupDataBase : ScriptableObject
{
	
	[SerializeField]
	private List<CustomerGroup> customerList = new List<CustomerGroup>();

	//�@�A�C�e�����X�g��Ԃ�
	public List<CustomerGroup> GetCustomerList()
	{
		return customerList;
	}
	public CustomerGroup GetCustomer(int searchNumber)
	{
		return GetCustomerList().Find(itemName => itemName.GetCustomerNumber() == searchNumber);
	}


}

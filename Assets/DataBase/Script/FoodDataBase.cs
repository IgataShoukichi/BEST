using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "FoodDataBase", menuName = "CreateFoodDataBase")]
public class FoodDataBase : ScriptableObject
{
	
	[SerializeField]
	private List<Food> foodList = new List<Food>();

	//[SerializeField]
	//private List<Item> itemList = new List<Item>();

	//　アイテムリストを返す
	public List<Food> GetFoodList()
	{
		return foodList;
	}

	//public List<Item> GetItemList()
	//{
		//return itemList;
	//}

	public Food GetFood(string searchName)
	{
		return GetFoodList().Find(itemName => itemName.GetFoodName() == searchName);
	}
	public Food GetFood(int searchNumber)
	{
		return GetFoodList().Find(itemName => itemName.GetFoodNumber() == searchNumber);
	}


}

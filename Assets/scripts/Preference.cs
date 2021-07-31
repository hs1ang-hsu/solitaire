using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preference : MonoBehaviour
{
	private Solitaire solitaire;
	void Start(){
		solitaire = FindObjectOfType<Solitaire>();
	}
	
	public void Select(int num){
		solitaire.player_data.card_back_pref = num;
		GetComponent<RectTransform>().anchoredPosition = new Vector2(-250 + (num%3) * 250, (num<3) ? 150 : -150);
	}
}

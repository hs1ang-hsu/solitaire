using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
	Solitaire solitaire;
    // Start is called before the first frame update
    void Start()
    {
		solitaire = FindObjectOfType<Solitaire>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnMouseUp(){
		Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (hit)
			if (hit.collider.transform.name == "deck_bottom")
				solitaire.DeckRestack();
	}
}

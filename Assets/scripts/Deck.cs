using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
	private Solitaire solitaire;
	private UIManager UIM;
	private Transform glowing;
    // Start is called before the first frame update
    void Start()
    {
		solitaire = FindObjectOfType<Solitaire>();
		UIM = FindObjectOfType<UIManager>();
		glowing = transform.Find("glowing");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnMouseUp(){
		Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (hit){
			if (hit.collider.transform.name == "deck_bottom"){
				solitaire.DeckRestack();
				UIM.score -= 10;
				UIM.click_count++;
				UIM.undo_count += (UIM.undo_count<15)?1:0;
			}
		}
	}
	
	public IEnumerator Glow()
    {
		for (int i=0; i<3; i++){
			for (int j=0; j<6; j++){
				glowing.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 0.15f);
				yield return new WaitForSeconds(0.05f);
			}
			for (int j=0; j<6; j++){
				glowing.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, 0.15f);
				yield return new WaitForSeconds(0.05f);
			}
		}
    }
}

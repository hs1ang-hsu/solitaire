using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public bool is_face_up = false;
    public int location = 0; //0: deck, 1:deck_pile, 2~5: top, 6~12: bottom
    public string suit;
    public int value;
	
    private Solitaire solitaire;
	private UIManager UIM;
	private AudioManager AM;
	
	private Vector3 offset;
	private Vector3 original_pos;
	private int limit_bottom;
	private int position_in_bottom;
	public bool movable;
	public bool freeze_action;
	private int face_up_num;
	private bool is_face_up_num_find;
	private List<string> card_list;
	
	private Transform glowing;

    // Start is called before the first frame update
    void Start()
    {
		card_list = new List<string>();
		limit_bottom = 0;
		position_in_bottom = 0;
		movable = true;
		freeze_action = false;
		face_up_num = 0;
		is_face_up_num_find = false;
		
        solitaire = FindObjectOfType<Solitaire>();
		UIM = FindObjectOfType<UIManager>();
		AM = FindObjectOfType<AudioManager>();
		glowing = transform.Find("glowing");
		
        if (CompareTag("card"))
        {
            suit = transform.name[0].ToString(); //CDHS

            if (transform.name.Length == 2)
            {
                char c = transform.name[1];
                switch (c)
                {
                    case 'A':
                        value = 1;
                        break;
                    case '2':
                        value = 2;
                        break;
                    case '3':
                        value = 3;
                        break;
                    case '4':
                        value = 4;
                        break;
                    case '5':
                        value = 5;
                        break;
                    case '6':
                        value = 6;
                        break;
                    case '7':
                        value = 7;
                        break;
                    case '8':
                        value = 8;
                        break;
                    case '9':
                        value = 9;
                        break;
                    case 'J':
                        value = 11;
                        break;
                    case 'Q':
                        value = 12;
                        break;
                    case 'K':
                        value = 13;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                value = 10;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnMouseDown(){
		if (movable && !solitaire.global_freeze){
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
			original_pos = gameObject.transform.position;
			offset += new Vector3(0, 0, -5);
			
			if (location > 5 && location < 13){ //bottom
				if (is_face_up){
					limit_bottom = solitaire.bottoms[location - 6].Count;
					position_in_bottom = limit_bottom - 1;
					face_up_num = limit_bottom - 1;
					is_face_up_num_find = false;

					for (int j = 0; j < limit_bottom; j++)
					{
						if (is_face_up_num_find == false)
						{
							Transform card_tmp = solitaire.bottom_pos[location - 6].transform.Find(solitaire.bottoms[location - 6][j]);
							if (card_tmp.GetComponent<Card>().is_face_up == true)
							{
								face_up_num = j;
								is_face_up_num_find = true;
							}
						}
						if (transform.name == solitaire.bottoms[location - 6][j])
							position_in_bottom = j;
					}
					
					card_list.Clear();
					for (int j = position_in_bottom + 1; j < limit_bottom; j++) //set the parents of the lower cards to the one we move
					{
						Transform card_tmp = solitaire.bottom_pos[location - 6].transform.Find(solitaire.bottoms[location - 6][j]);
						card_tmp.GetComponent<Card>().movable = false;
						card_tmp.GetComponent<Card>().freeze_action = true;
						card_tmp.parent = transform;
						card_list.Add(card_tmp.name);
					}
				}
			}
		}
		else
			freeze_action = true;
	}
	
	void OnMouseDrag(){
		if (!freeze_action && is_face_up){
			if (location == 1){
				if (transform.name == solitaire.deck_pile[solitaire.deck_pile.Count-1])
					transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) + offset;
			}
			else
				transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) + offset;
		}
	}
	
	void OnMouseUp(){
		bool success = false;
		bool auto = false;
		if (location == 1)
			if (transform.name != solitaire.deck_pile[solitaire.deck_pile.Count-1])
				freeze_action = true;
		if (!freeze_action){
			if (is_face_up)
					transform.position -= new Vector3(0, 0, -5);
			if (Vector3.Distance(original_pos, transform.position) < 0.2){
				auto = true;
				if (location == 0)
				{
					solitaire.DeckCardActions(gameObject.GetComponent<Card>());
					success = true;
				}
				else
					if (is_face_up)
						success = solitaire.AutoStack(gameObject.GetComponent<Card>(), true);
			}
			else{
				Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
				RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				for (int i=0; i<hits.Length; i++){
					RaycastHit2D hit = hits[i];
					if (hit.collider.CompareTag("card")){
						Card card_other = hit.transform.GetComponent<Card>();
						if (card_other.is_face_up){
							success = solitaire.StackByDrag(transform.GetComponent<Card>(), card_other);
							if (success)
								break;
						}
					}
					else if (hit.collider.CompareTag("top_pile")){
						int num = int.Parse(hit.transform.name[3].ToString());
						if (solitaire.tops[num].Count != 0){
							continue;
						}
						
						if (value == 1){
							if (location == 1){ //deck_pile to top
								solitaire.undo_stack.Push(new string[] {(num+2).ToString(), transform.name, location.ToString(), "0"});
								
								Vector3 shift = new Vector3(0, 0, -0.2f);
								StartCoroutine(MoveTo(solitaire.top_pos[num].transform.position, shift, true));
								transform.parent = solitaire.top_pos[num].transform;
								location = num + 2;

								solitaire.deck_pile.Remove(transform.name);
								solitaire.tops[num].Add(transform.name);
								solitaire.deck_location--;
								success = true;
								
								UIM.score += 10;
								break;
							}
							else if (location>5 && location<13){ //bottom to top
								if (value == 1)
								{
									//Turn up the face of the upper card.
									bool is_flip = false;
									if (face_up_num == position_in_bottom)
									{
										if (position_in_bottom != 0)
										{
											Transform card_tmp = solitaire.bottom_pos[location - 6].transform.Find(solitaire.bottoms[location - 6][position_in_bottom - 1]);
											card_tmp.GetComponent<Card>().is_face_up = true;
											is_flip = true;
										}
									}
									
									solitaire.undo_stack.Push(new string[] {(num+2).ToString(), transform.name, location.ToString(), is_flip?"1":"0"});

									//Moving
									Vector3 shift = new Vector3(0, 0, -0.2f);
									StartCoroutine(MoveTo(solitaire.top_pos[num].transform.position, shift, true));
									transform.parent = solitaire.top_pos[num].transform;

									solitaire.bottoms[location - 6].Remove(transform.name);
									solitaire.tops[num].Add(transform.name);
									location = num + 2;
									success = true;
									UIM.score += 10;
									break;
								}
							}
						}
					}
					else if (hit.collider.CompareTag("bottom_pile")){
						int num = int.Parse(hit.transform.name[6].ToString());
						if (solitaire.bottoms[num].Count != 0){
							continue;
						}
						
						if (value == 13){
							if (location == 1){ //deck_pile to bottom
								solitaire.undo_stack.Push(new string[] {(num+6).ToString(), transform.name, location.ToString(), "0"});
								
								Vector3 shift = new Vector3(0, 0, -0.2f);
								StartCoroutine(MoveTo(solitaire.bottom_pos[num].transform.position, shift, true));
								transform.parent = solitaire.bottom_pos[num].transform;
								location = num + 6;
								
								solitaire.deck_pile.Remove(transform.name);
								solitaire.bottoms[num].Add(transform.name);
								solitaire.deck_location--;
								success = true;
								UIM.score += 3;
								break;
							}
							else if (location>1 && location<6){ //top to bottom
								solitaire.undo_stack.Push(new string[] {(num+6).ToString(), transform.name, location.ToString(), "0"});
								
								Vector3 shift = new Vector3(0, 0, -0.2f);
								StartCoroutine(MoveTo(solitaire.bottom_pos[num].transform.position, shift, true));
								transform.parent = solitaire.bottom_pos[num].transform;
								
								solitaire.tops[location - 2].Remove(transform.name);
								solitaire.bottoms[num].Add(transform.name);
								location = num + 6;
								success = true;
								UIM.score -= 15;
								break;
							}
							else if (location>5 && location<13){ //bottom
								if (value == 13)
								{
									//Turn up the face of the upper card.
									bool is_flip = false;
									if (face_up_num == position_in_bottom)
									{
										if (position_in_bottom != 0)
										{
											Transform card_tmp = solitaire.bottom_pos[location - 6].transform.Find(solitaire.bottoms[location - 6][position_in_bottom - 1]);
											card_tmp.GetComponent<Card>().is_face_up = true;
											is_flip = true;
										}
									}
									
									solitaire.undo_stack.Push(new string[] {(num+6).ToString(), transform.name, location.ToString(), is_flip?"1":"0"});

									for (int j = position_in_bottom + 1; j < limit_bottom; j++)
									{
										Transform card_tmp = transform.Find(solitaire.bottoms[location - 6][j]);
										card_tmp.GetComponent<Card>().movable = true;
										card_tmp.GetComponent<Card>().freeze_action = false;
										card_tmp.GetComponent<Card>().location = num + 6;
									}

									solitaire.bottoms[location - 6].Remove(transform.name);
									solitaire.bottoms[num].Add(transform.name);
									foreach (string card in card_list)
									{
										solitaire.bottoms[location - 6].Remove(card);
										solitaire.bottoms[num].Add(card);
									}
									location = num + 6;
									
									Vector3 shift = new Vector3(0, 0, -0.2f);
									StartCoroutine(MoveTo(solitaire.bottom_pos[num].transform.position, shift, true, () =>
									{
										foreach (string card in card_list)
										{
											Transform card_tmp = transform.Find(card);
											card_tmp.parent = solitaire.bottom_pos[num].transform;
										}
										transform.parent = solitaire.bottom_pos[num].transform;
									}));
									success = true;
									break;
								}
							}
						}
					}
				}
			}
			if (!success){
				if (is_face_up)
					StartCoroutine(MoveTo(original_pos, new Vector3(0,0,0), false, () =>
					{
						if (auto){
							StartCoroutine(Shake(() =>
							{
								if (location > 5 && location < 13){
									for (int j = position_in_bottom + 1; j < limit_bottom; j++)
									{
										Transform card_tmp = transform.Find(solitaire.bottoms[location - 6][j]);
										card_tmp.GetComponent<Card>().movable = true;
										card_tmp.GetComponent<Card>().freeze_action = false;
										card_tmp.parent = solitaire.bottom_pos[location - 6].transform;
									}
								}
							}));
						}
						else{
							if (location > 5 && location < 13){
								for (int j = position_in_bottom + 1; j < limit_bottom; j++)
								{
									Transform card_tmp = transform.Find(solitaire.bottoms[location - 6][j]);
									card_tmp.GetComponent<Card>().movable = true;
									card_tmp.GetComponent<Card>().freeze_action = false;
									card_tmp.parent = solitaire.bottom_pos[location - 6].transform;
								}
							}
						}
						GetComponent<BoxCollider2D>().enabled = true;
					}));
			}
			else{
				AM.Play("card_move");
				UIM.click_count++;
				UIM.undo_count += (UIM.undo_count<15)?1:0;
				UIM.hint_delay = false;
				
				if(solitaire.IsGameEnd())
					UIM.EndGame();
				if(solitaire.IsCardAllFaceUp())
					UIM.CallAutoCompleteMenu();
			}
		}
		else
			freeze_action = false;
	}

    public IEnumerator MoveTo(Vector3 destiny, Vector3 shift, bool disable, System.Action callback = null)
    {
		GetComponent<BoxCollider2D>().enabled = false;
		movable = false;
		float move_period;
		Vector2 start_position = transform.position;
		Vector2 end_position = new Vector2(destiny.x + shift.x, destiny.y + shift.y);
		if (Vector2.Distance(start_position, end_position) < 0.2f)
			move_period = 0.05f;
		else
			move_period = 0.2f;
		for (float ratio = 0f; ratio < 1f;ratio += Time.deltaTime / move_period)
		{
			transform.position = Vector2.Lerp(start_position, end_position, ratio);
			transform.position += new Vector3(0, 0, -5f);
			yield return null;
		}
		transform.position = destiny + shift;
		callback?.Invoke();
		if (disable)
			GetComponent<BoxCollider2D>().enabled = true;
		movable = true;
    }

    public IEnumerator Shake(System.Action callback = null)
    {
		transform.GetComponent<BoxCollider2D>().enabled = false;
		AM.Play("card_shake");
		Vector3 shaking_vector = new Vector3(0.02f, 0, 0);
		for (int i = 0; i < 3; i++)
		{
			transform.position -= shaking_vector;
			yield return new WaitForSeconds(0.01f);
		}
		for (int i = 0; i < 6; i++)
		{
			transform.position += shaking_vector;
			yield return new WaitForSeconds(0.01f);
		}
		for (int i = 0; i < 6; i++)
		{
			transform.position -= shaking_vector;
			yield return new WaitForSeconds(0.01f);
		}
		for (int i = 0; i < 3; i++)
		{
			transform.position += shaking_vector;
			yield return new WaitForSeconds(0.01f);
		}
		callback?.Invoke();
		yield return new WaitForSeconds(0.3f);
		transform.GetComponent<BoxCollider2D>().enabled = true;
    }

    public IEnumerator Glow()
    {
		for (int i=0; i<3; i++){
			for (int j=0; j<8; j++){
				glowing.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 0.1f);
				if (!movable || solitaire.stop_glowing){
					glowing.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, glowing.GetComponent<SpriteRenderer>().color.a);
					yield break;
				}
				yield return new WaitForSeconds(0.06f);
			}
			for (int j=0; j<8; j++){
				glowing.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, 0.1f);
				if (!movable || solitaire.stop_glowing){
					glowing.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, glowing.GetComponent<SpriteRenderer>().color.a);
					yield break;
				}
				yield return new WaitForSeconds(0.06f);
			}
			if (!movable || solitaire.stop_glowing)
				yield break;
		}
    }

}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Solitaire : MonoBehaviour
{
    public Sprite[] card_faces;
    public GameObject card_prefab;
    public GameObject[] top_pos;
    public GameObject[] bottom_pos;
    public GameObject deck_pos;
    public GameObject deck_pile_pos;

    public static string[] suits = new string[] { "C", "D", "H", "S" };
    public static string[] values = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
    public List<string>[] bottoms;
    public List<string>[] tops;
    public List<string> deck_pile = new List<string>();

    public List<string> bottom0 = new List<string>();
    public List<string> bottom1 = new List<string>();
    public List<string> bottom2 = new List<string>();
    public List<string> bottom3 = new List<string>();
    public List<string> bottom4 = new List<string>();
    public List<string> bottom5 = new List<string>();
    public List<string> bottom6 = new List<string>();

    public List<string> top0 = new List<string>();
    public List<string> top1 = new List<string>();
    public List<string> top2 = new List<string>();
    public List<string> top3 = new List<string>();

    public List<string> deck;
    public int deck_location;
	
    private UIManager UIM;
	private int hint_position = 0;
	private bool bottom_to_top = false;
	
	public bool global_freeze;
	public bool stop_glowing;
	
	private AudioManager AM;
	
	public Stack<string[]> undo_stack = new Stack<string[]>();
	
	public PlayerData player_data;
	public DataIO data_io;

    // Start is called before the first frame update
    void Start()
    {
		player_data = data_io.LoadPlayerData();
		
		UIM = FindObjectOfType<UIManager>();
		UIM.audio_mixer.SetFloat("sound_effect", player_data.sound_effect_volume);
		UIM.slider.value = player_data.sound_effect_volume;
		AM = FindObjectOfType<AudioManager>();
        bottoms = new List<string>[] { bottom0, bottom1, bottom2, bottom3, bottom4, bottom5, bottom6 };
        tops = new List<string>[] { top0, top1, top2, top3 };
		global_freeze = false;
        PlayCards();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayCards()
    {
        deck = GenerateDeck();
        Shuffle(deck);

        SolitaireSort();
        StartCoroutine(SolitaireDeal());
        deck_location = 0;
    }

    public static List<string> GenerateDeck()
    {
        List<string> new_deck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                new_deck.Add(s + v);
            }
        }
        return new_deck;
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T tmp = list[k];
            list[k] = list[n];
            list[n] = tmp;
        }
    }

    IEnumerator SolitaireDeal()
    {
		AM.Play("card_deal");
        for (int i=0; i<7; i++)
        {
            float y_offset = 0;
            float z_offset = 0.2f;
            foreach (string card in bottoms[i])
            {
                yield return new WaitForSeconds(0.03f);
                GameObject new_card = Instantiate(card_prefab, new Vector3(bottom_pos[i].transform.position.x, bottom_pos[i].transform.position.y - y_offset, bottom_pos[i].transform.position.z - z_offset), Quaternion.identity, bottom_pos[i].transform);
                new_card.name = card;
                new_card.GetComponent<Card>().location = i + 6;
                if (card == bottoms[i][bottoms[i].Count - 1])
                {
                    new_card.GetComponent<Card>().is_face_up = true;
                }

                y_offset += 0.3f;
                z_offset += 0.2f;
                deck_pile.Add(card);
            }
        }
		AM.Play("card_stack");
        foreach (string card in deck_pile)
        {
            if (deck.Contains(card))
            {
                deck.Remove(card);
            }
        }
        deck_pile.Clear();

        float offset = 0.2f;
        foreach (string card in deck)
        {
            GameObject new_card = Instantiate(card_prefab, new Vector3(deck_pos.transform.position.x, deck_pos.transform.position.y, deck_pos.transform.position.z), Quaternion.identity, deck_pos.transform);
            Vector3 tmp = new Vector3(0, 0, -offset);
            new_card.GetComponent<Card>().transform.position += tmp;
            new_card.name = card;
            new_card.GetComponent<Card>().location = 0;

            offset += 0.2f;
        }
    }

    void SolitaireSort()
    {
        for (int i=6; i>=0; i--)
        {
            for (int j=i; j<7; j++)
            {
                bottoms[j].Add(deck[deck.Count-1]);
                deck.RemoveAt(deck.Count - 1);
            }
        }
    }

    public void DeckCardActions(Card card_obj)
    {
        card_obj.transform.parent = deck_pile_pos.transform;
        StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, -deck_location * 0.2f), true));

        card_obj.is_face_up = true;
        card_obj.location = 1;
		
        deck_pile.Add(card_obj.name);
        deck.Remove(card_obj.name);
        deck_location++;
		undo_stack.Push(new string[] {"1", card_obj.name, "0", "0"});
    }

    public void DeckRestack()
    {
		AM.Play("card_stack");
        deck.Clear();
		float offset = 0.2f;
        for (int i=deck_pile.Count-1; i>=0; i--)
        {
			Card card_obj = deck_pile_pos.transform.Find(deck_pile[i]).GetComponent<Card>();
			card_obj.transform.parent = deck_pos.transform;
			StartCoroutine(card_obj.MoveTo(deck_pos.transform.position, new Vector3(0, 0, -offset), true));

			card_obj.is_face_up = false;
			card_obj.location = 0;
			deck_location--;
			offset += 0.2f;
			
            deck.Add(deck_pile[i]);
        }
        deck_pile.Clear();
        deck_location = 0;
		undo_stack.Push(new string[] {"0", "0", "0", "0"});
    }

    public bool AutoStack(Card card_obj, bool move)
    {
        int location = card_obj.location;
        string suit = card_obj.suit;
        int value = card_obj.value;

        if (location>1 && location<6) //top
        {
            for (int i=6; i<13; i++) // top to bottom
            {
				if (bottoms[i - 6].Count == 0)
				{
					if (value == 13)
					{
						if (move){
							card_obj.transform.parent = bottom_pos[i - 6].transform;
							StartCoroutine(card_obj.MoveTo(bottom_pos[i - 6].transform.position, new Vector3(0, 0, -0.2f), true));

							card_obj.location = i;
							tops[location-2].Remove(card_obj.name);
							bottoms[i - 6].Add(card_obj.name);
							
							UIM.score -= 15;
							undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), "0"});
						}
						else
							hint_position = i - 6;
                        return true;
					}
				}
                else
                {
                    Transform card_check = bottom_pos[i - 6].transform.Find(bottoms[i - 6][bottoms[i - 6].Count - 1]);
                    Card card_check_obj = card_check.GetComponent<Card>();
                    string suit_check = card_check_obj.suit;
                    int value_check = card_check_obj.value;

                    bool is_card_red = true;
                    bool is_card_check_red = true;
                    if (suit == "C" || suit == "S")
                    {
                        is_card_red = false;
                    }
                    if (suit_check == "C" || suit_check == "S")
                    {
                        is_card_check_red = false;
                    }

                    if (is_card_red != is_card_check_red)
                    {
                        if (value == value_check - 1)
                        {
							if (move){
								card_obj.transform.parent = bottom_pos[i - 6].transform;
								int length = bottoms[i-6].Count;
								StartCoroutine(card_obj.MoveTo(bottom_pos[i-6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true));

								card_obj.location = i;
								tops[location-2].Remove(card_obj.name);
								bottoms[i - 6].Add(card_obj.name);
								
								UIM.score -= 15;
								undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), "0"});
							}
							else
								hint_position = i - 6;
                            return true;
                        }
                    }
                }
            }
        }
		else if (location > 5 && location < 13) //bottom
        {
            if (card_obj.is_face_up == false)
            {
                return false;
            }

            //find the position of card_obj in bottom
            int limit_bottom = bottoms[location - 6].Count;
            int position_in_bottom = limit_bottom - 1;
            int face_up_num = limit_bottom - 1;
            bool is_face_up_num_find = false;

            for (int j = 0; j < limit_bottom; j++)
            {
                if (is_face_up_num_find == false)
                {
                    Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][j]);
                    if (card_tmp.GetComponent<Card>().is_face_up == true)
                    {
                        face_up_num = j;
                        is_face_up_num_find = true;
                    }
                }
                if (card_obj.name == bottoms[location - 6][j])
                {
                    position_in_bottom = j;
                }
            }

            for (int i = 2; i < 6; i++) //bottom to top
            {
                if (position_in_bottom != limit_bottom - 1)
                {
                    break;
                }
                else //The card is the tail of the bottom pile
                {
                    if (tops[i - 2].Count == 0)
                    {
                        if (value == 1)
                        {
							if (move){
								//Turn up the face of the upper card.
								bool is_flip = false;
								if (face_up_num == position_in_bottom)
								{
									if (position_in_bottom != 0)
									{
										Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
										card_tmp.GetComponent<Card>().is_face_up = true;
										UIM.score += 5;
										is_flip = true;
									}
								}

								//Moving
								card_obj.transform.parent = top_pos[i - 2].transform;
								StartCoroutine(card_obj.MoveTo(top_pos[i-2].transform.position, new Vector3(0, 0, -0.2f), true));

								card_obj.location = i;
								bottoms[location - 6].Remove(card_obj.name);
								tops[i - 2].Add(card_obj.name);
								
								UIM.score += 10;
								undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), is_flip?"1":"0"});
							}
							else
								bottom_to_top = true;
                            return true;
                        }
                    }
                    else
                    {
                        Transform card_check = top_pos[i - 2].transform.Find(tops[i - 2][tops[i - 2].Count - 1]);
                        Card card_check_obj = card_check.GetComponent<Card>();
                        string suit_check = card_check_obj.suit;
                        int value_check = card_check_obj.value;

                        if (suit == suit_check)
                        {
                            if (value == value_check + 1)
                            {
								if (move){
									//Turn up the face of the upper card.
									bool is_flip = false;
									if (face_up_num == position_in_bottom)
									{
										if (position_in_bottom != 0)
										{
											Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
											card_tmp.GetComponent<Card>().is_face_up = true;
											UIM.score += 5;
											is_flip = true;
										}
									}

									//Moving
									card_obj.transform.parent = top_pos[i - 2].transform;
									int length = tops[i-2].Count;
									StartCoroutine(card_obj.MoveTo(top_pos[i-2].transform.position, new Vector3(0, 0, -(length+1)*0.2f), true));

									card_obj.location = card_check_obj.location;
									bottoms[location - 6].Remove(card_obj.name);
									tops[i - 2].Add(card_obj.name);
									
									UIM.score += 10;
									undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), is_flip?"1":"0"});
								}
								else
									bottom_to_top = true;
                                return true;
                            }
                        }
                    }
                }
            }


            for (int i = 6; i < 13; i++) //bottom to bottom
            {
                if (i != location)
                {
                    if (bottoms[i - 6].Count == 0)
                    {
                        if (value == 13)
                        {
							if (move){
								//Turn up the face of the upper card.
								bool is_flip = false;
								if (face_up_num == position_in_bottom)
								{
									if (position_in_bottom != 0)
									{
										Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
										card_tmp.GetComponent<Card>().is_face_up = true;
										UIM.score += 5;
										is_flip = true;
									}
								}
								List<string> tmp_list = new List<string>();
								for (int j = position_in_bottom + 1; j < limit_bottom; j++)
								{
									Transform card_tmp = card_obj.transform.Find(bottoms[location - 6][j]);
									tmp_list.Add(card_tmp.transform.name);
									card_tmp.GetComponent<Card>().location = i;
								}
								card_obj.location = i;
								bottoms[location - 6].Remove(card_obj.name);
								bottoms[i - 6].Add(card_obj.name);
								foreach (string card in tmp_list)
								{
									bottoms[location - 6].Remove(card);
									bottoms[i - 6].Add(card);
								}

								StartCoroutine(card_obj.MoveTo(bottom_pos[i-6].transform.position, new Vector3(0, 0, -0.2f), true, () =>
								{
									foreach (string card in tmp_list)
									{
										Transform card_tmp = card_obj.transform.Find(card);
										card_tmp.GetComponent<Card>().movable = true;
										card_tmp.GetComponent<Card>().freeze_action = false;
										card_tmp.parent = bottom_pos[i - 6].transform;
									}
									card_obj.transform.parent = bottom_pos[i - 6].transform;
									undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), is_flip?"1":"0"});
								}));
							}
							else
								bottom_to_top = false;
                            return true;
                        }
                    }
                    else
                    {
                        Transform card_check = bottom_pos[i - 6].transform.Find(bottoms[i - 6][bottoms[i - 6].Count - 1]);
                        Card card_check_obj = card_check.GetComponent<Card>();
                        string suit_check = card_check_obj.suit;
                        int value_check = card_check_obj.value;

                        bool is_card_red = true;
                        bool is_card_check_red = true;
                        if (suit == "C" || suit == "S")
                        {
                            is_card_red = false;
                        }
                        if (suit_check == "C" || suit_check == "S")
                        {
                            is_card_check_red = false;
                        }

                        if (is_card_red != is_card_check_red)
                        {
                            if (value == value_check - 1)
                            {
								if (move){
									//Turn up the face of the upper card.
									bool is_flip = false;
									if (face_up_num == position_in_bottom)
									{
										if (position_in_bottom != 0)
										{
											Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
											card_tmp.GetComponent<Card>().is_face_up = true;
											UIM.score += 5;
											is_flip = true;
										}
									}
									int length = bottoms[i-6].Count;
									
									List<string> tmp_list = new List<string>();
									for (int j = position_in_bottom + 1; j < limit_bottom; j++)
									{
										Transform card_tmp = card_obj.transform.Find(bottoms[location - 6][j]);
										tmp_list.Add(card_tmp.transform.name);
										card_tmp.GetComponent<Card>().location = i;
									}
									card_obj.location = i;
									bottoms[location - 6].Remove(card_obj.name);
									bottoms[i - 6].Add(card_obj.name);
									foreach (string card in tmp_list)
									{
										bottoms[location - 6].Remove(card);
										bottoms[i - 6].Add(card);
									}
									
									StartCoroutine(card_obj.MoveTo(bottom_pos[i-6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true, () =>
									{
										foreach (string card in tmp_list)
										{
											Transform card_tmp = card_obj.transform.Find(card);
											card_tmp.GetComponent<Card>().movable = true;
											card_tmp.GetComponent<Card>().freeze_action = false;
											card_tmp.parent = bottom_pos[i - 6].transform;
										}
										card_obj.transform.parent = bottom_pos[i - 6].transform;
										undo_stack.Push(new string[] {i.ToString(), card_obj.name, location.ToString(), is_flip?"1":"0"});
									}));
								}
								else
									bottom_to_top = false;
                                return true;
                            }
                        }
                    }
                }
            }
        }
        else if (location <= 1) //deck
        {
			for (int i = 2; i < 6; i++) //deck to top
            {
				if (tops[i - 2].Count == 0)
				{
					if (value == 1)
					{
						if (move){
							//Moving
							StartCoroutine(card_obj.MoveTo(top_pos[i-2].transform.position, new Vector3(0, 0, -0.2f), true));
							card_obj.transform.parent = top_pos[i - 2].transform;

							card_obj.location = i;
							deck_pile.Remove(card_obj.name);
							tops[i - 2].Add(card_obj.name);
							deck_location--;
							UIM.score += 10;
							undo_stack.Push(new string[] {i.ToString(), card_obj.name, "1", "0"});
						}
						return true;
					}
				}
				else
				{
					Transform card_check = top_pos[i - 2].transform.Find(tops[i - 2][tops[i - 2].Count - 1]);
					Card card_check_obj = card_check.GetComponent<Card>();
					string suit_check = card_check_obj.suit;
					int value_check = card_check_obj.value;

					if (suit == suit_check)
					{
						if (value == value_check + 1)
						{
							if (move){
								//Moving
								card_obj.transform.parent = top_pos[i - 2].transform;
								int length = tops[i-2].Count;
								StartCoroutine(card_obj.MoveTo(top_pos[i-2].transform.position, new Vector3(0, 0, -(length+1)*0.2f), true));

								card_obj.location = card_check_obj.location;
								deck_pile.Remove(card_obj.name);
								tops[i - 2].Add(card_obj.name);
								deck_location--;
								
								UIM.score += 10;
								undo_stack.Push(new string[] {i.ToString(), card_obj.name, "1", "0"});
							}
							return true;
						}
					}
				}
            }
			
            for (int i = 6; i < 13; i++) //deck to bottom
            {
                if (bottoms[i - 6].Count == 0)
                {
                    if (value == 13)
                    {
						if (move){
							StartCoroutine(card_obj.MoveTo(bottom_pos[i - 6].transform.position, new Vector3(0, 0, -0.2f), true));
							card_obj.transform.parent = bottom_pos[i - 6].transform;
							card_obj.location = i;

							deck_pile.Remove(card_obj.name);
							bottoms[i - 6].Add(card_obj.name);
							deck_location--;
							
							UIM.score += 3;
							undo_stack.Push(new string[] {i.ToString(), card_obj.name, "1", "0"});
						}
                        return true;
                    }
                }
                else
                {
                    Transform card_check = bottom_pos[i - 6].transform.Find(bottoms[i - 6][bottoms[i - 6].Count - 1]);
                    Card card_check_obj = card_check.GetComponent<Card>();
                    string suit_check = card_check_obj.suit;
                    int value_check = card_check_obj.value;

                    bool is_card_red = true;
                    bool is_card_check_red = true;
                    if (suit == "C" || suit == "S")
                    {
                        is_card_red = false;
                    }
                    if (suit_check == "C" || suit_check == "S")
                    {
                        is_card_check_red = false;
                    }

                    if (is_card_red != is_card_check_red)
                    {
                        if (value == value_check - 1)
                        {
							if (move){
								card_obj.transform.parent = bottom_pos[i - 6].transform;
								int length = bottoms[i-6].Count;
								StartCoroutine(card_obj.MoveTo(bottom_pos[i-6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true));

								card_obj.location = i;
								deck_pile.Remove(card_obj.name);
								bottoms[i - 6].Add(card_obj.name);
								deck_location--;
								
								UIM.score += 3;
								undo_stack.Push(new string[] {i.ToString(), card_obj.name, "1", "0"});
							}
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
	
	public bool StackByDrag(Card card_obj, Card card_other){
		if (card_obj.location == card_other.location){ //In the same pile.
			return false;
		}
		else{
			if (card_obj.location == 1){ //deck_pile
				if (card_other.location>1 && card_other.location<6){ //deck to top
					if (card_obj.suit == card_other.suit)
					{
						if (card_obj.value == card_other.value + 1)
						{
							undo_stack.Push(new string[] {card_other.location.ToString(), card_obj.name, card_obj.location.ToString(), "0"});
							
							//Moving
							card_obj.transform.parent = top_pos[card_other.location - 2].transform;
							int length = tops[card_other.location - 2].Count;
							StartCoroutine(card_obj.MoveTo(top_pos[card_other.location - 2].transform.position, new Vector3(0, 0, -(length+1)*0.2f), true));

							card_obj.location = card_other.location;
							deck_pile.Remove(card_obj.name);
							tops[card_other.location - 2].Add(card_obj.name);
							deck_location--;
							
							UIM.score += 10;
							return true;
						}
					}
				}
				else if (card_other.location>5 && card_other.location<13){ //deck to bottom
					int limit_bottom_other = bottoms[card_other.location - 6].Count;
					int position_in_bottom_other = limit_bottom_other - 1;
					for (int j = 0; j < limit_bottom_other; j++)
						if (card_other.name == bottoms[card_other.location - 6][j])
							position_in_bottom_other = j;
					if (position_in_bottom_other != limit_bottom_other - 1)
						return false;
					
                    bool is_card_obj_red = true;
                    bool is_card_other_red = true;
                    if (card_obj.suit == "C" || card_obj.suit == "S")
                    {
                        is_card_obj_red = false;
                    }
                    if (card_other.suit == "C" || card_other.suit == "S")
                    {
                        is_card_other_red = false;
                    }

                    if (is_card_obj_red != is_card_other_red)
                    {
                        if (card_obj.value == card_other.value - 1)
                        {
							undo_stack.Push(new string[] {card_other.location.ToString(), card_obj.name, card_obj.location.ToString(), "0"});
							
                            card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
							int length = bottoms[card_other.location - 6].Count;
                            StartCoroutine(card_obj.MoveTo(bottom_pos[card_other.location - 6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true));

                            card_obj.location = card_other.location;
                            deck_pile.Remove(card_obj.name);
                            bottoms[card_other.location - 6].Add(card_obj.name);
                            deck_location--;
							UIM.score += 3;
                            return true;
                        }
                    }
				}
				else
					return false;
			}
			else if (card_obj.location>1 && card_obj.location<6){ //top
				if (card_other.location>5 && card_other.location<13){ //top to bottom
					int limit_bottom_other = bottoms[card_other.location - 6].Count;
					int position_in_bottom_other = limit_bottom_other - 1;
					for (int j = 0; j < limit_bottom_other; j++)
						if (card_other.name == bottoms[card_other.location - 6][j])
							position_in_bottom_other = j;
					if (position_in_bottom_other != limit_bottom_other - 1)
						return false;
					
                    bool is_card_obj_red = true;
                    bool is_card_other_red = true;
                    if (card_obj.suit == "C" || card_obj.suit == "S")
                    {
                        is_card_obj_red = false;
                    }
                    if (card_other.suit == "C" || card_other.suit == "S")
                    {
                        is_card_other_red = false;
                    }

                    if (is_card_obj_red != is_card_other_red)
                    {
                        if (card_obj.value == card_other.value - 1)
                        {
							undo_stack.Push(new string[] {card_other.location.ToString(), card_obj.name, card_obj.location.ToString(), "0"});
							
                            card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
							int length = bottoms[card_other.location - 6].Count;
                            StartCoroutine(card_obj.MoveTo(bottom_pos[card_other.location - 6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true));

                            tops[card_obj.location-2].Remove(card_obj.name);
                            bottoms[card_other.location - 6].Add(card_obj.name);
							card_obj.location = card_other.location;
							UIM.score -= 15;
                            return true;
                        }
                    }
				}
				else{
					return false;
				}
			}
			else if (card_obj.location>5 && card_obj.location<13){ //bottom
				//find the position of card_obj in bottom
				int limit_bottom = bottoms[card_obj.location - 6].Count;
				int position_in_bottom = limit_bottom - 1;
				int face_up_num = limit_bottom - 1;
				bool is_face_up_num_find = false;

				for (int j = 0; j < limit_bottom; j++)
				{
					if (is_face_up_num_find == false)
					{
						Transform card_tmp = bottom_pos[card_obj.location - 6].transform.Find(bottoms[card_obj.location - 6][j]);
						if (card_tmp.GetComponent<Card>().is_face_up == true)
						{
							face_up_num = j;
							is_face_up_num_find = true;
							UIM.score += 5;
						}
					}
					if (card_obj.name == bottoms[card_obj.location - 6][j])
					{
						position_in_bottom = j;
					}
				}
				
				if (card_other.location>1 && card_obj.location<6){ //bottom to top
					if (position_in_bottom != limit_bottom - 1)
						return false;
					
					if (card_obj.suit == card_other.suit)
					{
						if (card_obj.value == card_other.value + 1)
						{
							//Turn up the face of the upper card.
							bool is_flip = false;
							if (face_up_num == position_in_bottom)
							{
								if (position_in_bottom != 0)
								{
									Transform card_tmp = bottom_pos[card_obj.location - 6].transform.Find(bottoms[card_obj.location - 6][position_in_bottom - 1]);
									card_tmp.GetComponent<Card>().is_face_up = true;
									UIM.score += 5;
									is_flip = true;
								}
							}
							
							undo_stack.Push(new string[] {card_other.location.ToString(), card_obj.name, card_obj.location.ToString(), is_flip?"1":"0"});

							//Moving
							card_obj.transform.parent = top_pos[card_other.location - 2].transform;
							int length = tops[card_other.location - 2].Count;
							StartCoroutine(card_obj.MoveTo(top_pos[card_other.location - 2].transform.position, new Vector3(0, 0, -(length+1)*0.2f), true));

							bottoms[card_obj.location - 6].Remove(card_obj.name);
							tops[card_other.location - 2].Add(card_obj.name);
							card_obj.location = card_other.location;
							UIM.score += 10;
							return true;
						}
					}
				}
				else if (card_other.location>5 && card_obj.location<13){ //bottom to bottom
					int limit_bottom_other = bottoms[card_other.location - 6].Count;
					int position_in_bottom_other = limit_bottom_other - 1;
					for (int j = 0; j < limit_bottom_other; j++)
						if (card_other.name == bottoms[card_other.location - 6][j])
							position_in_bottom_other = j;
					if (position_in_bottom_other != limit_bottom_other - 1)
						return false;
					
					bool is_card_obj_red = true;
                    bool is_card_other_red = true;
                    if (card_obj.suit == "C" || card_obj.suit == "S")
                    {
                        is_card_obj_red = false;
                    }
                    if (card_other.suit == "C" || card_other.suit == "S")
                    {
                        is_card_other_red = false;
                    }

					if (is_card_obj_red != is_card_other_red)
					{
						if (card_obj.value == card_other.value - 1)
						{
							//Turn up the face of the upper card.
							bool is_flip = false;
							if (face_up_num == position_in_bottom)
							{
								if (position_in_bottom != 0)
								{
									Transform card_tmp = bottom_pos[card_obj.location - 6].transform.Find(bottoms[card_obj.location - 6][position_in_bottom - 1]);
									card_tmp.GetComponent<Card>().is_face_up = true;
									UIM.score += 5;
									is_flip = true;
								}
							}
							undo_stack.Push(new string[] {card_other.location.ToString(), card_obj.name, card_obj.location.ToString(), is_flip?"1":"0"});
							int length = bottoms[card_other.location - 6].Count;
							
							List<string> tmp_list = new List<string>();
							for (int j = position_in_bottom + 1; j < limit_bottom; j++)
							{
								Transform card_tmp = card_obj.transform.Find(bottoms[card_obj.location - 6][j]);
								tmp_list.Add(card_tmp.transform.name);
								card_tmp.GetComponent<Card>().location = card_other.location;
							}
							bottoms[card_obj.location - 6].Remove(card_obj.name);
							bottoms[card_other.location - 6].Add(card_obj.name);
							foreach (string card in tmp_list)
							{
								bottoms[card_obj.location - 6].Remove(card);
								bottoms[card_other.location - 6].Add(card);
							}
							card_obj.location = card_other.location;
							
							StartCoroutine(card_obj.MoveTo(bottom_pos[card_other.location - 6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true, () =>
							{
								foreach (string card in tmp_list)
								{
									Transform card_tmp = card_obj.transform.Find(card);
									card_tmp.GetComponent<Card>().movable = true;
									card_tmp.GetComponent<Card>().freeze_action = false;
									card_tmp.parent = bottom_pos[card_other.location - 6].transform;
								}
								card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
							}
							));
							return true;
						}
					}
				}
			}
		}
		return false;
	}

    private Transform FindByPosition(int pos, string name)
    {
        if (pos == 1)
            return deck_pile_pos.transform.Find(name);
        else if (pos > 1 && pos < 6)
            return top_pos[pos - 2].transform.Find(name);
        else
            return bottom_pos[pos - 6].transform.Find(name);
    }
	
	public bool Hint(bool allow_second_check){
		bool valid_action;
		Card tmp_card;
		
		for (int i=0; i<7; i++){ //bottom
			bool is_last = false;
			for (int j=0; j<bottoms[i].Count; j++){
				tmp_card = bottom_pos[i].transform.Find(bottoms[i][j]).GetComponent<Card>();
				if (!tmp_card.is_face_up)
					continue;
				else{
					if (j == 0 && tmp_card.value == 13)
						break;
					valid_action = AutoStack(tmp_card, false);
					if (valid_action){
						tmp_card.Glow();
						if (allow_second_check)
							StartCoroutine(tmp_card.Glow());
						return true;
					}
					if (j == bottoms[i].Count-1)
						is_last = true;
					break;
				}
			}
			if (!is_last && bottoms[i].Count != 0){
				tmp_card = bottom_pos[i].transform.Find(bottoms[i][bottoms[i].Count-1]).GetComponent<Card>();
				valid_action = AutoStack(tmp_card, false);
				if (bottom_to_top && valid_action){
					if (allow_second_check)
						StartCoroutine(tmp_card.Glow());
					return true;
				}
			}
		}
		
		if (deck_pile.Count != 0){
			tmp_card = deck_pile_pos.transform.Find(deck_pile[deck_pile.Count-1]).GetComponent<Card>();
			valid_action = AutoStack(tmp_card, false);
			if (valid_action){
				if (allow_second_check)
					StartCoroutine(tmp_card.Glow());
				return true;
			}
		}
		
		if (deck.Count != 0){
			Deck deck_glow = deck_pos.transform.GetComponent<Deck>();
			for (int i=deck_pile.Count-2; i>=0; i--){
				tmp_card = deck_pile_pos.transform.Find(deck_pile[i]).GetComponent<Card>();
				valid_action = AutoStack(tmp_card, false);
				if (valid_action){
					if (allow_second_check)
						StartCoroutine(deck_glow.Glow());
					return true;
				}
			}
			for (int i=deck.Count-1; i>=0; i--){
				tmp_card = deck_pos.transform.Find(deck[i]).GetComponent<Card>();
				valid_action = AutoStack(tmp_card, false);
				if (valid_action){
					if (allow_second_check)
						StartCoroutine(deck_glow.Glow());
					return true;
				}
			}
		}
		else{
			Deck deck_glow = deck_pos.transform.GetComponent<Deck>();
			for (int i=deck_pile.Count-2; i>=0; i--){
				tmp_card = deck_pile_pos.transform.Find(deck_pile[i]).GetComponent<Card>();
				valid_action = AutoStack(tmp_card, false);
				if (valid_action){
					if (allow_second_check)
						StartCoroutine(deck_glow.Glow());
					return true;
				}
			}
			for (int i=deck.Count-1; i>=0; i--){
				tmp_card = deck_pos.transform.Find(deck[i]).GetComponent<Card>();
				valid_action = AutoStack(tmp_card, false);
				if (valid_action){
					if (allow_second_check)
						StartCoroutine(deck_glow.Glow());
					return true;
				}
			}
		}
		
		if (allow_second_check){
			for (int i=0; i<4; i++){
				if (tops[i].Count != 0){
					tmp_card = top_pos[i].transform.Find(tops[i][tops[i].Count-1]).GetComponent<Card>();
					valid_action = AutoStack(tmp_card, false);
					if (valid_action){
						tmp_card.transform.parent = bottom_pos[hint_position].transform;
						tmp_card.location = hint_position + 6;
						tops[i].Remove(tmp_card.name);
						bottoms[hint_position].Add(tmp_card.name);
						
						bool second_check = Hint(false);
						
						tmp_card.transform.parent = top_pos[i].transform;
						tmp_card.location = i + 2;
						tops[i].Add(tmp_card.name);
						bottoms[hint_position].Remove(tmp_card.name);
						
						if (second_check && !bottom_to_top){
							print("here");
							print(tmp_card.name);
							StartCoroutine(tmp_card.Glow());
							return true;
						}
					}
				}
			}
		}
		return false;
	}

    public void Undo()
    {
		string[] regret_move = undo_stack.Pop();
		//print($"{regret_move[0]}, {regret_move[1]}, {regret_move[2]}");
		int location = int.Parse(regret_move[0]);
		int des = int.Parse(regret_move[2]);
		
		if (location == 0){ //Restack
			for (int i=deck.Count-1; i>=0; i--)
			{
				Card card_obj = deck_pos.transform.Find(deck[i]).GetComponent<Card>();
				card_obj.transform.parent = deck_pile_pos.transform;
				StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, -deck_location*0.2f), true));

				card_obj.is_face_up = true;
				card_obj.location = 1;
				deck_location++;
				
				deck_pile.Add(card_obj.name);
			}
			deck.Clear();
		}
		else if (location == 1){ //from deck to deck_pile. Go back to deck;
			Card card_obj = FindByPosition(location, regret_move[1]).GetComponent<Card>();
			card_obj.transform.parent = deck_pos.transform;
			if (deck.Count == 0)
				StartCoroutine(card_obj.MoveTo(deck_pos.transform.position, new Vector3(0, 0, -0.2f), true));
			else{
				Card card_other_obj = deck_pos.transform.Find(deck[deck.Count-1]).GetComponent<Card>();
				StartCoroutine(card_obj.MoveTo(deck_pos.transform.position, new Vector3(0, 0, -(deck.Count+1)*0.2f), true));
			}
			
			card_obj.is_face_up = false;
			card_obj.location = 0;
			deck_location--;
			
			deck.Add(card_obj.name);
			deck_pile.Remove(card_obj.name);
		}
		else if (location > 1 && location < 6){ //To top
			Card card_obj = FindByPosition(location, regret_move[1]).GetComponent<Card>();
			if (des == 1){ //from deck pile to top
				card_obj.transform.parent = deck_pile_pos.transform;
				if (deck_pile.Count == 0)
					StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, 0), true));
				else
					StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, -deck_location*0.2f), true));
				deck_location++;
				
				card_obj.location = des;
				deck_pile.Add(card_obj.name);
				tops[location - 2].Remove(card_obj.name);
			}
			else{ //from bottom to top
				int length = bottoms[des-6].Count;
				if (int.Parse(regret_move[3]) == 1)
					bottom_pos[des-6].transform.Find(bottoms[des-6][length-1]).GetComponent<Card>().is_face_up = false;
				
				card_obj.transform.parent = bottom_pos[des-6].transform;
				StartCoroutine(card_obj.MoveTo(bottom_pos[des-6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true));

				card_obj.location = des;
				tops[location-2].Remove(card_obj.name);
				bottoms[des-6].Add(card_obj.name);
			}
		}
		else if (location > 5 && location < 13){
			Card card_obj = FindByPosition(location, regret_move[1]).GetComponent<Card>();
			if (des == 1){ //from deck pile to bottom
				card_obj.transform.parent = deck_pile_pos.transform;
				if (deck_pile.Count == 0)
					StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, 0), true));
				else
					StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, new Vector3(0, 0, -deck_location*0.2f), true));
				deck_location++;
				
				card_obj.location = des;
				deck_pile.Add(card_obj.name);
				bottoms[location-6].Remove(card_obj.name);
			}
			else if (des > 1 && des < 6){ //from top to bottom
				card_obj.transform.parent = top_pos[des-2].transform;
				StartCoroutine(card_obj.MoveTo(top_pos[des-2].transform.position, new Vector3(0, 0, -(tops[des-2].Count+1)*0.2f), true));

				card_obj.location = des;
				bottoms[location-6].Remove(card_obj.name);
				tops[des-2].Add(card_obj.name);
			}
			else if (des > 5 && des < 13){
				int length = bottoms[des-6].Count;
				if (int.Parse(regret_move[3]) == 1)
					bottom_pos[des-6].transform.Find(bottoms[des-6][length-1]).GetComponent<Card>().is_face_up = false;
				
				int limit_bottom = bottoms[location - 6].Count;
				int position_in_bottom = limit_bottom - 1;

				for (int i = 0; i < limit_bottom; i++)
					if (card_obj.name == bottoms[location - 6][i])
						position_in_bottom = i;
				
				List<string> card_list = new List<string>();
				for (int i = position_in_bottom + 1; i < limit_bottom; i++) //set the parents of the lower cards to the one we move
				{
					Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][i]);
					card_tmp.parent = card_obj.transform;
					card_tmp.GetComponent<Card>().location = des;
					card_tmp.GetComponent<Card>().movable = false;
					card_tmp.GetComponent<Card>().freeze_action = true;
					card_list.Add(card_tmp.name);
				}
				card_obj.location = des;
				bottoms[location - 6].Remove(card_obj.name);
				bottoms[des - 6].Add(card_obj.name);
				foreach (string card in card_list)
				{
					bottoms[location - 6].Remove(card);
					bottoms[des - 6].Add(card);
				}
				
				StartCoroutine(card_obj.MoveTo(bottom_pos[des - 6].transform.position, new Vector3(0, -length*0.3f, -(length+1)*0.2f), true, () =>
				{
					foreach (string card in card_list)
					{
						Transform card_tmp = card_obj.transform.Find(card);
						card_tmp.GetComponent<Card>().movable = true;
						card_tmp.GetComponent<Card>().freeze_action = false;
						card_tmp.parent = bottom_pos[des - 6].transform;
					}
					card_obj.transform.parent = bottom_pos[des - 6].transform;
				}));
			}
		}
    }
	
	public void Initialize(){
		stop_glowing = true;
		
		StartCoroutine(Stop(0.1f, ()=>{
			foreach(string name in deck)
				Destroy(deck_pos.transform.Find(name).gameObject);
			deck.Clear();
			
			foreach(string name in deck_pile)
				Destroy(deck_pile_pos.transform.Find(name).gameObject);
			deck_pile.Clear();
			
			for (int i=0; i<4; i++){
				foreach(string name in tops[i])
					Destroy(top_pos[i].transform.Find(name).gameObject);
				tops[i].Clear();
			}
			for (int i=0; i<7; i++){
				foreach(string name in bottoms[i])
					Destroy(bottom_pos[i].transform.Find(name).gameObject);
				bottoms[i].Clear();
			}
			undo_stack.Clear();
			global_freeze = false;
			stop_glowing = false;
			PlayCards();
		}));
	}
	
	private IEnumerator Stop(float delay_time, System.Action callback = null)
    {
		yield return new WaitForSeconds(delay_time);
		callback?.Invoke();
    }
	
	public bool IsGameEnd(){
		if (deck.Count != 0)
			return false;
		if (deck_pile.Count != 0)
			return false;
		for (int i=0; i<7; i++)
			if (bottoms[i].Count != 0)
				return false;
		return true;
	}
	
	public bool IsCardAllFaceUp(){
		for (int i=0; i<7; i++)
			if (bottoms[i].Count != 0)
				if (bottom_pos[i].transform.Find(bottoms[i][0])!=null)
					if(!bottom_pos[i].transform.Find(bottoms[i][0]).GetComponent<Card>().is_face_up)
						return false;
		return true;
	}
	
	public bool AutoComplete(){
		for (int i=0; i<7; i++){
			if (bottoms[i].Count != 0){
				bool validity = AutoStack(bottom_pos[i].transform.Find(bottoms[i][bottoms[i].Count-1]).GetComponent<Card>(), false);
				if (validity && bottom_to_top){
					AutoStack(bottom_pos[i].transform.Find(bottoms[i][bottoms[i].Count-1]).GetComponent<Card>(), true);
					return true;
				}
			}
		}
		if (deck_pile.Count != 0){
			bool validity = AutoStack(deck_pile_pos.transform.Find(deck_pile[deck_pile.Count-1]).GetComponent<Card>(), true);
			if (validity)
				return true;
		}
		return false;
	}
}

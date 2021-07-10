using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        bottoms = new List<string>[] { bottom0, bottom1, bottom2, bottom3, bottom4, bottom5, bottom6 };
        tops = new List<string>[] { top0, top1, top2, top3 };
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
        for (int i=0; i<7; i++)
        {
            float y_offset = 0;
            float z_offset = 0.2f;
            foreach (string card in bottoms[i])
            {
                yield return new WaitForSeconds(0.01f);
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
        Vector3 shift = new Vector3(0, 0, -deck_location * 0.2f);
        StartCoroutine(card_obj.MoveTo(deck_pile_pos.transform.position, shift));

        card_obj.is_face_up = true;
        card_obj.location = 1;

        deck_pile.Add(card_obj.name);
        //deck.Remove(card_obj.name); //If deck.Clear() is enough, then this line is redundant.
        deck_location++;
    }

    public void DeckRestack()
    {
        deck.Clear();
        foreach (string card in deck_pile)
        {
            foreach (Transform child in deck_pile_pos.transform)
            {
                if (child.CompareTag("card"))
                {
                    if (child.GetComponent<Card>().name == card)
                    {
                        Card card_obj = child.GetComponent<Card>();
                        card_obj.transform.parent = deck_pos.transform;
                        Vector3 shift = new Vector3(0, 0, -deck_location * 0.2f);
                        StartCoroutine(card_obj.MoveTo(deck_pos.transform.position, shift));

                        card_obj.is_face_up = false;
                        card_obj.location = 0;
                        deck_location--;
                    }
                }
            }
            deck.Add(card);
        }
        deck_pile.Clear();
        deck_location = 0;
    }

    public bool AutoStack(Card card_obj)
    {
        int location = card_obj.location;
        string suit = card_obj.suit;
        int value = card_obj.value;

        if (location>1 && location<6) //top
        {
            for (int i=6; i<13; i++)
            {
				if (bottoms[i - 6].Count == 0)
				{
					if (value == 13)
					{
                        Vector3 shift = new Vector3(0, 0, -0.2f);
                        StartCoroutine(card_obj.MoveTo(bottom_pos[i - 6].transform.position, shift));
                        card_obj.transform.parent = bottom_pos[i - 6].transform;

                        card_obj.location = i;
                        tops[location-2].Remove(card_obj.name);
                        bottoms[i - 6].Add(card_obj.name);
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
                            card_obj.transform.parent = bottom_pos[i - 6].transform;
                            Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                            StartCoroutine(card_obj.MoveTo(card_check_obj.transform.position, shift));

                            card_obj.location = card_check_obj.location;
                            tops[location-2].Remove(card_obj.name);
                            bottoms[i - 6].Add(card_obj.name);
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
                            //Turn up the face of the upper card.
                            if (face_up_num == position_in_bottom)
                            {
                                if (position_in_bottom != 0)
                                {
                                    Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
                                    card_tmp.GetComponent<Card>().is_face_up = true;
                                }
                            }

                            //Moving
                            Vector3 shift = new Vector3(0, 0, -0.2f);
                            StartCoroutine(card_obj.MoveTo(top_pos[i - 2].transform.position, shift));
                            card_obj.transform.parent = top_pos[i - 2].transform;

                            card_obj.location = i;
                            bottoms[location - 6].Remove(card_obj.name);
                            tops[i - 2].Add(card_obj.name);
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
                                //Turn up the face of the upper card.
                                if (face_up_num == position_in_bottom)
                                {
                                    if (position_in_bottom != 0)
                                    {
                                        Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
                                        card_tmp.GetComponent<Card>().is_face_up = true;
                                    }
                                }

                                //Moving
                                card_obj.transform.parent = top_pos[i - 2].transform;
                                Vector3 shift = new Vector3(0, 0, -0.2f);
                                StartCoroutine(card_obj.MoveTo(card_check_obj.transform.position, shift));

                                card_obj.location = card_check_obj.location;
                                bottoms[location - 6].Remove(card_obj.name);
                                tops[i - 2].Add(card_obj.name);
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
                            //Turn up the face of the upper card.
                            if (face_up_num == position_in_bottom)
                            {
                                if (position_in_bottom != 0)
                                {
                                    Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
                                    card_tmp.GetComponent<Card>().is_face_up = true;
                                }
                            }

                            Vector3 shift = new Vector3(0, 0, -0.2f);
                            StartCoroutine(card_obj.MoveTo(bottom_pos[i - 6].transform.position, shift, () =>
                            {
								List<string> tmp_list = new List<string>();
								tmp_list.Add(card_obj.name);
                                for (int j = position_in_bottom + 1; j < limit_bottom; j++)
                                {
                                    Transform card_tmp = card_obj.transform.Find(bottoms[location - 6][j]);
									tmp_list.Add(card_tmp.transform.name);
                                    card_tmp.parent = bottom_pos[i - 6].transform;
                                    card_tmp.GetComponent<Card>().location = i;
                                }

                                card_obj.transform.parent = bottom_pos[i - 6].transform;
                                card_obj.location = i;
                                foreach (string card in tmp_list)
                                {
                                    bottoms[location - 6].Remove(card);
                                    bottoms[i - 6].Add(card);
                                }
                            }));
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
                                //Turn up the face of the upper card.
                                if (face_up_num == position_in_bottom)
                                {
                                    if (position_in_bottom != 0)
                                    {
                                        Transform card_tmp = bottom_pos[location - 6].transform.Find(bottoms[location - 6][position_in_bottom - 1]);
                                        card_tmp.GetComponent<Card>().is_face_up = true;
                                    }
                                }

                                Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                                StartCoroutine(card_obj.MoveTo(card_check_obj.transform.position, shift, () =>
                                {
									List<string> tmp_list = new List<string>();
									tmp_list.Add(card_obj.name);
                                    for (int j = position_in_bottom + 1; j < limit_bottom; j++)
                                    {
                                        Transform card_tmp = card_obj.transform.Find(bottoms[location - 6][j]);
										tmp_list.Add(card_tmp.transform.name);
                                        card_tmp.parent = bottom_pos[i - 6].transform;
										card_tmp.GetComponent<Card>().location = i;
                                    }

                                    card_obj.transform.parent = bottom_pos[i - 6].transform;
                                    card_obj.location = i;

                                    foreach (string card in tmp_list)
                                    {
                                        bottoms[location - 6].Remove(card);
                                        bottoms[i - 6].Add(card);
                                    }
                                }));
                                return true;
                            }
                        }
                    }
                }
            }
        }
        else if (location == 1) //deck
        {
			for (int i = 2; i < 6; i++) //deck to top
            {
				if (tops[i - 2].Count == 0)
				{
					if (value == 1)
					{
						//Moving
						Vector3 shift = new Vector3(0, 0, -0.2f);
						StartCoroutine(card_obj.MoveTo(top_pos[i - 2].transform.position, shift));
						card_obj.transform.parent = top_pos[i - 2].transform;

						card_obj.location = i;
						deck_pile.Remove(card_obj.name);
						tops[i - 2].Add(card_obj.name);
						deck_location--;
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
							//Moving
							card_obj.transform.parent = top_pos[i - 2].transform;
							Vector3 shift = new Vector3(0, 0, -0.2f);
							StartCoroutine(card_obj.MoveTo(card_check_obj.transform.position, shift));

							card_obj.location = card_check_obj.location;
							deck_pile.Remove(card_obj.name);
							tops[i - 2].Add(card_obj.name);
							deck_location--;
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
                        Vector3 shift = new Vector3(0, 0, -0.2f);
                        StartCoroutine(card_obj.MoveTo(bottom_pos[i - 6].transform.position, shift));
                        card_obj.transform.parent = bottom_pos[i - 6].transform;
                        card_obj.location = i;

                        deck_pile.Remove(card_obj.name);
                        bottoms[i - 6].Add(card_obj.name);
                        deck_location--;
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
                            card_obj.transform.parent = bottom_pos[i - 6].transform;
                            Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                            StartCoroutine(card_obj.MoveTo(card_check_obj.transform.position, shift));

                            card_obj.location = i;
                            deck_pile.Remove(card_obj.name);
                            bottoms[i - 6].Add(card_obj.name);
                            deck_location--;
                            return true;
                        }
                    }
                }
            }
        }
        StartCoroutine(card_obj.Shake());
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
							//Moving
							card_obj.transform.parent = top_pos[card_other.location - 2].transform;
							Vector3 shift = new Vector3(0, 0, -0.2f);
							StartCoroutine(card_obj.MoveTo(card_other.transform.position, shift));

							card_obj.location = card_other.location;
							deck_pile.Remove(card_obj.name);
							tops[card_other.location - 2].Add(card_obj.name);
							deck_location--;
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
                            card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
                            Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                            StartCoroutine(card_obj.MoveTo(card_other.transform.position, shift));

                            card_obj.location = card_other.location;
                            deck_pile.Remove(card_obj.name);
                            bottoms[card_other.location - 6].Add(card_obj.name);
                            deck_location--;
                            return true;
                        }
                    }
				}
				else{
					return false;
				}
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
                            card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
                            Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                            StartCoroutine(card_obj.MoveTo(card_other.transform.position, shift));

                            tops[card_obj.location-2].Remove(card_obj.name);
                            bottoms[card_other.location - 6].Add(card_obj.name);
							card_obj.location = card_other.location;
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
							if (face_up_num == position_in_bottom)
							{
								if (position_in_bottom != 0)
								{
									Transform card_tmp = bottom_pos[card_obj.location - 6].transform.Find(bottoms[card_obj.location - 6][position_in_bottom - 1]);
									card_tmp.GetComponent<Card>().is_face_up = true;
								}
							}

							//Moving
							card_obj.transform.parent = top_pos[card_other.location - 2].transform;
							Vector3 shift = new Vector3(0, 0, -0.2f);
							StartCoroutine(card_obj.MoveTo(card_other.transform.position, shift));

							bottoms[card_obj.location - 6].Remove(card_obj.name);
							tops[card_other.location - 2].Add(card_obj.name);
							card_obj.location = card_other.location;
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
							if (face_up_num == position_in_bottom)
							{
								if (position_in_bottom != 0)
								{
									Transform card_tmp = bottom_pos[card_obj.location - 6].transform.Find(bottoms[card_obj.location - 6][position_in_bottom - 1]);
									card_tmp.GetComponent<Card>().is_face_up = true;
								}
							}
							
							Vector3 shift = new Vector3(0, -0.3f, -0.2f);
							StartCoroutine(card_obj.MoveTo(card_other.transform.position, shift, () =>
							{
								List<string> tmp_list = new List<string>();
								tmp_list.Add(card_obj.name);
								for (int j = position_in_bottom + 1; j < limit_bottom; j++)
								{
									Transform card_tmp = card_obj.transform.Find(bottoms[card_obj.location - 6][j]);
									tmp_list.Add(card_tmp.transform.name);
									card_tmp.parent = bottom_pos[card_other.location - 6].transform;
									card_tmp.GetComponent<Card>().location = card_other.location;
								}

								card_obj.transform.parent = bottom_pos[card_other.location - 6].transform;
								foreach (string card in tmp_list)
								{
									bottoms[card_obj.location - 6].Remove(card);
									bottoms[card_other.location - 6].Add(card);
								}
								card_obj.location = card_other.location;
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

    public Transform FindByPosition(int pos, string name)
    {
        if (pos == 1)
            return deck_pile_pos.transform.Find(name);
        else if (pos > 1 && pos < 6)
            return top_pos[pos - 2].transform.Find(name);
        else
            return bottom_pos[pos - 6].transform.Find(name);
    }

    public void MoveByPosition(int pos1, string name1, int pos2, string name2)
    {
        Transform card1 = FindByPosition(pos1, name1);
        Transform card2 = FindByPosition(pos2, name2);

        if (pos1 <6) //pos1 is in top piles
        {
            if (pos2 == 1)
            {
                Vector3 shift = new Vector3(0, 0, -0.2f);
                StartCoroutine(card1.GetComponent<Card>().MoveTo(card2.position, shift));

                card1.parent = deck_pile_pos.transform;
                card1.GetComponent<Card>().location = pos2;
                tops[pos1 - 2].Remove(name1);
                deck_pile.Add(name1);
                deck_location++;
            }
            else
            {
                Vector3 shift = new Vector3(0, -0.3f, -0.2f);
                StartCoroutine(card1.GetComponent<Card>().MoveTo(card2.position, shift));

                card1.parent = bottom_pos[pos2 - 6].transform;
                card1.GetComponent<Card>().location = pos2;
                tops[pos1 - 2].Remove(name1);
                bottoms[pos2 - 6].Add(name1);
            }
        }
    }

    public void Regret()
    {

    }
}

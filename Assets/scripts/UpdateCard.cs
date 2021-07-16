using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCard : MonoBehaviour
{
    public Sprite card_face;
    public Sprite[] card_back_list;
	private Sprite card_back;

    private SpriteRenderer sprite_renderer;
    private Card card;
    private Solitaire solitaire;


    // Start is called before the first frame update
    void Start()
    {
        List<string> deck = Solitaire.GenerateDeck();
        solitaire = FindObjectOfType<Solitaire>();

        int i = 0;
        foreach(string card in deck)
        {
            if (this.name == card)
            {
                card_face = solitaire.card_faces[i];
                break;
            }
            i++;
        }
        sprite_renderer = GetComponent<SpriteRenderer>();
        card = GetComponent<Card>();
		card_back = card_back_list[solitaire.player_data.card_back_pref];
    }

    // Update is called once per frame
    void Update()
    {
        if (card.is_face_up == true)
        {
            sprite_renderer.sprite = card_face;
        }
        else
        {
            sprite_renderer.sprite = card_back;
        }
    }
}

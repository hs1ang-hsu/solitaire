using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class UpdateCard : MonoBehaviour
{
    public Sprite card_face;
    public List<Sprite> card_back_list;
	public SpriteAtlas atlas;

	private List<string> card_back = new List<string>() {"blue_tiger", "red_tiger", "blue_bird", "red_bird", "orange_bird", "black_bird"};

    private SpriteRenderer sprite_renderer;
    private Card card;
    private Solitaire solitaire;

	void Awake(){
		for (int k = 0; k < card_back.Count; k++)
			card_back_list.Add(atlas.GetSprite(card_back[k]));
	}
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
            sprite_renderer.sprite = card_back_list[solitaire.player_data.card_back_pref];
        }
    }
}

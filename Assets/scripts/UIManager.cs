using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Text score_text;
	public Text click_count_text;
	public Text time_text;
	
	public int score;
    public int click_count;
	public int undo_count;
    private float time;
    private bool game_start;
	private bool undo_delay;
	
	private Solitaire solitaire;

    // Start is called before the first frame update
    void Start()
    {
		score = 0;
        click_count = 0;
		undo_count = 0;
        time = 0;
        game_start = false;
		undo_delay = false;
		
        solitaire = FindObjectOfType<Solitaire>();
    }

    // Update is called once per frame
    void Update()
    {
		if (score < 0)
			score = 0;
        if (game_start == false)
        {
            if (click_count > 0)
            {
                game_start = true;
                time += Time.deltaTime;
            }
        }
        else
        {
            time += Time.deltaTime;
        }
		score_text.text = score.ToString();
		click_count_text.text = click_count.ToString();
		time_text.text = Time2String();
    }
	
    string Time2String(){
		int minute = (int)System.Math.Floor(time/60);
		int second = (int)System.Math.Floor(time) % 60;
		string res;
		if (minute < 10)
			res = "0" + minute;
		else if (minute < 100)
			res = minute.ToString();
		else
			res = "99";
		if (second < 10)
			res += ":0" + second;
		else
			res += ":" + second;
		return res;
	}
	
	public void HintEvent(){
		bool success = solitaire.Hint(true);
		if (!success){
			//new window: "no possible moves. Start new game?"
		}
		//print(success);
	}
	
	public void UndoEvent(){
		if (!undo_delay){
			undo_delay = true;
			if (click_count != 0 && undo_count != 0){
				solitaire.Undo();
				undo_count--;
				click_count++;
			}
			StartCoroutine(Stop(()=>{undo_delay = false;}));
		}
	}
	
	private IEnumerator Stop(System.Action callback = null)
    {
		yield return new WaitForSeconds(0.2f);
		callback?.Invoke();
    }
}

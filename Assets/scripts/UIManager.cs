using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Text score_text;
	public Text click_count_text;
	public Text time_text;
	public Text score_result_text;
	
	public Button menu_button;
	public Button resume_button;
	public Button hint_button;
	
	public GameObject restart_menu;
	public GameObject end_game_menu;
	
	public int score;
    public int click_count;
	public int undo_count;
    private float time;
    private bool game_start;
	private bool undo_delay;
	public bool hint_delay;
	private bool game_paused;
	
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
		hint_delay = false;
		game_paused = false;
		
        solitaire = FindObjectOfType<Solitaire>();
    }

    // Update is called once per frame
    void Update()
    {
		if (score < 0)
			score = 0;
		if (!game_paused){
			if (game_start == false)
			{
				if (click_count > 0)
				{
					game_start = true;
					time += Time.deltaTime;
				}
			}
			else
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
		if (!hint_delay){
			hint_delay = true;
			bool success = solitaire.Hint(true);
			if (!success){
				restart_menu.SetActive(true);
				Pause();
			}
			StartCoroutine(Stop(2f, ()=>{hint_delay = false;}));
		}
	}
	
	public void UndoEvent(){
		if (!undo_delay){
			undo_delay = true;
			if (click_count != 0 && undo_count != 0){
				solitaire.Undo();
				undo_count--;
				click_count++;
			}
			StartCoroutine(Stop(0.3f, ()=>{undo_delay = false;}));
		}
	}
	
	private IEnumerator Stop(float delay_time, System.Action callback = null)
    {
		yield return new WaitForSeconds(delay_time);
		callback?.Invoke();
    }
	
	public void Pause(){
		if (!game_paused){
			solitaire.movable = false;
			game_paused = true;
			menu_button.interactable = false;
			resume_button.interactable = false;
			hint_button.interactable = false;
		}
	}
	
	public void Resume(){
		if (game_paused){
			solitaire.movable = true;
			game_paused = false;
			menu_button.interactable = true;
			resume_button.interactable = true;
			hint_button.interactable = true;
		}
	}
	
	public void NewGame(){
		if (game_paused){
			solitaire.Initialize();
			score = 0;
			click_count = 0;
			undo_count = 0;
			time = 0;
			game_start = false;
			undo_delay = false;
			hint_delay = false;
			game_paused = false;
			menu_button.interactable = true;
			resume_button.interactable = true;
			hint_button.interactable = true;
		}
	}
	
	public void EndGame(){
		end_game_menu.SetActive(true);
		Pause();
		int res = score;
		if (time < 300)
			res += (300 - (int)time) * 2;
		if (click_count < 300)
			res += (300 - click_count) * 2;
		score_result_text.text = res.ToString();
		Leaderboard[] leaderboard = solitaire.player_data.leaderboard;
		Leaderboard result = new Leaderboard() {score=score, time=(int)time, click_count=click_count};
		for (int i=0; i<10; i++){
			if (result > leaderboard[i]){
				for (int j=9; j>i; j--)
					leaderboard[j].assign(leaderboard[j-1]);
				leaderboard[i].assign(result);
				break;
			}
		}
	}
}

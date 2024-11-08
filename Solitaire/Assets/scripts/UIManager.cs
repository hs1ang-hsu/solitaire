﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class UIManager : MonoBehaviour
{
	public TMP_Text[] game_text;
	public TMP_Text[] result_text;
	public TMP_Text[] best_result_text;
	public TMP_Text[] leaderboard_score;
	public TMP_Text[] leaderboard_time;
	public TMP_Text[] leaderboard_click_count;
	
	public Button menu_button;
	public Button resume_button;
	public Button hint_button;
	
	public AudioMixer audio_mixer;
	
	public Slider slider;
	
	public GameObject restart_menu;
	public GameObject end_game_menu;
	public GameObject auto_complete_menu;
	
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
		//print(solitaire.player_data.sound_effect_volume);
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
		game_text[0].text = score.ToString();
		game_text[1].text = Time2String(time);
		game_text[2].text = click_count.ToString();
    }
	
    string Time2String(float t){
		int minute = (int)System.Math.Floor(t/60);
		int second = (int)System.Math.Floor(t) % 60;
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
				score -= 10;
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
			solitaire.global_freeze = true;
			game_paused = true;
			menu_button.interactable = false;
			resume_button.interactable = false;
			hint_button.interactable = false;
		}
	}
	
	public void Resume(){
		if (game_paused){
			solitaire.global_freeze = false;
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
		
		result_text[0].text = res.ToString();
		result_text[1].text = Time2String(time);
		result_text[2].text = click_count.ToString();
		
		Leaderboard[] leaderboard = solitaire.player_data.leaderboard;
		Leaderboard result = new Leaderboard() {score=res, time=(int)time, click_count=click_count};
		for (int i=0; i<10; i++){
			if (result > leaderboard[i]){
				for (int j=9; j>i; j--)
					leaderboard[j].assign(leaderboard[j-1]);
				leaderboard[i].assign(result);
				solitaire.data_io.WritePlayerData(solitaire.player_data);
				break;
			}
		}
		
		best_result_text[0].text = leaderboard[0].score.ToString();
		best_result_text[1].text = Time2String(leaderboard[0].time);
		best_result_text[2].text = leaderboard[0].click_count.ToString();
	}
	
	public void CallAutoCompleteMenu(){
		auto_complete_menu.SetActive(true);
		solitaire.global_freeze = true;
	}
	
	public void AutoComplete(){
		StartCoroutine(AutoCompleteSubtask(() =>
		{
			solitaire.global_freeze = false;
			EndGame();
		}));
	}
	
	private IEnumerator AutoCompleteSubtask(System.Action callback = null){
		while (!solitaire.IsGameEnd()){
			bool success = solitaire.AutoComplete();
			if (!success){
				if (solitaire.deck.Count == 0){
					solitaire.DeckRestack();
					click_count++;
				}
				solitaire.DeckCardActions(solitaire.deck_pos.transform.Find(solitaire.deck[solitaire.deck.Count-1]).GetComponent<Card>());
			}
			click_count++;
			yield return new WaitForSeconds(0.08f);
		}
		callback?.Invoke();
	}
	
	public void GenerateLeaderboard(){
		Leaderboard[] leaderboard = solitaire.player_data.leaderboard;
		for (int i=0; i<10; i++){
			leaderboard_score[i].text = leaderboard[i].score.ToString();
			leaderboard_time[i].text = Time2String(leaderboard[i].time);
			leaderboard_click_count[i].text = leaderboard[i].click_count.ToString();
		}
	}
	
	public void SetSoundEffectVolume (float sound_effect_volume){
		audio_mixer.SetFloat("sound_effect", sound_effect_volume);
		solitaire.player_data.sound_effect_volume = sound_effect_volume;
		solitaire.data_io.WritePlayerData(solitaire.player_data);
	}
	
	public void debug(){
		for (int i=0; i<7; i++){
			print($"bottom {i}");
			foreach(string card in solitaire.bottoms[i])
				print(card);
		}
	}
}

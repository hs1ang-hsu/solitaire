using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataIO : MonoBehaviour
{
	private string file_name = "PlayerData";
	
    public PlayerData LoadPlayerData(){
		PlayerData player_data;
		string tmp_path = Path.Combine(Application.persistentDataPath, file_name + ".json");
		if (File.Exists(tmp_path)){
			using (StreamReader stream = new StreamReader(tmp_path)){
				string json = stream.ReadToEnd();
				player_data = JsonUtility.FromJson<PlayerData>(json);
			}
		}
		else{
			print("write new.");
			TextAsset text = Resources.Load<TextAsset>(file_name);
			string json_text = text.ToString();
			player_data = new PlayerData();
			player_data.card_back_pref = 0;
			player_data.leaderboard = new Leaderboard[10];
			for (int i=0; i<10; i++)
				player_data.leaderboard[i] = new Leaderboard() {score=0, time=0, click_count=0};
			WritePlayerData(player_data);
		}
		return player_data;
	}
	
	public void WritePlayerData(PlayerData player_data){
		string tmp_path = Path.Combine(Application.persistentDataPath, file_name + ".json");
		using (StreamWriter stream = new StreamWriter(tmp_path)){
			string json = JsonUtility.ToJson(player_data);
			stream.Write(json);
		}
	}
}

using System;

[Serializable]
public class Leaderboard
{
    public int score;
	public int time;
	public int click_count;
	
	public static bool operator <(Leaderboard a, Leaderboard b){
		if (a.score != b.score)
			return a.score < b.score;
		else
			return (a.time==b.time) ? (a.click_count>b.click_count):(a.time>b.time);
	}
	public static bool operator >(Leaderboard a, Leaderboard b){
		if (a.score != b.score)
			return a.score > b.score;
		else
			return (a.time==b.time) ? (a.click_count<b.click_count):(a.time<b.time);
	}
	public void assign(Leaderboard a){
		this.score = a.score;
		this.time = a.time;
		this.click_count = a.click_count;
	}
}

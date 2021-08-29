using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsListener
{
	public Button[] new_game_button;
	private string GooglePlay_ID = "4240341";
	private string Android_adUnit_ID = "Interstitial_Android";
	private bool test_mode = false;
	private UIManager UIM;
	
    // Start is called before the first frame update
    void Awake()
    {
		Advertisement.AddListener(this);
        Advertisement.Initialize(GooglePlay_ID, test_mode, true, this);
		Advertisement.Load(Android_adUnit_ID);
		foreach (var button in new_game_button){
			button.onClick.AddListener(ShowAd);
		}
    }
	
	void Start(){
		UIM = FindObjectOfType<UIManager>();
	}
	
	public void OnInitializationComplete(){
		//Debug.Log("Unity Ads initialization complete.");
	}
	
	public void OnInitializationFailed(UnityAdsInitializationError error, string message){
		//Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
	}
	
	public void ShowAd(){
		Advertisement.Show(Android_adUnit_ID);
	}
	
	public void OnUnityAdsReady(string adUnit_ID){
		//print("ready");
	}
	
	public void OnUnityAdsDidError(string message){
		//print("error");
	}
	
	public void OnUnityAdsDidStart(string adUnit_ID){
		//print("start");
	}
	
	public void OnUnityAdsDidFinish(string adUnit_ID, ShowResult show_result){
		/*
		if (show_result == ShowResult.Finished){
			print("finished");
		}
		else if (show_result == ShowResult.Skipped){
			print("skipped");
		}
		else if (show_result == ShowResult.Failed){
			print("failed");
		}
		*/
		UIM.NewGame();
		Advertisement.Load(Android_adUnit_ID);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public int click_count;
    private float timer;
    private bool game_start;

    // Start is called before the first frame update
    void Start()
    {
        click_count = 0;
        timer = 0;
        game_start = false;
    }

    // Update is called once per frame
    void Update()
    {
		GetMouseClick();
        if (game_start == false)
        {
            if (click_count > 0)
            {
                game_start = true;
                timer += Time.deltaTime;
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
	
    void GetMouseClick()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float target_aspect = 9f / 16f;
		float window_aspect = (float)Screen.width / (float)Screen.height;
		float scale_height = window_aspect / target_aspect;
		
		Camera camera = GetComponent<Camera>();
		
		if (scale_height < 1f){
			Rect rect = camera.rect;
			rect.width = 1f;
			rect.height = scale_height;
			rect.x = 0;
			rect.y = (1f - scale_height) / 2f;
			
			camera.rect = rect;
		}
		else{
			float scale_width = 1f / scale_height;
			Rect rect = camera.rect;
			rect.width = scale_width;
			rect.height = 1f;
			rect.x = (1f - rect.width) / 2f;
			rect.y = 0;
			
			camera.rect = rect;
		}
    }
}

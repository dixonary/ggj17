﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartGen : MonoBehaviour {

    /* Generate a body part at random. */
    GameObject skin;
    GameObject lines;

    String[] parts = { "arm", "leg" };

    void Start () {
        
        var part = parts[(int)(UnityEngine.Random.value * parts.Length)];
        SpriteRenderer sr;
        Sprite res;
        
        lines = new GameObject();
        sr = lines.AddComponent<SpriteRenderer>();
        res = Resources.Load<Sprite>("Images/" + part + "_lines");
        Debug.Log(res);
        sr.sprite = res;
        sr.transform.parent = transform;


        skin = new GameObject();
        sr = skin.AddComponent<SpriteRenderer>();
        res = Resources.Load<Sprite>("Images/"+part + "_colour");
        Debug.Log(res);
        sr.sprite = res;

        float r, g, b;
        Func<float> colRandom = (() => UnityEngine.Random.value * 1 - 0.5f);
        if (UnityEngine.Random.value > 0.5) {
            // lighter skin tones (http://johnthemathguy.blogspot.co.uk/2013/08/what-color-is-human-skin.html)
            r = 224.3f + 9.6f* colRandom();
            g = 193.1f + 17f * colRandom();
            b = 177.6f + 21f * colRandom();

        }
        else {
            // darker skin tones
            r = 168.8f + 38.5f * colRandom();
            g = 122.5f + 32.1f * colRandom();
            b =  96.7f + 26.3f * colRandom();
        }

        sr.color = new Color(r/255f,g/255f,b/255f);
        sr.transform.parent = transform;

        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        transform.localPosition = new Vector3(0.1f, 0.1f, 1.0f);
    }
	
	void Update () {
		
	}
}

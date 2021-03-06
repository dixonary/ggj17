﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.EyeTracking;
using System.Linq;
using MidiJack;

[RequireComponent (typeof (MeshFilter))]
public class LaserDrawer : MonoBehaviour {
    
    private Queue<Vector2> _queue;
    private int _movingAverageLen = 60;
    private Vector2 _avg;
    private Vector2 _lerpPoint;

    private float _topMax = 10f/127;
    private float _botMax = 35f / 127;
    private float _topCurrent = 0f;
    private float _botCurrent = 0f;

    public bool _active = false;

    /* Aim circle */
    GameObject _aimObject;
    public float _aimRadius = 0.001f;

    void Start() {
        _queue = new Queue<Vector2>();
        gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("red_mat");

        _aimObject = new GameObject();
        _aimObject.transform.localScale = new Vector3(_aimRadius * 2, _aimRadius * 2, 1);
        var spr = _aimObject.AddComponent<SpriteRenderer>();
        spr.sprite = Resources.Load<Sprite>("Circle");
        spr.color = Color.red;
        var body = _aimObject.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        var collider = _aimObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
    }


    void FixedUpdate() {
        var knobOne = 43f / 127;   //MidiMaster.GetKnob(21, 0);
        var knobTwo = 5f / 127;    //MidiMaster.GetKnob(22, 0.5f);
        //var knobThree = 35f / 127; //MidiMaster.GetKnob(23, 0.5f);
        //var knobFour = 10f / 127;  //MidiMaster.GetKnob(24, 0.25f);

        if(_active) {
            _topCurrent = Mathf.Min(_topCurrent + Time.deltaTime * 2, _topMax);
            _botCurrent = Mathf.Min(_botCurrent + Time.deltaTime * 2, _botMax);
        }
        else {
            _topCurrent = Mathf.Max(_topCurrent - Time.deltaTime * 2, 0);
            _botCurrent = Mathf.Max(_botCurrent - Time.deltaTime * 2, 0);
        }


        int actualLen = (int)(1 + 2 * knobTwo * _movingAverageLen);

        var _gp = InputControls.getPosition();
        _queue.Enqueue(_gp);
        while (_queue.Count > actualLen)
            _queue.Dequeue();

        //Fixing near values
        _avg = new Vector2(_queue.Average(x => x.x), _queue.Average(x => x.y));
        var _dist = _avg - _gp;
        if (_dist.magnitude < knobOne * 0.1)
        {
            _gp = _avg;
        }

        if (_lerpPoint == null)
        {
            _lerpPoint = _avg;
        }
        else
        {
            _lerpPoint += (_avg - _lerpPoint) / 20;
        }


        // update the mesh!
        var centerPoint = new Vector2(0f, -1f);
        var lerpViewpoint = new Vector2((_lerpPoint.x * 2 - 1) * Camera.main.aspect, _lerpPoint.y * 2 - 1);

        var distance = lerpViewpoint - centerPoint;
        var normDist = distance.normalized;
        var farDiam = 0.1f * _topCurrent * (1 - lerpViewpoint.y * 0.5f);
        var angle = Mathf.Atan2(distance.y, distance.x) - Mathf.PI / 2;

        var leftPoint = new Vector2(centerPoint.x - (0.1f * _botCurrent) / Mathf.Max(0.2f, Mathf.Cos(angle)), centerPoint.y);
        var rightPoint = new Vector2(centerPoint.x + (0.1f * _botCurrent) / Mathf.Max(0.2f, Mathf.Cos(angle)), centerPoint.y);


        var farRightPoint = new Vector2(lerpViewpoint.x + normDist.y * farDiam, lerpViewpoint.y - normDist.x * farDiam);
        var farLeftPoint = new Vector2(lerpViewpoint.x - normDist.y * farDiam, lerpViewpoint.y + normDist.x * farDiam);
        //var farLeftPoint = new Vector2((_lerpPoint.x*2 - 1) *Camera.main.aspect - (0.1f * knobFour * (1-_lerpPoint.y*0.5f)), (_lerpPoint.y*2-1));
        //var farRightPoint = new Vector2((_lerpPoint.x*2 - 1 )* Camera.main.aspect + (0.1f * knobFour * (1-_lerpPoint.y*0.5f)), (_lerpPoint.y*2-1));

        Vector2[] vertexArray = { leftPoint, rightPoint, farRightPoint, farLeftPoint };
        Vector3[] vertex3d = vertexArray.Select(x => new Vector3(x.x, x.y, 0f)).ToArray();
        int[] indices = { 1, 0, 2, 2, 0, 3 };
        Mesh msh = new Mesh();
        msh.vertices = vertex3d;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = msh;

        updateHitbox((farRightPoint + farLeftPoint) / 2);
    }
    
    void updateHitbox(Vector2 p)
    {
        _aimObject.transform.position = p;
    }

    public void Activate() {
        if (_active) return;
        _active = true;
        AkSoundEngine.PostEvent("LaserOn", this.gameObject);
    }

    public void Deactivate() {
        if (!_active) return;
        AkSoundEngine.PostEvent("LaserOff", this.gameObject);
        _active = false;
    }
}


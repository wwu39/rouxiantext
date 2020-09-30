using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VibratoManager : MonoBehaviour
{
    static Vector2 defRes = new Vector2(1334, 750);
    static float lowSpeed = .4f;
    public class TouchTracker
    {
        public Vector2 lastPos;
        public float leftMax;
        public float rightMax;
        public int leftToRightCount;
        public int rightToLeftCount;
        public float time;
        public float cordLength;
        public string cordLengthComps;
        public Vector2 velocity;
        public float speed;
        public float distance;
        public TouchTracker(Touch t)
        {
            lastPos = t.position;
            leftMax = rightMax = -1;
            time = Time.time;
            cordLength = 0;
        }
        public string GetStateString()
        {
            float frac = cordLength / Screen.width;
            string ret = "";
            bool isLowSpeed = IsLowSpeed(speed, frac);
            ret += isLowSpeed ? "慢" : "快";
            SoundPlayer.SetVibRate(isLowSpeed ? VibRate.slow : VibRate.fast);
            string deepness = "揉";
            VibDepth vd = VibDepth.no;
            if (frac < VibratoManager.deepness[0])
            {
                deepness = "浅揉";
                vd = VibDepth.shallow;
            }
            else if (frac > VibratoManager.deepness[1])
            {
                deepness = "深揉";
                vd = VibDepth.deep;
            }
            //ret += cordLength + "=" + cordLengthComps;
            ret += deepness;
            SoundPlayer.SetVibDepth(vd);
            return ret;
        }
        static bool IsLowSpeed(float speed, float cordLengthFrac)
        {
            return speed * (1 - cordLengthFrac) < lowSpeed * Screen.width;
        }
    }

    public static VibratoManager ins;
    static float[] deepness = new float[2] { 0.2f, 0.5f };
    [SerializeField] Image[] deepnessZones;
    [SerializeField] float cordPos;
    [SerializeField] Text log;
    [SerializeField] GameObject trail;
    TouchTracker tt;
    int speedUpdateCount = 0;
    int speedUpdateRate = 30;
    private void Awake()
    {
        ins = this;
    }
    private void Start()
    {
        var cord = Instantiate(Resources.Load<GameObject>("Cord"), GameObject.Find("Canvas").transform);
        var pos = (cord.transform as RectTransform).anchoredPosition;
        pos.x = -1334 / 2 + 1334 * cordPos;
        (cord.transform as RectTransform).anchoredPosition = pos;
        var size = deepnessZones[0].rectTransform.sizeDelta;
        size.x = deepness[0] * 1334;
        deepnessZones[0].rectTransform.sizeDelta = size;
        size = deepnessZones[1].rectTransform.sizeDelta;
        size.x = deepness[1] * 1334;
        deepnessZones[1].rectTransform.sizeDelta = size;
        size = deepnessZones[2].rectTransform.sizeDelta;
        size.x = 1334;
        deepnessZones[2].rectTransform.sizeDelta = size;
        log.text = "没有在揉";
    }

    private void FixedUpdate()
    {
        TouchControl();
    }
    void TouchControl()
    {
        if (Input.touchCount == 0)
        {
            tt = null;
            SoundPlayer.SetVolume(0);
            log.text = "没有在揉";
            return;
        }
        SoundPlayer.SetVolume(1);
        Touch t = Input.GetTouch(0);
        if (tt == null) tt = new TouchTracker(t);
        var trail = Instantiate(Resources.Load<GameObject>("Trail"), GameObject.Find("Canvas").transform);
        (trail.transform as RectTransform).anchoredPosition = ScreenToCanvasPos(t.position);
        (trail.transform as RectTransform).localScale = 30 * Vector3.one;
        tt.velocity = t.position - tt.lastPos;

        float cpos = GetCordPos();
        if (t.position.x > cpos)
        {
            float right = t.position.x - cpos;
            if (right > tt.rightMax) tt.rightMax = right;
        }
        else if (t.position.x < cpos)
        {
            float left = cpos - t.position.x;
            if (left > tt.leftMax) tt.leftMax = left;
        }

        if (t.position.x > cpos && tt.lastPos.x < cpos)
        {
            tt.rightToLeftCount++;
            IfFinishOneRound();
        }

        if (t.position.x < cpos && tt.lastPos.x > cpos)
        {
            tt.leftToRightCount++;
            IfFinishOneRound();
        }

        // update speed
        tt.distance += (t.position - tt.lastPos).magnitude;
        if (speedUpdateCount >= speedUpdateRate)
        {
            float time = speedUpdateRate * Time.fixedDeltaTime;
            tt.speed = tt.distance / time;
            tt.distance = 0;
            tt.time = 0;
            speedUpdateCount = 0;
        }
        else
        {
            ++speedUpdateCount;
        }
        tt.lastPos = t.position;
        log.text = tt.GetStateString();
    }
    void IfFinishOneRound()
    {
        if (Mathf.Abs(tt.rightToLeftCount - tt. rightToLeftCount) > 1)
        {
            tt = null;
            log.text = "没有在揉";
            print("没有在揉");
            return;
        }
        if (tt.leftToRightCount == tt.rightToLeftCount)
        {
            //float newTime = Time.time;
           // float deltaTime = newTime - tt.time;
            if (tt.leftMax > 0 && tt.rightMax > 0)
            {
                tt.cordLength = tt.leftMax + tt.rightMax;
                tt.cordLengthComps = tt.leftMax + "+" + tt.rightMax;
                // tt.speed = tt.cordLength / deltaTime;
            }
            //tt.time = newTime;
            tt.leftMax = tt.rightMax = -1;
            print("Finish round " + tt.leftToRightCount + " " + tt.speed);
        }
    }
    public static Vector3 ScreenToWorldPos(Vector3 pos)
    {
        Plane p = new Plane(Vector3.back, Vector3.zero);
        float ent;
        var ray = ins.GetComponent<Camera>().ScreenPointToRay(pos);
        if (p.Raycast(ray, out ent))
            return ray.GetPoint(ent);
        return Vector3.zero;
    }
    public static float GetCordPos()
    {
        return Screen.width * ins.cordPos;
    }
    public static Vector2 ScreenToCanvasPos(Vector2 pos)
    {
        float frac_x = pos.x / Screen.width;
        float frac_y = pos.y / Screen.height;
        float x = defRes.x * frac_x - defRes.x / 2;
        float y = defRes.y * frac_y - defRes.y / 2;
        return new Vector2(x, y);
    }
}

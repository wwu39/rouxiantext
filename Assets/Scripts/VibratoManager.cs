using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VibratoManager : MonoBehaviour
{
    static Vector2 defRes = new Vector2(1334, 750);
    public class TouchTracker
    {
        public int id { private set; get; }
        public bool isValid;
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
        public TouchTracker(Touch t)
        {
            id = t.fingerId;
            isValid = false;
            lastPos = t.position;
            leftMax = rightMax = -1;
            time = Time.time;
            cordLength = 0;
        }
        public void Reset(Touch t)
        {
            print("Reset Called");
            if (t.fingerId != id)
            {
                Debug.LogError("TouchTracker: Wrong Reset " + t.fingerId + " to " + id);
                return;
            }
            isValid = false;
            lastPos = t.position;
            leftMax = rightMax = -1;
            time = Time.time;
            cordLength = 0;
        }
        public void Update()
        {

        }
        public string GetStateString()
        {
            string ret = "触摸";
            ret += id + ": ";
            ret += speed < 0.06f * Screen.width ? "慢" : "快";
            float frac = cordLength / Screen.width;
            string deepness = "揉";
            if (frac < VibratoManager.deepness[0]) deepness = "浅揉";
            else if (frac > VibratoManager.deepness[1]) deepness = "深揉";
            //ret += cordLength + "=" + cordLengthComps;
            ret += deepness;
            return ret;
        }
    }

    public static VibratoManager ins;
    static float[] deepness = new float[2] { 0.2f, 0.5f };
    [SerializeField] Image[] deepnessZones;
    [SerializeField] float cordPos;
    [SerializeField] Text log;
    [SerializeField] GameObject trail;
    Dictionary<int, TouchTracker> touchTrackers = new Dictionary<int, TouchTracker>();
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
    }
    private void Update()
    {
        string text = "";
        foreach (var t in touchTrackers)
        {
            if (t.Value.isValid)
            {
                t.Value.Update();
                text += t.Value.GetStateString() + Environment.NewLine;
            }
        }
        log.text = text;
    }

    private void FixedUpdate()
    {
        TouchControl();
    }
    void TouchControl()
    {
        foreach (var t in touchTrackers) t.Value.isValid = false;
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch t = Input.GetTouch(i);
                TouchTracker tt = GetTouchTracker(t);
                var trail = Instantiate(Resources.Load<GameObject>("Trail"), GameObject.Find("Canvas").transform);
                (trail.transform as RectTransform).anchoredPosition = ScreenToCanvasPos(t.position);
                (trail.transform as RectTransform).localScale = 30 * Vector3.one;
                tt.isValid = true;
                tt.velocity = t.position - tt.lastPos;

                // start
                float cpos = GetCordPos();
                if (t.position.x > cpos)
                {
                    float right = t.position.x - cpos;
                    if (right > tt.rightMax) tt.rightMax = right;
                }
                if (t.position.x < cpos)
                {
                    float left = cpos - t.position.x;
                    if (left > tt.leftMax) tt.leftMax = left;
                }
                if (t.position.x > cpos && tt.lastPos.x < cpos)
                {
                    tt.rightToLeftCount++;

                    if (tt.leftToRightCount == tt.rightToLeftCount)
                    {
                        float newTime = Time.time;
                        float deltaTime = newTime - tt.time;
                        if (tt.leftMax > 0 && tt.rightMax > 0)
                        {
                            tt.cordLength = tt.leftMax + tt.rightMax;
                            tt.cordLengthComps = tt.leftMax + "+" + tt.rightMax;
                            tt.speed = tt.cordLength / deltaTime;
                        }
                        tt.time = newTime;
                        tt.leftMax = tt.rightMax = -1;
                    }

                    tt.speed = tt.velocity.magnitude;
                }
                if (t.position.x < cpos && tt.lastPos.x > cpos)
                {
                    tt.leftToRightCount++;

                    if (tt.leftToRightCount == tt.rightToLeftCount)
                    {
                        float newTime = Time.time;
                        float deltaTime = newTime - tt.time;
                        if (tt.leftMax > 0 && tt.rightMax > 0)
                        {
                            tt.cordLength = tt.leftMax + tt.rightMax;
                            tt.cordLengthComps = tt.leftMax + "+" + tt.rightMax;
                            tt.speed = tt.cordLength / deltaTime;
                        }
                        tt.time = newTime;
                        tt.leftMax = tt.rightMax = -1;
                    }
                }
                // end

                tt.lastPos = t.position;
            }
        }

    }
    TouchTracker GetTouchTracker(Touch t)
    {
        TouchTracker ret;
        if (!touchTrackers.TryGetValue(t.fingerId, out ret))
        {
            ret = new TouchTracker(t);
            touchTrackers.Add(t.fingerId, ret);
        }
        if (t.phase == TouchPhase.Began) ret.Reset(t);
        return ret;
    }
    void Debug_MultiTouchLog()
    {
        string tlog = "Touches: ";
        for (int i = 0; i < Input.touchCount; ++i)
        {
            tlog += Input.GetTouch(i).fingerId + " ";
        }
        print(tlog);
    }
    static string VibratoSpeedToString(float v)
    {
        if (v < 50f) return "慢";
        else if (v > 100) return "快";
        else return "中";
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

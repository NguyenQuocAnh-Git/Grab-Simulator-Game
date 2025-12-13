using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapDrawer : MonoBehaviour
{
    public RectTransform minimapRect;
    public Vector2 worldSize = new Vector2(300, 260); // kích thước thật của bản đồ 3D

    public Vector2 WorldToMap(Vector3 w)
    {
        float cx = w.x - worldSize.x * 0.5f;
        float cy = w.z - worldSize.y * 0.5f;

        float px = (cx / worldSize.x) * minimapRect.sizeDelta.x;
        float py = (cy / worldSize.y) * minimapRect.sizeDelta.y;

        return new Vector2(px, py);
    }
}

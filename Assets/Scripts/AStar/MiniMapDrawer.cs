using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapDrawer : MonoBehaviour
{
    public RectTransform minimapRect;
    public Vector2 worldSize = new Vector2(200, 200); // kích thước thật của bản đồ 3D

    public Vector2 WorldToMap(Vector3 worldPos)
    {
        float x = (worldPos.x / worldSize.x) * minimapRect.sizeDelta.x;
        float y = (worldPos.z / worldSize.y) * minimapRect.sizeDelta.y;
        return new Vector2(x - minimapRect.sizeDelta.x / 2, y - minimapRect.sizeDelta.y / 2);
    }
}

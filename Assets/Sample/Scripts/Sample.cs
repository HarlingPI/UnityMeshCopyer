using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 创建者:   Harling
/// 创建时间: 2024-01-29 14:18:51
/// 备注:     由PIToolKit工具生成
/// </summary>
public class Sample : MonoBehaviour
{
    public MeshFilter target;

    private Rect rect;
    private void OnGUI()
    {
        rect = new Rect(Vector2.zero, new Vector2(100, 40));
        if (GUI.Button(rect, "CloneMesh"))
        {
            Clone();
        }
    }
    public void Clone()
    {
        using (MeshCopyer copyer = new MeshCopyer())
        {
            Mesh original = gameObject.GetComponent<MeshFilter>().sharedMesh;
            target.sharedMesh = copyer.Copy(original);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>
/// 创建者:   Harling
/// 创建时间: 2023-10-10 16:52:13
/// 备注:     网格拷贝器
/// </summary>
public class MeshCopyer : IDisposable
{
    private static int floatStride = Marshal.SizeOf<float>();
    private static int uintStride = Marshal.SizeOf<uint>();

    private bool disposed = false;

    private Material material = null;
    public MeshCopyer()
    {
        material = new Material(Shader.Find("Hidden/MeshDataReader"));
    }
    /// <summary>
    /// 无视isReadable深拷贝网格
    /// </summary>
    /// <param name="org"></param>
    /// <returns></returns>
    public Mesh Copy(Mesh org)
    {
        if (org.isReadable) return Object.Instantiate(org);
        else return CopyUnreadable(org);
    }
    private Mesh CopyUnreadable(Mesh orgmesh)
    {
        //获取所有Submesh描述
        int indexcount = 0;
        SubMeshDescriptor[] subs = new SubMeshDescriptor[orgmesh.subMeshCount];
        for (int i = 0; i < orgmesh.subMeshCount; i++)
        {
            subs[i] = orgmesh.GetSubMesh(i);
            indexcount += subs[i].indexCount;
        }
        //获取顶点属性描述
        int stride = 0;
        var attrs = orgmesh.GetVertexAttributes();
        for (int i = 0; i < attrs.Length; i++)
        {
            attrs[i].format = VertexAttributeFormat.Float32;
            stride += attrs[i].dimension;
            //激活相应关键字
            material.EnableKeyword($"_{attrs[i].attribute}");
            //设置相应偏移数据
            material.SetInt($"_{attrs[i].attribute}Dimension", attrs[i].dimension);
        }
        material.SetInt("Stride", stride);
        int count = stride * orgmesh.vertexCount;

        //声明数据容器
        ComputeBuffer verticsBuffer = new ComputeBuffer(count, floatStride);
        ComputeBuffer trianglesBuffer = new ComputeBuffer(indexcount, uintStride);

        float[] vertics = new float[count];
        uint[] triangles = new uint[indexcount];

        //绑定缓冲区
        Graphics.ClearRandomWriteTargets();
        material.SetPass(0);
        material.SetBuffer("Vertics", verticsBuffer);
        material.SetBuffer("Triangles", trianglesBuffer);
        Graphics.SetRandomWriteTarget(1, verticsBuffer, true);
        Graphics.SetRandomWriteTarget(2, trianglesBuffer, true);
        //获取数据
        for (int i = 0; i < orgmesh.subMeshCount; i++)
        {
            SubMeshDescriptor sub = subs[i];
            Graphics.DrawMeshNow(orgmesh, Matrix4x4.identity, i);
            trianglesBuffer.GetData(triangles, sub.indexStart, 0, sub.indexCount);
        }
        verticsBuffer.GetData(vertics);
        //构建新网格
        Mesh clone = new Mesh();
        clone.name = orgmesh.name;

        clone.SetVertexBufferParams(orgmesh.vertexCount, attrs);
        clone.SetVertexBufferData(vertics, 0, 0, vertics.Length);

        clone.SetIndexBufferParams(triangles.Length,triangles.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16);
        clone.SetIndexBufferData(triangles, 0, 0, triangles.Length);

        clone.subMeshCount = subs.Length;
        for (int i = 0; i < orgmesh.subMeshCount; i++)
        {
            clone.SetSubMesh(i, subs[i]);
        }

        clone.bounds = orgmesh.bounds;
        //销毁缓冲区
        verticsBuffer.Dispose();
        trianglesBuffer.Dispose();
        Graphics.ClearRandomWriteTargets();

        //关闭所有关键字
        string[] keywords = material.shaderKeywords;
        for (int i = 0; i < keywords.Length; i++)
        {
            material.DisableKeyword(keywords[i]);
        }

        return clone;
    }
    public void Dispose()
    {
        Disposinternal();
        GC.SuppressFinalize(this);
    }
    private void Disposinternal()
    {
        if (disposed) return;

        Object.DestroyImmediate(material);

        disposed = true;
    }

    ~MeshCopyer()
    {
        Disposinternal();
    }
}

﻿using System;
using System.Collections.Generic;
using UnityEngine;

public struct OriginalMeshfilter
{
    public Material[] Mats;
    public MeshRenderer Renderer;
}

public class Highlighter
{
    public static OriginalMeshfilter[] SetColor(MeshFilter [] filters, Color c)
    {
        List<OriginalMeshfilter> oris = new List<OriginalMeshfilter>();
        foreach( var mf in filters )
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr == null)
                continue;
            OriginalMeshfilter om = new OriginalMeshfilter()
            {
                Mats = mr.sharedMaterials,
                Renderer = mr
            };
            oris.Add(om);
            var mats = mr.materials;
            for(int i = 0; i < mats.Length; i++ )
            {
                mats[i].color = c;
            }
        }
        return oris.ToArray();
    }

    public static void ResetMaterials(OriginalMeshfilter [] oris )
    {
        foreach( var om in oris )
        {
            if (om.Renderer != null)
            {
                om.Renderer.materials = om.Mats;
            }
        }
    }
}

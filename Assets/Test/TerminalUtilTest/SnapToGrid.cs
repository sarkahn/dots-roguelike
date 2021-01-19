using Sark.Common;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField]
    float2 _snap = new float2(1, 1);

    // Update is called once per frame
    void Update()
    {
        float3 scale = transform.localScale;
        float3 p = transform.position;
        p.xy -= scale.xy / 2;
        p.xy = MathUtil.flooredincrement(p.xy, _snap);
        p.xy += scale.xy / 2;
        transform.position = p;
    }
}

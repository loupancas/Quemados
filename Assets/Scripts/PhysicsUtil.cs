using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public static class PhysicsUtil
{
    public static bool GetAvailablePosition(Vector3 source, out Vector3 p, int maxDepth = 10)
    {
        Physics.SyncTransforms();

        p = default;

        source.x = Mathf.Round(source.x);
        source.y = Mathf.Round(source.y) + 0.5f;
        source.z = Mathf.Round(source.z);

        Collider[] colliders = new Collider[1];
        Vector3 halfExtents = new(0.4f, 0.4f, 0.4f);

        p.Set(source.x, source.y, source.z);
        if (IsEmpty(p)) return FinalizePosition(ref p);

        int depth = 1;
        int i;
        while (depth < maxDepth)
        {
            for (i = 0; i < depth * 2; i++)
            {
                p.Set(source.x + (depth - 1) - i, source.y, source.z - depth);
                if (IsEmpty(p)) return FinalizePosition(ref p);
            }
            for (i = 0; i < depth * 2; i++)
            {
                p.Set(source.x - depth, source.y, source.z + (depth - 1) - i);
                if (IsEmpty(p)) return FinalizePosition(ref p);
            }
            for (i = 0; i < depth * 2; i++)
            {
                p.Set(source.x - (depth - 1) + i, source.y, source.z + depth);
                if (IsEmpty(p)) return FinalizePosition(ref p);
            }
            for (i = 0; i < depth * 2; i++)
            {
                p.Set(source.x + depth, source.y, source.z - (depth - 1) + i);
                if (IsEmpty(p)) return FinalizePosition(ref p);
            }
            depth++;
        }
        Debug.LogWarning("found no suitable location");
        return false;

        bool IsEmpty(Vector3 pos)
        {
            Debug.DrawRay(pos, Vector3.up, Color.red, 2);
            return Physics.OverlapBoxNonAlloc(pos, halfExtents, colliders, Quaternion.identity,
                ResourceManager.instance.dropPlacementObstacleMask, QueryTriggerInteraction.Collide) == 0;
        }

        static bool FinalizePosition(ref Vector3 pos)
        {
            pos.y -= 0.5f;
            return true;
        }
    }
}

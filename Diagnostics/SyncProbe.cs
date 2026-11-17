using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Read-only probe for the smoother fix: (1) how this build syncs 2D physics with transforms,
/// (2) which RenderPipelineManager events exist and whether they are obsolete.
/// RunPlayMode must be executed while EM_Gym is in play mode; it moves one patroller and puts it
/// back in the same call, and never saves anything.
/// </summary>
public static class SyncProbe
{
    public static string RunApi()
    {
        var lines = new System.Collections.Generic.List<string>();

        PropertyInfo autoSync = typeof(Physics2D).GetProperty("autoSyncTransforms", BindingFlags.Public | BindingFlags.Static);
        lines.Add("Physics2D.autoSyncTransforms: " + (autoSync == null ? "missing" :
            (autoSync.GetCustomAttribute<ObsoleteAttribute>() != null ? "present [Obsolete] " + autoSync.GetCustomAttribute<ObsoleteAttribute>().Message : "present")));

        MethodInfo sync = typeof(Physics2D).GetMethod("SyncTransforms", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
        lines.Add("Physics2D.SyncTransforms(): " + (sync == null ? "missing" :
            (sync.GetCustomAttribute<ObsoleteAttribute>() != null ? "[Obsolete] " + sync.GetCustomAttribute<ObsoleteAttribute>().Message : "present")));

        Type rpm = typeof(UnityEngine.Rendering.RenderPipelineManager);
        foreach (EventInfo e in rpm.GetEvents(BindingFlags.Public | BindingFlags.Static))
        {
            ObsoleteAttribute o = e.GetCustomAttribute<ObsoleteAttribute>();
            string args = string.Join(", ", e.EventHandlerType.GetGenericArguments().Select(a => a.Name));
            lines.Add($"RenderPipelineManager.{e.Name} ({e.EventHandlerType.Name}<{args}>){(o != null ? " [Obsolete] " + o.Message : "")}");
        }

        return string.Join("\n", lines);
    }

    public static string RunPlayMode()
    {
        if (!Application.isPlaying) return "not in play mode";

        GameObject mite = GameObject.Find("Mite_Fodder_A");
        if (mite == null) return "Mite_Fodder_A not found";

        var box = mite.GetComponent<BoxCollider2D>();
        if (box == null) return "no BoxCollider2D on Mite_Fodder_A root";

        var lines = new System.Collections.Generic.List<string>();
        Vector3 start = mite.transform.position;
        float before = box.bounds.center.x;
        int mask = 1 << mite.layer;

        mite.transform.position = start + new Vector3(0.5f, 0f, 0f);
        float afterMove = box.bounds.center.x;
        bool hitNew = Physics2D.OverlapPoint(new Vector2(afterMove, box.bounds.center.y), mask) == box;
        bool hitOld = Physics2D.OverlapPoint(new Vector2(before - box.bounds.extents.x + 0.01f, box.bounds.center.y), mask) == box;

        mite.transform.position = start;
        float afterRestore = box.bounds.center.x;

        lines.Add($"layer {mite.layer} ({LayerMask.LayerToName(mite.layer)}), isTrigger {box.isTrigger}, size {box.size}, offset {box.offset}");
        lines.Add($"bounds.x before {before:F3}, after +0.5 move {afterMove:F3}, after restore {afterRestore:F3}");
        lines.Add($"OverlapPoint at the moved centre hits it: {hitNew}; at the old left edge: {hitOld}");
        return string.Join("\n", lines);
    }
}

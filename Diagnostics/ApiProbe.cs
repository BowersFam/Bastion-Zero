using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Read-only API probe: reports the exact successor members Unity names in the CS0618 messages,
/// so the replacements are written against what this editor build really exposes.
/// </summary>
public static class ApiProbe
{
    public static string Run()
    {
        var lines = new System.Collections.Generic.List<string>();

        PropertyInfo handle = typeof(UnityEngine.SceneManagement.Scene).GetProperty("handle");
        Type handleType = handle != null ? handle.PropertyType : null;
        lines.Add("Scene.handle type: " + (handleType != null ? handleType.FullName : "missing"));

        if (handleType != null)
        {
            foreach (MethodInfo m in handleType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                         .Where(m => m.Name == "GetRawData" || m.Name == "FromRawData" || m.Name == "op_Equality" || m.Name == "op_Inequality"))
            {
                string ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
                bool obsolete = m.GetCustomAttribute<ObsoleteAttribute>() != null;
                lines.Add($"  {m.ReturnType.Name} {m.Name}({ps}){(obsolete ? " [Obsolete]" : "")}");
            }
        }

        Type obj = typeof(UnityEngine.Object);
        foreach (MethodInfo m in obj.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                     .Where(m => m.Name == "GetEntityId" || m.Name == "FindAnyObjectByType" || m.Name == "FindObjectsByType" || m.Name == "GetHashCode"))
        {
            string ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
            bool obsolete = m.GetCustomAttribute<ObsoleteAttribute>() != null;
            string generic = m.IsGenericMethodDefinition ? "<T>" : "";
            lines.Add($"Object.{m.Name}{generic}({ps}) -> {m.ReturnType.Name}{(obsolete ? " [Obsolete]" : "")}");
        }

        MethodInfo getEntityId = obj.GetMethod("GetEntityId", Type.EmptyTypes);
        if (getEntityId != null)
        {
            Type entity = getEntityId.ReturnType;
            MethodInfo hash = entity.GetMethod("GetHashCode", Type.EmptyTypes);
            lines.Add($"{entity.FullName}.GetHashCode declared on {hash?.DeclaringType?.Name}{(hash?.GetCustomAttribute<ObsoleteAttribute>() != null ? " [Obsolete]" : "")}");
        }

        return string.Join("\n", lines);
    }
}

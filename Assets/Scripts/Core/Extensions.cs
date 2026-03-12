using UnityEngine;
using System.Collections.Generic;

namespace RobotTD
{
    /// <summary>
    /// Extension methods and utility functions for the Robot Tower Defense game.
    /// Provides convenient helper methods used throughout the codebase.
    /// </summary>
    public static class Extensions
    {
        // ═══════════════════════════════════════════════════════════════════
        // VECTOR EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set the X component of a Vector3
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        /// <summary>
        /// Set the Y component of a Vector3
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        /// Set the Z component of a Vector3
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Get Vector3 with Y set to 0 (flatten to XZ plane)
        /// </summary>
        public static Vector3 Flatten(this Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }

        /// <summary>
        /// Get XZ distance between two Vector3s (ignoring Y)
        /// </summary>
        public static float DistanceXZ(this Vector3 from, Vector3 to)
        {
            return Vector2.Distance(
                new Vector2(from.x, from.z),
                new Vector2(to.x, to.z)
            );
        }

        /// <summary>
        /// Get direction from one Vector3 to another, flattened to XZ
        /// </summary>
        public static Vector3 DirectionToXZ(this Vector3 from, Vector3 to)
        {
            Vector3 dir = (to - from).Flatten();
            return dir.normalized;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TRANSFORM EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set position X component
        /// </summary>
        public static void SetPositionX(this Transform t, float x)
        {
            t.position = t.position.WithX(x);
        }

        /// <summary>
        /// Set position Y component
        /// </summary>
        public static void SetPositionY(this Transform t, float y)
        {
            t.position = t.position.WithY(y);
        }

        /// <summary>
        /// Set position Z component
        /// </summary>
        public static void SetPositionZ(this Transform t, float z)
        {
            t.position = t.position.WithZ(z);
        }

        /// <summary>
        /// Reset transform to origin with identity rotation
        /// </summary>
        public static void Reset(this Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// Destroy all children of a transform
        /// </summary>
        public static void DestroyChildren(this Transform t)
        {
            int childCount = t.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Find child by name recursively
        /// </summary>
        public static Transform FindDeep(this Transform t, string name)
        {
            if (t.name == name) return t;

            foreach (Transform child in t)
            {
                Transform result = child.FindDeep(name);
                if (result != null) return result;
            }

            return null;
        }

        // ═══════════════════════════════════════════════════════════════════
        // GAMEOBJECT EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get or add a component
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Check if GameObject has a component
        /// </summary>
        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() != null;
        }

        /// <summary>
        /// Set layer for GameObject and all children
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // COLOR EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set alpha value of a color
        /// </summary>
        public static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        /// <summary>
        /// Multiply RGB by a factor (useful for brightening/darkening)
        /// </summary>
        public static Color MultiplyRGB(this Color c, float multiplier)
        {
            return new Color(c.r * multiplier, c.g * multiplier, c.b * multiplier, c.a);
        }

        /// <summary>
        /// Convert Color to hex string
        /// </summary>
        public static string ToHex(this Color c)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(c)}";
        }

        // ═══════════════════════════════════════════════════════════════════
        // LIST EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get a random element from a list
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffle a list in place (Fisher-Yates)
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Check if list is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        // ═══════════════════════════════════════════════════════════════════
        // FLOAT/INT EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Remap a value from one range to another
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// Check if value is approximately zero
        /// </summary>
        public static bool IsApproximately(this float value, float target, float epsilon = 0.01f)
        {
            return Mathf.Abs(value - target) < epsilon;
        }

        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }

        // ═══════════════════════════════════════════════════════════════════
        // STRING EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Format number with K/M suffix (e.g., 1000 -> 1K)
        /// </summary>
        public static string ToShorthand(this int value)
        {
            if (value >= 1000000)
                return $"{value / 1000000f:F1}M";
            if (value >= 1000)
                return $"{value / 1000f:F1}K";
            return value.ToString();
        }

        /// <summary>
        /// Format time in MM:SS format
        /// </summary>
        public static string ToTimeString(this float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }

        /// <summary>
        /// Add color tags for Unity rich text
        /// </summary>
        public static string Colored(this string text, Color color)
        {
            return $"<color={color.ToHex()}>{text}</color>";
        }

        /// <summary>
        /// Add bold tags for Unity rich text
        /// </summary>
        public static string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        // ═══════════════════════════════════════════════════════════════════
        // RECT EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get a random point inside a Rect
        /// </summary>
        public static Vector2 RandomPoint(this Rect rect)
        {
            return new Vector2(
                Random.Range(rect.xMin, rect.xMax),
                Random.Range(rect.yMin, rect.yMax)
            );
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER MASK EXTENSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Check if a LayerMask contains a specific layer
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}

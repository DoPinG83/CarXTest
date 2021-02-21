using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Core.Utils;
using Object = UnityEngine.Object;


public static class GameObjectExtensions
{
    /// <summary>
    /// Ищет компонент среди чайлдов. Корректная работа обеспечена только если такой компонент один.
    /// Функция нужна для того чтобы можно было использовать GetComponentsInChildren(bool) но для единичного экземляра скрипта и без написания кучи однообразной логики.
    /// Внутри использует GetComponentsInChildren(bool) и возвращает первый элемент из массива или null если его нет
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <param name="includeInactive"></param>
    /// <returns></returns>
    public static T GetComponentInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        if (includeInactive)
        {
            var components = gameObject.GetComponentsInChildren<T>(true);
            if (components == null || components.Length == 0)
                return null;
            return components[0];
        }
        return gameObject.GetComponentInChildren<T>();
    }
    
    public static GameObject InstantiateInParent(this GameObject prefab, Transform parent, bool saveOffests = false)
    {
        GameObject obj = Object.Instantiate(prefab) as GameObject;
        obj.SetActive(true);
        obj.transform.InitializeParent(parent);
        if (saveOffests)
        {
            ((RectTransform)obj.transform).offsetMin = ((RectTransform)prefab.transform).offsetMin;
            ((RectTransform)obj.transform).offsetMax = ((RectTransform)prefab.transform).offsetMax;
        }
        return obj;
    }

    public static GameObject InstantiateInParent(this GameObject prefab, GameObject parent, bool saveOffests = false)
    {
        return prefab.InstantiateInParent(parent.transform, saveOffests);
    }

    public static T InstantiateInParent<T>(this GameObject prefab, Transform parent, bool saveOffests = false) where T : MonoBehaviour
    {
        GameObject obj = InstantiateInParent(prefab, parent, saveOffests);
        return obj.GetComponent<T>();
    }

    public static T InstantiateInParent<T>(this GameObject prefab, GameObject parent, bool saveOffests = false) where T : MonoBehaviour
    {
        return prefab.InstantiateInParent<T>(parent.transform, saveOffests);
    }

    public static void InitializeParent(this Transform transform, Transform parent)
    {
        transform.SetParent(parent);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    public static string GetFullHierarchyName(this Transform objTransform, string separator = "->")
    {
        if (objTransform == null)
        {
            CoreLog.LogError("Target transrom is null");
            return null;
        }
        var parents = new List<string>();
        var tr = objTransform;
        do
        {
            tr = tr.parent;
            if (tr != null)
                parents.Add(tr.name);
        } while (tr != null);
        parents.Reverse();
        StringBuilder sb = new StringBuilder();
        foreach (var p in parents)
            sb.Append(p + separator);
        sb.Append(objTransform.name);
        return sb.ToString();
    }

    /// <summary>
    /// Удаление всех детей в контейнере, удовлетворяющих заданному условию
    /// </summary>
    /// <param name="transform">Конейнер</param>
    /// <param name="predicate">Условие, при удовлетворении которому, объект удаляется</param>
    public static void RemoveAllChildren(this Transform transform, Func<GameObject, bool> predicate)
    {
        transform.GetChildren(predicate, child =>
        {
            child.transform.SetParent(null);
            GameObject.Destroy(child);
        });
    }

    public static void RemoveAllChildren(this Transform transform)
    {
        RemoveAllChildren(transform, x => true);
    }

    public static void GetChildren(this Transform transform, Func<GameObject, bool> predicate, Action<GameObject> action)
    {
        if(transform == null)
            return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != null && child.gameObject != null && predicate(child.gameObject))
                action(child.gameObject);
        }
    }

    public static void GetChildren<T>(this Transform transform, Func<T, bool> predicate, Action<T> action) where T : Component
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != null && child.gameObject != null)
            {
                var component = child.gameObject.GetComponent<T>();
                if (component != null && predicate(component))
                    action(component);
            }
        }
    }
    
    public static RectTransform RectTransform(this GameObject gameObject)
    {
        return gameObject.transform as RectTransform;
    }

    public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
    {
        // нотация ?? не всегда работает в Unity
        var component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    public static GameObject CreateEmptyChild(this GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return  go;
    }

    public static List<T> GetComponentsInChildHierarchy<T>(this GameObject go) where T : Component
    {
        List<T> components = new List<T>();
        if (go == null)
            return components;
        var component = go.GetComponents<T>();
        if (component != null)
            components.AddRange(component);
        for (int i = 0; i < go.transform.childCount; i++)
            components.AddRange(GetComponentsInChildHierarchy<T>(go.transform.GetChild(i).gameObject));
        return components;
    }

    public static T FixLocalShaders<T>(this T source) where T : Object
    {
        //CoreLog.LogError("FixLocalShaders for " + source.name);
/*#if UNITY_EDITOR && FORCE_BUNDLES
        if (source == null)
        {
            return source;
        }

        var sourceType = source.GetType();

        if (sourceType == typeof(Shader))
        {
            return (T)(object)Shader.Find(((Shader)(object)source).name);
        }

        if (sourceType == typeof(GameObject))
        {
            var sourceObj = (GameObject)(object)source;
            var renderers = sourceObj.GetComponentsInChildHierarchy<Renderer>();

            foreach (var material in renderers.SelectMany(renderer => renderer.sharedMaterials))
            {
                if (material == null)
                    continue;
                material.shader = Shader.Find(material.shader.name);
            }

            var projectors = sourceObj.GetComponentsInChildren<Projector>();
            foreach (var projector in projectors)
            {
                projector.material.shader = Shader.Find(projector.material.shader.name);
            }

            var graphics = sourceObj.GetComponentsInChildren<Graphic>();
            foreach (var graphic in graphics)
            {
                graphic.material.shader = Shader.Find(graphic.material.shader.name);
            }
        }
        else if (sourceType == typeof(Material))
        {
            var sourceMaterial = (Material)(object)source;
            sourceMaterial.shader = Shader.Find(sourceMaterial.shader.name);
        }
#endif*/
        return source;
    }

    public static void DestroyWithMaterials(this GameObject obj)
    {
        obj.ClearMaterials();
        Object.Destroy(obj);
    }

    public static void ClearMaterials(this GameObject obj)
    {
        obj.ClearMaterials(false);
    }

    public static void ClearSharedMaterials(this GameObject obj)
    {
        obj.ClearMaterials(true);
    }

    static void ClearMaterials(this GameObject obj, bool isShared)
    {
        ClearRendererMaterials(obj.GetComponent<Renderer>(), isShared);
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            ClearRendererMaterials(renderer, isShared);
        }
    }

    static void ClearRendererMaterials(Renderer renderer, bool isShared)
    {
        if (renderer == null)
            return;
        var mat = isShared ? renderer.sharedMaterial : renderer.material;
        if(checkMaterialIsCloneOrInstance(mat))
        {
            Object.DestroyImmediate(mat, true);
            mat = null;
        }
        
        var materials = isShared ? renderer.sharedMaterials : renderer.materials;
        foreach (var material in materials)
        {
            if (checkMaterialIsCloneOrInstance(material))
            {
                //CoreLog.LogError("Destroy " + material.name + " objName = " + renderer.name);
                Object.DestroyImmediate(material, true);
            }
        }
    }

    public static bool checkMaterialIsCloneOrInstance(Material mat)
    {
        if (mat != null)
        {
            if (mat.name.Contains("(Clone)") || mat.name.Contains("(Instance)"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Small number of extension methods that make it easier for PUN to work cross-Unity-versions.</summary>
    /// <summary>Unity-version-independent replacement for active GO property.</summary>
    /// <returns>Unity 3.5: active. Any newer Unity: activeInHierarchy.</returns>
    public static bool GetActive(this GameObject target)
    {
        return target.activeInHierarchy;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResourcesPrefabAttribute))]
public class ResourcesPrefabDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(property.stringValue);
            string []strs=property.stringValue.Split('/');
            if (strs.Length > 2 && strs[0] != "Assets" && strs[1] != "Resources")
                Debug.LogError($"{property.stringValue} doesn't in the Resources folder, assign the proper prefab in your Respawn");

            if (prefabObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
            {
                Debug.LogError($"Could not find Resources prefab {property.stringValue} in {property.propertyPath}, assign the proper prefab in your Respawn");
            }

            GameObject ShowPrefab = (GameObject)EditorGUI.ObjectField(position, label, prefabObject, typeof(GameObject), true);
            property.stringValue = AssetDatabase.GetAssetPath(ShowPrefab);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use [Prefab] with strings.");
        }
    }


}

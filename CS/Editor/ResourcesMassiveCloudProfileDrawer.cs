using Mewlist;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourcesMassiveCloudProfileAttribute))]
public class ResourcesMassiveCloudProfileDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            MassiveCloudsProfile prfile = AssetDatabase.LoadAssetAtPath<MassiveCloudsProfile>(property.stringValue);
            string[] strs = property.stringValue.Split('/');
            if (strs.Length > 2 && strs[0] != "Assets" && strs[1] != "Resources")
                Debug.LogError($"{property.stringValue} doesn't in the Resources folder, assign the MassiveCloudsProfile in your Binder");

            if (prfile == null && !string.IsNullOrWhiteSpace(property.stringValue))
            {
                Debug.LogError($"Could not find Resources prefab {property.stringValue} in {property.propertyPath}, assign the proper prefab in your Respawn");
            }

            MassiveCloudsProfile ShowProfile = (MassiveCloudsProfile)EditorGUI.ObjectField(position, label, prfile, typeof(MassiveCloudsProfile), true);
            property.stringValue = AssetDatabase.GetAssetPath(ShowProfile);
        }
        else
        {   
            EditorGUI.LabelField(position, label.text, "Use [MassiveCloudsProfile] with strings.");
        }
    }
}

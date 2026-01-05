using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]  // Add this line
public class AutoLinker
{
    static Dictionary<string, GameObject> hierarchyNameToGameObjectMap;
    static Dictionary<string, SerializedObject> inspectorFieldNameToSerializedPropertyMap;

    static AutoLinker()
    {
        EditorApplication.delayCall += RunAutoLinker;
    }

    static void RunAutoLinker()
    {
        hierarchyNameToGameObjectMap = new Dictionary<string, GameObject>();
        inspectorFieldNameToSerializedPropertyMap = new Dictionary<string, SerializedObject>();
        SetupHierachyMap();
        SetupInspectorMap();
        HandleAutoLinking();
    }

    static void SetupHierachyMap()
    {
        GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>(); 
        foreach (GameObject gameObject in gameObjects)
        {
            string key = gameObject.name.ToLower().Replace(" ", "");
            hierarchyNameToGameObjectMap.Add(key, gameObject);
        }
    }

    static void SetupInspectorMap()
    {
        foreach (GameObject gameObject in hierarchyNameToGameObjectMap.Values)
        {
            Component[] componenents = gameObject.GetComponents<Component>();

            foreach (Component component in componenents)
            {
                if (component == null)
                    continue;
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty serializedProperty = serializedObject.GetIterator();
                while (serializedProperty.NextVisible(true))
                {
                    string key = serializedProperty.displayName.ToLower().Replace(" ", "");
                    if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (!inspectorFieldNameToSerializedPropertyMap.ContainsKey(key))
                        {
                            inspectorFieldNameToSerializedPropertyMap.Add(key, serializedObject);
                        }
                    }
                }
            }
        }
    }

    static void HandleAutoLinking()
    {
        foreach (string name in inspectorFieldNameToSerializedPropertyMap.Keys)
        {
            string key = name.ToLower().Replace(" ", "");
            if (hierarchyNameToGameObjectMap.ContainsKey(key))
            {
                SerializedProperty serializedProperty = inspectorFieldNameToSerializedPropertyMap[key].FindProperty(name);
                serializedProperty.objectReferenceValue = hierarchyNameToGameObjectMap[key];
                inspectorFieldNameToSerializedPropertyMap[key].ApplyModifiedProperties();
            }
        }
    }
}

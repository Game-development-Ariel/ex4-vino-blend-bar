using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// Checks a loaded scene for what breaks a game silently: missing scripts and empty reference fields.
    /// </summary>
    public static class SceneChecks
    {
        /// <summary>The game objects (as paths) that have a component whose script is missing.</summary>
        public static List<string> ObjectsWithMissingScripts(Scene scene)
        {
            var broken = new List<string>();
            foreach (GameObject thing in AllObjects(scene))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(thing) > 0)
                {
                    broken.Add(PathOf(thing.transform));
                }
            }
            return broken;
        }

        /// <summary>
        /// Every object reference field of the game's own components that is empty, for example
        /// "Game/Bartender.dropPrefab". Our components have no optional references, so any empty one is a bug.
        /// </summary>
        public static List<string> UnassignedReferences(Scene scene)
        {
            var empty = new List<string>();
            foreach (GameObject thing in AllObjects(scene))
            {
                foreach (MonoBehaviour component in thing.GetComponents<MonoBehaviour>())
                {
                    if (component == null || component.GetType().Assembly != typeof(BarEvening).Assembly)
                    {
                        continue;
                    }
                    SerializedProperty property = new SerializedObject(component).GetIterator();
                    while (property.Next(true))
                    {
                        // Unity's own fields (m_Script, m_PrefabInstance...) are not ours, and are empty in a scene.
                        if (property.name.StartsWith("m_"))
                        {
                            continue;
                        }
                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                        {
                            empty.Add($"{PathOf(thing.transform)}/{component.GetType().Name}.{property.propertyPath}");
                        }
                    }
                }
            }
            return empty;
        }

        public static List<GameObject> AllObjects(Scene scene)
        {
            var all = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    all.Add(child.gameObject);
                }
            }
            return all;
        }

        public static List<T> All<T>(Scene scene) where T : Component
        {
            var found = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                found.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return found;
        }

        public static string PathOf(Transform transform)
        {
            return transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
        }
    }
}

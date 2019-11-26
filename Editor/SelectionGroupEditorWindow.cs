﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{

    public partial class SelectionGroupEditorWindow : EditorWindow
    {

        static SelectionGroupEditorWindow()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            SelectionGroupContainer.onLoaded += OnContainerLoaded;
        }

        static void SanitizeSceneReferences()
        {
            foreach (var i in SelectionGroupContainer.instanceMap.ToArray())
            {
                var scene = i.Key;
                var container = i.Value;
                foreach (var g in container.groups)
                {
                    var name = g.Key;
                    var group = g.Value;
                    foreach (var o in group.objects.ToArray())
                    {
                        if (o != null && o.scene != scene)
                        {
                            group.objects.Remove(o);
                            SelectionGroupEditorUtility.AddObjectToGroup(o, name);
                            EditorUtility.SetDirty(container);
                        }
                    }
                }
            }
        }

        static void OnContainerLoaded(SelectionGroupContainer container)
        {
            foreach (var name in container.groups.Keys.ToArray())
            {
                if (string.IsNullOrEmpty(name)) continue;
                var mainGroup = SelectionGroupUtility.GetFirstGroup(name);
                if (mainGroup == null) continue;
                var importedGroup = container.groups[name];
                if (importedGroup == null) continue;
                importedGroup.color = mainGroup.color;
                importedGroup.selectionQuery = mainGroup.selectionQuery;
                importedGroup.showMembers = mainGroup.showMembers;
                container.groups[name] = importedGroup;
            }
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                //clear all results so the queries can be refreshed with items from the new scene.
                foreach (var g in i.groups.Values)
                {
                    g.ClearQueryResults();
                }
                container.gameObject.hideFlags = HideFlags.None;
                // container.gameObject.hideFlags = HideFlags.HideInHierarchy;
                EditorUtility.SetDirty(i);
            }
            if (editorWindow != null) editorWindow.Repaint();
        }


        internal static void MarkAllContainersDirty()
        {
            foreach (var container in SelectionGroupContainer.instanceMap.Values)
                EditorUtility.SetDirty(container);
        }

        static void CreateNewGroup(Object[] objects)
        {
            SelectionGroupEditorUtility.RecordUndo("New Group");
            var actualName = SelectionGroupEditorUtility.CreateNewGroup("New Group");
            SelectionGroupEditorUtility.AddObjectToGroup(objects, actualName);
            MarkAllContainersDirty();
        }

        void QueueSelectionOperation(SelectionCommand command, GameObject gameObject)
        {
            if (nextSelectionOperation == null)
            {
                nextSelectionOperation = new SelectionOperation() { command = command, gameObject = gameObject };
            }
        }

        void PerformSelectionCommands()
        {
            if (nextSelectionOperation != null)
            {
                if (nextSelectionOperation.gameObject != null)
                    switch (nextSelectionOperation.command)
                    {
                        case SelectionCommand.Add:
                            Selection.objects = Selection.objects.Append(nextSelectionOperation.gameObject).ToArray();
                            break;
                        case SelectionCommand.Remove:
                            Selection.objects = (from i in Selection.objects where i != nextSelectionOperation.gameObject select i).ToArray();
                            break;
                        case SelectionCommand.Set:
                            Selection.objects = new Object[] { nextSelectionOperation.gameObject };
                            break;
                    }
                nextSelectionOperation = null;
            }
        }

    }
}

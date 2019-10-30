using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups
{
    [CustomPropertyDrawer(typeof(SelectionGroupAttribute))]
    public class SelectionGroupDrawer : PropertyDrawer
    {
        string[] names;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (names == null) names = SelectionGroupUtility.GetGroupNames().ToArray();
            var name = property.stringValue;
            position = EditorGUI.PrefixLabel(position, label);
            var index = System.Array.IndexOf(names, name);
            var newIndex = EditorGUI.Popup(position, index, names);
            if (newIndex != index)
            {
                property.stringValue = names[newIndex];
            }
        }
    }
}
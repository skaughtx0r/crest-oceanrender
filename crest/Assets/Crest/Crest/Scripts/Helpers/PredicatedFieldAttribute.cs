﻿// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using System;
using UnityEngine;

#if UNITY_EDITOR
using Crest.EditorHelpers;
using UnityEditor;
#endif

namespace Crest
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PredicatedFieldAttribute : MultiPropertyAttribute
    {
        public readonly string _propertyName;
        public readonly bool _inverted;
        public readonly int _disableIfValueIs;

        /// <summary>
        /// The field with this attribute will be drawn enabled/disabled based on another field. For example can be used
        /// to disable a field if a toggle is false. Limitation - conflicts with other property drawers such as Range().
        /// </summary>
        /// <param name="propertyName">The name of the other property whose value dictates whether this field is enabled or not.</param>
        /// <param name="inverted">Flip behaviour - for example disable if a bool field is set to true (instead of false).</param>
        /// <param name="disableIfValueIs">If the field has this value, disable the GUI (or enable if inverted is true).</param>
        public PredicatedFieldAttribute(string propertyName, bool inverted = false, int disableIfValueIs = 0)
        {
            _propertyName = propertyName;
            _inverted = inverted;
            _disableIfValueIs = disableIfValueIs;
        }

#if UNITY_EDITOR
        public bool GUIEnabled(SerializedProperty prop)
        {
            bool result;

            if (prop.type == "int")
            {
                // Enable GUI if int value of field is not equal to 0, or whatever the disable-value is set to
                result = prop.intValue != _disableIfValueIs;
            }
            else if (prop.type == "bool")
            {
                // Enable GUI if disable-value is 0 and field is true, or disable-value is not 0 and field is false
                result = prop.boolValue ^ (_disableIfValueIs != 0);
            }
            else if (prop.type == "float")
            {
                result = prop.floatValue != _disableIfValueIs;
            }
            else if (prop.type == "Enum")
            {
                result = prop.enumValueIndex != _disableIfValueIs;
            }
            else if (prop.type.StartsWith("PPtr"))
            {
                result = prop.objectReferenceValue != null;
            }
            else
            {
                Debug.LogError($"PredicatedFieldAttributePropertyDrawer - property type not implemented yet: {prop.type}.", prop.serializedObject.targetObject);
                return true;
            }

            return _inverted ? !result : result;
        }

        internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, MultiPropertyDrawer drawer)
        {
            // Get the other property to be the predicate for the enabled/disabled state of this property.
            var otherProperty = property.serializedObject.FindProperty(_propertyName);
            if (otherProperty != null)
            {
                GUI.enabled = GUIEnabled(otherProperty);
            }
          }
#endif
    }
}

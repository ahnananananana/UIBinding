using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace HDV.UIBinding
{
    [CustomEditor(typeof(UIBinder), true)]
    public class UIBinderEditor : Editor
    {
        private SerializedProperty _monoBehaviourModel;
        private SerializedProperty _scriptableObjectModel;
        private SerializedProperty _propertyName;

        private UIBinder _target;

        private void OnEnable()
        {
            _monoBehaviourModel = serializedObject.FindProperty("_monoBehaviourModel");
            _scriptableObjectModel = serializedObject.FindProperty("_scriptableObjectModel");
            _propertyName = serializedObject.FindProperty("_propertyName");

            _target = (UIBinder)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //TODO: Object만 따로 받는 변수를 만들어서 할당해주면 알아서 ScriptableObject나 MonoBehaivour로 변환시키게 해야
            if (_scriptableObjectModel.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(_monoBehaviourModel);
            }
            if (_monoBehaviourModel.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(_scriptableObjectModel);
            }

            Object model = _monoBehaviourModel.objectReferenceValue ? _monoBehaviourModel.objectReferenceValue : _scriptableObjectModel.objectReferenceValue;
            if (model)
            {
                Type modelType = model.GetType();

                PropertyInfo[] properties = modelType.GetProperties();
                List<string> dropdownList = new List<string>();
                foreach (PropertyInfo pi in properties)
                {
                    if (pi.GetCustomAttribute<UIBindable>() != null && _target.CanBind(pi.PropertyType))
                        dropdownList.Add(pi.Name);
                }

                GUIContent guiContent = new GUIContent("Property Name");
                if (dropdownList.Count > 0)
                {
                    int currentIdx = dropdownList.FindIndex(i => i == _propertyName.stringValue);
                    currentIdx = currentIdx < 0 ? 0 : currentIdx;

                    int newIdx = EditorGUILayout.Popup(guiContent, currentIdx, dropdownList.ToArray());
                    _propertyName.stringValue = dropdownList[newIdx];
                }
                else
                {
                    EditorGUILayout.Popup(guiContent, 0, dropdownList.ToArray());
                    _propertyName.stringValue = "";
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

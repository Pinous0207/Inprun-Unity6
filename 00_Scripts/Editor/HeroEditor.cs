using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.ReorderableList;
using UnityEditorInternal;

[CustomEditor(typeof(Hero_Scriptable))]
public class HeroEditor : Editor
{
    private ReorderableList effectTypeList;
    // 에디터가 활성화될 때 호출된다.
    private void OnEnable()
    {
        SerializedProperty debuffTypeProperty = serializedObject.FindProperty("effectType");
        effectTypeList = new ReorderableList(serializedObject, debuffTypeProperty, true, true, true ,true);

        effectTypeList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Hero Debuff Types");
        };

        // 리스트 요소의 높이를 설정하는 함수
        effectTypeList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = debuffTypeProperty.GetArrayElementAtIndex(index);
            SerializedProperty parametersProp = element.FindPropertyRelative("parameters");

            float baseHeight = EditorGUIUtility.singleLineHeight + 6.0f;
            float paramHeight = parametersProp.arraySize * (EditorGUIUtility.singleLineHeight + 4.0f);
            return baseHeight + paramHeight + 10.0f;
        };

        // 리스트의 각 요소를 그리는 콜백 함수
        effectTypeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = debuffTypeProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            SerializedProperty effectTypeProp = element.FindPropertyRelative("debuffType");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), effectTypeProp, new GUIContent("Debuff Type"));

            Debuff debuffType = (Debuff)effectTypeProp.enumValueIndex;
            SerializedProperty parametersProp = element.FindPropertyRelative("parameters");

            rect.y += EditorGUIUtility.singleLineHeight + 4.0f;

            switch (debuffType)
            {
                case Debuff.Slow:
                    parametersProp.arraySize = 3;
                    DrawParameterField(rect, "Chance:", parametersProp, 0);
                    rect.y += EditorGUIUtility.singleLineHeight + 4;
                    DrawParameterField(rect, "Slow Amount:", parametersProp, 1);
                    rect.y += EditorGUIUtility.singleLineHeight + 4;
                    DrawParameterField(rect, "Duration (seconds):", parametersProp, 2);
                    break;
                case Debuff.Stun:
                    parametersProp.arraySize = 2;
                    DrawParameterField(rect, "Chance:", parametersProp, 0);
                    rect.y += EditorGUIUtility.singleLineHeight + 4;
                    DrawParameterField(rect, "Duration (seconds):", parametersProp, 1);
                    break;
            }
        };

        effectTypeList.onAddCallback = (ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index); // 새로운 요소를 추가

            SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            newElement.FindPropertyRelative("debuffType").enumValueIndex = 0;
            newElement.FindPropertyRelative("parameters").arraySize = 0;
        };

        effectTypeList.onRemoveCallback = (ReorderableList list) =>
        {
            if (EditorUtility.DisplayDialog("Remove Effect", "Are you sure you want to remove this effect?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        };
    }

    private void DrawParameterField(Rect rect, string label,SerializedProperty parametersProp, int index)
    {
        EditorGUI.LabelField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), label);

        // 첫 번째 필드
        parametersProp.GetArrayElementAtIndex(index).floatValue =
        EditorGUI.FloatField(new Rect(rect.x + 130.0f, rect.y, 100, EditorGUIUtility.singleLineHeight)
        , parametersProp.GetArrayElementAtIndex(index).floatValue);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ATK"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ATK_Speed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Range"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Animator"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rare"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("HitParticle"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillData"));

        EditorGUILayout.Space(20f);

        effectTypeList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}

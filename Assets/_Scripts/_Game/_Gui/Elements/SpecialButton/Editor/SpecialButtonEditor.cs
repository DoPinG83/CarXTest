using UnityEngine;
using UnityEditor;
using Core.UI.Tutorial;

[CanEditMultipleObjects]
[CustomEditor(typeof (SpecialButton), true)]
public class SpecialButtonEditor : UnityEditor.UI.ButtonEditor
{
    private SerializedProperty _tutorialNameProperty;
    private SerializedProperty _handlesLongPressProperty;
    private SerializedProperty _longPressTimeout;
    private SerializedProperty _lockOnBatchProperty;
    private SerializedProperty _desaturateTextProperty;
    private SerializedProperty _enableAnimationProperty;
    private SerializedProperty _animationPivotProperty;
    private SerializedProperty _onLongPressProperty;
    private SerializedProperty _scaleTarget;

    public override void OnInspectorGUI()
    {
        SpecialButton button = (SpecialButton) target;

        this.serializedObject.Update();
        EditorGUILayout.PropertyField(_tutorialNameProperty, new GUIContent("Tutorial Name"));
        EditorGUILayout.PropertyField(_handlesLongPressProperty, new GUIContent("Handles long press"));
        EditorGUILayout.PropertyField(_scaleTarget, new GUIContent("Scale Target"));
        if (button.HandlesLongPress)
            EditorGUILayout.PropertyField(_longPressTimeout, new GUIContent("  Long press timeout"));
        EditorGUILayout.PropertyField(_lockOnBatchProperty, new GUIContent("Lock and desaturate on batch"));
        if (button.LockOnBatch)
            EditorGUILayout.PropertyField(_desaturateTextProperty, new GUIContent("  + Desaturate text"));
        EditorGUILayout.PropertyField(_enableAnimationProperty, new GUIContent("Enable Animation"));
        if (button.EnableAnimation)
            EditorGUILayout.PropertyField(_animationPivotProperty, new GUIContent("Animation Pivot"));
        this.serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Separator();
        base.OnInspectorGUI();

        if (button.HandlesLongPress)
            EditorGUILayout.PropertyField(_onLongPressProperty, new GUIContent("  On Long Press"));
        this.serializedObject.ApplyModifiedProperties();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _tutorialNameProperty = this.serializedObject.FindProperty("TutorialName");
        _handlesLongPressProperty = this.serializedObject.FindProperty("HandlesLongPress");
        _longPressTimeout = this.serializedObject.FindProperty("LongPressTimeout");
        _lockOnBatchProperty = this.serializedObject.FindProperty("LockOnBatch");
        _desaturateTextProperty = this.serializedObject.FindProperty("DesaturateTextOnLock");
        _enableAnimationProperty = this.serializedObject.FindProperty("EnableAnimation");
        _animationPivotProperty = this.serializedObject.FindProperty("AnimationPivot");
        _onLongPressProperty = this.serializedObject.FindProperty("onLongPress");
        _scaleTarget = this.serializedObject.FindProperty("ScaleTarget");
    }
}

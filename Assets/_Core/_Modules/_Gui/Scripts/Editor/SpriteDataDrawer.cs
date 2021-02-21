using Core.UI;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SpriteData))]
public class SpriteDataDrawer : PropertyDrawer
{
    private const float _typeLabel = 0.12f;
    private const float _typePopup = 0.3f;
    private const float _typePopupMargin = -0.04f;
    private const float _spriteLabel = 0.2f;
    private const float _spriteLabelMargin = -0.052f;
    private const float _spriteFieldMargin = -0.092f;
    private const float _spriteField = 1f - _typeLabel - _typePopup - _spriteLabel - _spriteFieldMargin;

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        SerializedProperty idProperty = property.FindPropertyRelative("Id");
        SerializedProperty spriteProperty = property.FindPropertyRelative("Sprite");

        float typeLabelWidth = pos.width * _typeLabel;
        float typePopupWidth = pos.width * _typePopup;
        float typePopupOffset = pos.width * _typePopupMargin;
        float spriteLabelWidth = pos.width * _spriteLabel;
        float spriteLabelOffset = pos.width * _spriteLabelMargin;
        float spriteFieldWidth = pos.width * _spriteField;
        float spriteFieldOffset = pos.width * _spriteFieldMargin;

        EditorGUI.LabelField(new Rect(pos.x, pos.y, typeLabelWidth, pos.height), idProperty.name);
        Rect idRect = new Rect(pos.x + typeLabelWidth + typePopupOffset, pos.y, typePopupWidth, pos.height);
        idProperty.stringValue = EditorGUI.TextField(idRect, idProperty.stringValue);
        
        EditorGUI.LabelField(new Rect(pos.x + typeLabelWidth + typePopupWidth + spriteLabelOffset, pos.y, spriteLabelWidth, pos.height), spriteProperty.name);
        Rect valueRect = new Rect(pos.x + typeLabelWidth + typePopupWidth + spriteLabelWidth + spriteFieldOffset, pos.y, spriteFieldWidth, pos.height);
        
        spriteProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, spriteProperty.objectReferenceValue, typeof(Sprite), false);
    }

}

using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.SimpleEmojiTMP.Editor.Scripts
{
  /// <summary>
  /// Adds "Sync" button to LocalizationSync script.
  /// </summary>
  [CustomEditor(typeof(EmojiAssetBuilder))]
    public class EmojiAssetBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var component = (EmojiAssetBuilder) target;

            GUILayout.Label("Include categories:");

            foreach (var category in component.Categories)
            {
                if (GUILayout.Toggle(component.IncludeCategories.Contains(category), category))
                {
                    if (!component.IncludeCategories.Contains(category)) component.IncludeCategories.Add(category);
                }
                else
                {
                    if (component.IncludeCategories.Contains(category)) component.IncludeCategories.Remove(category);
                }
            }

            if (GUILayout.Button("Build"))
            {
              component.Build();
            }

            if (GUILayout.Button("Refresh"))
            {
                component.RefreshCategories();
            }

            if (TMP_Settings.defaultSpriteAsset != component.TMPSpriteAsset)
            {
                EditorGUILayout.HelpBox("EmojiTMP is not assigned to [TMP Settings/Default Sprite Asset]. Find settings at 'TextMesh Pro/Resources/TMP Settings'.", MessageType.Warning);
            }
        }
    }
}
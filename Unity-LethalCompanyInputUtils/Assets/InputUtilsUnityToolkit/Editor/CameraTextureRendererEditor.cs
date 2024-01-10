using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputUtilsUnityToolkit.Editor
{
    [CustomEditor(typeof(CameraTextureRenderer))]
    public class CameraTextureRendererEditor : UnityEditor.Editor
    {
        [SerializeField]
        private string textureName = "";
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var nameField = new TextField("Texture Name");
            nameField.RegisterValueChangedCallback(NameFieldChanged);

            var genTextureButton = new Button(OnCreateTexture);
            genTextureButton.Add(new Label("Save Texture"));
            
            root.Add(nameField);
            root.Add(genTextureButton);

            return root;
        }

        private void NameFieldChanged(ChangeEvent<string> evt)
        {
            textureName = evt.newValue;
        }
        
        private void OnCreateTexture()
        {
            string path = EditorUtility.SaveFilePanel("Save Rendered Texture", "Assets/Controller Glyphs/Rendered/", textureName, "png");
            if (string.IsNullOrEmpty(path))
                return;

            path = FileUtil.GetProjectRelativePath(path);
            
            var renderer = (CameraTextureRenderer)target;
            if (!renderer)
                return;

            var texture = renderer.CaptureImage();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }
    }
}
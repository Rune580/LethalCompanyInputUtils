using InputUtilsUnityToolkit.Editor.Utils;
using LethalCompanyInputUtils.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputUtilsUnityToolkit.Editor
{
    [CustomEditor(typeof(RemapContainerController))]
    public class RemapContainerControllerEditor : CustomEditorBase<RemapContainerController>
    {
        [SerializeField] private int _sectionCount = 8;
        [SerializeField] private int _bindsPerSection = 8;
        
        
        [SerializeField] private int _generatedSectionCount = 0;
        [SerializeField] private GameObject _lastFooter;
        
        protected override VisualElement CreateGUI()
        {
            var root = new VisualElement();

            var sectionCountField = new IntegerField("Section Count")
            {
                value = _sectionCount
            };
            BindElement<int, IntegerField>(sectionCountField, nameof(_sectionCount));

            var bindsPerSectionField = new IntegerField("Binds Per Section")
            {
                value = _bindsPerSection
            };
            BindElement<int, IntegerField>(bindsPerSectionField, nameof(_bindsPerSection));

            var genButton = new Button(OnGenerate);
            genButton.Add(new Label("Generate"));
            
            root.Add(sectionCountField);
            root.Add(bindsPerSectionField);
            root.Add(genButton);

            return root;
        }

        private void OnGenerate()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("You can only generate while in play mode!");
                return;
            }

            if (_lastFooter)
            {
                DestroyImmediate(_lastFooter);
                _lastFooter = null;
            }
            
            var bindsController = Target.bindsList;
            if (bindsController is null)
                return;

            var sectionList = Target.sectionList;
            if (sectionList is null)
                return;

            for (int sectionIndex = 0; sectionIndex < _sectionCount; sectionIndex++)
            {
                var sectionName = $"Section {_generatedSectionCount + sectionIndex + 1}";
                bindsController.AddSection(sectionName);
                sectionList.AddSection(sectionName);

                for (int bindIndex = 0; bindIndex < _bindsPerSection; bindIndex++)
                {
                    bindsController.AddBinds(null, null, controlName: $"Bind {bindIndex + 1}");
                }
            }

            _generatedSectionCount += _sectionCount;
            
            bindsController.AddFooter();
            var contentTransform = bindsController.scrollRect!.content.transform;

            var footer = contentTransform.GetChild(contentTransform.childCount - 1);
            _lastFooter = footer.gameObject;
        }
    }
}
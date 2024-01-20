using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils.Utils;

internal static class CameraUtils
{
    private static Camera? _uiCamera;
    
    public static Camera GetBestUiCamera()
    {
        if (_uiCamera is not null && _uiCamera)
            return _uiCamera;

        _uiCamera = null;

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "MainMenu")
        {
            var uiCameraObject = activeScene.GetRootGameObjects()
                .FirstOrDefault(go => go.name == "UICamera");

            if (uiCameraObject is null)
            {
                Logging.Warn("Failed to find UICamera at MainMenu, falling back to Camera.current!");
                return Camera.current;
            }

            var uiCamera = uiCameraObject.GetComponent<Camera>();
            if (uiCamera is null)
            {
                Logging.Warn("Failed to find Camera component on UICamera, falling back to Camera.current!");
                return Camera.current;
            }

            _uiCamera = uiCamera;
        }
        else
        {
            var systems = activeScene.GetRootGameObjects()
                .FirstOrDefault(go => go.name == "Systems");

            if (systems is null)
            {
                Logging.Warn("Failed to find UICamera in active scene, falling back to Camera.current!");
                return Camera.current;
            }

            var uiCameraTransform = systems.transform.Find("UI/UICamera");
            if (uiCameraTransform is null)
            {
                Logging.Warn("Failed to find UICamera at MainMenu, falling back to Camera.current!");
                return Camera.current;
            }

            var uiCamera = uiCameraTransform.GetComponent<Camera>();
            if (uiCamera is null)
            {
                Logging.Warn("Failed to find Camera component on UICamera, falling back to Camera.current!");
                return Camera.current;
            }
            
            _uiCamera = uiCamera;
        }
        
        return _uiCamera;
    }

    public static void ClearUiCameraReference()
    {
        _uiCamera = null;
    }
}
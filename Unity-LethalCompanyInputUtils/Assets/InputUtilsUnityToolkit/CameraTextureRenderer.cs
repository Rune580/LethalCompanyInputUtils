using System;
using UnityEngine;

namespace InputUtilsUnityToolkit
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class CameraTextureRenderer : MonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            if (!_camera)
                _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            if (!_camera)
                _camera = GetComponent<Camera>();
        }

        public Texture2D CaptureImage()
        {
            if (!_camera)
                _camera = GetComponent<Camera>();

            var curRenderTex = RenderTexture.active;
            RenderTexture.active = _camera.targetTexture;
            
            _camera.Render();

            var targetTex = _camera.targetTexture;
            Texture2D image = new Texture2D(targetTex.width, targetTex.height);
            image.ReadPixels(new Rect(0, 0, targetTex.width, targetTex.height), 0, 0);
            image.Apply();
            
            ApplyTransparency(image);
            image.Apply();

            RenderTexture.active = curRenderTex;
            return image;
        }

        private void ApplyTransparency(Texture2D image)
        {
            var cutOff = Vector3.zero;
                
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    var color = image.GetPixel(x, y);

                    var curRgb = new Vector3(color.r, color.g, color.b);
                    color.a = Vector3.Distance(curRgb, cutOff);
                    
                    image.SetPixel(x, y, color);
                }
            }
        }
    }
}

using UnityEngine;

namespace SimsTools.WinMaze
{
    [RequireComponent(typeof(Transform))]
    public class MouseLook : MonoBehaviour
    {
        public Transform cameraTransform;
        public float sensitivity = 5f; 
        
        private Transform _transform;
        private float _mouseY;
        
        private const float MinY = -90f;
        private const float MaxY = 90f;
        
        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void Update()
        {
            var vertical = Input.GetAxis("Mouse Y");
            var horizontal = Input.GetAxis("Mouse X");
            
            _mouseY += vertical * sensitivity;
            _mouseY = Mathf.Clamp(_mouseY, MinY, MaxY);
            cameraTransform.localRotation = Quaternion.Euler(-_mouseY, 0f, 0f);
            _transform.Rotate(0f, horizontal * sensitivity, 0f);
        }
    }
}

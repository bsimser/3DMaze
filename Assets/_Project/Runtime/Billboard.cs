using UnityEngine;

namespace SimsTools.WinMaze
{
    public class Billboard : MonoBehaviour
    {
        private Camera _camera;
        private Transform _transform;
        
        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _camera = Camera.main;
        }

        private void Update()
        {
            _transform.rotation = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
        }
    }
}

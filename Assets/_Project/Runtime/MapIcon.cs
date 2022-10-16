using UnityEngine;

namespace SimsTools.WinMaze
{
    public class MapIcon : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;

        private Transform _transform;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void Update()
        {
            _transform.position = target.position + offset;
            _transform.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0f);
        }
    }
}

using UnityEngine;

namespace SimsTools.WinMaze
{
    public class SpinningMapIcon : MonoBehaviour
    {
        private Transform _transform;
        private readonly Vector3 _rotationAxis = new Vector3(0f, -1f, 0f);

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void FixedUpdate()
        {
            _transform.RotateAround(_transform.position, _rotationAxis, 2.0f);
        }
    }
}

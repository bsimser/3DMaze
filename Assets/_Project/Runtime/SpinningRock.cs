using UnityEngine;

namespace SimsTools.WinMaze
{
    public class SpinningRock : MonoBehaviour
    {
        private Transform _transform;
        private readonly Vector3 _rotationAxis = new Vector3(1f, 1f, 1f);

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void FixedUpdate()
        {
            _transform.RotateAround(_transform.position, _rotationAxis, 1f);
        }
    }
}

using UnityEngine;

namespace SimsTools.WinMaze
{
    public class RatScript : MonoBehaviour
    {
        private Transform _transform;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private int _direction;
        private int _x;
        private int _y;
        private GameControl _gameControl;

        private const float MoveSpeed = 5.0f;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void FixedUpdate()
        {
            _transform.position = Vector3.MoveTowards(_transform.position, _targetPosition, MoveSpeed * Time.fixedDeltaTime);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, _targetRotation, MoveSpeed);

            if (!(Vector3.Distance(_transform.position, _targetPosition) < 0.01f))
            {
                return;
            }

            _transform.position = _targetPosition;
            _transform.rotation = _targetRotation;

            if (_gameControl == null)
            {
                return;
            }

            var block = _gameControl.mazeBlocks[_x, _y];

            while (true)
            {
                if (GameControl.CheckMovement(block, _direction))
                {
                    _x += _gameControl.wallBlocks[_direction, 0];
                    _y += _gameControl.wallBlocks[_direction, 1];
                    _targetPosition = GameControl.GetGridCoords(_x, _y);
                    _targetRotation = _gameControl.lookDirections[_direction];
                    _direction += 3;
                    _direction %= 4;
                    break;
                }

                _direction++;
                _direction %= 4;
            }
        }

        public void SetPosition(int x, int y, GameControl game)
        {
            _direction = 1;
            _x = x;
            _y = y;
            _gameControl = game;
            _transform.position = GameControl.GetGridCoords(x, y);
            _targetPosition = _transform.position;
            _targetRotation = _transform.rotation;
        }
    }
}
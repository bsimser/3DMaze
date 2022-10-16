using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimsTools.WinMaze
{
    public class PlayerControl : MonoBehaviour
    {
        public enum ControlTypes { Original, Modern }
        
        public GameControl game;
        public Camera mapCamera;
        public ControlTypes controlType = ControlTypes.Original;
        
        private Transform _transform;
        private CharacterController _characterController;
        private MouseLook _mouseLookScript;
        private bool _flipPlayer;
        private Quaternion _targetSpin;
        private bool _isFlipped;
        private Action _controlAction;
        private Action _spinAction;
        private int _x;
        private int _y;
        private int _direction;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isMoving;
        private bool _isRotating;
        private bool _checkCollisions;

        private const float TurnSpeed = 5.0f;
        private const float QuickTurnSpeed = 8.0f;
        private const float WalkSpeed = 10.0f;
        private const float RunSpeed = 20.0f;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _characterController = GetComponent<CharacterController>();
            _mouseLookScript = GetComponent<MouseLook>();
        }

        private void Start()
        {
            if (controlType == ControlTypes.Original)
            {
                _controlAction = OriginalControl;
                _mouseLookScript.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _controlAction = ModernControl;
                _mouseLookScript.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                mapCamera.enabled = !mapCamera.enabled;
            }
        }

        private void FixedUpdate()
        {
            if (_flipPlayer)
            {
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, _targetSpin, TurnSpeed);
                if (!(Quaternion.Angle(_transform.rotation, _targetSpin) < 1.0f))
                {
                    return;
                }
                _flipPlayer = false;
                _isFlipped = !_isFlipped;
                _spinAction?.Invoke();
                return;
            }
            _controlAction();
        }

        private void OriginalControl()
        {
            var vertical = Input.GetAxisRaw("Vertical");
            var horizontal = Input.GetAxisRaw("Horizontal");
            var isRunning = Input.GetButton("Fire2");
            var speed = isRunning ? RunSpeed : WalkSpeed;
            var turnSpeed = isRunning ? QuickTurnSpeed : TurnSpeed;
            
            if(_isMoving || _isRotating)
            {
                if(_isMoving)
                {
                    _transform.position = Vector3.MoveTowards(_transform.position, _targetPosition, speed * Time.fixedDeltaTime);
                    if(Vector3.Distance(_transform.position,_targetPosition) < 0.01f)
                    {
                        _transform.position = _targetPosition;
                        _isMoving = false;
                    }
                }

                if (!_isRotating)
                {
                    return;
                }
                
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, _targetRotation, turnSpeed);
                
                if (!(Quaternion.Angle(_transform.rotation, _targetRotation) < 1.0f))
                {
                    return;
                }

                _transform.rotation = _targetRotation;
                _isRotating = false;
                
                return;
            }

            if (vertical == 0 && horizontal == 0)
            {
                return;
            }
            
            var block = MazeBlockWall.None;

            if (_checkCollisions)
            {
                block = game.mazeBlocks[_x, _y];
            }
            
            if(vertical != 0)
            {
                if(vertical > 0)
                {
                    if(!_checkCollisions || GameControl.CheckMovement(block, _direction))
                    {
                        _x += game.wallBlocks[_direction, 0];
                        _y += game.wallBlocks[_direction, 1];
                        _targetPosition = GameControl.GetGridCoords(_x, _y);
                        _isMoving = true;
                    }
                }	
                else
                {
                    if(!_checkCollisions || GameControl.CheckMovement(block,(_direction + 2) % 4))
                    {
                        _direction += 2;
                        _direction %= 4;
                        _x += game.wallBlocks[_direction, 0];
                        _y += game.wallBlocks[_direction, 1];
                        _targetPosition = GameControl.GetGridCoords(_x, _y);
                        _targetRotation = _transform.rotation * Quaternion.Euler(0, 180, 0);
                        _isMoving = true;
                        _isRotating = true;
                    }
                }
            }

            if (horizontal == 0) return;

            if (_isFlipped)
            {
                horizontal = -horizontal;
            }
                
            if(horizontal > 0)
            {
                _direction++;
                _direction %= 4;
            }
            else
            {
                _direction += 3;
                _direction %= 4;
            }

            if (_isFlipped)
            {
                horizontal = -horizontal;
            }

            _targetRotation = _transform.rotation * Quaternion.Euler(0, 90 * horizontal, 0);
            _isRotating = true;
        }

        private void ModernControl()
        {
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            var speed = Input.GetButton("Fire2") ? RunSpeed : WalkSpeed;

            if (vertical != 0 || horizontal != 0)
            {
                MovePlayer(vertical, horizontal, speed);
            }
        }

        private void MovePlayer(float vertical, float horizontal, float speed)
        {
            var motion = horizontal != 0 && vertical != 0 ? 0.7071f : 1.0f;
            var direction = Vector3.zero;

            direction.z = vertical;
            direction.x = horizontal;
            direction = _transform.TransformDirection(direction);
            direction.y = 0;
            
            _characterController.Move(speed * direction * motion * Time.fixedDeltaTime);
        }

        public void SetPosition(int x, int y, GameControl gameControl)
        {
            _direction = 1;
            _isMoving = true;
            _checkCollisions = true;
            _isFlipped = false;
            _x = x;
            _y = y;
            _transform.position = GameControl.GetGridCoords(x, y);
            _targetPosition = _transform.position;
            _targetRotation = _transform.rotation;
        }

        public void Spin(Action action)
        {
            _flipPlayer = true;
            _targetSpin = _transform.rotation * Quaternion.Euler(0, 0, 180f);
            _spinAction = action;
        }
    }
}

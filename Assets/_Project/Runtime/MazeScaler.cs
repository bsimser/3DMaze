using System;
using UnityEngine;

namespace SimsTools.WinMaze
{
    public class MazeScaler : MonoBehaviour
    {
        public static MazeScaler instance;

        public GameControl gameControl;

        private Vector3 _scale = new Vector3(1, 0, 1);
        private readonly Vector3 _scaleSpeed = new Vector3(0, Speed, 0);
        private Vector3 _position = new Vector3(0, -2, 0);
        private readonly Vector3 _positionSpeed = new Vector3(0, Speed * 2f, 0);
        private float _direction;
        private Action _scaleAction;
        
        private const float Speed = 0.02f;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void FixedUpdate()
        {
            _scale += _scaleSpeed * _direction;
            _position += _positionSpeed * _direction;
            SetObjectsToScale();
            if(Math.Abs(_direction - 1f) < Mathf.Epsilon && _scale.y >= 1f)
            {
                _scale.y = 1f;
                _position.y = 0f;
                _direction = -1f;
                SetObjectsToScale();
                enabled = false;
                _scaleAction?.Invoke();
            }
            else if(Math.Abs(_direction - (-1f)) < Mathf.Epsilon && _scale.y <= 0f)
            {
                _scale.y = 1f;
                _position.y = 0f;
                _direction = -1f;
                SetObjectsToScale();
                enabled = false;
                _scale.y = 0f;
                _position.y = -2f;
                _direction = 1f;
                _scaleAction?.Invoke();
            }
        }

        public void Scale(float direction, Action callback)
        {
            _direction = direction;
            _scaleAction = callback;
            enabled = true;
        }

        private void SetObjectsToScale()
        {
            gameControl.walls.localScale = _scale;
            gameControl.walls.position = _position;
            gameControl.sprites.position = _position;
            foreach (var tr in gameControl.spriteList)
            {
                tr.localScale = _scale;
            }
        }
    }
}

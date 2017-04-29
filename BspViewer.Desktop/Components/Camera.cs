using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;

namespace BspViewer
{
    sealed class Camera
    {
        public Vector3 Position { get; private set; }

        private MouseDevice _mouse;
        private KeyboardDevice _keyboard;

        private bool _captureMouse = false; // When set to true, the mouse is tracked by the camera

        private float _lastX;
        private float _lastY;

        private float _pitch = 0.0f;
	    private float _yaw = 0.0f;

        private float _lookSens = 0.25f; // Look sensitivity. Applies to mouse movement
        private float _moveSens = 500f; // Move sensitivity. Applies to keyboard movement

        private int _width;
        private int _height;

        public Camera(GameWindow game)
        {
            _mouse = game.Mouse;
            _keyboard = game.Keyboard;

            _width = game.ClientRectangle.Width;
            _height = game.ClientRectangle.Height;

            Position = new Vector3(0, 0, 0);
        }

        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void SetViewAngles(float pitch, float yaw)
        {
            _pitch = pitch;
            _yaw = yaw;
        }

        public void Update(double interval)
        {
            if (_captureMouse)
            {
                var x = _mouse.X;
                var y = _mouse.Y;

                // Update rotation based on mouse input
                _yaw += _lookSens * (_lastX - x);

                // Correct z angle to interval [0;360]
                if (_yaw >= 360.0)
                    _yaw -= 360.0f;

                if (_yaw < 0.0)
                    _yaw += 360.0f;

                // Update up down view
                _pitch += _lookSens * (_lastY - y);

                // Correct x angle to interval [-90;90]
                if (_pitch < -90.0)
                    _pitch = -90.0f;

                if (_pitch > 90.0)
                    _pitch = 90.0f;

                // Reset track point
                _lastX = x;
                _lastY = y;
            }
            
            var moveFactor = _moveSens * (float) interval;
            var oldPos = new Vector3(Position);
            var newPos = new Vector3(Position);

            if (_keyboard[Key.E] || _keyboard[Key.Space]) // E or SPACE - UP
            {
                newPos.Z += moveFactor;
            }

            if (_keyboard[Key.Q] || _keyboard[Key.LControl]) // Q or CRTL - DOWN
            {
                newPos.Z -= moveFactor;
            }

            // If strafing and moving reduce speed to keep total move per frame constant
            var strafing = (_keyboard[Key.W] || _keyboard[Key.S]) && (_keyboard[Key.A] || _keyboard[Key.D]);

            if (strafing)
                moveFactor = (float) Math.Sqrt((moveFactor * moveFactor) / 2.0);

            if (_keyboard[Key.W]) // W - FORWARD
            {
                newPos.X += (float) Math.Cos(MathLib.DegToRad(_yaw)) * moveFactor;
                newPos.Y += (float)Math.Sin(MathLib.DegToRad(_yaw)) * moveFactor;
            }

            if (_keyboard[Key.S]) // S - BACKWARD
            {
                newPos.X -= (float) Math.Cos(MathLib.DegToRad(_yaw)) * moveFactor;
                newPos.Y -= (float) Math.Sin(MathLib.DegToRad(_yaw)) * moveFactor;
            }

            if (_keyboard[Key.A]) // A - LEFT
            {
                newPos.X += (float) Math.Cos(MathLib.DegToRad(_yaw + 90.0f)) * moveFactor;
                newPos.Y += (float) Math.Sin(MathLib.DegToRad(_yaw + 90.0f)) * moveFactor;
            }

            if (_keyboard[Key.D]) // D - RIGHT
            {
                newPos.X += (float) Math.Cos(MathLib.DegToRad(_yaw - 90.0f)) * moveFactor;
                newPos.Y += (float) Math.Sin(MathLib.DegToRad(_yaw - 90.0f)) * moveFactor;
            }

            if (_keyboard[Key.Up]) // UP - ROTATE UP
            {
                _pitch += (moveFactor / 3);
            }

            if (_keyboard[Key.Down]) // DOWN - ROTATE DOWN
            {
                _pitch -= (moveFactor / 3);
            }

            if (_keyboard[Key.Left]) // LEFT - ROTATE LEFT
            {
                _yaw += (moveFactor / 3);
            }

            if (_keyboard[Key.Right]) // RIGHT - ROTATE RIGHT
            {
                _yaw -= (moveFactor / 3);
            }

            Position = newPos;
        }
        
        public void Look()
        {
            // In BSP v30 the z axis points up and we start looking parallel to x axis.

            // Look Up/Down
            GL.Rotate(-_pitch - 90.0, 1, 0, 0);

            // Look Left/Right
            GL.Rotate(-_yaw + 90.0, 0, 0, 1);

            // Move
            GL.Translate(-Position.X, -Position.Y, -Position.Z);
        }
    }
}
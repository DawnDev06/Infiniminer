/* ----------------------------------------------------------------------------
MIT License

Copyright (c) 2009 Zach Barth
Copyright (c) 2023 Christopher Whitley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
---------------------------------------------------------------------------- */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Infiniminer
{
    public class Camera
    {
        public float Pitch, Yaw;
        public Vector3 Position;
        public bool UseVrCamera;
        public Matrix VrHeadTransform;
        public float VrYaw;
        public Matrix ViewMatrix = Matrix.Identity;
        public Matrix ProjectionMatrix = Matrix.Identity;

        public Camera(GraphicsDevice device)
        {
            Pitch = 0;
            Yaw = 0;
            Position = Vector3.Zero;

            float aspectRatio = device.Viewport.AspectRatio;
            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), aspectRatio, 0.01f, 1000.0f);
        }

        // Returns a unit vector pointing in the direction that we're looking.
        public Vector3 GetLookVector()
        {
            Matrix rotation = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw + VrYaw);
            return Vector3.Transform(Vector3.Forward, rotation);
        }

        public Vector3 GetRightVector()
        {
            Matrix rotation = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw + VrYaw);
            return Vector3.Transform(Vector3.Right, rotation);
        }

        public void Update()
        {
            Vector3 target = Position + GetLookVector();
            this.ViewMatrix = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }

        // update Yaw and Pitch from a matrix
        public void ApplyHeadTransform(Matrix headTransform)
        {
            VrHeadTransform = headTransform;

            Vector3 right = headTransform.Right;
            Vector2 xz = new Vector2(right.X, -right.Z);

            if (xz != Vector2.Zero)
            {
                xz.Normalize();
                VrYaw = (float)Math.Atan2(xz.Y, xz.X);
                headTransform = headTransform * Matrix.CreateRotationY(-VrYaw); // invert yaw
            }

            Vector3 forward = headTransform.Forward;
            Vector2 yz = new Vector2(-forward.Z, forward.Y);
            if (yz != Vector2.Zero)
            {
                yz.Normalize();
                Pitch = (float)Math.Atan2(yz.Y, yz.X);
            }
        }
    }
}

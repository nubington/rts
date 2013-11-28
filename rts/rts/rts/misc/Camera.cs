using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class Camera
    {
        protected float _zoom; // Camera Zoom
        public Matrix transform; // Matrix Transform
        public Vector2 _pos; // Camera Position
        protected float _rotation; // Camera Rotation

        public Camera()
        {
            _zoom = 1.0f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
        }

        // Sets and gets zoom
        public float Zoom
        {
            get { return _zoom; }
            //set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image\
            set { _zoom = Util.ConvertToPositiveRadians(value); }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public Matrix get_transformation(Viewport viewport)
        {
            Projection = Matrix.CreateRotationZ(Rotation);
            View = Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            //World = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            World = Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));

            transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                            Projection *
                                            View *
                                            World;
            return transform;
        }

        public Matrix get_minimap_transformation(Viewport viewport)
        {
            Projection = Matrix.CreateRotationZ(Rotation);
            View = Matrix.CreateScale(new Vector3(1, 1, 1));
            //World = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            World = Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));

            transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-viewport.Height * 0.5f, -viewport.Height * 0.5f, 0)) *
                                            Projection *
                                            View *
                                            World;
            return transform;
        }

        public Matrix get_minimap_transformation_reverse_rotation(Viewport viewport)
        {
            Projection = Matrix.CreateRotationZ(Rotation + (float)Math.PI);
            View = Matrix.CreateScale(new Vector3(1, 1, 1));
            //World = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            World = Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));

            transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-viewport.Height * 0.5f, -viewport.Height * 0.5f, 0)) *
                                            Projection *
                                            View *
                                            World;
            return transform;
        }


        public Matrix Projection { get; set; }
        public Matrix View { get; set; }
        public Matrix World { get; set; }


    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Progress;
using SharpDX;

namespace IF.Ray.Core
{
    public class SceneRenderer : IAsyncRenderer
    {
        private Scene _scene;

        private Quaternion _oldQ;

        private SceneParameter<float> _zoom;
        private SceneParameter<float> _rotationX;
        private SceneParameter<float> _rotationY;
        private SceneParameter<float> _rotationZ;

        private WriteableBitmap _lastRender;
        private float _lastRotX;
        private float _lastRotY;
        private float _lastRotZ;

        #region Properties

        public SceneParameter<float> RotationX
        {
            get { return _rotationX; }
            set
            {
                _rotationX = value;
            }
        }

        public SceneParameter<float> RotationY
        {
            get { return _rotationY; }
            set
            {
                _rotationY = value;
            }
        }

        public SceneParameter<float> RotationZ
        {
            get { return _rotationZ; }
            set
            {
                _rotationZ = value;
            }
        }

        public SceneParameter<float> Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
            }
        }

        public bool Initialised { get; set; }

        #endregion

        public SceneRenderer()
        {
            // set the defaults for these parameters
            _rotationX = 0;
            _rotationY = 0;
            _rotationZ = 0;
            _zoom = 1;

            _oldQ = Quaternion.Identity;
        }

        public async Task InitialiseSceneAsync()
        {
            var camera = new Camera(new Vector3(0, 6, -10), new Vector3(0, -6, 10));

            _scene = new Scene(camera);
            var shapeFactory = new ShapeFactory();
            var square = await shapeFactory.GetShape<Cube>();
            var cylinder = await shapeFactory.GetShape<Cylinder>();

            _scene.Bindings.Add(new SceneBinding(cylinder, new Vector3(-10, 0, 0)));
            _scene.Bindings.Add(new SceneBinding(square, new Vector3(0, 0, 0)));

            _scene.Lights.Add(new Light(new Vector3(0, 50, -5), Color.White, 1));

            Initialised = true;
        }

        public async Task<WriteableBitmap> RenderAsync(int width, int height, ProgressToken token)
        {
            var wb = new WriteableBitmap(width, height);
            var stream = wb.PixelBuffer.AsStream();

            await Task.Run(() => Render(stream, width, height, token));

            token.Value = 100;

            _lastRender = wb;
            return _lastRender;
        }

        private void Render(Stream stream, int width, int height, ProgressToken token)
        {
            // get relative rotation amount
            var rotXDiff = _rotationX - _lastRotX;
            var rotYDiff = _rotationY - _lastRotY;
            var rotZDiff = _rotationZ - _lastRotZ;
            _lastRotX = _rotationX.Value;
            _lastRotY = _rotationY.Value;
            _lastRotZ = _rotationZ.Value;
            
            // rotation
            // get new *relative* world q
            var worldQ = Quaternion.RotationYawPitchRoll(rotYDiff.Value, rotXDiff.Value, rotZDiff.Value);

            // transform world q into local rotation q
            var rotateQ = _oldQ*Quaternion.Invert(_oldQ)*worldQ*_oldQ;
            _oldQ = rotateQ;

            // get the world projection matrix
            var worldViewProj = Matrix.RotationQuaternion(rotateQ);
            worldViewProj.Transpose();

            // set the camera zoom
            _scene.Camera.Scale = 1 / Zoom.Value;

            foreach (var triangle in _scene.Bindings.SelectMany(binding => binding.Mesh.Triangles))
            {
                triangle.Reset();
            }

            if (stream.CanWrite)
            {
                stream.Position = 0;

                // generate checkpoints
                var tenPercent = (width*height)/10;
                var list = Enumerable.Range(1, 9).Select(i => i*tenPercent).ToList();

                int p = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var color = TraceRay(worldViewProj, x, y, width, height);
                        stream.WriteByte(color.B);
                        stream.WriteByte(color.G);
                        stream.WriteByte(color.R);
                        stream.WriteByte(color.A);

                        if (list.Contains(++p))
                        {
                            token.Value += 10;
                        }
                    }
                }

                stream.Flush();
            }
        }

        private Color TraceRay(Matrix proj, int x, int y, int width, int height)
        {
            // get camera plane; plane that intersects camera location z-axis
            var cameraPlane = new Plane(_scene.Camera.Position, _scene.Camera.Direction);

            var cameraScaled = Vector3.Divide(_scene.Camera.Position, _scene.Camera.Scale);
            
            const float pixelSize = 0.1f;
            var u = x - (width * 0.5);
            var v = y - (height * 0.5);

            //TODO this isn't right
            var uvPixel = new Vector3(
                cameraScaled.X + (float)u * pixelSize,
                cameraScaled.Y + (float)v * pixelSize,
                cameraScaled.Z);

            // get the direction of the ray
            var rayDir = _scene.Camera.Target - uvPixel;

            // get the actual ray
            var ray = new SharpDX.Ray(uvPixel, rayDir);
            
            foreach (var binding in _scene.Bindings)
            {
                var intersecting = new List<ZBufferItem>();

                foreach (var triangle in binding.Mesh.Triangles)
                {
                    Vector3 intersection;
                    var intersects = triangle.TranslateTo(binding.Position).Transform(proj).Intersects(ray, out intersection);

                    if (intersects)
                    {
                        intersecting.Add(new ZBufferItem(triangle, intersection));
                    }
                }

                if (intersecting.Any())
                {
                    var closest = intersecting.OrderByDescending(i => i.Distance(ray)).First();

                    return closest.Triangle.Colorise(_scene.Lights, ray, closest.Intersection);
                }
            }

            return Color.White;
        }
    }
}
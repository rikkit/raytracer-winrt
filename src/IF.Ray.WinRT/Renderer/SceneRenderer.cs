using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using IF.Ray.WinRT.Models;
using SharpDX;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace IF.Ray.WinRT.Renderer
{
    public class SceneRenderer : IAsyncRenderer
    {
        private Scene _scene;

        private Quaternion _oldQ;
        private float _rotationX;
        private float _rotationY;
        private float _rotationZ;

        private bool _parametersChanged;

        private WriteableBitmap _lastRender;
        private float _zoom;
        private float _lastRotX;
        private float _lastRotY;
        private float _lastRotZ;

        public float RotationX
        {
            get { return _rotationX; }
            set
            {
                _rotationX = value;
                _parametersChanged = true;
            }
        }

        public float RotationY
        {
            get { return _rotationY; }
            set
            {
                _rotationY = value;
                _parametersChanged = true;
            }
        }

        public float RotationZ
        {
            get { return _rotationZ; }
            set
            {
                _rotationZ = value;
                _parametersChanged = true;
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                _parametersChanged = true;
            }
        }

        public bool Initialised { get; set; }

        public SceneRenderer()
        {
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;
            Zoom = 1;
            _oldQ = Quaternion.Identity;
        }

        public async Task InitialiseSceneAsync()
        {
            var camera = new Camera(new Vector3(0, 6, -10), new Vector3(0, -6, 10));

            _scene = new Scene(camera);
            var shapeFactory = new ShapeFactory();
            var square = await shapeFactory.GetShape<Cube>();
            var cylinder = await shapeFactory.GetShape<Cylinder>();

            _scene.Bindings.Add(new SceneBinding(square, new Vector3(0, 0, 0)));
            _scene.Bindings.Add(new SceneBinding(cylinder, new Vector3(-10, 0, 0)));

            _scene.Lights.Add(new Light(new Vector3(0, 10, -5), 1));

            Initialised = true;
        }

        public async Task<WriteableBitmap> RenderAsync(int width, int height)
        {
            // if no values have changed then don't bother with rendering
            if (!Initialised || !_parametersChanged)
            {
                return _lastRender;
            }
            _parametersChanged = false;
            
            // get relative rotation amount
            var rotXDiff = _rotationX - _lastRotX;
            var rotYDiff = _rotationY - _lastRotY;
            var rotZDiff = _rotationZ - _lastRotZ;
            _lastRotX = _rotationX;
            _lastRotY = _rotationY;
            _lastRotZ = _rotationZ;

            // rotation
            // get new *relative* world q
            var worldQ = Quaternion.RotationYawPitchRoll(rotYDiff, rotXDiff, rotZDiff);
            
            // transform world q into local rotation q
            var rotateQ = _oldQ*Quaternion.Invert(_oldQ)*worldQ*_oldQ;
            _oldQ = rotateQ;

            // get the world projection matrix
            var rotationMatrix = Matrix.RotationQuaternion(rotateQ);
            var worldViewProj = rotationMatrix;

            worldViewProj.ScaleVector = new Vector3(Zoom);
            worldViewProj.Transpose();

            foreach (var triangle in _scene.Bindings.SelectMany(binding => binding.Mesh.Triangles))
            {
                triangle.Reset();
            }

            var wb = new WriteableBitmap(width, height);
            var stream = wb.PixelBuffer.AsStream();
            if (stream.CanWrite)
            {
                stream.Position = 0;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var color = TraceRay(worldViewProj, x, y, width, height);
                        stream.WriteByte(color.B);
                        stream.WriteByte(color.G);
                        stream.WriteByte(color.R);
                        stream.WriteByte(color.A);
                    }
                }

                stream.Flush();

                _lastRender = wb;
            }
            
            return _lastRender;
        }

        private Color TraceRay(Matrix proj, int x, int y, int width, int height)
        {
            // get camera plane; plane that intersects camera location z-axis
            var cameraPlane = new Plane(_scene.Camera.Position, _scene.Camera.Direction);
            const float pixelSize = 0.1f;
            var u = x - (width * 0.5);
            var v = y - (height * 0.5);

            //TODO this isn't right
            var uvPixel = new Vector3(
                _scene.Camera.Position.X + (float)u * pixelSize,
                _scene.Camera.Position.Y + (float)v * pixelSize,
                _scene.Camera.Position.Z);

            // get the direction of the ray
            var rayDir = _scene.Camera.Target - uvPixel;

            // get the actual ray
            var ray = new SharpDX.Ray(uvPixel, rayDir);
            
            foreach (var binding in _scene.Bindings)
            {
                foreach (var triangle in binding.Mesh.Triangles)
                {
                    Vector3 intersection;
                    var intersects = triangle.TranslateTo(binding.Position).Transform(proj).Intersects(ray, out intersection);

                    if (intersects)
                    {
                        return triangle.Colorise(_scene.Lights, ray, intersection);
                    }
                }
            }

            return Color.White;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Progress;
using SharpDX;

namespace IF.Ray.Core
{
    public class SceneRenderer : PropertyChangingBase, IAsyncRenderer
    {
        private Scene _scene;

        private Quaternion _oldQ;
        
        private WriteableBitmap _lastRender;
        private float _lastRotX;
        private float _lastRotY;
        private float _lastRotZ;

        #region Properties

        public bool Initialised { get; set; }

        /// <summary>
        /// Set of parameters to use for rendering
        /// Also used for before parameters in the animation
        /// </summary>
        public ParameterBinding RenderParameters { get; set; }

        /// <summary>
        /// Set of parameters used as the end parameters in the animation
        /// </summary>s
        public ParameterBinding AnimationParameters { get; set; }

        #endregion

        public SceneRenderer(CoreDispatcher dispatcher) : base(dispatcher)
        {
            RenderParameters = new ParameterBinding(dispatcher);
            AnimationParameters = new ParameterBinding(dispatcher);

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
            var rotXDiff = RenderParameters.RotationX - _lastRotX;
            var rotYDiff = RenderParameters.RotationY - _lastRotY;
            var rotZDiff = RenderParameters.RotationZ - _lastRotZ;
            _lastRotX = RenderParameters.RotationX;
            _lastRotY = RenderParameters.RotationY;
            _lastRotZ = RenderParameters.RotationZ;
            
            // rotation
            // get new *relative* world q
            var worldQ = Quaternion.RotationYawPitchRoll(rotYDiff, rotXDiff, rotZDiff);

            // transform world q into local rotation q
            var rotateQ = _oldQ*Quaternion.Invert(_oldQ)*worldQ*_oldQ;
            _oldQ = rotateQ;

            // get the world projection matrix
            var worldViewProj = Matrix.RotationQuaternion(rotateQ);
            worldViewProj.Transpose();

            // set the camera zoom
            _scene.Camera.Scale = 1 / RenderParameters.Zoom;

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Progress;
using IF.Ray.Core.Shapes;
using SharpDX;
using Plane = IF.Ray.Core.Shapes.Plane;

namespace IF.Ray.Core
{
    public class SceneRenderer : IAsyncRenderer
    {
        private readonly IShapeFactory _shapeFactory;
        private Scene _scene;

        private Quaternion _oldQ;
        
        private float _lastRotX;
        private float _lastRotY;
        private float _lastRotZ;

        #region Properties

        public bool Initialised { get; set; }
        
        #endregion

        public SceneRenderer()
        {
            _oldQ = Quaternion.Identity;
            _shapeFactory = new ShapeFactory();
        }

        public async Task InitialiseSceneAsync()
        {
            var camera = new Camera(new Vector3(0, 6, -10), new Vector3(0, -6, 10));
            _scene = new Scene(camera);

            var square = await _shapeFactory.GetShape<Cube>();
            var cylinder = await _shapeFactory.GetShape<Cylinder>();

            var plane = new Shapes.Plane(new Vector3(2,2,2), new Vector3(1,0,0), new Vector3(0,0,1));

            _scene.AddBinding(cylinder, new Vector3(-10, 0, 0));
            _scene.AddBinding(square, Vector3.Zero);
            _scene.AddBinding(plane, Vector3.Zero);

            _scene.Lights.Add(new Light(new Vector3(0, 0, -50), Color.White, 1));

            Initialised = true;
        }

        public async Task<WriteableBitmap> RenderAsync(int width, int height, ParameterBinding rp, ProgressToken token)
        {
            var wb = new WriteableBitmap(width, height);
            var stream = wb.PixelBuffer.AsStream();
            await Task.Run(() => Render(stream, width, height, rp, token));

            token.Value = 100;

            return wb;
        }

        private void Render(Stream stream, int width, int height, ParameterBinding rp, ProgressToken token)
        {
            // get relative rotation amount
            var rotXDiff = rp.RotationX - _lastRotX;
            var rotYDiff = rp.RotationY - _lastRotY;
            var rotZDiff = rp.RotationZ - _lastRotZ;
            _lastRotX = rp.RotationX;
            _lastRotY = rp.RotationY;
            _lastRotZ = rp.RotationZ;
            
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
            _scene.Camera.Scale = 1 / rp.Zoom;
            
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

                        if (token != null && list.Contains(++p))
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
            var ray = new Shapes.Ray(uvPixel, rayDir);

            var items = _scene.Trace(ray, _scene.Origin);

            if (items.Count > 0)
            {
                // get the colour of the closest item

                var closest = items.OrderByDescending(d => d.Distance(ray.Origin)).First();
                return closest.Primitive.Colorise(_scene, ray, closest.Translation, closest.Intersection);
            }
            return Color.White;
        }

        public async Task<List<WriteableBitmap>> Animate(int renderWidth, int renderHeight, TimeSpan length, ProgressToken token, ParameterBinding start, ParameterBinding end)
        {
            const int parallelism = 2; // render this many at a time       
            const int fps = 24;
            var totalFrames = (int)length.TotalSeconds*fps;

            var frames = new List<WriteableBitmap>();
            for (var frame = 0; frame < totalFrames; frame += parallelism)
            {
                var frameBindings = Enumerable.Range(frame, parallelism).Select(i => start.Interpolate(end, i, totalFrames));
                var renderTasks = frameBindings.Select(b => RenderAsync(renderWidth, renderHeight, b, null));

                var rendered = await Task.WhenAll(renderTasks);
                frames.AddRange(rendered);
            }

            return frames;
        }
    }
}
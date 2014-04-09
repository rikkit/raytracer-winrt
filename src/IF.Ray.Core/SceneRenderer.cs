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

namespace IF.Ray.Core
{
    public class SceneRenderer : IAsyncRenderer
    {
        private readonly IShapeFactory _shapeFactory;
        private Scene _scene;

        private Matrix _lastRotation;
        
        private float _lastRotX;
        private float _lastRotY;
        private float _lastRotZ;

        #region Properties

        public bool Initialised { get; set; }
        
        #endregion

        public SceneRenderer()
        {
            _lastRotation = Matrix.Identity;
            _shapeFactory = new ShapeFactory();
        }

        public async Task InitialiseSceneAsync()
        {
            var cameraPosition = new Vector3(0, 8, -20);
            var camera = new Camera(-cameraPosition, cameraPosition);
            _scene = new Scene(camera);

            var square = await _shapeFactory.GetShape<Cube>();
            square.Shader = Shader.ShaderFromColour(Color.Red);

            var cylinder = await _shapeFactory.GetShape<Cylinder>();
            cylinder.Shader = Shader.ShaderFromColour(Color.Blue);

            var plane = await _shapeFactory.GetShape<ObjPlane>();
            plane.Shader = Shader.ShaderFromColour(Color.ForestGreen);

            _scene.AddBinding(cylinder,Vector3.Zero);
            _scene.AddBinding(square, Vector3.Zero);
            _scene.AddBinding(plane, Vector3.Zero);

            _scene.Lights.Add(new Light(new Vector3(0, 10, 0), Color.White, 10));
            _scene.Lights.Add(new Light(new Vector3(-10, 10, 0), Color.White, 10));
            _scene.Lights.Add(new Light(new Vector3(10, 10, 0), Color.White, 10));
            _scene.Lights.Add(new Light(new Vector3(0, 10, -10), Color.White, 100));
            _scene.Lights.Add(new Light(new Vector3(0, 10, 10), Color.White, 10));

            _scene.Shader = Shader.ShaderFromColour(Color.Black);

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
            var rotationDiff = Matrix.RotationYawPitchRoll(rotYDiff, rotXDiff, rotZDiff);

            // transform world q into local rotation q
            var fullRotation = _lastRotation*rotationDiff;
            _lastRotation = fullRotation;

            // get the world projection matrix
            fullRotation.Transpose();

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
                        var color = TraceRay(fullRotation, x, height - y, width, height);
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
            var camPositionScaled = Vector3.Divide(_scene.Camera.Position, _scene.Camera.Scale);

            // work out the orientation of the image plane
            var camDir = _scene.Camera.Target - camPositionScaled;
            var xDir = Vector3.Cross(Vector3.UnitY, camDir);
            var yDir = Vector3.Cross(camDir, xDir); // now correct the up dir
            camDir.Normalize();
            xDir.Normalize();
            yDir.Normalize();

            // now get the uv coordinates on our image plane, scaled by unitsize
            const float unitsize = 0.1f;
            var u = (float)((x - 0.5 * width) * unitsize);
            var v = (float)((y - 0.5 * height) * unitsize);

            // now move u units along x, v units along y, starting at camposition
            var uv = camPositionScaled + u * xDir + v * yDir;

            // now the direction of the ray from uv to the target
            var rayDir = _scene.Camera.Target - uv;
            rayDir.Normalize();

            // get the actual ray
            var ray = new Shapes.Ray(uv, rayDir);

            var items = _scene.Trace(ray, proj, _scene.Origin);

            if (items.Count > 0)
            {
                // get the colour of the closest item

                var closest = items.OrderByDescending(d => d.Distance(ray.Origin)).First();
                return closest.Primitive.Colorise(_scene, ray, closest.Translation, closest.Intersection);
            }
            return _scene.Ambient();
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
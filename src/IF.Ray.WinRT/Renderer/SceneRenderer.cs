using System.Linq;
using Windows.UI.Xaml.Media.Animation;
using CommonDX;
using IF.Ray.WinRT.Models;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.IO;
using System;
using System.Diagnostics;
using System.IO;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace IF.Ray.WinRT.Renderer
{
    public class SceneRenderer : Component, IRenderer
    {
        private Buffer _constantBuffer;
        private Scene _scene;
        private Stopwatch _clock;
        private Matrix _view;

        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public void Initialise(DeviceManager devices)
        {
            RemoveAndDispose(ref _constantBuffer);

            var dx3Device = devices.DeviceDirect3D;

            _scene = new Scene();
            var shapeFactory = new ShapeFactory();
            var shape = shapeFactory.GetShape<Cube>();
            var shape2 = shapeFactory.GetShape<Cube>();

            _scene.AddShape(shape, new Vector4(-10, 0, 10, 1));
            _scene.AddShape(shape2, _scene.Origin);

            foreach (var binding in _scene.Bindings)
            {
                binding.InitialiseBuffer(dx3Device);
            }

            var buffer = new Buffer(dx3Device,
                Utilities.SizeOf<Matrix>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
            _constantBuffer = ToDispose(buffer);

            // set up camera
            _view = Matrix.LookAtLH(new Vector3(-8, 10, 0), new Vector3(0, 0, 0), Vector3.UnitY);

            _clock = new Stopwatch();
            _clock.Start();
        }

        public void Render(TargetBase render)
        {
            var dx3Context = render.DeviceManager.ContextDirect3D;

            var width = (float) render.RenderTargetSize.Width;
            var height = (float) render.RenderTargetSize.Height;

            var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4f, width / (float)height, 0.1f, 100f);
            var viewProj = Matrix.Multiply(_view, proj);

            var rotationMatrix = Matrix.RotationX(RotationX) * Matrix.RotationY(RotationY) * Matrix.RotationZ(RotationZ);
            var worldViewProj = rotationMatrix * viewProj;
            worldViewProj.Transpose();

            // Set targets (This is mandatory in the loop)
            dx3Context.OutputMerger.SetTargets(render.DepthStencilView, render.RenderTargetView);

            // Clear the views
            dx3Context.ClearDepthStencilView(render.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
            
            dx3Context.ClearRenderTargetView(render.RenderTargetView, Color.Transparent);

            // Calculate world view projection
            //var time = (float)(_clock.ElapsedMilliseconds / 1000.0);
            

            foreach (var binding in _scene.Bindings)
            {
                // set up pipeline
                dx3Context.InputAssembler.SetVertexBuffers(0, binding.Dx3VertexBufferBinding);

                // vertex shader
                if (binding.Dx3InputLayout != null)
                {
                    dx3Context.InputAssembler.InputLayout = binding.Dx3InputLayout;
                }
                if (binding.Dx3VertexShader != null)
                {
                    dx3Context.VertexShader.Set(binding.Dx3VertexShader);
                }
                if (binding.Dx3PixelShader != null)
                {
                    dx3Context.PixelShader.Set(binding.Dx3PixelShader);
                }

                dx3Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                dx3Context.VertexShader.SetConstantBuffer(0, _constantBuffer);

                // Update Constant Buffer
                dx3Context.UpdateSubresource(ref worldViewProj, _constantBuffer, 0);

                // Draw the cube
                dx3Context.Draw(binding.VertexBufferCount, 0);
            }
        }
    }
}
using CommonDX;
using IF.Ray.WinRT.Models;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace IF.Ray.WinRT.Renderer
{
    public class SceneRenderer : Component, IRenderer
    {
        private Buffer _constantBuffer;
        private Scene _scene;
        private Stopwatch _clock;
        private Matrix _view;
        private Vector3 _defaultCameraLocation;
        private VertexBufferBinding _vertexBufferBinding;

        private Quaternion _oldQ;
        private float _rotationX;
        private float _rotXDiff;
        private float _rotationY;
        private float _rotYDiff;
        private float _rotationZ;
        private float _rotZDiff;

        public float RotationX
        {
            get { return _rotationX; }
            set
            {
                _rotXDiff = _rotationX - value;
                _rotationX = value;
            }
        }

        public float RotationY
        {
            get { return _rotationY; }
            set
            {
                _rotYDiff = _rotationY - value;
                _rotationY = value;
            }
        }

        public float RotationZ
        {
            get { return _rotationZ; }
            set
            {
                _rotZDiff = _rotationZ - value;
                _rotationZ = value;
            }
        }

        public float Zoom { get; set; }

        public SceneRenderer()
        {
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;
            Zoom = 1;
            _oldQ = Quaternion.Identity;
        }

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

            var list = new List<Vector4[]>();
            var bufferIndex = 0;
            foreach (var binding in _scene.Bindings)
            {
                binding.Initialise(dx3Device);
                list.Add(binding.BufferVertices);

                binding.BufferIndex = bufferIndex; // store the index of the start of this model's vertices
                bufferIndex += binding.BufferVertexCount;
            }

            // collapse model's vertices into one buffer
            var vertices = Buffer.Create(dx3Device, BindFlags.VertexBuffer, list.SelectMany(v => v).ToArray());
            _vertexBufferBinding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>(), 0);

            var buffer = new Buffer(dx3Device,
                Utilities.SizeOf<Matrix>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
            _constantBuffer = ToDispose(buffer);

            _defaultCameraLocation = new Vector3(0, 6, -10);

            _clock = new Stopwatch();
            _clock.Start();
        }

        public void Render(TargetBase render)
        {
            var dx3Context = render.DeviceManager.ContextDirect3D;

            var width = (float) render.RenderTargetSize.Width;
            var height = (float) render.RenderTargetSize.Height;

            // set up camera
            _view = Matrix.LookAtLH(_defaultCameraLocation * (1/Zoom), new Vector3(0, 0, 0), Vector3.UnitY);
            
            // get new *relative* world q
            var worldQ = Quaternion.RotationYawPitchRoll(_rotYDiff, _rotXDiff, _rotZDiff);

            // reset relative values
            _rotXDiff = 0;
            _rotYDiff = 0;
            _rotZDiff = 0;

            // transform world q into local rotation q
            var rotateQ = _oldQ * Quaternion.Invert(_oldQ) * worldQ * _oldQ;
            _oldQ = rotateQ;

            // get the matrix
            var rotationMatrix = Matrix.RotationQuaternion(rotateQ);
            var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4f, width / (float)height, 0.1f, 100f);
            var viewProj = _view * proj;

            // project!
            var worldViewProj = rotationMatrix * viewProj;
            worldViewProj.Transpose();

            // Set targets (This is mandatory in the loop)
            dx3Context.OutputMerger.SetTargets(render.DepthStencilView, render.RenderTargetView);

            // Clear the views
            dx3Context.ClearDepthStencilView(render.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
            
            dx3Context.ClearRenderTargetView(render.RenderTargetView, Color.Transparent);

            // Calculate world view projection
            //var time = (float)(_clock.ElapsedMilliseconds / 1000.0);

            // set up pipeline
            dx3Context.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);

            foreach (var binding in _scene.Bindings)
            {
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
                dx3Context.Draw(binding.BufferVertexCount, binding.BufferIndex);
            }
        }
    }

    public static class RenderEx
    {
        public static Vector3 AsVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
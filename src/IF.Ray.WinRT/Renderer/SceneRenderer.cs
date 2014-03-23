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
        private VertexBufferBinding _vertexBufferBinding;
        private Stopwatch _clock;
        private PixelShader _pixelShader;
        private InputLayout _layout;
        private VertexShader _vertexShader;

        public void Initialise(DeviceManager devices)
        {
            RemoveAndDispose(ref _constantBuffer);

            var dx3Device = devices.DeviceDirect3D;

            var shapeFactory = new ShapeFactory();
            var cube = shapeFactory.GetShape<Cube>();

            // group the mesh-vertices and normals by each face

            var faceVectorCount = 6; // number of vectors per face
            var group = cube.Groups.First(); // TODO factor this out
            var sdxVertices = new Vector4[faceVectorCount * group.Faces.Count];
            for (var f = 0; f < group.Faces.Count; f++)
            {
                var face = group.Faces[f];

                // interleave normals VNVNVN| or not VVVNNN| ?

                // interleaved
                //for (var fv = 0; fv < face.Count; fv += 2)
                //{
                //    var faceVertex = face[fv];
                //    var vertex = cube.Vertices[faceVertex.VertexIndex - 1];
                //    var v4 = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);

                //    var normal = cube.Normals[faceVertex.NormalIndex];
                //    var n4 = new Vector4(normal.X, normal.Y, normal.Z, 1.0f);
                //    sdxVertices[f * faceVectorCount + fv] = v4;
                //    sdxVertices[f * faceVectorCount + fv + 1] = n4;
                //}

                // sequential
                for (var fv = 0; fv < face.Count; fv++)
                {
                    var faceVertex = face[fv];
                    for (var vi = 0; vi < 3; vi++)
                    {
                        var vertex = cube.Vertices[faceVertex.VertexIndex - 1];
                        var v4 = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);
                        sdxVertices[f * faceVectorCount + vi] = v4;
                    }

                    for (var ni = 3; ni < 6; ni++)
                    {
                        var normal = cube.Normals[faceVertex.NormalIndex];
                        var n4 = new Vector4(normal.X, normal.Y, normal.Z, 1.0f);

                        sdxVertices[f * faceVectorCount + fv + ni] = n4;
                    }
                }
            }

            //TODO use the stuff from the obj file

            var path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            try
            {
                // Loads vertex shader bytecode
                var vertexShaderByteCode = NativeFile.ReadAllBytes(path + "\\MiniCube_VS.fxo");
                _vertexShader = new VertexShader(dx3Device, vertexShaderByteCode);

                // Loads pixel shader bytecode
                var pixelShaderByteCode = NativeFile.ReadAllBytes(path + "\\MiniCube_PS.fxo");
                _pixelShader = new PixelShader(dx3Device, pixelShaderByteCode);

                // Layout from VertexShader input signature
                _layout = new InputLayout(dx3Device, vertexShaderByteCode, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                });
            }
            catch (FileNotFoundException ex)
            {
                //TODO: handle file not found
            }

            //var sdxVertices2 = new Vector4[]
            //{
            //    new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
            //                          new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

            //                          new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
            //                          new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

            //                          new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
            //                          new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

            //                          new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
            //                          new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
            //                          new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

            //                          new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
            //                          new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //};

            var vertices = Buffer.Create(dx3Device, BindFlags.VertexBuffer, sdxVertices);
            _vertexBufferBinding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0);

            var buffer = new Buffer(dx3Device,
                Utilities.SizeOf<Matrix>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
            _constantBuffer = ToDispose(buffer);

            _clock = new Stopwatch();
            _clock.Start();
        }

        public void Render(TargetBase render)
        {
            var dx3Context = render.DeviceManager.ContextDirect3D;

            var width = (float) render.RenderTargetSize.Width;
            var height = (float) render.RenderTargetSize.Height;

            var view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4f, width / (float)height, 0.1f, 100f);
            var viewProj = Matrix.Multiply(view, proj);

            var time = (float)(_clock.ElapsedMilliseconds / 1000.0);

            // Set targets (This is mandatory in the loop)
            dx3Context.OutputMerger.SetTargets(render.DepthStencilView, render.RenderTargetView);

            // Clear the views
            dx3Context.ClearDepthStencilView(render.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

            dx3Context.ClearRenderTargetView(render.RenderTargetView, Color.Transparent);

            // Calculate world view projection
            var worldViewProj = Matrix.RotationX(time)*Matrix.RotationY(time*4)*Matrix.RotationZ(time*1.4f)*viewProj;
            worldViewProj.Transpose();
            
            // Setup the pipeline
            dx3Context.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);

            // Vertex shader
            if (_layout != null)
            {
                dx3Context.InputAssembler.InputLayout = _layout;
            }
            if (_vertexShader != null)
            {
                dx3Context.VertexShader.Set(_vertexShader);
            }
            if (_pixelShader != null)
            {
                dx3Context.PixelShader.Set(_pixelShader);
            }

            dx3Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            dx3Context.VertexShader.SetConstantBuffer(0, _constantBuffer);

            // Update Constant Buffer
            dx3Context.UpdateSubresource(ref worldViewProj, _constantBuffer, 0);

            // Draw the cube
            dx3Context.Draw(72, 0);
        }
    }
}
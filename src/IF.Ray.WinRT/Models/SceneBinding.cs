using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.IO;
using Device1 = SharpDX.Direct3D11.Device1;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace IF.Ray.WinRT.Models
{
    public class SceneBinding
    {
        private const int FaceVectorCount = 6;

        public Shape Shape { get; set; }
        public Vector3 Position { get; set; }
        public VertexBufferBinding Dx3VertexBufferBinding { get; private set; }
        public VertexShader Dx3VertexShader { get; private set; }
        public PixelShader Dx3PixelShader { get; private set; }
        public InputLayout Dx3InputLayout { get; private set; }

        public int VertexBufferCount { get; private set; }

        public SceneBinding(Shape shape, Vector3 position)
        {
            Shape = shape;
            Position = position;
        }

        public void InitialiseBuffer(Device1 dx3Device)
        {
            // group the mesh-vertices and normals by each face
            var group = Shape.Groups.First(); // TODO factor this out
            VertexBufferCount = FaceVectorCount*group.Faces.Count;

            var sdxVertices = new Vector4[VertexBufferCount];
            for (var f = 0; f < group.Faces.Count; f++)
            {
                var face = group.Faces[f];

                // sequential
                for (var fv = 0; fv < face.Count; fv++)
                {
                    for (var vi = 0; vi < 3; vi++)
                    {
                        var vertex = Shape.Vertices[face[vi].VertexIndex - 1];
                        var v4 = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);
                        sdxVertices[f * FaceVectorCount + vi] = v4;
                    }

                    for (var ni = 0; ni < 3; ni++)
                    {
                        var normal = Shape.Normals[face[ni].NormalIndex];
                        var n4 = new Vector4(normal.X, normal.Y, normal.Z, 1.0f);

                        sdxVertices[f * FaceVectorCount + ni + 3] = n4;
                    }
                }
            }

            var path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            try
            {
                // Loads vertex shader bytecode
                var vertexShaderByteCode = NativeFile.ReadAllBytes(path + "\\MiniCube_VS.fxo");
                Dx3VertexShader = new VertexShader(dx3Device, vertexShaderByteCode);

                // Loads pixel shader bytecode
                var pixelShaderByteCode = NativeFile.ReadAllBytes(path + "\\MiniCube_PS.fxo");
                Dx3PixelShader = new PixelShader(dx3Device, pixelShaderByteCode);

                // Layout from VertexShader input signature
                Dx3InputLayout = new InputLayout(dx3Device, vertexShaderByteCode, new[]
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
            Dx3VertexBufferBinding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>(), 0);
        }
    }
}
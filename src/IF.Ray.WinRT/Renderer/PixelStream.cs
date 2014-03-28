//using System;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using Windows.UI;
//using CjClutter.ObjLoader.WinRT.Data.Elements;
//using IF.Ray.WinRT.Models;
//using SharpDX;
//using Color = SharpDX.Color;

//namespace IF.Ray.WinRT.Renderer
//{
//    public class PixelStream : Stream
//    {
//        private readonly Scene _scene;
//        private Matrix _worldViewProj;
//        private float _focalLength = 5;
//        private readonly int _width;
//        private readonly int _height;
        
//        public int Pitch { get; private set; }

//        public override bool CanRead
//        {
//            get { return true; }
//        }

//        public override bool CanSeek
//        {
//            get { return true; }
//        }

//        public override bool CanWrite
//        {
//            get { return false; }
//        }

//        public override long Length
//        {
//            get { return _width * _height; }
//        }

//        public override long Position { get; set; }

//        public PixelStream(Scene scene, Matrix worldViewProj, int width, int height)
//        {
//            _scene = scene;
//            _worldViewProj = worldViewProj;
//            _width = width;
//            _height = height;

//            // precalc scene projection?

//            Pitch = 24; // 24bit bitmap
//        }

//        public override void Flush()
//        {
//        }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            //TODO trace the ray for this position
//            var i = 0;
//            for (i = offset; i < offset + count; i += 4)
//            {
//                if (i > Length)
//                {
//                    return 0;
//                }

//                var x = i % _width;
//                var y = i / _height;

//                var color = TraceRay(new Vector2(x, y));
//                buffer[i] = color.B;
//                buffer[i + 1] = color.G;
//                buffer[i + 2] = color.R;
//                buffer[i + 3] = color.A;
//            }

//            return i;
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            switch (origin)
//            {
//                case SeekOrigin.Begin:
//                    Position = offset > Length
//                        ? Length
//                        : offset;
//                    break;
//                case SeekOrigin.Current:
//                case SeekOrigin.End:
//                    Position = offset > Length
//                        ? Length
//                        : Position + offset;
//                    break;
//            }

//            return Position;
//        }

//        public override void SetLength(long value)
//        {
//        }

//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            throw new NotSupportedException("Read only");
//        }

        
//    }
//} 

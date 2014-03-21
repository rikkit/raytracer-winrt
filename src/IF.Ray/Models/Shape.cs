using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using CjClutter.ObjLoader.WinRT.Data;
using CjClutter.ObjLoader.WinRT.Data.Elements;
using CjClutter.ObjLoader.WinRT.Data.VertexData;
using CjClutter.ObjLoader.WinRT.Loaders;
using IF.Common.Metro.Framework;
using IF.Common.Metro.Framework.Storage;

namespace IF.Ray.Models
{
    public abstract class Shape
    {
        public IList<Vertex> Vertices { get; set; }
        public IList<Texture> Textures { get; set; }
        public IList<Normal> Normals { get; set; }
        public IList<Group> Groups { get; set; }
        public IList<Material> Materials { get; set; }

        protected Shape()
        {

        }
    }

    public class Cube : Shape
    {

    }


    public class ShapeFactory
    {
        private const string ObjFolderPath = "ms-appx:///Assets/Objects";

        private IObjLoader _loader;

        public ShapeFactory()
        {
            IObjLoaderFactory loaderFactory = new ObjLoaderFactory();
            _loader = loaderFactory.Create();
        }

        /// <summary>
        /// *Synchronous* wrapper for loading an obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetShape<T>() where T : Shape, new()
        {
            var loadResult = AsyncHelpers.RunSync(async () =>
            {
                var fileUriString = string.Format("{0}/{1}", ObjFolderPath, GetFilePathForType(typeof (T)));
                var fileUri = new Uri(fileUriString, UriKind.Absolute);
                var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
                var result = _loader.Load(await file.OpenReadAsync());

                return result;
            });

            return LoadResultToShape<T>(loadResult);
        }

        private static string GetFilePathForType(Type type)
        {
            if (type == typeof (Cube))
            {
                return "cube.obj";
            }
            else
            {
                throw new InvalidOperationException(string.Format("Shape {0} isnae supported", type));
            }
        }

        private static T LoadResultToShape<T>(LoadResult result) where T : Shape, new()
        {
            var shape = new T
            {
                Vertices = result.Vertices,
                Normals = result.Normals,
                Textures = result.Textures,
                Groups = result.Groups,
                Materials = result.Materials
            };

            return shape;
        }
    }
}

using System;
using Windows.Storage;
using CjClutter.ObjLoader.WinRT.Loaders;
using IF.Common.Metro.Framework;

namespace IF.Ray.WinRT.Models
{
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
                var fileUriString = string.Format("{0}/{1}", ObjFolderPath, GetFilePathForType(typeof(T)));
                var fileUri = new Uri(fileUriString, UriKind.Absolute);
                var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
                var result = _loader.Load(await file.OpenReadAsync());

                return result;
            });

            return LoadResultToShape<T>(loadResult);
        }

        private static string GetFilePathForType(Type type)
        {
            if (type == typeof(Cube))
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

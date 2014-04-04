using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SharpDX;

namespace IF.Ray.Core
{
    public class ShapeFactory
    {
        private const string ObjFolderPath = "ms-appx:///Assets/Objects";

        /// <summary>
        /// *Synchronous* wrapper for loading an obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetShape<T>() where T : Mesh, new()
        {
            var fileUriString = string.Format("{0}/{1}", ObjFolderPath, GetFilePathForType(typeof(T)));
            var fileUri = new Uri(fileUriString, UriKind.Absolute);
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
            var result = await LoadObjFileAsync(file);

            return LoadResultToShape<T>(result);
        }

        private static string GetFilePathForType(Type type)
        {
            if (type == typeof(Cube))
            {
                return "cube.obj";
            }
            else if (type == typeof (Cylinder))
            {
                return "cylinder.obj";
            }
            else
            {
                throw new InvalidOperationException(string.Format("Shape {0} isnae supported", type));
            }
        }

        private static T LoadResultToShape<T>(IList<Triangle> mesh) where T : Mesh, new()
        {
            var shape = new T
            {
                Triangles = mesh
            };

            return shape;
        }

        private async Task<List<Triangle>> LoadObjFileAsync(StorageFile file)
        {
            var mesh = new List<Triangle>();

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var texc = new List<Vector3>();

            var lines = await FileIO.ReadLinesAsync(file);

            foreach (var line in lines)
            {
                var toks = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (!toks.Any())
                {
                    continue;
                }

                if (toks[0] == "v")
                {
                    verts.Add(new Vector3(float.Parse(toks[1]), float.Parse(toks[2]), float.Parse(toks[3])));
                }
                else if (toks[0] == "vt")
                {
                    texc.Add(new Vector3(float.Parse(toks[1]), float.Parse(toks[2]), 0));
                }
                else if (toks[0] == "vn")
                {
                    norms.Add(new Vector3(float.Parse(toks[1]), float.Parse(toks[2]), float.Parse(toks[3])));
                }
                else if (toks[0] == "f")
                {
                    var vals0 = toks[1].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                    var vals1 = toks[2].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                    var vals2 = toks[3].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                    int i0 = int.Parse(vals0[0]) - 1;
                    int i1 = int.Parse(vals1[0]) - 1;
                    int i2 = int.Parse(vals2[0]) - 1;

                    var t = new Triangle(verts[i0], verts[i1], verts[i2]);

                    if (norms.Any())
                    {
                        // well this is messy

                        if (vals0.Length == 1)
                        {
                            int k0 = int.Parse(vals0[0]) - 1;
                            int k1 = int.Parse(vals1[0]) - 1;
                            int k2 = int.Parse(vals2[0]) - 1;

                            t.SetNormals(norms[k0], norms[k1], norms[k2]);
                        }
                        else if (vals0.Length > 2)
                        {
                            int k0 = int.Parse(vals0[2]) - 1;
                            int k1 = int.Parse(vals1[2]) - 1;
                            int k2 = int.Parse(vals2[2]) - 1;

                            t.SetNormals(norms[k0], norms[k1], norms[k2]);
                        }
                    }

                    mesh.Add(t);
                }
            }


            return mesh;
        }
    }

    
}

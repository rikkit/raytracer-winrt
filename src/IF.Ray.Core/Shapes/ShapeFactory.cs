﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class ShapeFactory : IShapeFactory
    {
        private const string ObjFolderPath = "ms-appx:///Assets/Objects";

        /// <summary>
        /// There must be a better way of doing this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IOccluder> GetShape<T>() where T : IOccluder, new()
        {
            var type = typeof (T);
            if (type == typeof (Cube))
            {
                return await LoadShape<Cube>("cube.obj");
            }
            else if (type == typeof(Cylinder))
            {
                return await LoadShape<Cylinder>("cylinder.obj");
            }
            else if (type == typeof(ObjPlane))
            {
                return await LoadShape<ObjPlane>("plane.obj");
            }
            else return null;
        }

        private async Task<T> LoadShape<T>(string filename) where T : Mesh, new()
        {
            var fileUriString = string.Format("{0}/{1}", ObjFolderPath, filename);
            var fileUri = new Uri(fileUriString, UriKind.Absolute);
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
            var result = await LoadObjFileAsync(file);

            var shape = new T
            {
                Triangles = result
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
            var faceIndex = 0;
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

                    var t = new Triangle(verts[i0], verts[i1], verts[i2], norms[faceIndex]);
                    faceIndex++;
                    mesh.Add(t);
                }
            }

            return mesh;
        }
    }

    
}

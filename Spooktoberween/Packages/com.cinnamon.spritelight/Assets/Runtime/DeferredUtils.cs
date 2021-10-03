using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteLightRendering
{
    public class DeferredUtils
    {
        static Mesh s_SphereMesh;
        public static Mesh SphereMesh
        {
            get
            {
                if (!s_SphereMesh)
                {
                    s_SphereMesh = new Mesh() { name = "Sphere" };

                    Vector3[] positions = {
                        new Vector3( 0.000f,  0.000f, -1.070f), new Vector3( 0.174f, -0.535f, -0.910f),
                        new Vector3(-0.455f, -0.331f, -0.910f), new Vector3( 0.562f,  0.000f, -0.910f),
                        new Vector3(-0.455f,  0.331f, -0.910f), new Vector3( 0.174f,  0.535f, -0.910f),
                        new Vector3(-0.281f, -0.865f, -0.562f), new Vector3( 0.736f, -0.535f, -0.562f),
                        new Vector3( 0.296f, -0.910f, -0.468f), new Vector3(-0.910f,  0.000f, -0.562f),
                        new Vector3(-0.774f, -0.562f, -0.478f), new Vector3( 0.000f, -1.070f,  0.000f),
                        new Vector3(-0.629f, -0.865f,  0.000f), new Vector3( 0.629f, -0.865f,  0.000f),
                        new Vector3(-1.017f, -0.331f,  0.000f), new Vector3( 0.957f,  0.000f, -0.478f),
                        new Vector3( 0.736f,  0.535f, -0.562f), new Vector3( 1.017f, -0.331f,  0.000f),
                        new Vector3( 1.017f,  0.331f,  0.000f), new Vector3(-0.296f, -0.910f,  0.478f),
                        new Vector3( 0.281f, -0.865f,  0.562f), new Vector3( 0.774f, -0.562f,  0.478f),
                        new Vector3(-0.736f, -0.535f,  0.562f), new Vector3( 0.910f,  0.000f,  0.562f),
                        new Vector3( 0.455f, -0.331f,  0.910f), new Vector3(-0.174f, -0.535f,  0.910f),
                        new Vector3( 0.629f,  0.865f,  0.000f), new Vector3( 0.774f,  0.562f,  0.478f),
                        new Vector3( 0.455f,  0.331f,  0.910f), new Vector3( 0.000f,  0.000f,  1.070f),
                        new Vector3(-0.562f,  0.000f,  0.910f), new Vector3(-0.957f,  0.000f,  0.478f),
                        new Vector3( 0.281f,  0.865f,  0.562f), new Vector3(-0.174f,  0.535f,  0.910f),
                        new Vector3( 0.296f,  0.910f, -0.478f), new Vector3(-1.017f,  0.331f,  0.000f),
                        new Vector3(-0.736f,  0.535f,  0.562f), new Vector3(-0.296f,  0.910f,  0.478f),
                        new Vector3( 0.000f,  1.070f,  0.000f), new Vector3(-0.281f,  0.865f, -0.562f),
                        new Vector3(-0.774f,  0.562f, -0.478f), new Vector3(-0.629f,  0.865f,  0.000f),
                    };

                    int[] indices = {
                         0,  1,  2,  0,  3,  1,  2,  4,  0,  0,  5,  3,  0,  4,  5,  1,  6,  2,
                         3,  7,  1,  1,  8,  6,  1,  7,  8,  9,  4,  2,  2,  6, 10, 10,  9,  2,
                         8, 11,  6,  6, 12, 10, 11, 12,  6,  7, 13,  8,  8, 13, 11, 10, 14,  9,
                        10, 12, 14,  3, 15,  7,  5, 16,  3,  3, 16, 15, 15, 17,  7, 17, 13,  7,
                        16, 18, 15, 15, 18, 17, 11, 19, 12, 13, 20, 11, 11, 20, 19, 17, 21, 13,
                        13, 21, 20, 12, 19, 22, 12, 22, 14, 17, 23, 21, 18, 23, 17, 21, 24, 20,
                        23, 24, 21, 20, 25, 19, 19, 25, 22, 24, 25, 20, 26, 18, 16, 18, 27, 23,
                        26, 27, 18, 28, 24, 23, 27, 28, 23, 24, 29, 25, 28, 29, 24, 25, 30, 22,
                        25, 29, 30, 14, 22, 31, 22, 30, 31, 32, 28, 27, 26, 32, 27, 33, 29, 28,
                        30, 29, 33, 33, 28, 32, 34, 26, 16,  5, 34, 16, 14, 31, 35, 14, 35,  9,
                        31, 30, 36, 30, 33, 36, 35, 31, 36, 37, 33, 32, 36, 33, 37, 38, 32, 26,
                        34, 38, 26, 38, 37, 32,  5, 39, 34, 39, 38, 34,  4, 39,  5,  9, 40,  4,
                         9, 35, 40,  4, 40, 39, 35, 36, 41, 41, 36, 37, 41, 37, 38, 40, 35, 41,
                        40, 41, 39, 41, 38, 39,
                    };

                    s_SphereMesh.indexFormat = IndexFormat.UInt16;
                    s_SphereMesh.vertices = positions;
                    s_SphereMesh.triangles = indices;
                }
                return s_SphereMesh;
            }
        }

        static Mesh s_FullscreenQuadDoubleSided;
        public static Mesh FullscreenQuadDoubleSided
        {
            get
            {
                if(!s_FullscreenQuadDoubleSided)
                {
                    s_FullscreenQuadDoubleSided = new Mesh { name = "Fullscreen Quad" };
                    Vector3[] positions = {
                        new Vector3(-1.0f, -1.0f, 0.0f),
                        new Vector3(-1.0f,  1.0f, 0.0f),
                        new Vector3(1.0f, -1.0f, 0.0f),
                        new Vector3(1.0f,  1.0f, 0.0f)
                    };

                    int[] indices = { 0, 3, 1, 0, 2, 3, 0, 1, 3, 0, 3, 2 };

                    s_FullscreenQuadDoubleSided.indexFormat = IndexFormat.UInt16;
                    s_FullscreenQuadDoubleSided.vertices = positions;
                    s_FullscreenQuadDoubleSided.triangles = indices;
                }
                return s_FullscreenQuadDoubleSided;
            }
        }
    }
}

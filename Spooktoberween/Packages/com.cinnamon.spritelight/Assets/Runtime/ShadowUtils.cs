using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class ShadowUtils
    {
        public static void GetPointLightShadowParams(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, int atlasLightIndex, CubemapFace cubemapFace, int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out ShadowSliceData shadowSliceData, out ShadowSplitData shadowSplitData)
        {
            Matrix4x4 viewMatrix, projMatrix;
            cullResults.ComputePointShadowMatricesAndCullingPrimitives(shadowLightIndex, cubemapFace, 5f, out viewMatrix, out projMatrix, out shadowSplitData);

            shadowSliceData = new ShadowSliceData();
            int imageIdx = (int)cubemapFace + 6*atlasLightIndex;
            int atlasX = imageIdx % 4;
            int atlasY = imageIdx >> 2;

            shadowSliceData.offsetX = atlasX * shadowResolution;
            shadowSliceData.offsetY = atlasY * shadowResolution;
            shadowSliceData.resolution = shadowResolution;
            shadowSliceData.viewMatrix = viewMatrix;

            shadowSliceData.projectionMatrix = projMatrix;
            shadowSliceData.shadowTransform = GetShadowTransform(projMatrix, viewMatrix);

            UnityEngine.Rendering.Universal.ShadowUtils.ApplySliceTransform(ref shadowSliceData, shadowmapWidth, shadowmapHeight);
        }



        static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
        {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;

            // Apply texture scale and offset to save a MAD in shader.
            return textureScaleAndBias * worldToShadow;
        }

        public static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, ref ShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
        {
            if (shadowLightIndex < 0 || shadowLightIndex >= shadowData.bias.Count)
            {
                Debug.LogWarning(string.Format("{0} is not a valid light index.", shadowLightIndex));
                return Vector4.zero;
            }

            float frustumSize;
            if (shadowLight.lightType == LightType.Directional)
            {
                // Frustum size is guaranteed to be a cube as we wrap shadow frustum around a sphere
                frustumSize = 2.0f / lightProjectionMatrix.m00;
            }
            else if (shadowLight.lightType == LightType.Spot)
            {
                // For perspective projections, shadow texel size varies with depth
                // It will only work well if done in receiver side in the pixel shader. Currently UniversalRP
                // do bias on caster side in vertex shader. When we add shader quality tiers we can properly
                // handle this. For now, as a poor approximation we do a constant bias and compute the size of
                // the frustum as if it was orthogonal considering the size at mid point between near and far planes.
                // Depending on how big the light range is, it will be good enough with some tweaks in bias
                frustumSize = Mathf.Tan(shadowLight.spotAngle * 0.5f * Mathf.Deg2Rad) * shadowLight.range;
            }
            else if (shadowLight.lightType == LightType.Point)
            {
                frustumSize = Mathf.Tan(90f * 0.5f * Mathf.Deg2Rad) * shadowLight.range * .1f;
            }
            else
            {
                Debug.LogWarning("Only spot and directional shadow casters are supported in universal pipeline");
                frustumSize = 0.0f;
            }

            // depth and normal bias scale is in shadowmap texel size in world space
            float texelSize = frustumSize / shadowResolution;
            float depthBias = -shadowData.bias[shadowLightIndex].x * texelSize;
            float normalBias = -shadowData.bias[shadowLightIndex].y * texelSize;

            return new Vector4(depthBias, normalBias, 0.0f, 0.0f);
        }

        public static void SetupPointLightShadowCasterConstantBuffer(CommandBuffer cmd, CubemapFace cubemapFace, Vector4 shadowBias)
        {
            Vector3 lightDirection = Vector3.one;
            switch(cubemapFace)
            {
                case CubemapFace.NegativeX:
                    lightDirection = Vector3.left;
                    break;
                case CubemapFace.NegativeY:
                    lightDirection = Vector3.down;
                    break;
                case CubemapFace.NegativeZ:
                    //lightDirection = SystemInfo.usesReversedZBuffer ? Vector3.forward : Vector3.back;
                    lightDirection = Vector3.back;
                    break;
                case CubemapFace.PositiveX:
                    lightDirection = Vector3.right;
                    break;
                case CubemapFace.PositiveY:
                    lightDirection = Vector3.up;
                    break;
                case CubemapFace.PositiveZ:
                    //lightDirection = SystemInfo.usesReversedZBuffer ? Vector3.back : Vector3.forward;
                    lightDirection = Vector3.forward;
                    break;
            }

            cmd.SetGlobalVector("_ShadowBias", shadowBias);
            cmd.SetGlobalVector("_LightDirection", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
        }
    }
}

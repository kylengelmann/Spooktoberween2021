//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

//namespace SpriteLightRendering
//{
//    [ExcludeFromPreset]
//    public class SpriteLightRenderPipelineAsset : UniversalRenderPipelineAsset
//    {
//        public static SpriteLightRenderPipelineAsset Create(ScriptableRendererData rendererData = null)
//        {
//            // Create Universal RP Asset
//            var instance = CreateInstance<SpriteLightRenderPipelineAsset>();
//            if (rendererData != null)
//                instance.m_RendererDataList[0] = rendererData;
//            else
//                instance.m_RendererDataList[0] = CreateInstance<ForwardRendererData>();

//            // Initialize default Renderer
//            instance.m_EditorResourcesAsset = instance.editorResources;

//            return instance;
//        }

//        static ScriptableRendererData CreateSpriteLightRendererAsset(string path, bool relativePath = true)
//        {
//            ScriptableRendererData data = CreateInstance<SpriteLightRendererData>();
//            string dataPath;
//            if (relativePath)
//                dataPath =
//                    $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_Renderer{Path.GetExtension(path)}";
//            else
//                dataPath = path;
//            AssetDatabase.CreateAsset(data, dataPath);
//            return data;
//        }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
//        internal class CreateUniversalPipelineAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
//        {
//            public override void Action(int instanceId, string pathName, string resourceFile)
//            {
//                //Create asset
//                AssetDatabase.CreateAsset(Create(CreateSpriteLightRendererAsset(pathName)), pathName);
//            }
//        }

//        [MenuItem("Assets/Create/SpriteLightRendering/Pipeline Asset (Sprite Light Renderer)", priority = CoreUtils.assetCreateMenuPriority1)]
//        static void CreateSprightLightUniversalPipeline()
//        {
//            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateUniversalPipelineAsset>(),
//                "SpriteLightPipelineAsset.asset", null, null);
//        }
//    }
//}

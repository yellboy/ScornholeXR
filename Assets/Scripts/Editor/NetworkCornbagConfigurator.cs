using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Editor;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [InitializeOnLoad]
    public static class NetworkCornbagConfigurator
    {
        private static readonly string[] PrefabPaths =
        {
            "Assets/Prefabs/Cornbag.prefab",
            "Assets/Prefabs/Cornbag green.prefab",
            "Assets/Prefabs/Cornbag purple.prefab",
            "Assets/Prefabs/Cornbag red.prefab",
            "Assets/Prefabs/Cornbag yellow.prefab"
        };

        static NetworkCornbagConfigurator()
        {
            EditorApplication.delayCall += ConfigureIfNeeded;
        }

        [MenuItem("Tools/Scornhole/Configure Network Cornbags")]
        public static void Configure()
        {
            foreach (var path in PrefabPaths)
            {
                ConfigurePrefab(path);
            }

            NetworkProjectConfigUtilities.RebuildPrefabTable();
            Debug.Log("Configured Scornhole network cornbags.");
        }

        private static void ConfigureIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            if (PrefabPaths.Any(path => !IsConfigured(path)))
            {
                Configure();
            }
        }

        private static bool IsConfigured(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null &&
                prefab.GetComponent<NetworkObject>() != null &&
                prefab.GetComponent<NetworkRigidbody3D>() != null &&
                prefab.GetComponent<NetworkCornbag>() != null;
        }

        private static void ConfigurePrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var networkObject = root.GetComponent<NetworkObject>() ?? root.AddComponent<NetworkObject>();
                networkObject.Flags &= ~NetworkObjectFlags.Ignore;
                networkObject.Flags &= ~NetworkObjectFlags.DestroyWhenStateAuthorityLeaves;
                networkObject.Flags |= NetworkObjectFlags.AllowStateAuthorityOverride;

                if (root.GetComponent<NetworkRigidbody3D>() == null)
                {
                    root.AddComponent<NetworkRigidbody3D>();
                }

                if (root.GetComponent<NetworkCornbag>() == null)
                {
                    root.AddComponent<NetworkCornbag>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}

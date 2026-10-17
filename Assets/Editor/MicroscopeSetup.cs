using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LabSetup
{
    [InitializeOnLoad]
    public static class MicroscopeSetup
    {
        private const string RoomScenePath = "Assets/Scenes/Room Scene.unity";
        private const string MicroscopeScenePath = "Assets/Scenes/Microscope View.unity";

        private const string TableFbxPath = "Assets/old assets/lab assets/labtable1/source/LabTable1.fbx";
        private const string MicroscopeFbxPath = "Assets/old assets/lab assets/microscope/source/microscope.fbx";
        private const string SlideMaterialPath = "Assets/old assets/New Material.mat";

        // static MicroscopeSetup()
        // {
        //     EditorApplication.delayCall += () =>
        //     {
        //         SetupWorkstationInScenes();
        //     };
        // }

        [MenuItem("Laboratory/Setup Microscope Workstation")]
        public static void SetupWorkstationInScenes()
        {
            SetupScene(MicroscopeScenePath, true);
            SetupScene(RoomScenePath, false);
            Debug.Log("<color=green>[Laboratory Setup] Microscope workstation configured in both scenes successfully!</color>");
        }

        public static void SetupScene(string scenePath, bool isMicroscopeDedicatedScene)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Rebuild or ensure Room Shell exists
            RoomBuilder.BuildLaboratoryRoom();

            // Load Models and Materials
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TableFbxPath);
            var microscopePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MicroscopeFbxPath);
            var slideMat = AssetDatabase.LoadAssetAtPath<Material>(SlideMaterialPath);
            var doorFrameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Door Frames.mat");

            // Workstation location: Corner underneath ceiling light 1 at X = 2.5m, Z = 2.3m
            Vector3 workstationPos = new Vector3(2.5f, 0f, 2.3f);

            var workstationRoot = new GameObject("Microscope_Workstation");
            workstationRoot.transform.position = workstationPos;

            // 1. LAB TABLE (Positioned on floor at Y=0, table top surface at Y=0.52m)
            GameObject tableGo;
            if (tablePrefab != null)
            {
                tableGo = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab, workstationRoot.transform);
                tableGo.name = "LabTable";
                tableGo.transform.localPosition = Vector3.zero;
                tableGo.transform.localRotation = Quaternion.identity;
                tableGo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            else
            {
                tableGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tableGo.name = "LabTable";
                tableGo.transform.SetParent(workstationRoot.transform);
                tableGo.transform.localPosition = new Vector3(0, 0.26f, 0);
                tableGo.transform.localScale = new Vector3(1.6f, 0.52f, 0.9f);
                if (doorFrameMat != null) tableGo.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;
            }

            // Table top height where microscope rests
            float tableTopY = 0.52f;

            // 2. MICROSCOPE (Placed directly resting on table surface at Y=0.52m, NOT floating)
            GameObject microscopeGo;
            Vector3 microscopeLocalPos = new Vector3(0f, tableTopY, 0f);
            if (microscopePrefab != null)
            {
                microscopeGo = (GameObject)PrefabUtility.InstantiatePrefab(microscopePrefab, workstationRoot.transform);
                microscopeGo.name = "microscope";
                microscopeGo.transform.localPosition = microscopeLocalPos;
                microscopeGo.transform.localRotation = Quaternion.identity;
                microscopeGo.transform.localScale = Vector3.one;
            }
            else
            {
                microscopeGo = new GameObject("microscope");
                microscopeGo.transform.SetParent(workstationRoot.transform);
                microscopeGo.transform.localPosition = microscopeLocalPos;
            }

            // 3. DEDICATED MICROSCOPE CAMERA (Child of microscope, placed on optical pinhole tube)
            var camGo = new GameObject("microscopeCamera");
            camGo.transform.SetParent(microscopeGo.transform);
            // Positioned at exact pinhole opening of the eyepiece tube looking down through the optical axis
            camGo.transform.localPosition = new Vector3(0.018f, 0.48f, 0.0f);
            camGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 7.0f;
            cam.nearClipPlane = 0.02f;
            cam.farClipPlane = 50f;
            cam.clearFlags = CameraClearFlags.Skybox;
            camGo.tag = isMicroscopeDedicatedScene ? "MainCamera" : "Untagged";
            var audioListener = camGo.AddComponent<AudioListener>();
            if (!isMicroscopeDedicatedScene) audioListener.enabled = false;

            var urpCam = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;
            urpCam.renderShadows = true;

            // Add Zoom/Pan controller & Eyepiece Circular Overlay
            var controller = camGo.AddComponent<MicroscopeController>();
            controller.currentFOV = 7.0f;
            controller.minFOV = 1.5f;
            controller.maxFOV = 25f;

            var overlay = camGo.AddComponent<MicroscopeEyepieceOverlay>();
            camGo.SetActive(true);

            // 4. PATHOLOGY GLASS SLIDE (Resting on the microscope stage at Y=0.656m)
            var slideGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slideGo.name = "Pathology_Glass_Slide";
            slideGo.transform.SetParent(workstationRoot.transform);
            slideGo.transform.localPosition = new Vector3(0.018f, 0.656f, 0.0f);
            slideGo.transform.localRotation = Quaternion.identity;
            slideGo.transform.localScale = new Vector3(0.029f, 0.0076f, 0.048f);
            if (slideMat != null) slideGo.GetComponent<MeshRenderer>().sharedMaterial = slideMat;

            // 5. DEDICATED MICROSCOPE ILLUMINATION LIGHT (Under stage pointing upward)
            var lightGo = new GameObject("Microscope_Substage_Illuminator");
            lightGo.transform.SetParent(workstationRoot.transform);
            lightGo.transform.localPosition = new Vector3(0.018f, 0.60f, 0.0f);
            lightGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Pointing UPWARD toward the slide

            var lightComp = lightGo.AddComponent<Light>();
            lightComp.type = LightType.Spot;
            lightComp.spotAngle = 65f;
            lightComp.innerSpotAngle = 45f;
            lightComp.color = new Color(1.0f, 0.98f, 0.94f);
            lightComp.intensity = 4.5f;
            lightComp.range = 0.5f;
            lightComp.shadows = LightShadows.Soft;
            lightComp.shadowStrength = 0.6f;

            var uLightData = lightGo.AddComponent<UniversalAdditionalLightData>();
            uLightData.usePipelineSettings = true;

            // If dedicated Microscope View scene, remove all other development cameras
            if (isMicroscopeDedicatedScene)
            {
                var camerasRoot = GameObject.Find("Cameras");
                if (camerasRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(camerasRoot);
                }
            }
            else
            {
                // In Room Scene, add microscope camera to switcher
                var camerasRoot = GameObject.Find("Cameras");
                if (camerasRoot != null)
                {
                    var switcher = camerasRoot.GetComponent<CameraSwitcher>();
                    if (switcher != null && !switcher.cameras.Contains(cam))
                    {
                        switcher.cameras.Add(cam);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}

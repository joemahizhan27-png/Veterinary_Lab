using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LabSetup
{
    [InitializeOnLoad]
    public static class RoomBuilder
    {
        private const string ScenePath = "Assets/Scenes/Room Scene.unity";

        static RoomBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                BuildLaboratoryRoom();
            };
        }

        [MenuItem("Laboratory/Build Room Shell")]
        public static void BuildLaboratoryRoom()
        {
            // Open or create the scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Load existing materials
            var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Floor.mat");
            var wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Walls.mat");
            var ceilingMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Ceiling.mat");
            var doorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Doors.mat");
            var doorFrameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Door Frames.mat");
            var windowFrameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Window Frames.mat");

            // Clean up existing GameObjects in the scene
            var rootObjects = scene.GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                UnityEngine.Object.DestroyImmediate(go);
            }

            // Create Root Groups
            var roomRoot = new GameObject("Room Shell");
            var lightingRoot = new GameObject("Lighting");
            var camerasRoot = new GameObject("Cameras");

            // ==========================================
            // 1. FLOOR (14m long [X] x 9m wide [Z], Top at Y = 0)
            // ==========================================
            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Floor";
            floorGo.transform.SetParent(roomRoot.transform);
            floorGo.transform.position = new Vector3(7f, -0.05f, 4.5f);
            floorGo.transform.localScale = new Vector3(14f, 0.1f, 9f);
            if (floorMat != null) floorGo.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // ==========================================
            // 2. CEILING (14m x 9m, Bottom at Y = 3.5)
            // ==========================================
            var ceilingGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceilingGo.name = "Ceiling";
            ceilingGo.transform.SetParent(roomRoot.transform);
            ceilingGo.transform.position = new Vector3(7f, 3.55f, 4.5f);
            ceilingGo.transform.localScale = new Vector3(14f, 0.1f, 9f);
            if (ceilingMat != null) ceilingGo.GetComponent<MeshRenderer>().sharedMaterial = ceilingMat;

            // ==========================================
            // 3. WALLS
            // ==========================================
            var wallsRoot = new GameObject("Walls");
            wallsRoot.transform.SetParent(roomRoot.transform);

            // 3a. Right Wall (Solid wall at Z = 0, length = 14m, height = 3.5m)
            var wallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallRight.name = "Wall_Right_Solid";
            wallRight.transform.SetParent(wallsRoot.transform);
            wallRight.transform.position = new Vector3(7f, 1.75f, -0.05f);
            wallRight.transform.localScale = new Vector3(14f, 3.5f, 0.1f);
            if (wallMat != null) wallRight.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // 3b. Back Wall (Solid wall at X = 14, width = 9m, height = 3.5m)
            var wallBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallBack.name = "Wall_Back_Solid";
            wallBack.transform.SetParent(wallsRoot.transform);
            wallBack.transform.position = new Vector3(14.05f, 1.75f, 4.5f);
            wallBack.transform.localScale = new Vector3(0.1f, 3.5f, 9f);
            if (wallMat != null) wallBack.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // 3c. Front Wall (at X = 0, width = 9m, height = 3.5m with Door at center Z = 4.5, width 1.5m, height 2.2m)
            var wallFrontRoot = new GameObject("Wall_Front");
            wallFrontRoot.transform.SetParent(wallsRoot.transform);

            // Front Left Segment (Z from 0 to 3.75, width 3.75)
            var frontLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontLeft.name = "Wall_Front_Left";
            frontLeft.transform.SetParent(wallFrontRoot.transform);
            frontLeft.transform.position = new Vector3(-0.05f, 1.75f, 1.875f);
            frontLeft.transform.localScale = new Vector3(0.1f, 3.5f, 3.75f);
            if (wallMat != null) frontLeft.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Front Right Segment (Z from 5.25 to 9.0, width 3.75)
            var frontRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontRight.name = "Wall_Front_Right";
            frontRight.transform.SetParent(wallFrontRoot.transform);
            frontRight.transform.position = new Vector3(-0.05f, 1.75f, 7.125f);
            frontRight.transform.localScale = new Vector3(0.1f, 3.5f, 3.75f);
            if (wallMat != null) frontRight.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Front Top Header (above door: Y from 2.2 to 3.5, height 1.3, width 1.5 at Z=4.5)
            var frontTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontTop.name = "Wall_Front_Header";
            frontTop.transform.SetParent(wallFrontRoot.transform);
            frontTop.transform.position = new Vector3(-0.05f, 2.85f, 4.5f);
            frontTop.transform.localScale = new Vector3(0.1f, 1.3f, 1.5f);
            if (wallMat != null) frontTop.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // 3d. Left Wall (at Z = 9, length = 14m, height = 3.5m with 4 large windows)
            var wallLeftRoot = new GameObject("Wall_Left_With_Windows");
            wallLeftRoot.transform.SetParent(wallsRoot.transform);

            // Bottom sill wall (Y from 0 to 1.1, full length 14m)
            var sill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sill.name = "Wall_Left_Bottom_Sill";
            sill.transform.SetParent(wallLeftRoot.transform);
            sill.transform.position = new Vector3(7f, 0.55f, 9.05f);
            sill.transform.localScale = new Vector3(14f, 1.1f, 0.1f);
            if (wallMat != null) sill.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Top header wall (Y from 2.6 to 3.5, full length 14m)
            var header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            header.name = "Wall_Left_Top_Header";
            header.transform.SetParent(wallLeftRoot.transform);
            header.transform.position = new Vector3(7f, 3.05f, 9.05f);
            header.transform.localScale = new Vector3(14f, 0.9f, 0.1f);
            if (wallMat != null) header.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // 5 Wall Piers between and around windows (Y from 1.1 to 2.6, height 1.5)
            float[] pierCenters = { 0.6f, 3.8f, 7.0f, 10.2f, 13.4f };
            for (int i = 0; i < pierCenters.Length; i++)
            {
                var pier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pier.name = $"Wall_Left_Pier_{i + 1}";
                pier.transform.SetParent(wallLeftRoot.transform);
                pier.transform.position = new Vector3(pierCenters[i], 1.85f, 9.05f);
                pier.transform.localScale = new Vector3(1.2f, 1.5f, 0.1f);
                if (wallMat != null) pier.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
            }

            // ==========================================
            // 4. DOORS (Main Entrance at Center of 9m front wall: Z = 4.5, X = 0)
            // ==========================================
            var doorRoot = new GameObject("Doors");
            doorRoot.transform.SetParent(roomRoot.transform);

            // Door Frame (Dark Grey)
            var frameRoot = new GameObject("Door_Frame");
            frameRoot.transform.SetParent(doorRoot.transform);

            var frameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameLeft.name = "Frame_Left_Jamb";
            frameLeft.transform.SetParent(frameRoot.transform);
            frameLeft.transform.position = new Vector3(0f, 1.1f, 3.78f);
            frameLeft.transform.localScale = new Vector3(0.14f, 2.2f, 0.06f);
            if (doorFrameMat != null) frameLeft.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;

            var frameRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameRight.name = "Frame_Right_Jamb";
            frameRight.transform.SetParent(frameRoot.transform);
            frameRight.transform.position = new Vector3(0f, 1.1f, 5.22f);
            frameRight.transform.localScale = new Vector3(0.14f, 2.2f, 0.06f);
            if (doorFrameMat != null) frameRight.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;

            var frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.name = "Frame_Top_Lintel";
            frameTop.transform.SetParent(frameRoot.transform);
            frameTop.transform.position = new Vector3(0f, 2.17f, 4.5f);
            frameTop.transform.localScale = new Vector3(0.14f, 0.06f, 1.5f);
            if (doorFrameMat != null) frameTop.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;

            // Door Leaf (Hinged at Z = 3.82, opening inward by 40 degrees into room)
            var doorHinge = new GameObject("Door_Inward_Hinge");
            doorHinge.transform.SetParent(doorRoot.transform);
            doorHinge.transform.position = new Vector3(0f, 0f, 3.82f);
            doorHinge.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);

            var doorLeaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeaf.name = "Door_Leaf";
            doorLeaf.transform.SetParent(doorHinge.transform);
            doorLeaf.transform.localPosition = new Vector3(0.02f, 1.07f, 0.675f);
            doorLeaf.transform.localRotation = Quaternion.identity;
            doorLeaf.transform.localScale = new Vector3(0.045f, 2.14f, 1.35f);
            if (doorMat != null) doorLeaf.GetComponent<MeshRenderer>().sharedMaterial = doorMat;

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Door_Handle";
            handle.transform.SetParent(doorHinge.transform);
            handle.transform.localPosition = new Vector3(0.06f, 1.0f, 1.25f);
            handle.transform.localRotation = Quaternion.identity;
            handle.transform.localScale = new Vector3(0.06f, 0.16f, 0.03f);
            if (doorFrameMat != null) handle.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;

            // ==========================================
            // 5. WINDOWS (4 Large Windows along Left 14m Wall at Z = 9)
            // ==========================================
            var windowsRoot = new GameObject("Windows");
            windowsRoot.transform.SetParent(roomRoot.transform);

            float[] windowCenters = { 2.2f, 5.4f, 8.6f, 11.8f };
            for (int i = 0; i < windowCenters.Length; i++)
            {
                float cx = windowCenters[i];
                var winGo = new GameObject($"Window_{i + 1}");
                winGo.transform.SetParent(windowsRoot.transform);

                var fTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fTop.name = "Frame_Top";
                fTop.transform.SetParent(winGo.transform);
                fTop.transform.position = new Vector3(cx, 2.57f, 9.0f);
                fTop.transform.localScale = new Vector3(2.0f, 0.06f, 0.14f);
                if (windowFrameMat != null) fTop.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;

                var fBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fBottom.name = "Frame_Bottom";
                fBottom.transform.SetParent(winGo.transform);
                fBottom.transform.position = new Vector3(cx, 1.13f, 9.0f);
                fBottom.transform.localScale = new Vector3(2.0f, 0.06f, 0.14f);
                if (windowFrameMat != null) fBottom.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;

                var fLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fLeft.name = "Frame_Left";
                fLeft.transform.SetParent(winGo.transform);
                fLeft.transform.position = new Vector3(cx - 0.97f, 1.85f, 9.0f);
                fLeft.transform.localScale = new Vector3(0.06f, 1.5f, 0.14f);
                if (windowFrameMat != null) fLeft.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;

                var fRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fRight.name = "Frame_Right";
                fRight.transform.SetParent(winGo.transform);
                fRight.transform.position = new Vector3(cx + 0.97f, 1.85f, 9.0f);
                fRight.transform.localScale = new Vector3(0.06f, 1.5f, 0.14f);
                if (windowFrameMat != null) fRight.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;

                var mullionV = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullionV.name = "Mullion_Vertical";
                mullionV.transform.SetParent(winGo.transform);
                mullionV.transform.position = new Vector3(cx, 1.85f, 9.0f);
                mullionV.transform.localScale = new Vector3(0.04f, 1.38f, 0.08f);
                if (windowFrameMat != null) mullionV.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;

                var transomH = GameObject.CreatePrimitive(PrimitiveType.Cube);
                transomH.name = "Transom_Horizontal";
                transomH.transform.SetParent(winGo.transform);
                transomH.transform.position = new Vector3(cx, 1.85f, 9.0f);
                transomH.transform.localScale = new Vector3(1.88f, 0.04f, 0.08f);
                if (windowFrameMat != null) transomH.GetComponent<MeshRenderer>().sharedMaterial = windowFrameMat;
            }

            // ==========================================
            // 6. LIGHTING (6 Ceiling Lights inside the room, Height 3.2m, 3x2 grid)
            // ==========================================
            var ceilingLightsRoot = new GameObject("Ceiling_Lights_Grid_3x2");
            ceilingLightsRoot.transform.SetParent(lightingRoot.transform);

            float[] gridX = { 2.5f, 7.0f, 11.5f };
            float[] gridZ = { 2.3f, 6.7f };
            int lightIndex = 1;

            for (int ix = 0; ix < gridX.Length; ix++)
            {
                for (int iz = 0; iz < gridZ.Length; iz++)
                {
                    float lx = gridX[ix];
                    float lz = gridZ[iz];

                    var lightUnit = new GameObject($"Ceiling_Light_Panel_{lightIndex}");
                    lightUnit.transform.SetParent(ceilingLightsRoot.transform);
                    lightUnit.transform.position = Vector3.zero; // Use world positions for exact inside placement

                    // Fixture Mesh attached flush to ceiling underside (Y = 3.48m inside 3.5m high room)
                    var fixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fixture.name = "Fixture_Housing";
                    fixture.transform.SetParent(lightUnit.transform);
                    fixture.transform.position = new Vector3(lx, 3.48f, lz);
                    fixture.transform.localScale = new Vector3(2.0f, 0.04f, 1.0f);
                    if (doorFrameMat != null) fixture.GetComponent<MeshRenderer>().sharedMaterial = doorFrameMat;

                    // Light Diffuser Face
                    var diffuser = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    diffuser.name = "Diffuser_Panel";
                    diffuser.transform.SetParent(lightUnit.transform);
                    diffuser.transform.position = new Vector3(lx, 3.455f, lz);
                    diffuser.transform.localScale = new Vector3(1.88f, 0.015f, 0.88f);
                    if (ceilingMat != null) diffuser.GetComponent<MeshRenderer>().sharedMaterial = ceilingMat;

                    // Point Light component emitting from inside the room at Y = 3.2m
                    var lightGo = new GameObject("Light_Source");
                    lightGo.transform.SetParent(lightUnit.transform);
                    lightGo.transform.position = new Vector3(lx, 3.2f, lz);

                    var lightComp = lightGo.AddComponent<Light>();
                    lightComp.type = LightType.Point;
                    lightComp.color = new Color(0.96f, 0.98f, 1.0f); // Clean neutral laboratory white
                    lightComp.intensity = 2.2f;
                    lightComp.range = 9.5f;
                    lightComp.shadows = LightShadows.Soft;
                    lightComp.shadowStrength = 0.5f;

                    var uLightData = lightGo.AddComponent<UniversalAdditionalLightData>();
                    uLightData.usePipelineSettings = true;

                    lightIndex++;
                }
            }

            // Directional Light: Daylight through windows
            var sunGo = new GameObject("Daylight_Directional_Light");
            sunGo.transform.SetParent(lightingRoot.transform);
            sunGo.transform.position = new Vector3(7.0f, 3.5f, 9.5f);
            sunGo.transform.rotation = Quaternion.Euler(32f, 195f, 0f);

            var sunLight = sunGo.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(0.92f, 0.96f, 1.0f);
            sunLight.intensity = 0.65f;
            sunLight.shadows = LightShadows.Soft;
            sunLight.shadowStrength = 0.6f;

            var sunURPData = sunGo.AddComponent<UniversalAdditionalLightData>();
            sunURPData.usePipelineSettings = true;

            // ==========================================
            // 7. CAMERAS & CAMERA SWITCHER SCRIPT
            // ==========================================
            var camList = new List<Camera>();
            var cam1 = CreateCamera(camerasRoot.transform, "Camera_1_Entrance", new Vector3(2.0f, 2.5f, 3.0f), new Vector3(7.5f, 1.8f, 4.5f), 70f, true, true, true);
            var cam2 = CreateCamera(camerasRoot.transform, "Camera_2_OppositeCorner", new Vector3(12.0f, 2.5f, 3.0f), new Vector3(3.5f, 1.8f, 6.0f), 70f, false, false, false);
            var cam3 = CreateCamera(camerasRoot.transform, "Camera_3_BackView", new Vector3(7.0f, 2.5f, 7.5f), new Vector3(0.0f, 1.8f, 4.5f), 70f, false, false, false);
            var cam4 = CreateCamera(camerasRoot.transform, "Camera_4_CenterView", new Vector3(7.0f, 2.2f, 4.5f), new Vector3(0.0f, 1.8f, 4.5f), 70f, false, false, false);

            camList.Add(cam1);
            camList.Add(cam2);
            camList.Add(cam3);
            camList.Add(cam4);

            // Attach CameraSwitcher script to Cameras root
            var switcher = camerasRoot.AddComponent<CameraSwitcher>();
            switcher.cameras = camList;
            switcher.currentCameraIndex = 0;
            switcher.showOnScreenHint = true;

            // Save the scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[Laboratory Setup] Room shell built and saved successfully to Room Scene.unity!</color>");
        }

        private static Camera CreateCamera(Transform parent, string name, Vector3 position, Vector3 targetLookAt, float fov, bool isActive, bool isMain, bool enableAudio)
        {
            var camGo = new GameObject(name);
            camGo.transform.SetParent(parent);
            camGo.transform.position = position;
            camGo.transform.LookAt(targetLookAt);

            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.clearFlags = CameraClearFlags.Skybox;

            if (isMain)
            {
                camGo.tag = "MainCamera";
            }

            if (enableAudio)
            {
                camGo.AddComponent<AudioListener>();
            }

            var urpCam = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;
            urpCam.renderShadows = true;

            camGo.SetActive(isActive);
            return cam;
        }
    }
}

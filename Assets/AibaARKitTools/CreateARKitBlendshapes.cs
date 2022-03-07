using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using VRM;

#if UNITY_EDITOR
public class CreateARKitBlendshapes : EditorWindow
{
    VRMBlendShapeProxy m_proxy;
    private SkinnedMeshRenderer meshRenderer;

    string [] allShapes = {
        "EyeBlinkLeft",
        "EyeLookDownLeft",
        "EyeLookInLeft",
        "EyeLookOutLeft",
        "EyeLookUpLeft",
        "EyeSquintLeft",
        "EyeWideLeft",
        "EyeBlinkRight",
        "EyeLookDownRight",
        "EyeLookInRight",
        "EyeLookOutRight",
        "EyeLookUpRight",
        "EyeSquintRight",
        "EyeWideRight",
        "JawForward",
        "JawLeft",
        "JawRight",
        "JawOpen",
        "MouthClose",
        "MouthFunnel",
        "MouthPucker",
        "MouthRight",
        "MouthLeft",
        "MouthSmileRight",
        "MouthSmileLeft",
        "MouthFrownRight",
        "MouthFrownLeft",
        "MouthDimpleLeft",
        "MouthDimpleRight",
        "MouthStretchLeft",
        "MouthStretchRight",
        "MouthRollLower",
        "MouthRollUpper",
        "MouthShrugLower",
        "MouthShrugUpper",
        "MouthPressLeft",
        "MouthPressRight",
        "MouthLowerDownLeft",
        "MouthLowerDownRight",
        "MouthUpperUpLeft",
        "MouthUpperUpRight",
        "BrowDownLeft",
        "BrowDownRight",
        "BrowInnerUp",
        "BrowOuterUpLeft",
        "BrowOuterUpRight",
        "CheekPuff",
        "CheekSquintLeft",
        "CheekSquintRight",
        "NoseSneerLeft",
        "NoseSneerRight",
        "TongueOut"
    };

    public int index = 0;
    [MenuItem("Aiba Tools/Setup ARKit Blendshapes")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(CreateARKitBlendshapes));
        window.Show();
    }

    void OnGUI()
    {
        this.m_proxy = (VRMBlendShapeProxy) EditorGUILayout.ObjectField("Avatar", this.m_proxy, typeof (VRMBlendShapeProxy), true);

        this.meshRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField("Blendshape Mesh", this.meshRenderer, typeof (SkinnedMeshRenderer), true);
        if (GUILayout.Button("Setup ARKit Blendshapes")) {
            SetupARKitBlendshapes();
        }
    }

    void SetupARKitBlendshapes()
    {
        BlendShapeAvatar m_avatar = this.m_proxy.BlendShapeAvatar;
        if (m_avatar == null) {
            Debug.LogError("No avatar selected.");
        }

        if (meshRenderer == null) {
            Debug.LogError("No mesh renderer selected.");
        }

        // The skinned mesh renderer name.
        string name = this.meshRenderer.name;

        Mesh mesh = this.meshRenderer.sharedMesh;

        // Waidayo is case sensitive and requires the blendshapes to be named a certain name.
        // This is my overly complicated solution so the blendshapes are non case sensitive and the output is formatted correctly.
        Dictionary<string, string> meshBlendshapeMap = new Dictionary<string, string>();
        for (int i = 0; i < mesh.blendShapeCount; i++) {
            string blendShapeName = mesh.GetBlendShapeName(i);
            meshBlendshapeMap.Add(blendShapeName.ToLower(), blendShapeName);
        }

        Dictionary<string, string> targetBlendshapeMap = new Dictionary<string, string>();
        foreach (string s in allShapes) {
            targetBlendshapeMap.Add(s.ToLower(), s);
        }

        // Get the folder we need to write the blendshapes to
        string path = EditorUtility.OpenFolderPanel("Blendshape Folder", "", "");

        // TODO: Allow a way to filter out some blendshapes that you don't want to use.
        foreach (string s in this.allShapes) {
            // Find if the clip exists
            IEnumerable<BlendShapeClip> workingClipSearch = m_avatar.Clips.Where(c => c.BlendShapeName.ToLower() == s.ToLower());
            BlendShapeClip workingClip = workingClipSearch.Count() > 0 ? workingClipSearch.First() : null;
            if (workingClip == null) {
                // Clip wasn't found. Let's create it.
                BlendShapeClip clip = BlendShapeAvatar.CreateBlendShapeClip(path.ToUnityRelativePath() + "/" + targetBlendshapeMap[s.ToLower()] + ".asset");

                clip.name = targetBlendshapeMap[s.ToLower()];

                m_avatar.Clips.Add(clip);

                // save clips
                EditorUtility.SetDirty(m_avatar);

                workingClip = clip;
            }

            // We have a clip. Update it.
            BlendShapeBinding binding = new BlendShapeBinding();
            binding.RelativePath = name;
            if (meshBlendshapeMap.ContainsKey(s.ToLower())) {
                // Contains blendshape
                index = mesh.GetBlendShapeIndex(meshBlendshapeMap[s.ToLower()]);
                binding.Index = index;
                binding.Weight = 100;

            } else {
                // Blendshape doesn't exist. Use fallback.
                // TODO: Actually be able to assign a fallback.
                binding.Index = 0;
                binding.Weight = 0.01f;
            }
            var temp = workingClip.Values.Where(c => c.Index == index);
            if (temp.Count() == 0) {
                workingClip.Values = workingClip.Values.Append(binding).ToArray();
            }
        }
                
        EditorUtility.SetDirty(m_avatar);
    }
}
#endif

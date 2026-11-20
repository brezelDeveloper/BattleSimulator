// Battle-sim unit FBX helper (Editor only)
// 1) On import: Generic rig, Idle/Move clips set to loop, others play once.
// 2) Menu  Tools > Battle Units > Create Animator Controllers
//    -> creates "<file>_Animator.controller" next to each unit FBX
//       and assigns it to every prefab made from that FBX.
//    Parameters: Moving (bool), Attack / Hit / Death (trigger)
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class BattleUnitAnimImporter : AssetPostprocessor
{
    static readonly string[] Units = { "infantry", "archer", "cavalry", "artillery", "medic", "bomber" };
    static readonly string[] Looping = { "Idle", "Move" };

    static bool IsUnit(string path)
    {
        if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) return false;
        string n = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        return Units.Any(u => n == u + "_blue" || n == u + "_red");
    }

    void OnPreprocessModel()
    {
        if (!IsUnit(assetPath)) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.importAnimation = true;
        mi.preserveHierarchy = true;   // keep the file root so the Body/Horse part stays an animated child
    }

    void OnPreprocessAnimation()
    {
        if (!IsUnit(assetPath)) return;
        var mi = (ModelImporter)assetImporter;
        var clips = mi.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.loopTime = Looping.Contains(c.name);
            c.loopPose = false;
        }
        mi.clipAnimations = clips;
    }

    [MenuItem("Tools/Battle Units/Create Animator Controllers")]
    static void CreateControllers()
    {
        int made = 0, linked = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Model"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!IsUnit(path)) continue;

            var clips = new Dictionary<string, AnimationClip>();
            foreach (var c in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                if (!c.name.StartsWith("__preview__")) clips[c.name] = c;

            string ctrlPath = path.Substring(0, path.Length - 4) + "_Animator.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null) AssetDatabase.DeleteAsset(ctrlPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var sm = ctrl.layers[0].stateMachine;
            AnimatorState idle = AddState(sm, "Idle", clips, new Vector3(300, 0));
            AnimatorState move = AddState(sm, "Move", clips, new Vector3(300, 120));
            AnimatorState attack = AddState(sm, "Attack", clips, new Vector3(560, -60));
            AnimatorState hit = AddState(sm, "Hit", clips, new Vector3(560, 60));
            AnimatorState death = AddState(sm, "Death", clips, new Vector3(560, 180));
            sm.defaultState = idle;

            Go(idle, move, false).AddCondition(AnimatorConditionMode.If, 0, "Moving");
            Go(move, idle, false).AddCondition(AnimatorConditionMode.IfNot, 0, "Moving");
            foreach (var from in new[] { idle, move })
            {
                Go(from, attack, false).AddCondition(AnimatorConditionMode.If, 0, "Attack");
                Go(from, hit, false).AddCondition(AnimatorConditionMode.If, 0, "Hit");
            }
            Go(attack, hit, false).AddCondition(AnimatorConditionMode.If, 0, "Hit");
            Go(attack, idle, true);
            Go(hit, idle, true);

            var toDeath = sm.AddAnyStateTransition(death);
            toDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
            toDeath.canTransitionToSelf = false;
            toDeath.hasExitTime = false;
            toDeath.duration = 0.05f;
            made++;
            linked += AssignToPrefabs(path, ctrl);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[BattleUnits] Animator Controllers created: " + made + ", prefabs linked: " + linked);
    }

    // Give every prefab built from this FBX an Animator using the new controller.
    static int AssignToPrefabs(string fbxPath, RuntimeAnimatorController ctrl)
    {
        int n = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (asset == null) continue;
            var src = PrefabUtility.GetCorrespondingObjectFromOriginalSource(asset);
            if (src == null || AssetDatabase.GetAssetPath(src) != fbxPath) continue;
            var root = PrefabUtility.LoadPrefabContents(p);
            var anim = root.GetComponent<Animator>();
            if (anim == null) anim = root.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            anim.applyRootMotion = false;
            PrefabUtility.SaveAsPrefabAsset(root, p);
            PrefabUtility.UnloadPrefabContents(root);
            n++;
        }
        return n;
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string name, Dictionary<string, AnimationClip> clips, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        AnimationClip clip;
        if (clips.TryGetValue(name, out clip)) s.motion = clip;
        return s;
    }

    static AnimatorStateTransition Go(AnimatorState from, AnimatorState to, bool afterClipEnds)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = afterClipEnds;
        t.exitTime = afterClipEnds ? 1f : 0f;
        t.duration = 0.08f;
        return t;
    }
}

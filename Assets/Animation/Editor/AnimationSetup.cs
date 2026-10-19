using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor utilities for the animation assignment: importer setup, Animator Controller and demo scene.
// Re-running "Build All" regenerates everything from the two source FBX files.
public static class AnimationSetup
{
    const string ReaperPath = "Assets/Animation/Models/Reaper/Reaper.fbx";
    const string LibraryPath = "Assets/Animation/Models/Quaternius/AnimationLibrary_Unity_Standard.fbx";
    const string ControllerPath = "Assets/Animation/Animators/BossAnimator.controller";
    const string ScenePath = "Assets/Animation/Scenes/BossAnimation.unity";

    [MenuItem("Tools/Animation/Build All")]
    public static void BuildAll()
    {
        ConfigureImporters();
        BuildController();
        BuildScene();
        DumpClipInfo();
    }

    [MenuItem("Tools/Animation/1. Configure Importers")]
    public static void ConfigureImporters()
    {
        ConfigureReaper();
        ConfigureLibrary();
    }

    static void ConfigureReaper()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(ReaperPath);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        // The FBX holds every take twice ("idle" and "Armature|idle"); keep the plain ones.
        var clips = importer.defaultClipAnimations.Where(c => !c.takeName.Contains("|")).ToArray();
        foreach (var c in clips)
            SetupClip(c, c.name == "idle" || c.name.StartsWith("walk_"));
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    static void ConfigureLibrary()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(LibraryPath);
        importer.bakeAxisConversion = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.motionNodeName = "Rig/root";

        var clips = importer.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.name = c.takeName.Replace("Rig|", "");
            SetupClip(c, c.name.EndsWith("_Loop"));
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    // Locomotion is driven by script, so root motion is baked into the pose to keep clips in place.
    static void SetupClip(ModelImporterClipAnimation c, bool loop)
    {
        c.loopTime = loop;
        c.loopPose = loop;
        c.lockRootRotation = true;
        c.lockRootHeightY = true;
        c.lockRootPositionXZ = true;
        c.keepOriginalOrientation = true;
        c.keepOriginalPositionY = true;
        c.keepOriginalPositionXZ = true;
    }

    static AnimationClip Clip(string fbxPath, string name)
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>()
            .FirstOrDefault(c => c.name == name);
        if (clip == null) throw new System.Exception("Missing clip " + name + " in " + fbxPath);
        return clip;
    }

    [MenuItem("Tools/Animation/2. Build Animator Controller")]
    public static void BuildController()
    {
        // Reuse the existing asset so the scene keeps its reference (same GUID) after a rebuild.
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        else
            ClearController(controller);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveZ", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("AttackIndex", AnimatorControllerParameterType.Int);
        controller.AddParameter("Block", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;

        // 2D blend tree mixing both sources: Reaper idle/walks around the centre, Quaternius jog/sprint further out on +Z.
        BlendTree tree;
        var locomotion = controller.CreateBlendTreeInController("Locomotion", out tree, 0);
        tree.blendType = BlendTreeType.FreeformDirectional2D;
        tree.blendParameter = "MoveX";
        tree.blendParameterY = "MoveZ";
        tree.AddChild(Clip(ReaperPath, "idle"), Vector2.zero);
        tree.AddChild(Clip(ReaperPath, "walk_fwd"), new Vector2(0f, 1f));
        tree.AddChild(Clip(ReaperPath, "walk_bkw"), new Vector2(0f, -1f));
        tree.AddChild(Clip(ReaperPath, "walk_left"), new Vector2(-1f, 0f));
        tree.AddChild(Clip(ReaperPath, "walk_right"), new Vector2(1f, 0f));
        tree.AddChild(Clip(LibraryPath, "Jog_Fwd_Loop"), new Vector2(0f, 2f));
        tree.AddChild(Clip(LibraryPath, "Sprint_Loop"), new Vector2(0f, 3f));
        sm.defaultState = locomotion;

        // Three-hit combo. Attacks start only from Locomotion and leave only by exit time,
        // so once an attack begins nothing (another attack, block, movement) can cut it short.
        for (int i = 0; i < 3; i++)
        {
            var attack = sm.AddState("Attack" + (i + 1), new Vector3(600f, i * 80f));
            attack.motion = Clip(ReaperPath, "atk_" + (i + 1));
            attack.tag = "Attack";
            var enter = locomotion.AddTransition(attack);
            enter.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            enter.AddCondition(AnimatorConditionMode.Equals, i, "AttackIndex");
            enter.hasExitTime = false;
            enter.duration = 0.1f;
            var back = attack.AddTransition(locomotion);
            back.hasExitTime = true;
            back.exitTime = 0.9f;
            back.duration = 0.2f;
            back.interruptionSource = TransitionInterruptionSource.None;
        }

        var block = sm.AddState("Block", new Vector3(300f, 260f));
        block.motion = Clip(ReaperPath, "block");
        var toBlock = locomotion.AddTransition(block);
        toBlock.AddCondition(AnimatorConditionMode.If, 0f, "Block");
        toBlock.hasExitTime = false;
        toBlock.duration = 0.15f;
        var fromBlock = block.AddTransition(locomotion);
        fromBlock.AddCondition(AnimatorConditionMode.IfNot, 0f, "Block");
        fromBlock.hasExitTime = false;
        fromBlock.duration = 0.2f;

        AssetDatabase.SaveAssets();
    }

    static void ClearController(AnimatorController controller)
    {
        foreach (var p in controller.parameters)
            controller.RemoveParameter(p);
        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions)
            sm.RemoveAnyStateTransition(t);
        foreach (var child in sm.states)
            sm.RemoveState(child.state);
        foreach (var tree in AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>())
            Object.DestroyImmediate(tree, true);
    }

    [MenuItem("Tools/Animation/3. Build Demo Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(30f, 1f, 30f);

        var boss = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ReaperPath));
        boss.name = "Reaper";
        var animator = boss.GetComponent<Animator>();
        if (animator == null) animator = boss.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        animator.applyRootMotion = false;
        boss.AddComponent<BossAnimationDriver>();

        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 14f, -26f);
        cam.farClipPlane = 1000f;
        var follow = cam.gameObject.AddComponent<FollowCamera>();
        follow.target = boss.transform;

        EditorSceneManager.SaveScene(scene, ScenePath);

        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == ScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    [MenuItem("Tools/Animation/Dump Clip Info")]
    public static void DumpClipInfo()
    {
        using (var w = new StreamWriter("Logs/clip_dump.txt"))
        {
            foreach (var path in new[] { ReaperPath, LibraryPath })
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
                w.WriteLine("== " + path + " type=" + importer.animationType +
                    " avatar=" + (avatar == null ? "none" : "valid=" + avatar.isValid + " human=" + avatar.isHuman));
                if (avatar != null && avatar.isHuman)
                {
                    var mapped = avatar.humanDescription.human;
                    w.WriteLine("  mapped bones=" + mapped.Length + " e.g. " + string.Join(", ",
                        mapped.Where(h => h.humanName == "Hips" || h.humanName == "LeftHand" || h.humanName == "Head")
                            .Select(h => h.humanName + "->" + h.boneName).ToArray()));
                }
                foreach (var c in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")))
                    w.WriteLine("  clip " + c.name + " len=" + c.length.ToString("0.00") + " loop=" + c.isLooping + " human=" + c.humanMotion);
            }
        }
    }
}

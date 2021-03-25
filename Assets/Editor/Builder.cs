using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

public class Builder
{
    [MenuItem("Build/BuildAndroid", false, 0)]
    public static void BuildAndroid()
    {
        var scenes = GetEnabledEditorScenes();
        GenericBuild(scenes, GetDefaultPathProjectAndroid(), BuildTargetGroup.Android, BuildTarget.Android);
    }
    
    public static void BuildIOS()
    {
        var scenes = GetEnabledEditorScenes();
        GenericBuild(scenes, GetDefaultPathProjectIOS(), BuildTargetGroup.iOS, BuildTarget.iOS);
    }
    
    public static string[] GetEnabledEditorScenes()
    {
        List<string> editorScenes = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene == null) continue;
            if (!scene.enabled) continue;
            var sceneAsset = AssetImporter.GetAtPath(scene.path);
            if (sceneAsset == null || !string.IsNullOrEmpty(sceneAsset.assetBundleName)) continue;

            editorScenes.Add(scene.path);
        }

        return editorScenes.ToArray();
    }

public static void GenericBuild(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

        var buildOptions = new BuildPlayerOptions();
        buildOptions.locationPathName = targetDir;
        buildOptions.scenes = scenes;
        buildOptions.targetGroup = buildTargetGroup;
        buildOptions.target = buildTarget;

        AssetDatabase.Refresh();

        var result = BuildPipeline.BuildPlayer(buildOptions);
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("BuildPlayer failure: \n<<<" + result.summary.result + ">>>");
    }

    private static string GetDefaultPathProjectIOS()
    {
        string path = "CarX_Text";
        return path;
    }

    private static string GetDefaultPathProjectAndroid()
    {
        string path = "CarX_Text.apk";
        return path;
    }
}
#endif

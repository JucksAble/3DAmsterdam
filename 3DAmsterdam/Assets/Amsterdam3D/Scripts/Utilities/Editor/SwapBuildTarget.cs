﻿using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Amsterdam3D.Utilities
{
    public class SwapBuildTarget : MonoBehaviour
    {
        [MenuItem("3D Amsterdam/Environment target/Production")]
        public static void SwitchBranchMaster()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL,"PRODUCTION");
            PlayerSettings.bundleVersion = ""; 
            Debug.Log("Set scripting define symbols to PRODUCTION");
        }
        [MenuItem("3D Amsterdam/Environment target/Development")]
        public static void SwitchBranchDevelop()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "DEVELOPMENT");
            PlayerSettings.bundleVersion = "develop";
            Debug.Log("Set scripting define symbols to DEVELOPMENT");
        }
        [MenuItem("3D Amsterdam/Environment target/Development - Feature")]
        public static void SwitchBranchFeature()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "DEVELOPMENT_FEATURE");

            var gitHeadFile = Application.dataPath + "/../../.git/HEAD";
            var headLine = File.ReadAllText(gitHeadFile);
            Debug.Log("Reading git HEAD file:" + headLine);

            if (!headLine.Contains("feature/")){
                Debug.Log("Your branch does not seem to be a feature/ branch");
            }
            var positionLastSlash = headLine.LastIndexOf("/") + 1;
            var featureName = headLine.Substring(positionLastSlash, headLine.Length - positionLastSlash);

            PlayerSettings.bundleVersion = "feature/" + featureName;
            Debug.Log("Version set to feature name: " + Application.version);

            Debug.Log("Set scripting define symbols to DEVELOPMENT_FEATURE");
        }
        [MenuItem("3D Amsterdam/Build for WebGL platform")]
        public static void BuildWebGL()
        {
            TargetedBuild(BuildTarget.WebGL);
        }

        //Optional other future platform targets, for example desktop:
        /*[MenuItem("3D Amsterdam/Build for Windows 64 bit")]
        public static void BuildWindows()
        {
            TargetedBuild(BuildTarget.StandaloneWindows64);
        }*/

        public static void TargetedBuild(BuildTarget buildTarget = BuildTarget.WebGL)
        {           
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                target = buildTarget,
                locationPathName = "../../" + ((buildTarget==BuildTarget.WebGL) ? "BuildWebGL" : "BuildDesktop"),
                options = BuildOptions.AutoRunPlayer
            };            

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary buildSummary = report.summary;

            if (buildSummary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + buildSummary.totalSize + " bytes");
            }

            if (buildSummary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}
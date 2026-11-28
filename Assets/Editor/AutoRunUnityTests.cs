using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
public static class AutoRunUnityTests
{
    const string State = "ZelenRunState5";
    static TestRunnerApi api;

    static AutoRunUnityTests()
    {
        EditorApplication.delayCall += Start;
    }

    static string Root { get { return Directory.GetParent(Application.dataPath).FullName; } }

    static void Start()
    {
        string state = EditorPrefs.GetString(State, "");
        if (state == "done" || state == "play_running") return;

        api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new Callbacks());

        if (state == "play_requested")
        {
            EditorPrefs.SetString(State, "play_running");
            return;
        }

        EditorPrefs.SetString(State, "edit_running");
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
    }

    static void Save(TestMode mode, ITestResultAdaptor result)
    {
        string file = Path.Combine(Root, "Testing", mode == TestMode.EditMode
            ? "Unity_Unit_TestRunner.xml" : "Unity_Scenario_TestRunner.xml");
        string text;
        try
        {
            var m = result.GetType().GetMethod("ToXml", Type.EmptyTypes);
            if (m != null)
            {
                var node = m.Invoke(result, null);
                var outer = node != null ? node.GetType().GetProperty("OuterXml") : null;
                text = outer != null ? (string)outer.GetValue(node, null) : Summary(result);
            }
            else text = Summary(result);
        }
        catch { text = Summary(result); }

        Directory.CreateDirectory(Path.GetDirectoryName(file));
        File.WriteAllText(file, text);
        Debug.Log("AUTORUN saved " + file);

        if (mode == TestMode.EditMode)
        {
            EditorPrefs.SetString(State, "play_requested");
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode }));
        }
        else
        {
            EditorPrefs.SetString(State, "done");
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }
    }

    static string Summary(ITestResultAdaptor r)
    {
        return "<test-run result=\"" + r.ResultState + "\" passed=\"" + r.PassCount +
               "\" failed=\"" + r.FailCount + "\" />";
    }

    class Callbacks : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            string state = EditorPrefs.GetString(State, "");
            Save(state == "play_running" ? TestMode.PlayMode : TestMode.EditMode, result);
        }
    }
}

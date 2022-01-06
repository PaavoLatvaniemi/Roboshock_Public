
using Assets.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
[CustomEditor(typeof(TestingFrameRateLimiter))]
public class ChangeableOnEditor : Editor
{

    public SerializedProperty value;
    [SerializeField] public UnityEvent method;
    void OnEnable()
    {
        value = serializedObject.FindProperty("frameRate");

    }

    public override void OnInspectorGUI()
    {
        // Get value before change
        int previousValue = value.intValue;

        // Make all the public and serialized fields visible in Inspector
        base.OnInspectorGUI();

        // Load changed values
        serializedObject.Update();

        // Check if value has changed
        if (previousValue != value.intValue)
        {
            TestingFrameRateLimiter testingFrameRateLimiter = (TestingFrameRateLimiter)target;
            testingFrameRateLimiter.ChangeTargetFramerate();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
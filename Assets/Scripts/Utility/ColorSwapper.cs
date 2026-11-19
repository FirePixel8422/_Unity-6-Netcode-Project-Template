#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ColorSwapper : MonoBehaviour
{
    [SerializeField] private Color colorA;
    [SerializeField] private Color colorB;


    [InspectorButton]
    private void SwapColors()
    {
        DebugLogger.Log("Swapping colors...");

        Image[] images = this.FindObjectsOfType<Image>(true);

        // Collect only affected images
        System.Collections.Generic.List<Image> targets = new System.Collections.Generic.List<Image>();

        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];

            if (Vector4.Distance(image.color, colorA) < 0.05f)
            {
                targets.Add(image);
            }
        }

        if (targets.Count == 0)
        {
            DebugLogger.Log("No images found to swap.");
            return;
        }

        // Register ALL undo in one operation
        Undo.RegisterCompleteObjectUndo(targets.ToArray(), "Swap UI Colors");

        for (int i = 0; i < targets.Count; i++)
        {
            Image image = targets[i];
            image.color = colorB;
            EditorUtility.SetDirty(image);
        }
    }

    [InspectorButton]
    private void ReverseSwapColors()
    {
        DebugLogger.Log("Reverse swapping colors...");

        Image[] images = this.FindObjectsOfType<Image>(true);

        System.Collections.Generic.List<Image> targets = new System.Collections.Generic.List<Image>();

        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];

            if (Vector4.Distance(image.color, colorB) < 0.05f)
            {
                targets.Add(image);
            }
        }

        if (targets.Count == 0)
        {
            DebugLogger.Log("No images found to undo swap.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(targets.ToArray(), "Undo UI Colors");

        for (int i = 0; i < targets.Count; i++)
        {
            Image image = targets[i];
            image.color = colorA;
            EditorUtility.SetDirty(image);
        }
    }
}
#endif

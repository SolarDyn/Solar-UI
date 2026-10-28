// CollimatedHUD.cs
// Solar Dynamics 2026

#region

using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Solar.UI.Input;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#endregion

namespace Solar.UI.Chud;

internal static class CollimatedHUDPlugin
{
    private static bool _installed;

    internal static void Install()
    {
        if (_installed)
            throw new InvalidOperationException("CollimatedHUDPlugin is already installed.");

        MethodInfo initialize = AccessTools.Method(typeof(CameraStateManager), nameof(CameraStateManager.OnEnable), Type.EmptyTypes)
                                ?? throw new MissingMethodException(typeof(CameraStateManager).FullName, nameof(CameraStateManager.OnEnable));

        Harmony harmony = new Harmony("Solar.UI.CollimatedHUD");
        try
        {
            harmony.Patch(initialize, postfix: new HarmonyMethod(
                typeof(CollimatedHUDPlugin), nameof(Postfix)) { priority = Priority.HigherThanNormal });
            _installed = true;
        }
        catch
        {
            harmony.UnpatchSelf();
            throw;
        }
    }

    [UsedImplicitly]
    private static void Postfix(CameraStateManager __instance)
    {
        CollimatedHUD chud = __instance.gameObject.GetComponent<CollimatedHUD>();
        if (!chud) __instance.gameObject.AddComponent<CollimatedHUD>();
    }

    // [UsedImplicitly]
    // private static void Postfix(CameraStateManager __instance)
    // {
    //     CollimatedHUD chud = __instance.gameObject.GetComponent<CollimatedHUD>();
    //     if (!chud)
    //     {
    //         throw new ArgumentNullException(nameof(chud), "Collimated HUD not found after creation.");
    //     }
    // }
}

public class CollimatedHUD : SceneSingleton<CollimatedHUD>
{
    public const int UILayer = 24;
    public const float UIDistance = 1000f;

    public static CameraStateManager _cameraStateManager;
    public static GameObject _object;
    public static Camera _camera;

    // public static FlightHud? flightHud;

    static CollimatedHUD()
    {
        _cameraStateManager = SceneSingleton<CameraStateManager>.i;

        _object = SetupChudCamera();
        _camera = _object.GetComponent<Camera>();
    }

    // private void FixedUpdate()
    // {
    //
    // }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        FlightHud flightHud = SceneSingleton<FlightHud>.i;

        if (!flightHud.gameObject.activeSelf)
        {
            if (_camera.enabled)
                _camera.enabled = false;
            return;
        }

        if (!_camera.enabled)
            _camera.enabled = true;

        if (!_cameraStateManager || !_cameraStateManager.mainCamera)
        {
            Plugin.Log("ChudCamera could not find cameraStateManager. Destroying.");
            DestroyChudCamera();
            return;
        }

        _camera.fieldOfView = _cameraStateManager.mainCamera.fieldOfView;
    }

    public static GameObject SetupChudCamera()
    {
        DestroyChudCamera();

        Camera mainCamera = _cameraStateManager.mainCamera;

        if (!mainCamera) throw new ArgumentNullException(nameof(mainCamera), "Could not find mainCamera.");

        GameObject newObject = new("ChudCamera", typeof(Camera));
        newObject.transform.SetParent(mainCamera.transform, false);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        newObject.transform.localScale = Vector3.one;

        Camera newCamera = newObject.GetComponent<Camera>();
        newCamera.CopyFrom(mainCamera);

        mainCamera.cullingMask &= ~(1 << UILayer);
        newCamera.cullingMask = 1 << UILayer;

        SetLayerRecursive(newObject, UILayer);

        UniversalAdditionalCameraData mainData = mainCamera.GetUniversalAdditionalCameraData();
        UniversalAdditionalCameraData hudData = newCamera.GetUniversalAdditionalCameraData();

        hudData.renderType = CameraRenderType.Overlay;
        hudData.renderPostProcessing = true;
        hudData.renderShadows = false;
        hudData.m_ClearDepth = true;

        // Make sure new camera is last in the stack.
        mainData.cameraStack.Remove(newCamera);
        mainData.cameraStack.Add(newCamera);

        newCamera.enabled = true;

        return newObject;
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
    }

    public static void DestroyChudCamera()
    {
        if (!_object)
            return;

        Destroy(_object);
        _object = null!;
        _camera = null!;
    }

    // public static void UpdateHUDCamera()
    // {
    //     if (!_camera)
    //         return;
    //
    //     FlightHud flightHUD = SceneSingleton<FlightHud>.i;
    //     if (_camera.enabled != flightHUD.enabled) _camera.enabled = flightHUD.enabled;
    //
    //     Camera mainCamera = SceneSingleton<CameraStateManager>.i.mainCamera;
    //
    //     if (!mainCamera)
    //         return;
    //
    //     _camera.fieldOfView = mainCamera.fieldOfView;
    // }
}
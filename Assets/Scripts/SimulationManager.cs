using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public RawImage rawImage;
    public Slider slider;

    [Header("Simulation Parameters")]
    public int VolumeSize = 5;
    public int FluidDiffusion = 0;
    public int FluidViscosity = 0;

    [Header("Mouse Settings")]
    public int ClickDensity = 10;
    public int ClickRadius = 10;
    public float VelocityScale = 0.01f;

    FluidVolume FluidVolume;
    RectTransform RectTransform;
    Vector2 oldMousePosition = new Vector2();

    void Start() {
        FluidVolume = new FluidVolume(VolumeSize, FluidDiffusion, FluidViscosity, 0.1f);
        rawImage.texture = new Texture2D(FluidVolume.Size, FluidVolume.Size);

        RectTransform = rawImage.GetComponent<RectTransform>();
        slider.value = ClickRadius;
    }

    void Update() {
        FluidVolume.Step();
        DrawLiquid();

        ClickRadius = (int)slider.value;

        Vector2 dir = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - oldMousePosition;
        if (Input.GetMouseButton(0) && MouseInsideBounds()) {
            Vector2 pos = GetRelativeMousePos();

            for (int a = -(ClickRadius / 2); a < (ClickRadius / 2); a++) {
                for (int b = -(ClickRadius / 2); b < (ClickRadius / 2); b++) {
                    if (new Vector2(a, b).magnitude < ClickRadius / 2) {
                        FluidVolume.AddDensity((int)pos.x + a, (int)pos.y + b, ClickDensity);
                        FluidVolume.AddVelocity((int)pos.x + a, (int)pos.y + b, dir.x * VelocityScale, dir.y * VelocityScale);
                    }
                }
            }
        }

        oldMousePosition = Input.mousePosition;
    }

    private void DrawLiquid() {
        Texture2D texture = new Texture2D(FluidVolume.Size, FluidVolume.Size);

        for (int x = 0; x < FluidVolume.Size; x++) {
            for (int y = 0; y < FluidVolume.Size; y++) {
                float d = FluidVolume.GetDensity(x, y);
                texture.SetPixel(x, y, new Color(d / ClickDensity, d / ClickDensity, d / ClickDensity));
            }
        }
        texture.Apply();
        rawImage.texture = texture;
    }

    private Vector2 GetRelativeMousePos() {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out localPoint);
        Rect r = RectTransform.rect;
        float px = Mathf.Clamp((int)(((localPoint.x - r.x) * rawImage.texture.width) / r.width), 0, rawImage.texture.width - 1);
        float py = Mathf.Clamp((int)(((localPoint.y - r.y) * rawImage.texture.height) / r.height), 0, rawImage.texture.height - 1);
        return new Vector2(px, py);
    }

    private bool MouseInsideBounds() {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out localPoint);
        Rect r = RectTransform.rect;
        float x = (((localPoint.x - r.x) * rawImage.texture.width) / r.width);
        float y = (((localPoint.y - r.y) * rawImage.texture.height) / r.height);

        return !(x < 0 || x > rawImage.texture.width || y < 0 || y > rawImage.texture.height);
    }
}

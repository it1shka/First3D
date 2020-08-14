using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DayCycle : MonoBehaviour
{
    [Range(0f, 1f)]
    public float timeOfDay;
    public float dayDuration = 30f;

    public Light
        sun,
        moon;

    public Material
        daySkybox,
        nightSkybox;

    public AnimationCurve
        SunIntensityPerDay,
        MoonIntensityPerDay,
        skyboxCurve,
        starsAlphaCurve;
    private float 
        sunIntensity,
        moonIntensity;

    public ParticleSystem stars;
    private ParticleSystem.MainModule main;

    private void Start()
    {
        sunIntensity = sun.intensity;
        moonIntensity = moon.intensity;
        main = stars.main;
    }
    private void Update()
    {
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1) timeOfDay = 0;

        UpdScene();
    }
    private void UpdScene()
    {
        RenderSettings.skybox.Lerp(nightSkybox, daySkybox, skyboxCurve.Evaluate(timeOfDay));

        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, starsAlphaCurve.Evaluate(timeOfDay))
        );

        RenderSettings.sun = timeOfDay > .5f ? moon : sun;

        DynamicGI.UpdateEnvironment();

        sun.transform.localRotation = Quaternion.Euler(timeOfDay * 360f, 180f, 0f);
        moon.transform.localRotation = Quaternion.Euler(timeOfDay * 360f + 180f, 180f, 0f);
        sun.intensity = sunIntensity * SunIntensityPerDay.Evaluate(timeOfDay);
        moon.intensity = moonIntensity * MoonIntensityPerDay.Evaluate(timeOfDay);
    }
}

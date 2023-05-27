using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PUSpeedUp : PowerUp
{
    [SerializeField]
    private GameObject speedUpPickUpEffect;
    [SerializeField]
    private float speedMultiplier = 2f;
    [SerializeField]
    private float duration = 3f;

    protected override float GetDuration()
    {
        return duration;
    }
    protected override void PlayParticalEffect()
    {
        Instantiate(speedUpPickUpEffect, transform.position, transform.rotation);
    }
    protected override void ApplyPowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        controllerSettings.maxSpeed *= speedMultiplier;
        controllerSettings.accelerationFactor *= speedMultiplier;
    }
    protected override void RemovePowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        controllerSettings.maxSpeed /= speedMultiplier;
        controllerSettings.accelerationFactor /= speedMultiplier;
    }
    protected override string GetMessage()
    {
        return "Speed Up";
    } 
}

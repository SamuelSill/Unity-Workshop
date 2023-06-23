using UnityEngine;

public class PUSpeedDown : PowerUp
{
    [SerializeField]
    private GameObject speedDownPickUpEffect;
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
        Instantiate(speedDownPickUpEffect, transform.position, transform.rotation);
    }
    protected override void ApplyPowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        controllerSettings.maxSpeed /= speedMultiplier;
        controllerSettings.accelerationFactor /= speedMultiplier;
    }
    protected override void RemovePowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        controllerSettings.maxSpeed *= speedMultiplier;
        controllerSettings.accelerationFactor *= speedMultiplier;
    }
    protected override string GetMessage()
    {
        return "Speed Down";
    }
}

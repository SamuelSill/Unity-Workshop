using UnityEngine;

public class PUSizeUp : PowerUp
{
    [SerializeField]
    private GameObject sizeUpPickUpEffect;
    [SerializeField]
    private float sizeMultiplier = 1.5f;
    [SerializeField]
    private float duration = 3f;

    protected override float GetDuration()
    {
        return duration;
    }
    protected override void PlayParticalEffect()
    {
        Instantiate(sizeUpPickUpEffect, transform.position, transform.rotation);
    }
    protected override void ApplyPowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        player.transform.localScale *= sizeMultiplier;
    }
    protected override void RemovePowerUpEffectOnPlayer(Collider2D player)
    {
        TopDownCarController controllerSettings = player.GetComponent<TopDownCarController>();
        player.transform.localScale /= sizeMultiplier;
    }
    protected override string GetMessage()
    {
        return "Size UP";
    }
}

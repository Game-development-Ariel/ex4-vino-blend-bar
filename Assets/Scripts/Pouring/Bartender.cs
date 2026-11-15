using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pours: when a bottle is clicked, a splash of its wine is thrown in an arc into the glass.
/// While this component is disabled, clicking bottles does nothing.
/// </summary>
public class Bartender : MonoBehaviour
{
    [SerializeField] private GrapeBottle[] bottles;
    [SerializeField] private Glass glass;
    [SerializeField] private WineDrop dropPrefab;

    [Tooltip("The point above the glass that the splashes fly to")]
    [SerializeField] private Transform pourTarget;

    [Tooltip("How many percent of the glass one click pours")]
    [SerializeField] private int percentPerPour = 10;

    [Tooltip("Seconds a splash is in the air")]
    [SerializeField] private float flightSeconds = 0.7f;

    // The splashes that are still in the air. A splash that lands or falls is destroyed,
    // and then its entry here becomes null.
    private readonly List<WineDrop> dropsInFlight = new List<WineDrop>();

    private void OnEnable()
    {
        foreach (GrapeBottle bottle in bottles)
        {
            bottle.Clicked += Pour;
        }
        glass.Emptied += DestroyDropsInFlight;
    }

    // Disabled means the guest was served or left, so a splash still in the air must not
    // land in the next guest's glass.
    private void OnDisable()
    {
        foreach (GrapeBottle bottle in bottles)
        {
            bottle.Clicked -= Pour;
        }
        glass.Emptied -= DestroyDropsInFlight;
        DestroyDropsInFlight();
    }

    private void Pour(GrapeBottle bottle)
    {
        if (glass.Blend.IsFull)
        {
            return;
        }

        WineDrop drop = Instantiate(dropPrefab, bottle.SpoutPosition, Quaternion.identity);
        drop.Setup(bottle.Grape, percentPerPour);
        dropsInFlight.RemoveAll(oldDrop => oldDrop == null);
        dropsInFlight.Add(drop);

        // The throw: a body under gravity moves by  distance = velocity * t + gravity * t * t / 2.
        // Solving for the velocity that covers the distance to the glass in exactly flightSeconds:
        Vector2 distance = (Vector2)pourTarget.position - bottle.SpoutPosition;
        Vector2 velocity = distance / flightSeconds - Physics2D.gravity * flightSeconds / 2f;

        // An impulse of velocity * mass gives the body exactly that velocity.
        Rigidbody2D body = drop.GetComponent<Rigidbody2D>();
        body.AddForce(velocity * body.mass, ForceMode2D.Impulse);
    }

    // A splash thrown before "Empty glass" would otherwise land in the glass that was just emptied.
    private void DestroyDropsInFlight()
    {
        foreach (WineDrop drop in dropsInFlight)
        {
            if (drop != null)
            {
                Destroy(drop.gameObject);
            }
        }
        dropsInFlight.Clear();
    }
}

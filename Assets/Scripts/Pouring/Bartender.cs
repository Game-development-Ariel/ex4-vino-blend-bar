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

    private void OnEnable()
    {
        foreach (GrapeBottle bottle in bottles)
        {
            bottle.Clicked += Pour;
        }
    }

    private void OnDisable()
    {
        foreach (GrapeBottle bottle in bottles)
        {
            bottle.Clicked -= Pour;
        }
    }

    private void Pour(GrapeBottle bottle)
    {
        if (glass.Blend.IsFull)
        {
            return;
        }

        WineDrop drop = Instantiate(dropPrefab, bottle.SpoutPosition, Quaternion.identity);
        drop.Setup(bottle.Grape, percentPerPour);

        // The throw: a body under gravity moves by  distance = velocity * t + gravity * t * t / 2.
        // Solving for the velocity that covers the distance to the glass in exactly flightSeconds:
        Vector2 distance = (Vector2)pourTarget.position - bottle.SpoutPosition;
        Vector2 velocity = distance / flightSeconds - Physics2D.gravity * flightSeconds / 2f;

        // An impulse of velocity * mass gives the body exactly that velocity.
        Rigidbody2D body = drop.GetComponent<Rigidbody2D>();
        body.AddForce(velocity * body.mass, ForceMode2D.Impulse);
    }
}

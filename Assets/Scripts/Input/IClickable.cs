/// <summary>
/// Something in the world that reacts to a click or a touch. WorldClicker finds it and calls OnClicked.
/// </summary>
public interface IClickable
{
    void OnClicked();
}

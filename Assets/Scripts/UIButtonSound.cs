using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySE(SEType.Select);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySE(SEType.Decide);
    }
}
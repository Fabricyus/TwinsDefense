using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Forwards pointer enter/exit off a UI element to plain C# events, so a
    /// sibling/parent script (which doesn't itself sit on the hovered element)
    /// can react to hover without implementing the interfaces itself.
    /// </summary>
    public class PointerHoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnEnter;
        public event Action OnExit;

        public void OnPointerEnter(PointerEventData eventData) => OnEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => OnExit?.Invoke();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using TwinsDefense.Systems;

namespace TwinsDefense.UI
{
    public enum MoveDirection { Up, Down, Left, Right, Confirm }

    /// <summary>
    /// One rebindable row in the Settings panel: shows "&lt;Direction&gt;: &lt;Key&gt;",
    /// and on click waits for the next physical key press to become the new
    /// binding for that direction (see KeyBindings). No InputActionAsset/
    /// RebindingOperation involved — PlayerController just polls
    /// Keyboard.current[KeyBindings.X] directly, so this polls the same way
    /// rather than pulling in a whole rebinding subsystem for 4 keys.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KeybindRowUI : MonoBehaviour
    {
        [SerializeField] private MoveDirection direction;
        [SerializeField] private string directionDisplayName = "Up";
        [SerializeField] private TextMeshProUGUI label;

        private Button button;
        private bool isListening;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(BeginListening);

            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            isListening = false;
            RefreshLabel();
        }

        private void BeginListening()
        {
            if (isListening) return;

            isListening = true;
            if (label != null)
            {
                label.text = $"{directionDisplayName}: press a key...";
            }
        }

        private void Update()
        {
            if (!isListening) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            foreach (KeyControl control in keyboard.allKeys)
            {
                if (!control.wasPressedThisFrame) continue;

                if (control.keyCode == Key.Escape)
                {
                    isListening = false;
                    RefreshLabel();
                    return;
                }

                AssignKey(control.keyCode);
                isListening = false;
                return;
            }
        }

        private void AssignKey(Key key)
        {
            switch (direction)
            {
                case MoveDirection.Up: KeyBindings.SetUp(key); break;
                case MoveDirection.Down: KeyBindings.SetDown(key); break;
                case MoveDirection.Left: KeyBindings.SetLeft(key); break;
                case MoveDirection.Right: KeyBindings.SetRight(key); break;
                case MoveDirection.Confirm: KeyBindings.SetConfirm(key); break;
            }

            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (label == null) return;

            Key current;
            switch (direction)
            {
                case MoveDirection.Up: current = KeyBindings.Up; break;
                case MoveDirection.Down: current = KeyBindings.Down; break;
                case MoveDirection.Left: current = KeyBindings.Left; break;
                case MoveDirection.Right: current = KeyBindings.Right; break;
                case MoveDirection.Confirm: current = KeyBindings.Confirm; break;
                default: current = Key.None; break;
            }

            label.text = $"{directionDisplayName}: {current}";
        }
    }
}

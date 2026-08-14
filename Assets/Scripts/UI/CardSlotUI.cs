using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TwinsDefense.Data;

namespace TwinsDefense.UI
{
    /// <summary>
    /// A single level-up card slot (Card1/2/3). Displays a rolled CardData's
    /// effect description on its txtCard child, scales up while hovered and
    /// punches on pick, then invokes a callback after a short delay once its
    /// button is clicked. Animations run on unscaled time since the game is
    /// paused (Time.timeScale = 0) while the card draft is shown.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CardSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Card Frame")]
        [Tooltip("Left unassigned, resolved via GetComponent on first Awake (the frame Image lives on this same GameObject).")]
        [SerializeField] private Image cardFrameImage;
        [Tooltip("Swapped in in place of the frame's default sprite while a special (buff+debuff) card is shown.")]
        [SerializeField] private Sprite specialCardSprite;

        [Header("Animation")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverAnimDuration = 0.12f;
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchAnimDuration = 0.2f;
        [Tooltip("Delay (unscaled) between picking a card and resuming the game.")]
        [SerializeField] private float pickConfirmDelay = 0.5f;

        private Button button;
        private CardData assignedCard;
        private Action<CardData> onPicked;
        private Vector3 baseScale;
        private Coroutine scaleRoutine;
        private bool isPicked;
        private Sprite defaultCardFrameSprite;

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;

            if (descriptionText == null)
            {
                descriptionText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (cardFrameImage == null)
            {
                cardFrameImage = GetComponent<Image>();
            }

            if (cardFrameImage != null)
            {
                defaultCardFrameSprite = cardFrameImage.sprite;
            }

            button.onClick.AddListener(HandleClick);
        }

        /// <summary>Assigns a rolled card to this slot and shows its description.</summary>
        public void Show(CardData card, Action<CardData> pickedCallback)
        {
            assignedCard = card;
            onPicked = pickedCallback;
            isPicked = false;
            button.interactable = true;
            transform.localScale = baseScale;
            descriptionText.text = BuildDescription(card);

            if (cardFrameImage != null)
            {
                cardFrameImage.sprite = card.isSpecial && specialCardSprite != null ? specialCardSprite : defaultCardFrameSprite;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isPicked) return;
            SetScaleTarget(baseScale * hoverScale, hoverAnimDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isPicked) return;
            SetScaleTarget(baseScale, hoverAnimDuration);
        }

        private void HandleClick()
        {
            if (assignedCard == null || isPicked) return;

            isPicked = true;
            button.interactable = false;
            StartCoroutine(PunchThenConfirm());
        }

        private void SetScaleTarget(Vector3 targetScale, float duration)
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(AnimateScale(targetScale, duration));
        }

        private IEnumerator AnimateScale(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, duration <= 0f ? 1f : t / duration);
                yield return null;
            }

            transform.localScale = targetScale;
            scaleRoutine = null;
        }

        /// <summary>Punches the card (overshoot then settle), waits out the remainder of pickConfirmDelay, then fires onPicked.</summary>
        private IEnumerator PunchThenConfirm()
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
                scaleRoutine = null;
            }

            yield return AnimateScale(baseScale * punchScale, punchAnimDuration * 0.2f);
            yield return AnimateScale(baseScale, punchAnimDuration * 0.2f);

            float remainingDelay = pickConfirmDelay - punchAnimDuration;
            if (remainingDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(remainingDelay);
            }

            onPicked?.Invoke(assignedCard);
        }

        private static string BuildDescription(CardData card)
        {
            string primaryLine = FormatEffectLine(card.value, card.isPercentage, card.effectType);

            if (!card.isSpecial)
            {
                return $"{card.displayName}\n{primaryLine}";
            }

            string secondaryLine = FormatEffectLine(card.secondValue, card.secondIsPercentage, card.secondEffectType);
            return $"{card.displayName}\n{primaryLine}\n{secondaryLine}";
        }

        private static string FormatEffectLine(float value, bool isPercentage, CardEffectType effectType)
        {
            string sign = value >= 0f ? "+" : string.Empty;
            string amount = isPercentage ? $"{sign}{value:0.#}%" : $"{sign}{value:0.#}";
            return $"{amount} {EffectLabel(effectType)}";
        }

        private static string EffectLabel(CardEffectType effectType)
        {
            switch (effectType)
            {
                case CardEffectType.Damage: return "Damage";
                case CardEffectType.AttackFireRate: return "Attack Speed";
                case CardEffectType.ProjectileSpeed: return "Projectile Speed";
                case CardEffectType.CritChance: return "Crit Chance";
                case CardEffectType.CritDamage: return "Crit Damage";
                case CardEffectType.ExtraProjectile: return "Projectile";
                case CardEffectType.Pierce: return "Pierce";
                case CardEffectType.AttackRange: return "Attack Range";
                case CardEffectType.AreaOfEffect: return "Area of Effect";
                case CardEffectType.MaxHP: return "Max HP";
                case CardEffectType.Defense: return "Defense";
                case CardEffectType.HPRegen: return "HP Regen";
                case CardEffectType.IFrameDuration: return "Invincibility Duration";
                case CardEffectType.MoveSpeed: return "Move Speed";
                case CardEffectType.PickupRadius: return "Pickup Radius";
                case CardEffectType.XPGain: return "XP Gain";
                case CardEffectType.CoinGain: return "Coin Gain";
                case CardEffectType.InstantHeal: return "HP";
                case CardEffectType.ExplodeOnKillChance: return "Explode Chance";
                default: return effectType.ToString();
            }
        }
    }
}

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
        [Tooltip("Rarity glow/embers — shown for Rare (blue) and Epic (yellow), hidden for Common.")]
        [SerializeField] private CardRarityVFX rarityVFX;
        [Tooltip("Small badge stuck to the card corner like a seal, shown only for Exclusive cards (card.requiredChallengeTier > 0) that have an icon assigned — lets the player recognize which challenge unlocked it at a glance.")]
        [SerializeField] private Image exclusiveBadgeImage;
        [Tooltip("Rainbow rim-glow sat behind exclusiveBadgeImage, shown only for the Rainbow Nova card specifically (see RainbowNovaCardId below) — reuses the same Mat_RainbowAuraInner look as the player/boss's own Rainbow Aura VFX.")]
        [SerializeField] private Image cardIconRainbowAuraImage;

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
        private bool isHighlighted;
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
            isHighlighted = false;
            button.interactable = true;
            transform.localScale = baseScale;
            descriptionText.text = BuildDescription(card);

            if (cardFrameImage != null)
            {
                cardFrameImage.sprite = card.isSpecial && specialCardSprite != null ? specialCardSprite : defaultCardFrameSprite;
            }

            rarityVFX?.SetRarity(card.rarity);

            if (exclusiveBadgeImage != null)
            {
                bool showBadge = (card.requiredChallengeTier > 0 || card.requiresMegaMagpieKill) && card.icon != null;
                exclusiveBadgeImage.gameObject.SetActive(showBadge);

                if (showBadge)
                {
                    exclusiveBadgeImage.sprite = card.icon;
                }
            }

            if (cardIconRainbowAuraImage != null)
            {
                bool showRainbowAura = card.cardId == RainbowNovaCardId;
                cardIconRainbowAuraImage.gameObject.SetActive(showRainbowAura);

                if (showRainbowAura)
                {
                    cardIconRainbowAuraImage.sprite = card.icon;
                }
            }
        }

        /// <summary>Hardcoded on purpose — a one-off cosmetic tied to this exact secret card, not a data-driven flag on CardData like requiresMegaMagpieKill above.</summary>
        private const string RainbowNovaCardId = "rainbow_nova";

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isPicked) return;
            SetScaleTarget(baseScale * hoverScale, hoverAnimDuration);
        }

public void OnPointerExit(PointerEventData eventData)
        {
            if (isPicked) return;
            SetScaleTarget(isHighlighted ? baseScale * hoverScale : baseScale, hoverAnimDuration);
        }

/// <summary>Keyboard-navigation equivalent of a mouse hover — used by LevelUpCardsUI's A/D selection so the currently-selected card reads the same as a moused-over one.</summary>
        public void SetHighlighted(bool highlighted)
        {
            if (isPicked || highlighted == isHighlighted) return;
            isHighlighted = highlighted;
            SetScaleTarget(highlighted ? baseScale * hoverScale : baseScale, hoverAnimDuration);
        }

        /// <summary>Keyboard-navigation equivalent of a mouse click — used by LevelUpCardsUI's Space/Enter confirm.</summary>
        public void Pick()
        {
            if (!isPicked) button.onClick.Invoke();
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
            string description = $"{card.displayName}\n{FormatEffectLine(card.value, card.isPercentage, card.effectType)}";

            // Not gated by isSpecial — that flag only controls milestone-only draft pool membership
            // and the special card frame. A normal card can also carry a second effect (e.g. Vital
            // Boost's heal-on-pick alongside its Max HP bump); secondValue == 0 means "none set".
            if (card.secondValue != 0f)
            {
                description += $"\n{FormatEffectLine(card.secondValue, card.secondIsPercentage, card.secondEffectType)}";
            }

            if (card.additionalEffects != null)
            {
                foreach (CardEffect effect in card.additionalEffects)
                {
                    description += $"\n{FormatEffectLine(effect.value, effect.isPercentage, effect.effectType)}";
                }
            }

            return description;
        }

        private static string FormatEffectLine(float value, bool isPercentage, CardEffectType effectType)
        {
            string sign = value >= 0f ? "+" : string.Empty;
            string amount = isPercentage ? $"{sign}{value:0.#}%" : $"{sign}{value:0.#}";

            if (effectType == CardEffectType.InstantHeal)
            {
                return $"Recover {amount} HP";
            }

            if (effectType == CardEffectType.ProjectileSplitOnHit)
            {
                return "Projectiles fork in two on hit";
            }

            if (effectType == CardEffectType.RainbowNova)
            {
                return "Periodic rainbow nova hits every enemy";
            }

            if (effectType == CardEffectType.CupidsArrow)
            {
                return "Every 5th shot fires a piercing heart arrow that maxes Slow";
            }

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
                case CardEffectType.BlockChance: return "Chance to Block a Hit";
                case CardEffectType.StarProjectileCount: return "Star Projectile";
                case CardEffectType.PassiveProcChanceBonus: return "Passive Skill Chance";
                case CardEffectType.StarDamageBonus: return "Star Damage";
                case CardEffectType.StarRangeBonus: return "Star Range";
                case CardEffectType.StarCooldownReduction: return "Star Cooldown";
                case CardEffectType.HolyStrikeChance: return "Chance for a Holy Strike";
                case CardEffectType.StaticStrikeChance: return "Chance for a Lightning Strike";
                default: return effectType.ToString();
            }
        }
    }
}

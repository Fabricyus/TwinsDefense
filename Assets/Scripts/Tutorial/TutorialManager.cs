using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TwinsDefense.Data;
using TwinsDefense.Placement;
using TwinsDefense.Waves;
using TwinsDefense.UI;

namespace TwinsDefense.Tutorial
{
    /// <summary>
    /// Drives the placement/upgrade tutorial for Izzy, Court and Ralph in order,
    /// then triggers the phase's mid-boss wave. Every step advances on a real
    /// gameplay event (never a fixed timer).
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private enum StepKind { PlaceTower, ShowAttackHint, OpenPanel, UpgradeHint, TriggerBoss }

        [Serializable]
        private struct TutorialStep
        {
            public StepKind kind;
            [TextArea] public string message;
            public TowerCharacter character;
        }

        [Header("References")]
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private GameObject hintBanner;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private WaveData earlyWave;
        [SerializeField] private WaveData midBossWave;
        [SerializeField] private PlacementNode izzyNode;
        [SerializeField] private PlacementNode courtNode;
        [SerializeField] private PlacementNode ralphNode;

        private List<TutorialStep> steps;
        private int currentStepIndex = -1;

        private void Awake()
        {
            steps = new List<TutorialStep>
            {
                new TutorialStep { kind = StepKind.PlaceTower, character = TowerCharacter.Izzy, message = "Drag Izzy onto the glowing tile to place her on the path." },
                new TutorialStep { kind = StepKind.ShowAttackHint, character = TowerCharacter.Izzy, message = "Great! Izzy will now attack enemies in range automatically." },
                new TutorialStep { kind = StepKind.OpenPanel, character = TowerCharacter.Izzy, message = "Tap on Izzy to open her upgrade panel. Spend Gems to level her up with Stars!" },
                new TutorialStep { kind = StepKind.UpgradeHint, character = TowerCharacter.Izzy, message = "Each Star makes Izzy stronger — but costs more Gems. Choose wisely!" },

                new TutorialStep { kind = StepKind.PlaceTower, character = TowerCharacter.Court, message = "Drag Court onto the glowing tile to place her on the path." },
                new TutorialStep { kind = StepKind.ShowAttackHint, character = TowerCharacter.Court, message = "Great! Court will now attack enemies in range automatically." },
                new TutorialStep { kind = StepKind.OpenPanel, character = TowerCharacter.Court, message = "Tap on Court to open her upgrade panel. Spend Gems to level her up with Stars!" },
                new TutorialStep { kind = StepKind.UpgradeHint, character = TowerCharacter.Court, message = "Each Star makes Court stronger — but costs more Gems. Choose wisely!" },

                new TutorialStep { kind = StepKind.PlaceTower, character = TowerCharacter.Ralph, message = "Ralph doesn't attack directly, but his aura boosts everyone nearby. Try upgrading him too!" },
                new TutorialStep { kind = StepKind.ShowAttackHint, character = TowerCharacter.Ralph, message = "Great! Ralph's aura is now boosting nearby towers." },
                new TutorialStep { kind = StepKind.OpenPanel, character = TowerCharacter.Ralph, message = "Tap on Ralph to open his upgrade panel. Spend Gems to level him up with Stars!" },
                new TutorialStep { kind = StepKind.UpgradeHint, character = TowerCharacter.Ralph, message = "Each Star makes Ralph stronger — but costs more Gems. Choose wisely!" },

                new TutorialStep { kind = StepKind.TriggerBoss, message = "A boss is coming. Make sure your team is ready!" },
            };

            PlacementNode.OnTowerPlaced += HandleTowerPlaced;
            UpgradePanelController.OnPanelShown += HandlePanelShown;
            UpgradePanelController.OnPanelHidden += HandlePanelHidden;
        }

        private void OnDestroy()
        {
            PlacementNode.OnTowerPlaced -= HandleTowerPlaced;
            UpgradePanelController.OnPanelShown -= HandlePanelShown;
            UpgradePanelController.OnPanelHidden -= HandlePanelHidden;
        }

        private void Start()
        {
            AdvanceTo(0);
        }

        private void AdvanceTo(int index)
        {
            currentStepIndex = index;

            if (currentStepIndex >= steps.Count)
            {
                if (hintBanner != null) hintBanner.SetActive(false);
                return;
            }

            TutorialStep step = steps[currentStepIndex];

            if (hintBanner != null) hintBanner.SetActive(true);
            if (hintText != null) hintText.text = step.message;

            switch (step.kind)
            {
                case StepKind.PlaceTower:
                    HighlightNodeFor(step.character);
                    break;
                case StepKind.ShowAttackHint:
                    if (waveManager != null && earlyWave != null)
                    {
                        waveManager.TriggerWave(earlyWave);
                    }
                    AdvanceTo(currentStepIndex + 1);
                    break;
                case StepKind.UpgradeHint:
                    break;
                case StepKind.TriggerBoss:
                    if (waveManager != null && midBossWave != null)
                    {
                        waveManager.TriggerWave(midBossWave);
                    }
                    break;
            }
        }

        private void HighlightNodeFor(TowerCharacter character)
        {
            if (izzyNode != null) izzyNode.SetHighlighted(character == TowerCharacter.Izzy);
            if (courtNode != null) courtNode.SetHighlighted(character == TowerCharacter.Court);
            if (ralphNode != null) ralphNode.SetHighlighted(character == TowerCharacter.Ralph);
        }

        private void HandleTowerPlaced(PlacementNode node)
        {
            if (!IsCurrentStep(StepKind.PlaceTower, out TutorialStep step)) return;

            if (node.allowedCharacter == step.character)
            {
                AdvanceTo(currentStepIndex + 1);
            }
        }

        private void HandlePanelShown(GameObject towerObject)
        {
            if (!IsCurrentStep(StepKind.OpenPanel, out TutorialStep step)) return;

            Tower tower = towerObject.GetComponent<Tower>();

            if (tower != null && tower.Data != null && tower.Data.character == step.character)
            {
                AdvanceTo(currentStepIndex + 1);
            }
        }

        private void HandlePanelHidden()
        {
            if (!IsCurrentStep(StepKind.UpgradeHint, out _)) return;

            AdvanceTo(currentStepIndex + 1);
        }

        private bool IsCurrentStep(StepKind kind, out TutorialStep step)
        {
            step = default;

            if (currentStepIndex < 0 || currentStepIndex >= steps.Count) return false;

            step = steps[currentStepIndex];
            return step.kind == kind;
        }
    }
}

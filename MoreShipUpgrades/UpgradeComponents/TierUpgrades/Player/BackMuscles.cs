using GameNetcodeStuff;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades
{
    public class BackMuscles : TierUpgrade, IUpgradeWorldBuilding
    {
        internal float alteredWeight = 1f;
        internal static BackMuscles Instance;
        public const string UPGRADE_NAME = "背部肌肉";
        public const string PRICES_DEFAULT = "600,700,800";
        internal const string WORLD_BUILDING_TEXT = "\n\n公司为员工配发的液压腰带, 仅限为公司创造了高绩效和有\n能力的员工个人领取." +
            " 公司所有的员工都非常珍惜和爱护它! 由于它集神奇的保健功效和人为制造的稀缺性于一身, 所以\n被一些员工诙谐地称为'背部肌肉升级版'. 在液压腰带的上\n面有很多电话号码, 但是大部分电话号码都已失效!\n\n";

        public enum UpgradeMode
        {
            ReduceWeight,
            ReduceCarryInfluence,
            ReduceCarryStrain,
        }
        public static UpgradeMode CurrentUpgradeMode
        {
            get
            {
                return GetConfiguration().BACK_MUSCLES_UPGRADE_MODE;
            }
        }
        void Awake()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().BACK_MUSCLES_OVERRIDE_NAME;
            Instance = this;
        }
        public override void Increment()
        {
            base.Increment();
            UpdatePlayerWeight();
        }

        public override void Load()
        {
            base.Load();
            UpdatePlayerWeight();
        }
        public override void Unwind()
        {
            base.Unwind();
            UpdatePlayerWeight();
        }
        public static float DecreaseStrain(float defaultWeight)
        {
            return DecreaseValue(defaultWeight, GetConfiguration().BACK_MUSCLES_ENABLED, UpgradeMode.ReduceCarryStrain, 1f);
        }

        public static float DecreaseCarryLoss(float defaultWeight)
        {
            return DecreaseValue(defaultWeight, GetConfiguration().BACK_MUSCLES_ENABLED, UpgradeMode.ReduceCarryInfluence, 1f);
        }

        public static float DecreasePossibleWeight(float defaultWeight)
        {
            return DecreaseValue(defaultWeight, GetConfiguration().BACK_MUSCLES_ENABLED, UpgradeMode.ReduceWeight, 0f);
        }

        public static float DecreaseValue(float defaultWeight, bool enabled, UpgradeMode intendedMode, float lowerBound)
        {
            if (!enabled) return defaultWeight;
            if (CurrentUpgradeMode != intendedMode) return defaultWeight;
            if (!GetActiveUpgrade(UPGRADE_NAME)) return defaultWeight;
            LategameConfiguration config = GetConfiguration();
            return Mathf.Max(defaultWeight * (config.CARRY_WEIGHT_REDUCTION.Value - (GetUpgradeLevel(UPGRADE_NAME) * config.CARRY_WEIGHT_INCREMENT.Value)), lowerBound);
        }

        public static void UpdatePlayerWeight()
        {
            if (CurrentUpgradeMode != UpgradeMode.ReduceWeight) return;
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player.ItemSlots.Length == 0) return;

            Instance.alteredWeight = 1f;
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                GrabbableObject obj = player.ItemSlots[i];
                if (obj == null) continue;

                Instance.alteredWeight += Mathf.Clamp(DecreasePossibleWeight(obj.itemProperties.weight - 1f), 0f, 10f);
            }
            player.carryWeight = Instance.alteredWeight;
            if (player.carryWeight < 1f) { player.carryWeight = 1f; }
        }

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return string.Format(WORLD_BUILDING_TEXT, shareStatus ? "departments" : "employees");
        }

        public override string GetDisplayInfo(int initialPrice = -1, int maxLevels = -1, int[] incrementalPrices = null)
        {
            static float infoFunction(int level)
            {
                LategameConfiguration config = GetConfiguration();
                return (config.CARRY_WEIGHT_REDUCTION.Value - (level * config.CARRY_WEIGHT_INCREMENT.Value)) * 100;
            }
            string infoFormat;
            switch (CurrentUpgradeMode)
            {
                case UpgradeMode.ReduceWeight:
                    {
                        infoFormat = AssetBundleHandler.GetInfoFromJSON(UPGRADE_NAME);
                        break;
                    }
                case UpgradeMode.ReduceCarryInfluence:
                    {
                        infoFormat = "等级{0} - 价格:${1} - 将负重对员工奔跑速度的影响降低{2}%\n";
                        break;
                    }
                case UpgradeMode.ReduceCarryStrain:
                    {
                        infoFormat = "等级{0} - 价格:${1} - 将负重对玩家奔跑时的耐力消耗的影响降低{2}%\n";
                        break;
                    }
                default:
                    {
                        infoFormat = "未定义";
                        break;
                    }
            }
            return Tools.GenerateInfoForUpgrade(infoFormat, initialPrice, incrementalPrices, infoFunction);
        }
        public override bool CanInitializeOnStart
        {
            get
            {
                LategameConfiguration config = GetConfiguration();
                string[] prices = config.BACK_MUSCLES_UPGRADE_PRICES.Value.Split(',');
                return config.BACK_MUSCLES_PRICE.Value <= 0 && prices.Length == 1 && (prices[0].Length == 0 || prices[0] == "0");
            }
        }

        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<BackMuscles>(UPGRADE_NAME);
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().BACK_MUSCLES_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = GetConfiguration();

            return UpgradeBus.Instance.SetupMultiplePurchasableTerminalNode(UPGRADE_NAME,
                                                configuration.SHARED_UPGRADES.Value || !configuration.BACK_MUSCLES_INDIVIDUAL.Value,
                                                configuration.BACK_MUSCLES_ENABLED.Value,
                                                configuration.BACK_MUSCLES_PRICE.Value,
                                                UpgradeBus.ParseUpgradePrices(configuration.BACK_MUSCLES_UPGRADE_PRICES.Value),
                                                configuration.OVERRIDE_UPGRADE_NAMES ? configuration.BACK_MUSCLES_OVERRIDE_NAME : "");
        }
    }
}

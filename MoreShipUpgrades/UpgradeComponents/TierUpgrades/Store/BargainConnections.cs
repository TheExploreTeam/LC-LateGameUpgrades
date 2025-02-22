﻿using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;

namespace MoreShipUpgrades.UpgradeComponents.TierUpgrades.Store
{
    class BargainConnections : TierUpgrade, IUpgradeWorldBuilding
    {
        internal const string UPGRADE_NAME = "Bargain Connections";
        internal const string PRICES_DEFAULT = "225,300,375";
        internal const string WORLD_BUILDING_TEXT = "\n\nSubscription to 'Coupon Cutters' magazine. Every once in a while the magazine comes with coupons already cut out from it. Strange.\n\n";
        internal override void Start()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().BARGAIN_CONNECTIONS_OVERRIDE_NAME;
            base.Start();
        }
        public static int GetBargainConnectionsAdditionalItems(int defaultAmountItems)
        {
            LategameConfiguration config = GetConfiguration();
            if (!GetActiveUpgrade(UPGRADE_NAME)) return defaultAmountItems;
            return defaultAmountItems + config.BARGAIN_CONNECTIONS_INITIAL_ITEM_AMOUNT.Value + (GetUpgradeLevel(UPGRADE_NAME) * config.BARGAIN_CONNECTIONS_INCREMENTAL_ITEM_AMOUNT.Value);
        }
        public override string GetDisplayInfo(int initialPrice = -1, int maxLevels = -1, int[] incrementalPrices = null)
        {
            static float infoFunction(int level)
            {
                LategameConfiguration config = GetConfiguration();
                return config.BARGAIN_CONNECTIONS_INITIAL_ITEM_AMOUNT.Value + (level * config.BARGAIN_CONNECTIONS_INCREMENTAL_ITEM_AMOUNT.Value);
            }
            const string infoFormat = "LVL {0} - ${1} - Increases the amount of items that can be on sale by {2}\n";
            return Tools.GenerateInfoForUpgrade(infoFormat, initialPrice, incrementalPrices, infoFunction);
        }
        public override bool CanInitializeOnStart
        {
            get
            {
                LategameConfiguration config = GetConfiguration();
                string[] prices = config.BARGAIN_CONNECTIONS_PRICES.Value.Split(',');
                return config.BARGAIN_CONNECTIONS_PRICE.Value <= 0 && prices.Length == 1 && (prices[0].Length == 0 || prices[0] == "0");
            }
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().BARGAIN_CONNECTIONS_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<BargainConnections>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = GetConfiguration();

            return UpgradeBus.Instance.SetupMultiplePurchasableTerminalNode(UPGRADE_NAME,
                                                shareStatus: true,
                                                configuration.BARGAIN_CONNECTIONS_ENABLED.Value,
                                                configuration.BARGAIN_CONNECTIONS_PRICE.Value,
                                                UpgradeBus.ParseUpgradePrices(configuration.BARGAIN_CONNECTIONS_PRICES.Value),
                                                configuration.OVERRIDE_UPGRADE_NAMES ? configuration.BARGAIN_CONNECTIONS_OVERRIDE_NAME : "");
        }

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return WORLD_BUILDING_TEXT;
        }
    }
}

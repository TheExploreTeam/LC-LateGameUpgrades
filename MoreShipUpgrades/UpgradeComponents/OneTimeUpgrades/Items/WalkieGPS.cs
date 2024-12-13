using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Items
{
    class WalkieGPS : OneTimeUpgrade, IUpgradeWorldBuilding
    {
        public const string UPGRADE_NAME = "手持式GPS";
        internal const string WORLD_BUILDING_TEXT = "\n\n在同一时间，你们与另一个部门的同事在公司造船厂用5个\nTZP吸入器和2个专业手电筒交换了一份手写的手持GPS破解\n指南，用于解锁公司配发的手持GPS终端。使用他们记录的方\n法，你找到了如何让手持GPS终端打印出通常不可见的调试信\n息。其中最有用的两个读数是'基于磁场的本地时间显示'和\n'基于卫星估算的相对坐标位置'。\n\n";
        public static WalkieGPS instance;
        bool walkieUIActive;

        private GameObject canvas;
        private Text x, y, z, time;

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return WORLD_BUILDING_TEXT;
        }
        void Awake()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().WALKIE_GPS_OVERRIDE_NAME;
            instance = this;
        }
        internal override void Start()
        {
            base.Start();
            canvas = transform.GetChild(0).gameObject;
            x = canvas.transform.GetChild(0).GetComponent<Text>();
            y = canvas.transform.GetChild(1).GetComponent<Text>();
            z = canvas.transform.GetChild(2).GetComponent<Text>();
            time = canvas.transform.GetChild(3).GetComponent<Text>();
        }
        public void Update()
        {
            if (!walkieUIActive) return;

            Vector3 pos = GameNetworkManager.Instance.localPlayerController.transform.position;
            x.text = $"X坐标: {pos.x:F1}";
            y.text = $"Y坐标: {pos.y:F1}";
            z.text = $"Z坐标: {pos.z:F1}";

            int num = (int)(TimeOfDay.Instance.normalizedTimeOfDay * (60f * TimeOfDay.Instance.numberOfHours)) + 360;
            int num2 = (int)Mathf.Floor(num / 60f);
            string amPM = "上午";
            if (num2 > 12)
            {
                amPM = "下午";
            }
            if (num2 > 12)
            {
                num2 %= 12;
            }
            int num3 = num % 60;
            string text = string.Format("{0:00}:{1:00}", num2, num3).TrimStart('0') + amPM;
            time.text = text;
        }

        public void WalkieActive()
        {
            if (canvas.activeInHierarchy) return;

            walkieUIActive = true;
            canvas.SetActive(true);
        }

        public void WalkieDeactivate()
        {
            walkieUIActive = false;
            canvas.SetActive(false);
        }

        public override string GetDisplayInfo(int price = -1)
        {
            return $"价格:${price} - 当你拿着手持GPS终端时，显示你的位置坐标\n和当前时间。这在大雾这种极端天气下特别有用。";
        }
        public override bool CanInitializeOnStart => GetConfiguration().WALKIE_PRICE.Value <= 0;
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().WALKIE_GPS_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<WalkieGPS>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = GetConfiguration();

            return UpgradeBus.Instance.SetupOneTimeTerminalNode(UPGRADE_NAME,
                                    shareStatus: true,
                                    configuration.WALKIE_ENABLED.Value,
                                    configuration.WALKIE_PRICE.Value,
                                    configuration.OVERRIDE_UPGRADE_NAMES ? configuration.WALKIE_GPS_OVERRIDE_NAME : "");
        }
    }
}

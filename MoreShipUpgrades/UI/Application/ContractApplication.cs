using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Screen;
using MoreShipUpgrades.Compat;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Util;
using System;
using System.Linq;

namespace MoreShipUpgrades.UI.Application
{
    internal class ContractApplication : InteractiveTerminalApplication
    {
        const string MAIN_MENU_TITLE = "合同 (Contract)";
        const string CONTRACT_INFO_CURSOR_ELEMENT = "合同列表";
        const string PICK_CONTRACT_CURSOR_ELEMENT = "签署合同";
        const string CURRENT_CONTRACT_CURSOR_ELEMENT = "已签署的合同";

        const string CONTRACT_MAIN_PAGE_PROMPT = "请选择你想要进行的操作";
        const string CONTRACT_LIST_PAGE_PROMPT = "请选择你想要了解的合同列表";
        const string CONTRACT_SIGN_PAGE_PROMPT = "请选择你想要签署的合同类型";

        const string CONTRACT_PAGE_OPTION_PROMPT = "[W/S] 上下移动选项 [Enter] 确认选项";

        const string RANDOM_MOON_CURSOR_ELEMENT = "随机星球";
        const string SPECIFIED_MOON_CURSOR_ELEMENT = "指定星球";
        static readonly string[] MAIN_MENU_CURSOR_ELEMENTS = [CONTRACT_INFO_CURSOR_ELEMENT, PICK_CONTRACT_CURSOR_ELEMENT, CURRENT_CONTRACT_CURSOR_ELEMENT];

        IScreen mainScreen;
        CursorMenu mainCursorMenu;
        public override void Initialization()
        {
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements = new CursorElement[MAIN_MENU_CURSOR_ELEMENTS.Length];
            cursorElements[0] = CursorElement.Create(
                    name: MAIN_MENU_CURSOR_ELEMENTS[0],
                    description: "",
                    action: ShowContractInformation,
                    active: (_) => true,
                    selectInactive: false
                );
            cursorElements[1] = CursorElement.Create(
                    name: MAIN_MENU_CURSOR_ELEMENTS[1],
                    description: "",
                    action: PickContract,
                    active: (_) => ContractManager.Instance.contractType == "None",
                    selectInactive: true
                );
            cursorElements[2] = CursorElement.Create(
                    name: MAIN_MENU_CURSOR_ELEMENTS[2],
                    description: "",
                    action: ShowCurrentContract,
                    active: (_) => ContractManager.Instance.contractType != "None",
                    selectInactive: true
                );
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create(CONTRACT_MAIN_PAGE_PROMPT + "\n" + CONTRACT_PAGE_OPTION_PROMPT ),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: MAIN_MENU_TITLE, elements: textElements);
            currentCursorMenu = menu;
            currentScreen = screen;
            mainScreen = screen;
            mainCursorMenu = menu;
        }

        void ShowContractInformation()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements = new CursorElement[CommandParser.contractInfos.Count + 1];
            for (int i = 0; i < CommandParser.contracts.Count; i++)
            {
                int counter = i;
                cursorElements[i] = CursorElement.Create(
                    name: CommandParser.contracts[counter],
                    description: string.Empty,
                    action: () => ShowContractInformation(CommandParser.contracts[counter], CommandParser.contractInfos[counter]),
                    active: (_) => true,
                    selectInactive: false
                    );
            }
            cursorElements[CommandParser.contractInfos.Count] = CursorElement.Create(
                    name: "返回",
                    description: string.Empty,
                    action: () => SwitchScreen(previousScreen, previousCursorMenu, true),
                    active: (_) => true,
                    selectInactive: true
                    );
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create(CONTRACT_LIST_PAGE_PROMPT + "\n" + CONTRACT_PAGE_OPTION_PROMPT),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: MAIN_MENU_TITLE, elements: textElements);
            SwitchScreen(screen, menu, previous: true);
        }

        void ShowContractInformation(string contractType, string information)
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements =
            [
                CursorElement.Create(
                        name: "返回",
                        description: string.Empty,
                        action: () => SwitchScreen(previousScreen, previousCursorMenu, true),
                        active: (_) => true,
                        selectInactive: true
                        ),
            ];
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create(information),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: contractType, elements: textElements);
            SwitchScreen(screen, menu, previous: true);
        }


        void PickContract()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            if (ContractManager.Instance.contractType != "None")
            {
                ErrorMessage(MAIN_MENU_TITLE, () => SwitchScreen(previousScreen, previousCursorMenu, true), "你目前有一个正在进行的合同需要完成.");
                return;
            }
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements =
            [
                CursorElement.Create(
                        name: RANDOM_MOON_CURSOR_ELEMENT,
                        description: string.Empty,
                        action: ConfirmRandomMoonContract,
                        active: (_) => terminal.groupCredits >= UpgradeBus.Instance.PluginConfiguration.CONTRACT_PRICE,
                        selectInactive: true
                        ),
                CursorElement.Create(
                        name: SPECIFIED_MOON_CURSOR_ELEMENT,
                        description: string.Empty,
                        action: PickSpecifiedMoonContract,
                        active: (_) => terminal.groupCredits >= UpgradeBus.Instance.PluginConfiguration.CONTRACT_SPECIFY_PRICE,
                        selectInactive: true
                        ),
                CursorElement.Create(
                        name: "返回",
                        description: string.Empty,
                        action: () => SwitchScreen(previousScreen, previousCursorMenu, true),
                        active: (_) => true,
                        selectInactive: true
                        ),
            ];
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create(CONTRACT_SIGN_PAGE_PROMPT + "\n" + CONTRACT_PAGE_OPTION_PROMPT),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: MAIN_MENU_TITLE, elements: textElements);
            SwitchScreen(screen, menu, previous: true);
        }
        void PickSpecifiedMoonContract()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            if (terminal.groupCredits < UpgradeBus.Instance.PluginConfiguration.CONTRACT_SPECIFY_PRICE)
            {
                ErrorMessage(MAIN_MENU_TITLE, () => SwitchScreen(previousScreen, previousCursorMenu, true), "你没有足够的金钱来签署指定星球合同.");
                return;
            }
            SelectableLevel[] levels = StartOfRound.Instance.levels.Where(x => !x.PlanetName.Contains("Gordion")).ToArray();
            if (LethalLevelLoaderCompat.Enabled)
            {
                LethalLevelLoaderCompat.GrabAllAvailableLevels(ref levels);
            }
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements = new CursorElement[levels.Length + 1];
            for (int i = 0; i < levels.Length; i++)
            {
                int counter = i;
                cursorElements[i] = CursorElement.Create(
                    name: levels[counter].PlanetName,
                    description: string.Empty,
                    action: () => ConfirmSpecifiedMoonContract(levels[counter]),
                    active: (_) => true,
                    selectInactive: false
                    );
            }
            cursorElements[levels.Length] = CursorElement.Create(
                    name: "返回",
                    description: string.Empty,
                    action: () => SwitchScreen(previousScreen, previousCursorMenu, true),
                    active: (_) => true,
                    selectInactive: true
                    );
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create("请选择你想要签署的星球合同."),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: MAIN_MENU_TITLE, elements: textElements);
            SwitchScreen(screen, menu, previous: true);

        }
        void ConfirmSpecifiedMoonContract(SelectableLevel level)
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            Confirm(MAIN_MENU_TITLE, $"你是否想要以${UpgradeBus.Instance.PluginConfiguration.CONTRACT_SPECIFY_PRICE.Value}的价格签署{level.PlanetName}星球合同?", () => PurchaseSpecifiedMoonContract(level, () => SwitchScreen(mainScreen, mainCursorMenu, true)), () => SwitchScreen(previousScreen, previousCursorMenu, true));
        }

        void PurchaseSpecifiedMoonContract(SelectableLevel level, Action backAction)
        {
            terminal.BuyItemsServerRpc([], terminal.groupCredits - UpgradeBus.Instance.PluginConfiguration.CONTRACT_SPECIFY_PRICE.Value, terminal.numberOfItemsInDropship);
            int i = UnityEngine.Random.Range(0, CommandParser.contracts.Count);
            if (CommandParser.contracts.Count > 1)
            {
                while (i == ContractManager.Instance.lastContractIndex)
                {
                    i = UnityEngine.Random.Range(0, CommandParser.contracts.Count);
                }
            }
            ContractManager.Instance.contractType = CommandParser.contracts[i];
            ContractManager.Instance.contractLevel = level.PlanetName;

            if (terminal.IsHost || terminal.IsServer) ContractManager.Instance.SyncContractDetailsClientRpc(ContractManager.Instance.contractLevel, i);
            else ContractManager.Instance.ReqSyncContractDetailsServerRpc(ContractManager.Instance.contractLevel, i);
            ErrorMessage(MAIN_MENU_TITLE, backAction, $"星球{ContractManager.Instance.contractLevel}的合同{ContractManager.Instance.contractType}上被签署! {CommandParser.contractInfos[i]}");
        }

        void ConfirmRandomMoonContract()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            if (terminal.groupCredits < UpgradeBus.Instance.PluginConfiguration.CONTRACT_PRICE)
            {
                ErrorMessage(MAIN_MENU_TITLE, () => SwitchScreen(previousScreen, previousCursorMenu, true), "你没有足够的金钱以签署随机星球合同!");
                return;
            }
            Confirm(MAIN_MENU_TITLE, $"你是否想要以${UpgradeBus.Instance.PluginConfiguration.CONTRACT_PRICE.Value}的价格来签署随机星球合同?", () => PurchaseRandomMoonContract(() => SwitchScreen(mainScreen, mainCursorMenu, true)), () => SwitchScreen(previousScreen, previousCursorMenu, true));
        }

        void PurchaseRandomMoonContract(Action backAction)
        {
            terminal.BuyItemsServerRpc([], terminal.groupCredits - UpgradeBus.Instance.PluginConfiguration.CONTRACT_PRICE.Value, terminal.numberOfItemsInDropship);
            int i = UnityEngine.Random.Range(0, CommandParser.contracts.Count);
            if (CommandParser.contracts.Count > 1)
            {
                while (i == ContractManager.Instance.lastContractIndex)
                {
                    i = UnityEngine.Random.Range(0, CommandParser.contracts.Count);
                }
            }
            ContractManager.Instance.contractType = CommandParser.contracts[i];
            ContractManager.RandomLevel();
            if (terminal.IsHost || terminal.IsServer) ContractManager.Instance.SyncContractDetailsClientRpc(ContractManager.Instance.contractLevel, i);
            else ContractManager.Instance.ReqSyncContractDetailsServerRpc(ContractManager.Instance.contractLevel, i);
            ErrorMessage(MAIN_MENU_TITLE, backAction, $"星球{ContractManager.Instance.contractLevel}的{ContractManager.Instance.contractType}合同已被签署! {CommandParser.contractInfos[i]}");
        }

        void ShowCurrentContract()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            if (ContractManager.Instance.contractType == "None")
            {
                ErrorMessage(MAIN_MENU_TITLE, () => SwitchScreen(previousScreen, previousCursorMenu, true), "你目前没有已签署的合同!");
                return;
            }
            IScreen screen;
            ITextElement[] textElements;
            CursorMenu menu;
            CursorElement[] cursorElements =
            [
                CursorElement.Create(
                        name: "取消当前合同",
                        description: string.Empty,
                        action: ConfirmCancelContract,
                        active: (_) => true,
                        selectInactive: true
                        ),
                CursorElement.Create(
                        name: "返回",
                        description: string.Empty,
                        action: () => SwitchScreen(previousScreen, previousCursorMenu, true),
                        active: (_) => true,
                        selectInactive: true
                        ),
            ];
            menu = CursorMenu.Create(startingCursorIndex: 0, elements: cursorElements);
            textElements =
                [
                    TextElement.Create(CommandParser.contractInfos[CommandParser.contracts.IndexOf(ContractManager.Instance.contractType)]),
                    //TextElement.Create(" "),
                    menu,
                ];
            screen = BoxedScreen.Create(title: ContractManager.Instance.contractType + " - " + ContractManager.Instance.contractLevel, elements: textElements);
            SwitchScreen(screen, menu, previous: true);
        }

        void ConfirmCancelContract()
        {
            IScreen previousScreen = currentScreen;
            CursorMenu previousCursorMenu = currentCursorMenu;
            Confirm(MAIN_MENU_TITLE, "你想要取消当前的合同吗?", () => CancelContract(() => SwitchScreen(mainScreen, mainCursorMenu, true)), () => SwitchScreen(previousScreen, previousCursorMenu, true));
        }

        void CancelContract(Action backAction)
        {
            if (terminal.IsHost || terminal.IsServer) ContractManager.Instance.SyncContractDetailsClientRpc("None", -1);
            else ContractManager.Instance.ReqSyncContractDetailsServerRpc("None", -1);
            ErrorMessage(MAIN_MENU_TITLE, backAction, LguConstants.CONTRACT_CANCEL_CONFIRM_PROMPT_SUCCESS);
        }
    }
}

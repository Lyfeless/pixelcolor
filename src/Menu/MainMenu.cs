namespace PixelColor;

class MainMenu : Scene {
    Dictionary<string, MenuList> menus;

    #region Specific Entry References
    NumberListEntry nearColorEntry = new("Near Color Blending", 0, 255, null);
    TextInputListEntry addressEntry = new("Address", 15, null);
    NumberListEntry portEntry = new("Port", 0, 65535, null);
    TextInputListEntry usernameEntry = new("Username", 15, null);

    #endregion

    string activeMenu;
    bool inSingleplayer;


    public MainMenu(string startMenu = "main") {
        menus = [];

        #region Menu Instantiations

        menus.Add("main", new(
            0.4f,
            [
                new TextListEntry("Singleplayer", actionSelectSingleplayer),
                new TextListEntry("Host Multiplayer", actionSelectHost),
                new TextListEntry("Join Multiplayer", actionSelectJoin),
                new TextListEntry("Quit", actionQuit),
            ],
            false
        ) {
            title = "Coloring Game With No Name"
        });

        DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/" + Game.contentPath);
        FileInfo[] levelNames = dir.GetFiles("*.png");
        IEnumerable<MenuListEntry> LevelEntries = levelNames.Select(e => new LevelListEntry(Path.GetFileNameWithoutExtension(e.Name), actionLevelSelect));

        menus.Add("levels", new(
            0.6f,
            LevelEntries.Prepend(new TextListEntry("Back", actionLevelListBack)).ToArray(),
            true
        ) {
            title = "Level Select"
        });

        menus.Add("newLevel", new(
            0.5f,
            [
                new TextListEntry("Back", actionNewLevelBack),
                nearColorEntry,
                new TextListEntry("Start", actionStartLevel),
            ],
            true
        ));
        // Title is set in menu change code based on selected level

        menus.Add("resumeLevel", new(
            0.2f,
            [
                new TextListEntry("Back", actionResumeLevelBack),
                new TextListEntry("Clear Save", actionResetLevel),
                new TextListEntry("Resume", actionStartLevel),
            ],
            true
        ));
        // Title is set in menu change code based on selected level

        menus.Add("resetConfirm", new(
            0.06f,
            [
                new TextListEntry("No", actionResetNo),
                new TextListEntry("Yes", actionResetYes),
            ],
            true
        ));
        // Title is set in menu change code based on selected level

        menus.Add("host", new(
            0.6f,
            [
                new TextListEntry("Back", actionHostBack),
                portEntry,
                usernameEntry,
                new TextListEntry("Host", actionHostStart)
            ],
            true
        ) {
            title = "Host a Multiplayer Game"
        });

        menus.Add("hostCancel", new(
            0.06f,
            [
                new TextListEntry("No", actionHostCancelNo),
                new TextListEntry("Yes", actionHostCancelYes),
            ],
            true
        ) {
            title = "Cancel the Current Server?"
        });

        menus.Add("join", new(
            0.6f,
            [
                new TextListEntry("Back", actionJoinBack),
                addressEntry,
                portEntry,
                usernameEntry,
                new TextListEntry("Join", actionJoinStart)
            ],
            false
        ) {
            title = "Join a Multiplayer Game"
        });

        menus.Add("joinWait", new(
            0.15f,
            [
                new TextListEntry("Cancel", actionJoinCancel)
            ],
            false
        ) {
            title = "Waiting for connection..."
        });

        menus.Add("joinUsernameFail", new(
            0.6f,
            [
                new TextListEntry("Back", actionJoinUsernameCancel),
                usernameEntry,
                new TextListEntry("Retry", actionUsernameRetry)
            ],
            false
        ) {
            title = "Username is already in use"
        });

        menus.Add("joinSucceed", new(
            0.1f,
            [
                new TextListEntry("Back", actionJoinWaitCancel),
            ],
            false
        ) {
            title = "Join Suceeded, Waiting for Host"
        });

        menus.Add("joinDisconnect", new(
            0.1f,
            [
                new TextListEntry("Back", actionJoinDisconnectBack),
            ],
            false
        ) {
            title = "Disconnected"
        });

        #endregion

        activeMenu = "";

        selectMenu(startMenu);
    }

    #region Management Functions
    public void update() {
        menus[activeMenu]?.update();
    }

    public void draw() {
        menus[activeMenu]?.draw();
    }

    public void exit() { }

    void selectMenu(string menu) {
        menus.GetValueOrDefault(activeMenu)?.setActive(false);
        menus[menu].setActive(true);
        activeMenu = menu;
    }
    #endregion

    #region Actions

    void actionSelectSingleplayer() {
        inSingleplayer = true;
        selectMenu("levels");
    }

    void actionSelectHost() {
        inSingleplayer = false;
        selectMenu("host");
    }

    void actionSelectJoin() {
        inSingleplayer = false;
        selectMenu("join");
    }

    void actionQuit() {
        Game.exit();
    }

    void actionLevelListBack() {
        selectMenu(inSingleplayer ? "main" : "hostCancel");
    }

    void actionLevelSelect() {
        string targetMenu = getSelectedLevel().inProgress ? "resumeLevel" : "newLevel";

        menus[targetMenu].title = getSelectedLevel().fileName;
        selectMenu(targetMenu);
    }

    void actionNewLevelBack() {
        selectMenu("levels");
    }

    void actionResumeLevelBack() {
        selectMenu("levels");
    }

    void actionResetLevel() {
        menus["resetConfirm"].title = $"Reset {getSelectedLevel().fileName}";
        selectMenu("resetConfirm");
    }

    void actionResetNo() {
        selectMenu("resumeLevel");
    }

    void actionResetYes() {
        LevelListEntry currentLevel = getSelectedLevel();

        File.Delete(Game.savePath + currentLevel.fileName + Game.saveExtension);
        currentLevel.clearProgress();

        selectMenu("newLevel");
    }

    void actionStartLevel() {
        Game.switchSceneLevel(getSelectedLevel().fileName, nearColorEntry.value);
    }

    void actionHostBack() {
        selectMenu("main");
    }

    void actionHostStart() {
        Game.server = new NetServer(portEntry.value);
        Game.client = new NetClient("localhost", portEntry.value, usernameEntry.inputText);

        selectMenu("levels");
    }

    void actionHostCancelNo() {
        selectMenu("levels");
    }

    void actionHostCancelYes() {
        Game.server?.shutdown();
        selectMenu("host");
    }

    void actionJoinBack() {
        selectMenu("main");
    }

    void actionJoinStart() {
        Game.client = new NetClient(addressEntry.inputText, portEntry.value, usernameEntry.inputText);
        selectMenu("joinWait");
    }

    void actionJoinCancel() {
        Game.client?.disconnect();
        selectMenu("join");
    }

    void actionJoinWaitCancel() {
        Game.client?.disconnect();
        selectMenu("join");
    }

    void actionJoinUsernameCancel() {
        Game.client?.disconnect();
        selectMenu("join");
    }

    void actionUsernameRetry() {
        Game.client.sendUsername(usernameEntry.inputText);
        selectMenu("joinWait");
    }

    void actionJoinDisconnectBack() {
        selectMenu("join");
    }

    #endregion

    #region Action Helper Functions

    LevelListEntry getSelectedLevel() {
        return (LevelListEntry)menus["levels"].getActiveEntry();
    }

    public void clientJoinSucceed() {
        if (activeMenu == "joinWait" || activeMenu == "join") {
            selectMenu("joinSucceed");
        }
    }

    public void clientUsernameFail() {
        if (activeMenu == "joinWait" || activeMenu == "join") {
            selectMenu("joinUsernameFail");
        }
    }

    public void clientDisconnect() {
        Game.client?.disconnect();

        if (activeMenu == "joinSucceed" || activeMenu == "joinUsernameFail" || activeMenu == "join") {
            selectMenu("joinDisconnect");
        }
    }

    #endregion
}
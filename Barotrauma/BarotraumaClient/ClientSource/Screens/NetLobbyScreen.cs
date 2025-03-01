﻿using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Barotrauma.IO;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
    partial class NetLobbyScreen : Screen
    {
        //private readonly List<Sprite> jobPreferenceSprites = new List<Sprite>();

        private readonly GUIFrame infoFrame, modeFrame;
        private readonly GUILayoutGroup infoFrameContent;
        private readonly GUIFrame myCharacterFrame;

        private readonly GUIListBox subList, modeList;

        private readonly GUIListBox chatBox, playerList;
        private readonly GUIButton serverLogReverseButton;
        private readonly GUIListBox serverLogBox, serverLogFilterTicks;

        private GUIComponent jobVariantTooltip;

        private SubmarinePreview submarinePreview;

        private readonly GUITextBox chatInput;
        private readonly GUITextBox serverLogFilter;
        public GUITextBox ChatInput
        {
            get
            {
                return chatInput;
            }
        }

        private readonly GUIImage micIcon;

        private readonly GUIScrollBar levelDifficultyScrollBar;

        private readonly GUITickBox radiationEnabledTickBox;

        private readonly GUIButton[] maxMissionCountButtons;
        private readonly GUITextBlock maxMissionCountText;
        private readonly GUITextBlock maxMissionCountDescription;

        private readonly GUIButton[] traitorProbabilityButtons;
        private readonly GUITextBlock traitorProbabilityText;

        private readonly GUIButton[] botCountButtons;
        private readonly GUITextBlock botCountText;

        private readonly GUIButton[] botSpawnModeButtons;
        private readonly GUITextBlock botSpawnModeText;

        public readonly GUIFrame MissionTypeFrame;
        public readonly GUIFrame CampaignSetupFrame;
        public readonly GUIFrame CampaignFrame;
        public readonly GUIButton ContinueCampaignButton, QuitCampaignButton;

        private readonly GUITickBox[] missionTypeTickBoxes;
        private readonly GUIListBox missionTypeList;

        public GUITextBox SeedBox
        {
            get; private set;
        }

        private readonly GUIComponent gameModeContainer;
        private readonly GUIButton spectateButton;
        private readonly GUILayoutGroup roundControlsHolder;
        public GUIButton SettingsButton { get; private set; }
        public static GUIButton JobInfoFrame;

        private readonly GUITickBox spectateBox;

        private readonly GUIFrame playerInfoContainer;

        private GUILayoutGroup infoContainer;
        private GUIComponent changesPendingText;
        private bool createPendingChangesText = true;
        public GUIButton PlayerFrame;

        private readonly GUIComponent subPreviewContainer;

        private readonly GUITickBox autoRestartBox;
        private readonly GUITextBlock autoRestartText;

        private readonly GUIDropDown shuttleList;
        private readonly GUITickBox shuttleTickBox;

        private readonly GUIComponent settingsBlocker;

        private Sprite backgroundSprite;

        private GUIButton jobPreferencesButton;
        private GUIButton appearanceButton;

        private GUIFrame characterInfoFrame;
        private GUIFrame appearanceFrame;

        public CharacterInfo.AppearanceCustomizationMenu CharacterAppearanceCustomizationMenu;
        public GUIFrame JobSelectionFrame;

        public GUIFrame JobPreferenceContainer;
        public GUIListBox JobList;

        private float autoRestartTimer;

        //persistent characterinfo provided by the server
        //(character settings cannot be edited when this is set)
        private CharacterInfo campaignCharacterInfo;
        public bool CampaignCharacterDiscarded
        {
            get;
            private set;
        }

        //elements that can only be used by the host
        private readonly List<GUIComponent> clientDisabledElements = new List<GUIComponent>();
        //elements that can't be interacted with but don't look disabled
        private readonly List<GUITextBox> clientReadonlyElements = new List<GUITextBox>();
        //elements that aren't shown client-side
        private readonly List<GUIComponent> clientHiddenElements = new List<GUIComponent>();

        public GUIComponent FileTransferFrame { get; private set; }
        public GUITextBlock FileTransferTitle { get; private set; }
        public GUIProgressBar FileTransferProgressBar { get; private set; }
        public GUITextBlock FileTransferProgressText { get; private set; }

        public GUITextBox ServerName
        {
            get;
            private set;
        }

        public GUITickBox Favorite
        {
            get;
            private set;
        }

        public GUITextBox ServerMessage
        {
            get;
            private set;
        }

        public GUILayoutGroup LogButtons
        {
            get;
            private set;
        }

        private readonly GUIButton showChatButton;
        private readonly GUIButton showLogButton;

        private readonly GUITextBlock publicOrPrivate;

        public GUIListBox SubList
        {
            get { return subList; }
        }

        public GUIDropDown ShuttleList
        {
            get { return shuttleList; }
        }

        public GUIListBox ModeList
        {
            get { return modeList; }
        }

        private int selectedModeIndex;
        public int SelectedModeIndex
        {
            get { return selectedModeIndex; }
            set
            {
                if (HighlightedModeIndex == selectedModeIndex)
                {
                    modeList.Select(value);
                }
                selectedModeIndex = value;
            }
        }

        public int HighlightedModeIndex
        {
            get { return modeList.SelectedIndex; }
            set
            {
                modeList.Select(value, true);
            }
        }

        public GUIListBox PlayerList
        {
            get { return playerList; }
        }

        public GUITextBox CharacterNameBox
        {
            get;
            private set;
        }

        public GUIListBox TeamPreferenceListBox
        {
            get;
            private set;
        }

        public GUIButton StartButton
        {
            get;
            private set;
        }

        public GUITickBox ReadyToStartBox
        {
            get;
            private set;
        }

        public SubmarineInfo SelectedSub
        {
            get { return subList.SelectedData as SubmarineInfo; }
            set { subList.Select(value); }
        }

        public SubmarineInfo SelectedShuttle
        {
            get { return shuttleList.SelectedData as SubmarineInfo; }
        }

        public MultiPlayerCampaignSetupUI CampaignSetupUI;
        public List<SubmarineInfo> CampaignSubmarines = new List<SubmarineInfo>();

        // Passed onto the gamesession when created
        public List<SubmarineInfo> ServerOwnedSubmarines = new List<SubmarineInfo>();

        public bool UsingShuttle
        {
            get { return shuttleTickBox.Selected; }
            set { shuttleTickBox.Selected = value; }
        }

        public GameModePreset SelectedMode
        {
            get { return modeList.SelectedData as GameModePreset; }
        }

        public MissionType MissionType
        {
            get
            {
                MissionType retVal = MissionType.None;
                int index = 0;
                foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
                {
                    if (type == MissionType.None || type == MissionType.All) { continue; }

                    if (missionTypeTickBoxes[index].Selected)
                    {
                        retVal = (MissionType)((int)retVal | (int)type);
                    }

                    index++;
                }

                return retVal;
            }
            set
            {
                int index = 0;
                foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
                {
                    if (type == MissionType.None || type == MissionType.All) { continue; }
                    missionTypeTickBoxes[index].Selected = ((int)type & (int)value) != 0;
                    index++;
                }
            }
        }

        public List<Pair<JobPrefab, int>> JobPreferences
        {
            get
            {
                // JobList if the server has already assigned the player a job
                // (e.g. the player has a pre-existing campaign character)
                if (JobList?.Content == null)
                {
                    return new List<Pair<JobPrefab, int>>();
                }

                List<Pair<JobPrefab, int>> jobPreferences = new List<Pair<JobPrefab, int>>();
                foreach (GUIComponent child in JobList.Content.Children)
                {
                    if (!(child.UserData is Pair<JobPrefab, int> jobPrefab)) { continue; }
                    jobPreferences.Add(jobPrefab);
                }
                return jobPreferences;
            }
        }

        public string LevelSeed
        {
            get
            {
                return levelSeed;
            }
            set
            {
                if (levelSeed == value) return;

                levelSeed = value;

                int intSeed = ToolBox.StringToInt(levelSeed);
                backgroundSprite = LocationType.Random(new MTRandom(intSeed))?.GetPortrait(intSeed);
                SeedBox.Text = levelSeed;
            }
        }

        public NetLobbyScreen()
        {
            float panelSpacing = 0.005f;
            var innerFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), Frame.RectTransform, Anchor.Center) { MaxSize = new Point(int.MaxValue, GameMain.GraphicsHeight - 50) }, isHorizontal: false)
            {
                Stretch = true,
                RelativeSpacing = panelSpacing
            };

            var panelContainer = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 1.0f), innerFrame.RectTransform, Anchor.Center), isHorizontal: true)
            {
                Stretch = true,
                RelativeSpacing = panelSpacing
            };

            GUILayoutGroup panelHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 1.0f), panelContainer.RectTransform))
            {
                Stretch = true,
                RelativeSpacing = panelSpacing
            };

            GUILayoutGroup bottomBar = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.05f), innerFrame.RectTransform), childAnchor: Anchor.CenterLeft)
            {
                Stretch = true,
                IsHorizontal = true,
                RelativeSpacing = panelSpacing
            };
            GUILayoutGroup bottomBarLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1.0f), bottomBar.RectTransform), childAnchor: Anchor.CenterLeft)
            {
                Stretch = true,
                IsHorizontal = true,
                RelativeSpacing = panelSpacing
            };
            GUILayoutGroup bottomBarMid = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1.0f), bottomBar.RectTransform), childAnchor: Anchor.CenterLeft)
            {
                Stretch = true,
                IsHorizontal = true,
                RelativeSpacing = panelSpacing
            };
            GUILayoutGroup bottomBarRight = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1.0f), bottomBar.RectTransform), childAnchor: Anchor.CenterLeft)
            {
                Stretch = true,
                IsHorizontal = true,
                RelativeSpacing = panelSpacing
            };

            //server info panel ------------------------------------------------------------

            infoFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.5f), panelHolder.RectTransform));
            infoFrameContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), infoFrame.RectTransform, Anchor.Center))
            {
                Stretch = true,
                RelativeSpacing = 0.025f
            };

            //server game panel ------------------------------------------------------------

            modeFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.5f), panelHolder.RectTransform))
            {
                CanBeFocused = false
            };

            gameModeContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), modeFrame.RectTransform, Anchor.Center))
            {
                RelativeSpacing = panelSpacing * 4.0f,
                Stretch = true
            };

            var disconnectButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1.0f), bottomBarLeft.RectTransform), TextManager.Get("disconnect"))
            {
                OnClicked = (bt, userdata) => { GameMain.QuitToMainMenu(save: false, showVerificationPrompt: true); return true; }
            };
            disconnectButton.TextBlock.AutoScaleHorizontal = true;

            // file transfers  ------------------------------------------------------------
            FileTransferFrame = new GUIFrame(new RectTransform(Vector2.One, bottomBarLeft.RectTransform), style: "TextFrame");
            var fileTransferContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), FileTransferFrame.RectTransform, Anchor.Center))
            {
                Stretch = true,
                RelativeSpacing = 0.05f
            };
            FileTransferTitle = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.5f), fileTransferContent.RectTransform), "", font: GUI.SmallFont);
            var fileTransferBottom = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.5f), fileTransferContent.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft)
            {
                Stretch = true
            };
            FileTransferProgressBar = new GUIProgressBar(new RectTransform(new Vector2(0.6f, 1.0f), fileTransferBottom.RectTransform), 0.0f, Color.DarkGreen);
            FileTransferProgressText = new GUITextBlock(new RectTransform(Vector2.One, FileTransferProgressBar.RectTransform), "",
                font: GUI.SmallFont, textAlignment: Alignment.CenterLeft);
            new GUIButton(new RectTransform(new Vector2(0.4f, 1.0f), fileTransferBottom.RectTransform), TextManager.Get("cancel"), style: "GUIButtonSmall")
            {
                OnClicked = (btn, userdata) =>
                {
                    if (!(FileTransferFrame.UserData is FileReceiver.FileTransferIn transfer)) { return false; }
                    GameMain.Client?.CancelFileTransfer(transfer);
                    GameMain.Client?.FileReceiver.StopTransfer(transfer);
                    return true;
                }
            };

            // Sidebar area (Character customization/Chat)

            GUILayoutGroup sideBar = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1.0f), panelContainer.RectTransform, maxSize: new Point(650, panelContainer.RectTransform.Rect.Height)))
            {
                RelativeSpacing = panelSpacing,
                Stretch = true
            };

            //player info panel ------------------------------------------------------------

            myCharacterFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.5f), sideBar.RectTransform));
            playerInfoContainer = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), myCharacterFrame.RectTransform, Anchor.Center), style: null);

            spectateBox = new GUITickBox(new RectTransform(new Vector2(0.4f, 0.06f), myCharacterFrame.RectTransform) { RelativeOffset = new Vector2(0.05f, 0.05f) },
                TextManager.Get("spectatebutton"))
            {
                Selected = false,
                OnSelected = ToggleSpectate,
                UserData = "spectate"
            };

            // Social area

            GUIFrame logBackground = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.5f), sideBar.RectTransform));
            GUILayoutGroup logHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), logBackground.RectTransform, Anchor.Center))
            {
                Stretch = true
            };

            GUILayoutGroup socialHolder = null; GUILayoutGroup serverLogHolder = null;

            LogButtons = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.05f), logHolder.RectTransform), true)
            {
                Stretch = true,
                RelativeSpacing = 0.02f
            };

            clientHiddenElements.Add(LogButtons);

            // Show chat button
            showChatButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1.25f), LogButtons.RectTransform),
               TextManager.Get("Chat"), style: "GUITabButton")
            {
                Selected = true,
                OnClicked = (GUIButton button, object userData) =>
                {
                    if (socialHolder != null) { socialHolder.Visible = true; }
                    if (serverLogHolder != null) { serverLogHolder.Visible = false; }
                    showChatButton.Selected = true;
                    showLogButton.Selected = false;
                    return true;
                }
            };

            // Server log button
            showLogButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1.25f), LogButtons.RectTransform),
                TextManager.Get("ServerLog"), style: "GUITabButton")
            {
                OnClicked = (GUIButton button, object userData) =>
                {
                    if (socialHolder != null) { socialHolder.Visible = false; }
                    if (!(serverLogHolder?.Visible ?? true))
                    {
                        if (GameMain.Client?.ServerSettings?.ServerLog == null) { return false; }
                        serverLogHolder.Visible = true;
                        GameMain.Client.ServerSettings.ServerLog.AssignLogFrame(serverLogReverseButton, serverLogBox, serverLogFilterTicks.Content, serverLogFilter);
                    }
                    showChatButton.Selected = false;
                    showLogButton.Selected = true;
                    return true;
                }
            };

            GUITextBlock.AutoScaleAndNormalize(showChatButton.TextBlock, showLogButton.TextBlock);

            GUIFrame logHolderBottom = new GUIFrame(new RectTransform(Vector2.One, logHolder.RectTransform), style: null)
            {
                CanBeFocused = false
            };

            socialHolder = new GUILayoutGroup(new RectTransform(Vector2.One, logHolderBottom.RectTransform, Anchor.Center))
            {
                Stretch = true
            };

            // Spacing
            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.02f), socialHolder.RectTransform), style: null)
            {
                CanBeFocused = false
            };

            GUILayoutGroup socialHolderHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.9f), socialHolder.RectTransform), isHorizontal: true)
            {
                Stretch = true
            };

            //chatbox ----------------------------------------------------------------------

            chatBox = new GUIListBox(new RectTransform(new Vector2(0.6f, 1.0f), socialHolderHorizontal.RectTransform));

            //player list ------------------------------------------------------------------

            playerList = new GUIListBox(new RectTransform(new Vector2(0.4f, 1.0f), socialHolderHorizontal.RectTransform))
            {
                OnSelected = (component, userdata) => { SelectPlayer(userdata as Client); return true; }
            };

            // Spacing
            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.02f), socialHolder.RectTransform), style: null)
            {
                CanBeFocused = false
            };

            // Chat input

            var chatRow = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.07f), socialHolder.RectTransform),
                isHorizontal: true, childAnchor: Anchor.CenterLeft)
            {
                Stretch = true
            };

            chatInput = new GUITextBox(new RectTransform(new Vector2(0.95f, 1.0f), chatRow.RectTransform))
            {
                MaxTextLength = ChatMessage.MaxLength,
                Font = GUI.SmallFont,
                DeselectAfterMessage = false
            };

            micIcon = new GUIImage(new RectTransform(new Vector2(0.05f, 1.0f), chatRow.RectTransform), style: "GUIMicrophoneUnavailable");

            serverLogHolder = new GUILayoutGroup(new RectTransform(Vector2.One, logHolderBottom.RectTransform, Anchor.Center))
            {
                Stretch = true,
                Visible = false
            };

            // Spacing
            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.02f), serverLogHolder.RectTransform), style: null)
            {
                CanBeFocused = false
            };

            GUILayoutGroup serverLogHolderHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.9f), serverLogHolder.RectTransform), isHorizontal: true)
            {
                Stretch = true
            };

            //server log ----------------------------------------------------------------------

            GUILayoutGroup serverLogListboxLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), serverLogHolderHorizontal.RectTransform))
            {
                Stretch = true
            };

            serverLogReverseButton = new GUIButton(new RectTransform(new Vector2(1.0f, 0.05f), serverLogListboxLayout.RectTransform), style: "UIToggleButtonVertical");
            serverLogBox = new GUIListBox(new RectTransform(new Vector2(1.0f, 0.95f), serverLogListboxLayout.RectTransform));

            //filter tickbox list ------------------------------------------------------------------

            serverLogFilterTicks = new GUIListBox(new RectTransform(new Vector2(0.5f, 1.0f), serverLogHolderHorizontal.RectTransform) { MinSize = new Point(150, 0) })
            {
                OnSelected = (component, userdata) => { return false; }
            };

            // Spacing
            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.02f), serverLogHolder.RectTransform), style: null)
            {
                CanBeFocused = false
            };

            // Filter text input

            serverLogFilter = new GUITextBox(new RectTransform(new Vector2(1.0f, 0.07f), serverLogHolder.RectTransform))
            {
                MaxTextLength = ChatMessage.MaxLength,
                Font = GUI.SmallFont
            };

            roundControlsHolder = new GUILayoutGroup(new RectTransform(Vector2.One, bottomBarRight.RectTransform),
                isHorizontal: true, childAnchor: Anchor.CenterLeft)
            {
                Stretch = true
            };

            GUIFrame readyToStartContainer = new GUIFrame(new RectTransform(Vector2.One, roundControlsHolder.RectTransform), style: "TextFrame")
            {
                Visible = false
            };

            // Ready to start tickbox
            ReadyToStartBox = new GUITickBox(new RectTransform(new Vector2(0.95f, 0.75f), readyToStartContainer.RectTransform, anchor: Anchor.Center),
                TextManager.Get("ReadyToStartTickBox"));

            // Spectate button
            spectateButton = new GUIButton(new RectTransform(Vector2.One, roundControlsHolder.RectTransform),
                TextManager.Get("SpectateButton"));

            // Start button
            StartButton = new GUIButton(new RectTransform(Vector2.One, roundControlsHolder.RectTransform),
                TextManager.Get("StartGameButton"))
            {
                OnClicked = (btn, obj) =>
                {
                    GameMain.Client.RequestStartRound();
                    CoroutineManager.StartCoroutine(WaitForStartRound(StartButton), "WaitForStartRound");
                    return true;
                }
            };
            clientHiddenElements.Add(StartButton);
            bottomBar.RectTransform.MinSize =
                new Point(0, (int)Math.Max(ReadyToStartBox.RectTransform.MinSize.Y / 0.75f, StartButton.RectTransform.MinSize.Y));

            //autorestart ------------------------------------------------------------------

            autoRestartText = new GUITextBlock(new RectTransform(Vector2.One, bottomBarMid.RectTransform), "", font: GUI.SmallFont, style: "TextFrame", textAlignment: Alignment.Center);
            GUIFrame autoRestartBoxContainer = new GUIFrame(new RectTransform(Vector2.One, bottomBarMid.RectTransform), style: "TextFrame");
            autoRestartBox = new GUITickBox(new RectTransform(new Vector2(0.95f, 0.75f), autoRestartBoxContainer.RectTransform, Anchor.Center), TextManager.Get("AutoRestart"))
            {
                OnSelected = (tickBox) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, autoRestart: tickBox.Selected);
                    return true;
                }
            };
            clientDisabledElements.Add(autoRestartBoxContainer);

            //--------------------------------------------------------------------------------------------------------------------------------
            //infoframe contents
            //--------------------------------------------------------------------------------------------------------------------------------

            //server info ------------------------------------------------------------------

            // Server Info Header
            GUILayoutGroup lobbyHeader = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), infoFrameContent.RectTransform),
                isHorizontal: true, childAnchor: Anchor.CenterLeft)
            {
                RelativeSpacing = 0.05f,
                Stretch = true
            };

            ServerName = new GUITextBox(new RectTransform(Vector2.One, lobbyHeader.RectTransform))
            {
                MaxTextLength = NetConfig.ServerNameMaxLength,
                OverflowClip = true
            };
            ServerName.OnDeselected += (textBox, key) =>
            {
                if (!textBox.Readonly)
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Name);
                }
            };
            clientReadonlyElements.Add(ServerName);

            Favorite = new GUITickBox(new RectTransform(new Vector2(1.0f, 1.0f), lobbyHeader.RectTransform, scaleBasis: ScaleBasis.BothHeight),
                "", null, "GUIServerListFavoriteTickBox")
            {
                Selected = false,
                ToolTip = TextManager.Get("addtofavorites"),
                OnSelected = (tickbox) =>
                {
                    ServerInfo info = GameMain.Client.ServerSettings.GetServerListInfo();
                    if (tickbox.Selected)
                    {
                        GameMain.ServerListScreen.AddToFavoriteServers(info);
                    }
                    else
                    {
                        GameMain.ServerListScreen.RemoveFromFavoriteServers(info);
                    }
                    tickbox.ToolTip = TextManager.Get(tickbox.Selected ? "removefromfavorites" : "addtofavorites");
                    return true;
                }
            };

            SettingsButton = new GUIButton(new RectTransform(new Vector2(0.25f, 1.0f), lobbyHeader.RectTransform, Anchor.TopRight),
                TextManager.Get("ServerSettingsButton"));
            clientHiddenElements.Add(SettingsButton);

            lobbyHeader.RectTransform.MinSize = new Point(0, Math.Max(ServerName.Rect.Height, SettingsButton.Rect.Height));

            GUILayoutGroup lobbyContent = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.9f), infoFrameContent.RectTransform), isHorizontal: true)
            {
                Stretch = true,
                RelativeSpacing = 0.025f
            };

            GUILayoutGroup serverInfoHolder = new GUILayoutGroup(new RectTransform(Vector2.One, lobbyContent.RectTransform))
            {
                Stretch = true,
                RelativeSpacing = 0.025f
            };

            var serverBanner = new GUICustomComponent(new RectTransform(new Vector2(1.0f, 0.25f), serverInfoHolder.RectTransform), DrawServerBanner)
            {
                HideElementsOutsideFrame = true
            };
            new GUITextBlock(new RectTransform(new Vector2(0.15f, 0.05f), serverBanner.RectTransform) { RelativeOffset = new Vector2(0.01f, 0.04f) },
                "", font: GUI.SmallFont, textAlignment: Alignment.Center, textColor: Color.White, style: "GUISlopedHeader")
            {
                CanBeFocused = false
            };

            publicOrPrivate = new GUITextBlock(new RectTransform(new Vector2(0.15f, 1.0f), serverBanner.RectTransform, Anchor.BottomRight, Pivot.BottomRight),
                "", font: GUI.SmallFont, textAlignment: Alignment.Center, textColor: Color.White, style: "GUISlopedHeader")
            {
                CanBeFocused = false
            };

            var serverMessageContainer = new GUIListBox(new RectTransform(new Vector2(1.0f, 0.75f), serverInfoHolder.RectTransform));
            ServerMessage = new GUITextBox(new RectTransform(Vector2.One, serverMessageContainer.Content.RectTransform), style: "GUITextBoxNoBorder")
            {
                Wrap = true
            };
            ServerMessage.OnTextChanged += (textBox, text) =>
            {
                Vector2 textSize = textBox.Font.MeasureString(textBox.WrappedText);
                textBox.RectTransform.NonScaledSize = new Point(textBox.RectTransform.NonScaledSize.X, Math.Max(serverMessageContainer.Content.Rect.Height, (int)textSize.Y + 10));
                serverMessageContainer.UpdateScrollBarSize();
                serverMessageContainer.BarScroll = 1.0f;
                return true;
            };
            ServerMessage.OnDeselected += (textBox, key) =>
            {
                if (!textBox.Readonly)
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Message);
                }
            };
            clientReadonlyElements.Add(ServerMessage);

            //submarine list ------------------------------------------------------------------

            GUILayoutGroup subHolder = new GUILayoutGroup(new RectTransform(Vector2.One, lobbyContent.RectTransform))
            {
                RelativeSpacing = panelSpacing,
                Stretch = true
            };

            var subLabel = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.055f), subHolder.RectTransform) { MinSize = new Point(0, 25) }, TextManager.Get("Submarine"), font: GUI.SubHeadingFont);

            var filterContainer = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.05f), subHolder.RectTransform), isHorizontal: true)
            {
                Stretch = true
            };
            var searchTitle = new GUITextBlock(new RectTransform(new Vector2(0.001f, 1.0f), filterContainer.RectTransform), TextManager.Get("serverlog.filter"), textAlignment: Alignment.CenterLeft, font: GUI.Font);
            var searchBox = new GUITextBox(new RectTransform(new Vector2(1.0f, 1.0f), filterContainer.RectTransform, Anchor.CenterRight), font: GUI.Font, createClearButton: true);
            filterContainer.RectTransform.MinSize = searchBox.RectTransform.MinSize;
            searchBox.OnSelected += (sender, userdata) => { searchTitle.Visible = false; };
            searchBox.OnDeselected += (sender, userdata) => { searchTitle.Visible = true; };
            searchBox.OnTextChanged += (textBox, text) =>
            {
                foreach (GUIComponent child in subList.Content.Children)
                {
                    if (!(child.UserData is SubmarineInfo sub)) { continue; }
                    child.Visible = string.IsNullOrEmpty(text) || sub.DisplayName.ToLower().Contains(text.ToLower());
                }
                return true;
            };

            subList = new GUIListBox(new RectTransform(Vector2.One, subHolder.RectTransform))
            {
                OnSelected = VotableClicked
            };

            var voteText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), subLabel.RectTransform, Anchor.TopRight),
                TextManager.Get("Votes"), textAlignment: Alignment.CenterRight)
            {
                UserData = "subvotes",
                Visible = false
            };

            //respawn shuttle / submarine preview ------------------------------------------------------------------

            GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 1.0f), lobbyContent.RectTransform))
            {
                RelativeSpacing = panelSpacing,
                Stretch = true
            };

            GUILayoutGroup shuttleHolder = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.05f), rightColumn.RectTransform), isHorizontal: true)
            {
                Stretch = true
            };

            shuttleTickBox = new GUITickBox(new RectTransform(Vector2.One, shuttleHolder.RectTransform), TextManager.Get("RespawnShuttle"))
            {
                Selected = true,
                OnSelected = (GUITickBox box) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, useRespawnShuttle: box.Selected);
                    return true;
                }
            };
            shuttleTickBox.TextBlock.RectTransform.SizeChanged += () =>
            {
                shuttleTickBox.TextBlock.AutoScaleHorizontal = true;
                shuttleTickBox.TextBlock.TextScale = 1.0f;
                if (shuttleTickBox.TextBlock.TextScale < 0.75f)
                {
                    shuttleTickBox.TextBlock.Wrap = true;
                    shuttleTickBox.TextBlock.AutoScaleHorizontal = true;
                    shuttleTickBox.TextBlock.TextScale = 1.0f;
                }
            };
            shuttleList = new GUIDropDown(new RectTransform(Vector2.One, shuttleHolder.RectTransform), elementCount: 10)
            {
                OnSelected = (component, obj) =>
                {
                    GameMain.Client.RequestSelectSub(component.Parent.GetChildIndex(component), isShuttle: true);
                    return true;
                }
            };
            shuttleList.ListBox.RectTransform.MinSize = new Point(250, 0);
            shuttleHolder.RectTransform.MinSize = new Point(0, shuttleList.RectTransform.Children.Max(c => c.MinSize.Y));

            subPreviewContainer = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.9f), rightColumn.RectTransform), style: null);
            subPreviewContainer.RectTransform.SizeChanged += () =>
            {
                if (SelectedSub != null) { CreateSubPreview(SelectedSub); }
            };

            //------------------------------------------------------------------------------------------------------------------
            //   Gamemode panel
            //------------------------------------------------------------------------------------------------------------------

            GUILayoutGroup gameModeBackground = new GUILayoutGroup(new RectTransform(Vector2.One, gameModeContainer.RectTransform), isHorizontal: true)
            {
                Stretch = true,
                RelativeSpacing = 0.01f
            };

            GUILayoutGroup gameModeHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.333f, 1.0f), gameModeBackground.RectTransform))
            {
                Stretch = true
            };

            var modeLabel = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.055f), gameModeHolder.RectTransform) { MinSize = new Point(0, 25) }, TextManager.Get("GameMode"), font: GUI.SubHeadingFont);
            voteText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), modeLabel.RectTransform, Anchor.TopRight),
                TextManager.Get("Votes"), textAlignment: Alignment.CenterRight)
            {
                UserData = "modevotes",
                Visible = false
            };
            modeList = new GUIListBox(new RectTransform(Vector2.One, gameModeHolder.RectTransform))
            {
                OnSelected = VotableClicked
            };

            foreach (GameModePreset mode in GameModePreset.List)
            {
                if (mode.IsSinglePlayer) { continue; }

                var modeFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.15f), modeList.Content.RectTransform), style: null)
                {
                    UserData = mode
                };

                var modeContent = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 0.9f), modeFrame.RectTransform, Anchor.CenterRight) { RelativeOffset = new Vector2(0.02f, 0.0f) })
                {
                    AbsoluteSpacing = (int)(5 * GUI.Scale),
                    Stretch = true
                };

                var modeTitle = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), modeContent.RectTransform), mode.Name, font: GUI.SubHeadingFont);
                var modeDescription = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), modeContent.RectTransform), mode.Description, font: GUI.SmallFont, wrap: true);
                modeTitle.HoverColor = modeDescription.HoverColor = modeTitle.SelectedColor = modeDescription.SelectedColor = Color.Transparent;
                modeTitle.HoverTextColor = modeDescription.HoverTextColor = modeTitle.TextColor;
                modeTitle.TextColor = modeDescription.TextColor = modeTitle.TextColor * 0.5f;
                modeFrame.OnAddedToGUIUpdateList = (c) =>
                {
                    modeTitle.State = modeDescription.State = c.State;
                };

                new GUIImage(new RectTransform(new Vector2(0.2f, 0.8f), modeFrame.RectTransform, Anchor.CenterLeft) { RelativeOffset = new Vector2(0.02f, 0.0f) },
                    style: "GameModeIcon." + mode.Identifier, scaleToFit: true);

                modeFrame.RectTransform.MinSize = new Point(0, (int)(modeContent.Children.Sum(c => c.Rect.Height + modeContent.AbsoluteSpacing) / modeContent.RectTransform.RelativeSize.Y));
            }

            var gameModeSpecificFrame = new GUIFrame(new RectTransform(new Vector2(0.333f, 1.0f), gameModeBackground.RectTransform), style: null);
            CampaignSetupFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform), style: null)
            {
                Visible = false
            };
            CampaignFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform), style: null)
            {
                Visible = false
            };
            GUILayoutGroup campaignContent = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.5f), CampaignFrame.RectTransform, Anchor.Center))
            {
                RelativeSpacing = 0.05f,
                Stretch = true
            };
            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), campaignContent.RectTransform),
                TextManager.Get("gamemode.multiplayercampaign"), font: GUI.SubHeadingFont, textAlignment: Alignment.Center);
            ContinueCampaignButton = new GUIButton(new RectTransform(new Vector2(1.0f, 0.3f), campaignContent.RectTransform),
                TextManager.Get("campaigncontinue"), textAlignment: Alignment.Center)
            {
                OnClicked = (_, __) =>
                {
                    CoroutineManager.StartCoroutine(WaitForStartRound(ContinueCampaignButton), "WaitForStartRound");
                    GameMain.Client?.RequestStartRound(true);
                    return true;
                }
            };
            QuitCampaignButton = new GUIButton(new RectTransform(new Vector2(1.0f, 0.3f), campaignContent.RectTransform),
                TextManager.Get("quitbutton"), textAlignment: Alignment.Center)
            {
                OnClicked = (_, __) =>
                {
                    GameMain.Client.RequestSelectMode(modeList.Content.GetChildIndex(modeList.Content.GetChildByUserData(GameModePreset.Sandbox)));
                    return true;
                }
            };

            //mission type ------------------------------------------------------------------
            MissionTypeFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform), style: null);

            GUILayoutGroup missionHolder = new GUILayoutGroup(new RectTransform(Vector2.One, MissionTypeFrame.RectTransform))
            {
                Stretch = true
            };

            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.055f), missionHolder.RectTransform) { MinSize = new Point(0, 25) },
                TextManager.Get("MissionType"), font: GUI.SubHeadingFont);
            missionTypeList = new GUIListBox(new RectTransform(Vector2.One, missionHolder.RectTransform))
            {
                OnSelected = (component, obj) =>
                {
                    return false;
                }
            };

            var missionTypes = (MissionType[])Enum.GetValues(typeof(MissionType));
            missionTypeTickBoxes = new GUITickBox[missionTypes.Length - 2];
            int index = 0;
            for (int i = 0; i < missionTypes.Length; i++)
            {
                var missionType = missionTypes[i];
                if (missionType == MissionType.None || missionType == MissionType.All) { continue; }

                GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.05f), missionTypeList.Content.RectTransform) { MinSize = new Point(0, (int)(30 * GUI.Scale)) }, style: "ListBoxElement")
                {
                    UserData = missionType,
                };

                if (MissionPrefab.HiddenMissionClasses.Contains(missionType))
                {
                    missionTypeTickBoxes[index] = new GUITickBox(new RectTransform(Vector2.One, frame.RectTransform), string.Empty)
                    {
                        UserData = (int)missionType,
                        Visible = false,
                        CanBeFocused = false
                    };
                }
                else
                {
                    missionTypeTickBoxes[index] = new GUITickBox(new RectTransform(Vector2.One, frame.RectTransform),
                    TextManager.Get("MissionType." + missionType.ToString()))
                    {
                        UserData = (int)missionType,
                        ToolTip = TextManager.Get("MissionTypeDescription." + missionType.ToString(), returnNull: true),
                        OnSelected = (tickbox) =>
                        {
                            int missionTypeOr = tickbox.Selected ? (int)tickbox.UserData : (int)MissionType.None;
                            int missionTypeAnd = (int)MissionType.All & (!tickbox.Selected ? (~(int)tickbox.UserData) : (int)MissionType.All);
                            GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, (int)missionTypeOr, (int)missionTypeAnd);
                            return true;
                        }
                    };
                    frame.RectTransform.MinSize = missionTypeTickBoxes[index].RectTransform.MinSize;
                }
                index++;
            }
            clientDisabledElements.AddRange(missionTypeTickBoxes);

            //------------------------------------------------------------------
            // settings panel
            //------------------------------------------------------------------

            GUILayoutGroup settingsHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.333f, 1.0f), gameModeBackground.RectTransform))
            {
                Stretch = true
            };

            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.055f), settingsHolder.RectTransform) { MinSize = new Point(0, 25) },
                TextManager.Get("Settings"), font: GUI.SubHeadingFont);
            var settingsFrame = new GUIFrame(new RectTransform(Vector2.One, settingsHolder.RectTransform), style: "InnerFrame");
            var settingsContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), settingsFrame.RectTransform, Anchor.Center))
            {
                RelativeSpacing = 0.025f
            };

            //seed ------------------------------------------------------------------

            var seedLabel = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), TextManager.Get("LevelSeed"));
            SeedBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1.0f), seedLabel.RectTransform, Anchor.CenterRight));
            SeedBox.OnDeselected += (textBox, key) =>
            {
                GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.LevelSeed);
            };
            clientDisabledElements.Add(SeedBox);
            LevelSeed = ToolBox.RandomSeed(8);

            //level difficulty ------------------------------------------------------------------

            var difficultyHolder = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.2f), settingsContent.RectTransform), style: null);

            var difficultyLabel = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.5f), difficultyHolder.RectTransform), TextManager.Get("LevelDifficulty"))
            {
                ToolTip = TextManager.Get("leveldifficultyexplanation")
            };

            levelDifficultyScrollBar = new GUIScrollBar(new RectTransform(new Vector2(1.0f, 0.5f), difficultyHolder.RectTransform, Anchor.BottomCenter), style: "GUISlider", barSize: 0.2f)
            {
                Step = 0.01f,
                Range = new Vector2(0.0f, 100.0f),
                ToolTip = TextManager.Get("leveldifficultyexplanation"),
                OnReleased = (scrollbar, value) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, levelDifficulty: scrollbar.BarScrollValue);
                    return true;
                }
            };
            var difficultyName = new GUITextBlock(new RectTransform(new Vector2(1.0f, 1.0f), difficultyLabel.RectTransform), "", textAlignment: Alignment.CenterRight)
            {
                ToolTip = TextManager.Get("leveldifficultyexplanation")
            };
            levelDifficultyScrollBar.OnMoved = (scrollbar, value) =>
            {
                if (EventManagerSettings.List.Count == 0) { return true; }
                difficultyName.Text =
                    EventManagerSettings.List[Math.Min((int)Math.Floor(value * EventManagerSettings.List.Count), EventManagerSettings.List.Count - 1)].Name
                    + " (" + ((int)Math.Round(scrollbar.BarScrollValue)) + " %)";
                difficultyName.TextColor = ToolBox.GradientLerp(scrollbar.BarScroll, GUI.Style.Green, GUI.Style.Orange, GUI.Style.Red);
                return true;
            };

            clientDisabledElements.Add(levelDifficultyScrollBar);

            //traitor probability ------------------------------------------------------------------

            var traitorsSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { Stretch = true };

            new GUITextBlock(new RectTransform(new Vector2(0.7f, 0.0f), traitorsSettingHolder.RectTransform), TextManager.Get("Traitors"), wrap: true);

            var traitorProbContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), traitorsSettingHolder.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { RelativeSpacing = 0.05f, Stretch = true };
            traitorProbabilityButtons = new GUIButton[2];
            traitorProbabilityButtons[0] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), traitorProbContainer.RectTransform), style: "GUIButtonToggleLeft")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, traitorSetting: -1);

                    return true;
                }
            };

            traitorProbabilityText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1.0f), traitorProbContainer.RectTransform), TextManager.Get("No"),
                textAlignment: Alignment.Center, style: "GUITextBox");
            traitorProbabilityButtons[1] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), traitorProbContainer.RectTransform), style: "GUIButtonToggleRight")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, traitorSetting: 1);

                    return true;
                }
            };

            clientDisabledElements.AddRange(traitorProbabilityButtons);

            //bot count ------------------------------------------------------------------

            var botCountSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { Stretch = true };

            new GUITextBlock(new RectTransform(new Vector2(0.7f, 0.0f), botCountSettingHolder.RectTransform), TextManager.Get("BotCount"), wrap: true);
            var botCountContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), botCountSettingHolder.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { RelativeSpacing = 0.05f, Stretch = true };
            botCountButtons = new GUIButton[2];
            botCountButtons[0] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), botCountContainer.RectTransform), style: "GUIButtonToggleLeft")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, botCount: -1);
                    return true;
                }
            };

            botCountText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1.0f), botCountContainer.RectTransform), "0", textAlignment: Alignment.Center, style: "GUITextBox");
            botCountButtons[1] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), botCountContainer.RectTransform), style: "GUIButtonToggleRight")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, botCount: 1);
                    return true;
                }
            };

            clientDisabledElements.AddRange(botCountButtons);

            var botSpawnModeSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { Stretch = true };

            new GUITextBlock(new RectTransform(new Vector2(0.7f, 0.0f), botSpawnModeSettingHolder.RectTransform), TextManager.Get("BotSpawnMode"), wrap: true);
            var botSpawnModeContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), botSpawnModeSettingHolder.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { RelativeSpacing = 0.05f, Stretch = true };
            botSpawnModeButtons = new GUIButton[2];
            botSpawnModeButtons[0] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), botSpawnModeContainer.RectTransform), style: "GUIButtonToggleLeft")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, botSpawnMode: -1);
                    return true;
                }
            };

            botSpawnModeText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1.0f), botSpawnModeContainer.RectTransform), "", textAlignment: Alignment.Center, style: "GUITextBox");
            botSpawnModeButtons[1] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), botSpawnModeContainer.RectTransform), style: "GUIButtonToggleRight")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, botSpawnMode: 1);
                    return true;
                }
            };

            if (MapGenerationParams.Instance.RadiationParams != null)
            {
                radiationEnabledTickBox = new GUITickBox(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), TextManager.Get("CampaignOption.EnableRadiation"), font: GUI.Style.Font)
                {
                    Selected = true,
                    OnSelected = box =>
                    {
                        GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, radiationEnabled: box.Selected);
                        return true;
                    }
                };
            }

            var maxMissionCountSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), settingsContent.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { Stretch = true };
            maxMissionCountDescription = new GUITextBlock(new RectTransform(new Vector2(0.7f, 0.0f), maxMissionCountSettingHolder.RectTransform), TextManager.Get("maxmissioncount", fallBackTag: "missions"), wrap: true)
            {
                ToolTip = TextManager.Get("maxmissioncounttooltip")
            };
            var maxMissionCountContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), maxMissionCountSettingHolder.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft) { RelativeSpacing = 0.05f, Stretch = true };
            maxMissionCountButtons = new GUIButton[2];
            maxMissionCountButtons[0] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), maxMissionCountContainer.RectTransform), style: "GUIButtonToggleLeft")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, maxMissionCount: -1);
                    return true;
                }
            };
            maxMissionCountText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1.0f), maxMissionCountContainer.RectTransform), "0", textAlignment: Alignment.Center, style: "GUITextBox");
            maxMissionCountButtons[1] = new GUIButton(new RectTransform(new Vector2(0.15f, 1.0f), maxMissionCountContainer.RectTransform), style: "GUIButtonToggleRight")
            {
                OnClicked = (button, obj) =>
                {
                    GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, maxMissionCount: 1);
                    return true;
                }
            };
            maxMissionCountSettingHolder.Children.ForEach(c => c.ToolTip = maxMissionCountSettingHolder.ToolTip);

            List<GUIComponent> settingsElements = settingsContent.Children.ToList();
            for (int i = 0; i < settingsElements.Count; i++)
            {
                if (settingsElements[i].CountChildren > 0)
                {
                    settingsElements[i].RectTransform.MinSize = new Point(0, Math.Max(settingsElements[i].RectTransform.Children.Max(c => c.Rect.Height), (int)(20 * GUI.Scale)));
                }
            }

            settingsBlocker = new GUIFrame(new RectTransform(Vector2.One, settingsFrame.RectTransform), style: "InnerFrame")
            {
                Color = Color.Black * 0.5f,
                IgnoreLayoutGroups = true,
                Visible = false
            };

            clientDisabledElements.AddRange(botSpawnModeButtons);
        }

        public void StopWaitingForStartRound()
        {
            CoroutineManager.StopCoroutines("WaitForStartRound");

            if (StartButton != null)
            {
                StartButton.Enabled = true;
            }
            GUI.ClearCursorWait();
        }

        public IEnumerable<object> WaitForStartRound(GUIButton startButton)
        {
            GUI.SetCursorWaiting();
            string headerText = TextManager.Get("RoundStartingPleaseWait");
            var msgBox = new GUIMessageBox(headerText, TextManager.Get("RoundStarting"), new string[0]);

            if (startButton != null)
            {
                startButton.Enabled = false;
            }

            DateTime timeOut = DateTime.Now + new TimeSpan(0, 0, 10);
            while (Selected == GameMain.NetLobbyScreen && DateTime.Now < timeOut)
            {
                msgBox.Header.Text = headerText + new string('.', ((int)Timing.TotalTime % 3 + 1));
                yield return CoroutineStatus.Running;
            }

            msgBox.Close();
            if (startButton != null)
            {
                startButton.Enabled = true;
            }
            GUI.ClearCursorWait();
            yield return CoroutineStatus.Success;
        }

        public override void Deselect()
        {
            chatInput.Deselect();
            CampaignCharacterDiscarded = false;
            
            CharacterAppearanceCustomizationMenu?.Dispose();
            JobSelectionFrame = null;

            /*foreach (Sprite sprite in jobPreferenceSprites) { sprite.Remove(); }
            jobPreferenceSprites.Clear();*/
        }

        public override void Select()
        {
            if (GameMain.NetworkMember == null) { return; }

            CharacterAppearanceCustomizationMenu?.Dispose();
            JobSelectionFrame = null;

            infoFrameContent.Recalculate();

            Character.Controlled = null;
            GameMain.LightManager.LosEnabled = false;
            GUI.PreventPauseMenuToggle = false;
            CampaignCharacterDiscarded = false;

            chatInput.Select();
            chatInput.OnEnterPressed = GameMain.Client.EnterChatMessage;
            chatInput.OnTextChanged += GameMain.Client.TypingChatMessage;
            chatInput.OnDeselected += (sender, key) =>
            {
                if (GameMain.Client != null)
                {
                    GameMain.Client.ChatBox.ChatManager.Clear();
                }
            };

            //disable/hide elements the clients are not supposed to use/see
            clientDisabledElements.ForEach(c => c.Enabled = false);
            clientReadonlyElements.ForEach(c => c.Readonly = true);
            clientHiddenElements.ForEach(c => c.Visible = false);

            RefreshEnabledElements();

            if (GameMain.Client != null)
            {
                ChatManager.RegisterKeys(chatInput, GameMain.Client.ChatBox.ChatManager);
                spectateButton.Visible = GameMain.Client.GameStarted;
                ReadyToStartBox.Selected = false;
                GameMain.Client.SetReadyToStart(ReadyToStartBox);
            }
            else
            {
                spectateButton.Visible = false;
            }
            SetSpectate(spectateBox.Selected);

            if (GameMain.Client != null)
            {
                GameMain.Client.ServerSettings.Voting.ResetVotes(GameMain.Client.ConnectedClients);
                spectateButton.OnClicked = GameMain.Client.SpectateClicked;
                ReadyToStartBox.OnSelected = GameMain.Client.SetReadyToStart;
            }

            roundControlsHolder.Children.ForEach(c => c.IgnoreLayoutGroups = !c.Visible);
            roundControlsHolder.Recalculate();

            GameMain.NetworkMember.EndVoteCount = 0;
            GameMain.NetworkMember.EndVoteMax = 1;

            base.Select();
        }

        public void SetPublic(bool isPublic)
        {
            publicOrPrivate.Text = isPublic ? TextManager.Get("PublicLobbyTag") : TextManager.Get("PrivateLobbyTag");
        }

        public void RefreshEnabledElements()
        {
            ServerName.Readonly = !GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            ServerMessage.Readonly = !GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            missionTypeList.Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            foreach (var tickBox in missionTypeTickBoxes)
            {
                tickBox.Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            }
            SeedBox.Enabled = !CampaignFrame.Visible && !CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            levelDifficultyScrollBar.Enabled = !CampaignFrame.Visible && !CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            if (radiationEnabledTickBox != null)
            {
                radiationEnabledTickBox.Enabled = CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            }
            maxMissionCountDescription.Enabled = CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            maxMissionCountText.Enabled = CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            foreach (var button in maxMissionCountButtons)
            {
                button.Enabled = CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            }

            traitorProbabilityButtons[0].Enabled = traitorProbabilityButtons[1].Enabled = traitorProbabilityText.Enabled =
                !CampaignFrame.Visible && !CampaignSetupFrame.Visible && GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            botCountButtons[0].Enabled = botCountButtons[1].Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            botSpawnModeButtons[0].Enabled = botSpawnModeButtons[1].Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);

            autoRestartBox.Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);

            SettingsButton.Visible = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            SettingsButton.OnClicked = GameMain.Client.ServerSettings.ToggleSettingsFrame;
            StartButton.Visible = GameMain.Client.HasPermission(ClientPermissions.ManageRound) && !GameMain.Client.GameStarted && !CampaignSetupFrame.Visible && !CampaignFrame.Visible;
            ServerName.Readonly = !GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            ServerMessage.Readonly = !GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            shuttleTickBox.Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageSettings);
            SubList.Enabled = !CampaignFrame.Visible && (GameMain.Client.ServerSettings.Voting.AllowSubVoting || GameMain.Client.HasPermission(ClientPermissions.SelectSub));
            shuttleList.Enabled = shuttleList.ButtonEnabled = GameMain.Client.HasPermission(ClientPermissions.SelectSub);
            ModeList.Enabled = GameMain.Client.ServerSettings.Voting.AllowModeVoting || GameMain.Client.HasPermission(ClientPermissions.SelectMode);
            LogButtons.Visible = GameMain.Client.HasPermission(ClientPermissions.ServerLog);
            GameMain.Client.ShowLogButton.Visible = GameMain.Client.HasPermission(ClientPermissions.ServerLog);
            roundControlsHolder.Children.ForEach(c => c.IgnoreLayoutGroups = !c.Visible);
            roundControlsHolder.Children.ForEach(c => c.RectTransform.RelativeSize = Vector2.One);
            roundControlsHolder.Recalculate();

            ReadyToStartBox.Parent.Visible = !GameMain.Client.GameStarted;

            RefreshGameModeContent();
        }

        public void ShowSpectateButton()
        {
            if (GameMain.Client == null) { return; }
            spectateButton.Visible = true;
            spectateButton.Enabled = true;
            StartButton.Visible = false;
        }

        public void SetCampaignCharacterInfo(CharacterInfo newCampaignCharacterInfo)
        {
            if (newCampaignCharacterInfo != null)
            {
                if (CampaignCharacterDiscarded) { return; }
                if (campaignCharacterInfo != newCampaignCharacterInfo)
                {
                    campaignCharacterInfo = newCampaignCharacterInfo;
                    UpdatePlayerFrame(campaignCharacterInfo, false);
                }
            }
            else if (campaignCharacterInfo != null)
            {
                campaignCharacterInfo = null;
                UpdatePlayerFrame(null, false);
            }
        }

        private void UpdatePlayerFrame(CharacterInfo characterInfo, bool allowEditing = true)
        {
            UpdatePlayerFrame(characterInfo, allowEditing, playerInfoContainer);
        }

        public void CreatePlayerFrame(GUIComponent parent, bool createPendingText = true, bool alwaysAllowEditing = false)
        {
            UpdatePlayerFrame(
                Character.Controlled?.Info ?? playerInfoContainer.Children?.First().UserData as CharacterInfo,
                allowEditing: alwaysAllowEditing || campaignCharacterInfo == null,
                parent: parent,
                createPendingText: createPendingText);
        }

        private void UpdatePlayerFrame(CharacterInfo characterInfo, bool allowEditing, GUIComponent parent, bool createPendingText = true)
        {
            createPendingChangesText = createPendingText;
            if (characterInfo == null || CampaignCharacterDiscarded)
            {
                characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, GameMain.Client.Name, null);
                characterInfo.RecreateHead(GameMain.Config.PlayerCharacterCustomization);
                GameMain.Client.CharacterInfo = characterInfo;
                characterInfo.OmitJobInPortraitClothing = false;
            }

            parent.ClearChildren();

            bool isGameRunning = GameMain.GameSession?.IsRunning ?? false;

            infoContainer = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, isGameRunning ? 0.97f : 0.92f), parent.RectTransform, Anchor.BottomCenter), childAnchor: Anchor.TopCenter)
            {
                RelativeSpacing = 0.0f,
                Stretch = true,
                UserData = characterInfo
            };

            bool nameChangePending = isGameRunning && GameMain.Client.PendingName != string.Empty && GameMain.Client?.Character?.Name != GameMain.Client.PendingName;
            changesPendingText = null;

            if (isGameRunning)
            {
                infoContainer.RectTransform.AbsoluteOffset = new Point(0, (int)(parent.Rect.Height * 0.025f));
            }

            if (TabMenu.PendingChanges)
            {
                CreateChangesPendingText();
            }


            CharacterNameBox = new GUITextBox(new RectTransform(new Vector2(1.0f, 0.065f), infoContainer.RectTransform), !nameChangePending ? characterInfo.Name : GameMain.Client.PendingName, textAlignment: Alignment.Center)
            {
                MaxTextLength = Client.MaxNameLength,
                OverflowClip = true
            };

            CharacterNameBox.OnEnterPressed += (tb, text) => { CharacterNameBox.Deselect(); return true; };
            CharacterNameBox.OnDeselected += (tb, key) =>
            {
                if (GameMain.Client == null) { return; }
                string newName = Client.SanitizeName(tb.Text);
                newName = newName.Replace(":", "").Replace(";", "");
                if (newName == GameMain.Client.Name) return;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    tb.Text = GameMain.Client.Name;
                }
                else
                {
                    if (isGameRunning)
                    {
                        GameMain.Client.PendingName = tb.Text;
                        TabMenu.PendingChanges = true;
                        if (createPendingText)
                        {
                            CreateChangesPendingText();
                        }
                    }
                    else
                    {
                        ReadyToStartBox.Selected = false;
                    }

                    GameMain.Client.SetName(tb.Text);
                }
            };

            //spacing
            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.006f), infoContainer.RectTransform), style: null);
            
            if (allowEditing)
            {
                GUILayoutGroup characterInfoTabs = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.016f), infoContainer.RectTransform), isHorizontal: true)
                {
                    Stretch = true,
                    RelativeSpacing = 0.02f
                };

                jobPreferencesButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), characterInfoTabs.RectTransform),
                    TextManager.Get("JobPreferences"), style: "GUITabButton")
                {
                    Selected = true,
                    OnClicked = SelectJobPreferencesTab
                };
                appearanceButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), characterInfoTabs.RectTransform),
                    TextManager.Get("CharacterAppearance"), style: "GUITabButton")
                {
                    OnClicked = SelectAppearanceTab
                };

                GUITextBlock.AutoScaleAndNormalize(jobPreferencesButton.TextBlock, appearanceButton.TextBlock);

                // Unsubscribe from previous events, not even sure if this matters here but it doesn't hurt so why not
                if (characterInfoFrame != null) { characterInfoFrame.RectTransform.SizeChanged -= RecalculateSubDescription; }
                characterInfoFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.2f), infoContainer.RectTransform), style: null);
                characterInfoFrame.RectTransform.SizeChanged += RecalculateSubDescription;

                JobPreferenceContainer = new GUIFrame(new RectTransform(Vector2.One, characterInfoFrame.RectTransform),
                    style: "GUIFrameListBox");
                characterInfo.CreateIcon(new RectTransform(new Vector2(1.0f, 0.4f), JobPreferenceContainer.RectTransform, Anchor.TopCenter) { RelativeOffset = new Vector2(0f, 0.025f) });
                JobList = new GUIListBox(new RectTransform(new Vector2(1.0f, 0.6f), JobPreferenceContainer.RectTransform, Anchor.BottomCenter), true)
                {
                    Enabled = true,
                    OnSelected = (child, obj) =>
                    {
                        if (child.IsParentOf(GUI.MouseOn)) return false;
                        return OpenJobSelection(child, obj);
                    }
                };

                for (int i = 0; i < 3; i++)
                {
                    Pair<JobPrefab, int> jobPrefab = null;
                    while (i < GameMain.Config.JobPreferences.Count)
                    {
                        var jobIdentifier = GameMain.Config.JobPreferences[i];
                        if (!JobPrefab.Prefabs.ContainsKey(jobIdentifier.First))
                        {
                            GameMain.Config.JobPreferences.RemoveAt(i);
                            continue;
                        }
                        // The old job variant system used one-based indexing
                        // so let's make sure no one get to pick a variant which doesn't exist
                        var prefab = JobPrefab.Prefabs[jobIdentifier.First];
                        var variant = Math.Min(jobIdentifier.Second, prefab.Variants - 1);
                        jobPrefab = new Pair<JobPrefab, int>(prefab, variant);
                        break;
                    }

                    var slot = new GUIFrame(new RectTransform(new Vector2(0.333f, 1.0f), JobList.Content.RectTransform), style: "ListBoxElementSquare")
                    {
                        CanBeFocused = true,
                        UserData = jobPrefab
                    };
                }

                UpdateJobPreferences();

                appearanceFrame = new GUIFrame(new RectTransform(Vector2.One, characterInfoFrame.RectTransform), style: "GUIFrameListBox")
                {
                    Visible = false,
                    Color = Color.White
                };
            }
            else
            {
                characterInfo.CreateIcon(new RectTransform(new Vector2(0.6f, 0.16f), infoContainer.RectTransform, Anchor.TopCenter));

                new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), infoContainer.RectTransform), characterInfo.Job.Name, textAlignment: Alignment.Center, font: GUI.SubHeadingFont, wrap: true)
                {
                    HoverColor = Color.Transparent,
                    SelectedColor = Color.Transparent
                };

                new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), infoContainer.RectTransform), TextManager.Get("Skills"), font: GUI.SubHeadingFont);
                foreach (Skill skill in characterInfo.Job.Skills)
                {
                    Color textColor = Color.White * (0.5f + skill.Level / 200.0f);
                    var skillText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), infoContainer.RectTransform),
                        "  - " + TextManager.AddPunctuation(':', TextManager.Get("SkillName." + skill.Identifier), ((int)skill.Level).ToString()),
                        textColor,
                        font: GUI.SmallFont);
                }

                // Spacing
                new GUIFrame(new RectTransform(new Vector2(1.0f, 0.15f), infoContainer.RectTransform), style: null);

                new GUIButton(new RectTransform(new Vector2(0.8f, 0.1f), infoContainer.RectTransform, Anchor.BottomCenter), TextManager.Get("CreateNew"))
                {
                    IgnoreLayoutGroups = true,
                    OnClicked = (btn, userdata) =>
                    {
                        TryDiscardCampaignCharacter(() =>
                        {
                            UpdatePlayerFrame(null, true, parent);
                        });
                        return true;
                    }
                };
            }

            TeamPreferenceListBox = null;
            if (SelectedMode == GameModePreset.PvP)
            {
                TeamPreferenceListBox = new GUIListBox(new RectTransform(new Vector2(1.0f, 0.04f), infoContainer.RectTransform, anchor: Anchor.TopLeft, pivot: Pivot.TopLeft), isHorizontal: true, style: null)
                {
                    Enabled = true,
                    KeepSpaceForScrollBar = false,
                    ScrollBarEnabled = false,
                    ScrollBarVisible = false
                };

                TeamPreferenceListBox.UpdateDimensions();

                Color team1Color = new Color(0, 110, 150, 255);
                var team1Option = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1.0f), TeamPreferenceListBox.Content.RectTransform), TextManager.Get("teampreference.team1"), textAlignment: Alignment.Center, style: null)
                {
                    UserData = CharacterTeamType.Team1,
                    CanBeFocused = true,
                    Padding = Vector4.One * 10.0f * GUI.Scale,
                    Color = Color.Lerp(team1Color, Color.Black, 0.7f) * 0.7f,
                    HoverColor = team1Color * 0.95f,
                    SelectedColor = team1Color * 0.8f,
                    OutlineColor = team1Color,
                    TextColor = Color.White,
                    HoverTextColor = Color.White,
                    SelectedTextColor = Color.White
                };

                Color noPreferenceColor = new Color(100, 100, 100, 255);
                var noPreferenceOption = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1.0f), TeamPreferenceListBox.Content.RectTransform), TextManager.Get("teampreference.nopreference"), textAlignment: Alignment.Center, style: null)
                {
                    UserData = CharacterTeamType.None,
                    CanBeFocused = true,
                    Padding = Vector4.One * 10.0f * GUI.Scale,
                    Color = Color.Lerp(noPreferenceColor, Color.Black, 0.7f) * 0.7f,
                    HoverColor = noPreferenceColor * 0.95f,
                    SelectedColor = noPreferenceColor * 0.8f,
                    OutlineColor = noPreferenceColor,
                    TextColor = Color.White,
                    HoverTextColor = Color.White,
                    SelectedTextColor = Color.White
                };

                Color team2Color = new Color(150, 110, 0, 255);
                var team2Option = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1.0f), TeamPreferenceListBox.Content.RectTransform), TextManager.Get("teampreference.team2"), textAlignment: Alignment.Center, style: null)
                {
                    UserData = CharacterTeamType.Team2,
                    CanBeFocused = true,
                    Padding = Vector4.One * 10.0f * GUI.Scale,
                    Color = Color.Lerp(team2Color, Color.Black, 0.7f) * 0.7f,
                    HoverColor = team2Color * 0.95f,
                    SelectedColor = team2Color * 0.8f,
                    OutlineColor = team2Color,
                    TextColor = Color.White,
                    HoverTextColor = Color.White,
                    SelectedTextColor = Color.White
                };

                TeamPreferenceListBox.Select(GameMain.Config.TeamPreference);

                TeamPreferenceListBox.OnSelected += (component, obj) =>
                {
                    if ((CharacterTeamType)obj == GameMain.Config.TeamPreference) { return true; }

                    GameMain.Config.TeamPreference = (CharacterTeamType)obj;
                    GameMain.Client.ForceNameAndJobUpdate();
                    GameMain.Config.SaveNewPlayerConfig();

                    return true;
                };
            }
        }

        public void TryDiscardCampaignCharacter(Action onYes)
        {
            var confirmation = new GUIMessageBox(TextManager.Get("NewCampaignCharacterHeader"), TextManager.Get("NewCampaignCharacterText"),
                new[] { TextManager.Get("Yes"), TextManager.Get("No") });
            confirmation.Buttons[0].OnClicked += confirmation.Close;
            confirmation.Buttons[0].OnClicked += (btn2, userdata2) =>
            {
                CampaignCharacterDiscarded = true;
                campaignCharacterInfo = null;
                onYes();
                return true;
            };
            confirmation.Buttons[1].OnClicked += confirmation.Close;
        }

        private void CreateChangesPendingText()
        {
            if (!createPendingChangesText || changesPendingText != null || infoContainer == null) { return; }

            changesPendingText = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.065f), infoContainer.Parent.Parent.RectTransform, Anchor.BottomCenter, Pivot.TopCenter) { RelativeOffset = new Vector2(0f, -0.03f) },
                style: "OuterGlow")
            {
                Color = Color.Black,
                IgnoreLayoutGroups = true
            };
            var text = new GUITextBlock(new RectTransform(Vector2.One, changesPendingText.RectTransform, Anchor.Center),
                TextManager.Get("tabmenu.characterchangespending"), textColor: GUI.Style.Orange, textAlignment: Alignment.Center, style: null);
            changesPendingText.RectTransform.MinSize = new Point((int)(text.TextSize.X * 1.2f), (int)(text.TextSize.Y * 2.0f));
        }

        public static void CreateChangesPendingFrame(GUIComponent parent)
        {
            parent.ClearChildren();
            GUIFrame changesPendingFrame = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center),
                style: "OuterGlow")
            {
                Color = Color.Black
            };
            new GUITextBlock(new RectTransform(Vector2.One, changesPendingFrame.RectTransform, Anchor.Center),
                TextManager.Get("tabmenu.characterchangespending"), textColor: GUI.Style.Orange, textAlignment: Alignment.Center, style: null)
            {
                AutoScaleHorizontal = true
            };
        }

        private void CreateJobVariantTooltip(JobPrefab jobPrefab, int variant, GUIComponent parentSlot)
        {
            jobVariantTooltip = new GUIFrame(new RectTransform(new Point((int)(400 * GUI.Scale), (int)(180 * GUI.Scale)), GUI.Canvas, pivot: Pivot.BottomRight),
                style: "GUIToolTip")
            {
                UserData = new Pair<JobPrefab, int>(jobPrefab, variant)
            };
            jobVariantTooltip.RectTransform.AbsoluteOffset = new Point(parentSlot.Rect.Right, parentSlot.Rect.Y);

            var content = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), jobVariantTooltip.RectTransform, Anchor.Center))
            {
                Stretch = true,
                AbsoluteSpacing = (int)(15 * GUI.Scale)
            };

            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), content.RectTransform), TextManager.GetWithVariable("startingequipmentname", "[number]", (variant + 1).ToString()), font: GUI.SubHeadingFont, textAlignment: Alignment.Center);

            var itemIdentifiers = jobPrefab.PreviewItems[variant]
                .Where(it => it.ShowPreview)
                .Select(it => it.ItemIdentifier)
                .Distinct();

            int itemsPerRow = 5;
            int rows = (int)Math.Max(Math.Ceiling(itemIdentifiers.Count() / (float)itemsPerRow), 1);

            new GUICustomComponent(new RectTransform(new Vector2(1.0f, 0.4f * rows), content.RectTransform, Anchor.BottomCenter),
                onDraw: (sb, component) => { DrawJobVariantItems(sb, component, new Pair<JobPrefab, int>(jobPrefab, variant), itemsPerRow); });

            jobVariantTooltip.RectTransform.MinSize = new Point(0, content.RectTransform.Children.Sum(c => c.Rect.Height + content.AbsoluteSpacing));
        }

        public bool ToggleSpectate(GUITickBox tickBox)
        {
            SetSpectate(tickBox.Selected);
            return false;
        }

        public void SetSpectate(bool spectate)
        {
            this.spectateBox.Selected = spectate;
            if (spectate)
            {
                playerInfoContainer.ClearChildren();

                GameMain.Client.CharacterInfo?.Remove();
                GameMain.Client.CharacterInfo = null;
                GameMain.Client.Character?.Remove();
                GameMain.Client.Character = null;
                new GUITextBlock(new RectTransform(Vector2.One, playerInfoContainer.RectTransform, Anchor.Center),
                    TextManager.Get("PlayingAsSpectator"),
                    textAlignment: Alignment.Center);
            }
            else
            {
                UpdatePlayerFrame(campaignCharacterInfo, allowEditing: campaignCharacterInfo == null);
            }
        }

        public void SetAllowSpectating(bool allowSpectating)
        {
            // Server owner is allowed to spectate regardless of the server settings
            if (GameMain.Client != null && GameMain.Client.IsServerOwner) { return; }

            // Show the player config menu if spectating is not allowed
            if (spectateBox.Selected && !allowSpectating) { spectateBox.Selected = false; }

            // Hide spectate tickbox if spectating is not allowed
            spectateBox.Visible = allowSpectating;
        }

        public void SetAutoRestart(bool enabled, float timer = 0.0f)
        {
            autoRestartBox.Selected = enabled;
            autoRestartTimer = timer;
        }

        public void SetMissionType(MissionType missionType)
        {
            MissionType = missionType;
        }

        public void UpdateSubList(GUIComponent subList, List<SubmarineInfo> submarines)
        {
            if (subList == null) { return; }

            subList.ClearChildren();

            foreach (SubmarineInfo sub in submarines)
            {
                AddSubmarine(subList, sub);
            }
        }

        private void AddSubmarine(GUIComponent subList, SubmarineInfo sub)
        {
            if (subList is GUIListBox listBox)
            {
                subList = listBox.Content;
            }
            else if (subList is GUIDropDown dropDown)
            {
                subList = dropDown.ListBox.Content;
            }

            var frame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.1f), subList.RectTransform) { MinSize = new Point(0, 20) },
                style: "ListBoxElement")
            {
                ToolTip = sub.Description,
                UserData = sub
            };

            int buttonSize = (int)(frame.Rect.Height * 0.8f);
            var subTextBlock = new GUITextBlock(new RectTransform(new Vector2(0.8f, 1.0f), frame.RectTransform, Anchor.CenterLeft) /*{ AbsoluteOffset = new Point(buttonSize + 5, 0) }*/,
                ToolBox.LimitString(sub.DisplayName, GUI.Font, subList.Rect.Width - 65), textAlignment: Alignment.CenterLeft)
            {
                CanBeFocused = false
            };

            var matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == sub.Name && s.MD5Hash?.Hash == sub.MD5Hash?.Hash);
            if (matchingSub == null) matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == sub.Name);

            if (matchingSub == null)
            {
                subTextBlock.TextColor = new Color(subTextBlock.TextColor, 0.5f);
                frame.ToolTip = TextManager.Get("SubNotFound");
            }
            else if (matchingSub?.MD5Hash == null || matchingSub.MD5Hash?.Hash != sub.MD5Hash?.Hash)
            {
                subTextBlock.TextColor = new Color(subTextBlock.TextColor, 0.5f);
                frame.ToolTip = TextManager.Get("SubDoesntMatch");
            }
            else
            {
                if (subList == shuttleList || subList == shuttleList.ListBox || subList == shuttleList.ListBox.Content)
                {
                    subTextBlock.TextColor = new Color(subTextBlock.TextColor, sub.HasTag(SubmarineTag.Shuttle) ? 1.0f : 0.6f);
                }
            }

            if (!sub.RequiredContentPackagesInstalled)
            {
                subTextBlock.TextColor = Color.Lerp(subTextBlock.TextColor, Color.DarkRed, 0.5f);
                frame.ToolTip = TextManager.Get("ContentPackageMismatch") + "\n\n" + frame.RawToolTip;
            }

            if (sub.HasTag(SubmarineTag.Shuttle))
            {
                var shuttleText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), frame.RectTransform, Anchor.CenterRight) { AbsoluteOffset = new Point(GUI.IntScale(20), 0) },
                    TextManager.Get("Shuttle", fallBackTag: "RespawnShuttle"), textAlignment: Alignment.CenterRight, font: GUI.SmallFont)
                {
                    TextColor = subTextBlock.TextColor * 0.8f,
                    ToolTip = subTextBlock.RawToolTip,
                    CanBeFocused = false
                };
                //make shuttles more dim in the sub list (selecting a shuttle as the main sub is allowed but not recommended)
                if (subList == this.subList.Content)
                {
                    subTextBlock.TextColor *= 0.8f;
                    foreach (GUIComponent child in frame.Children)
                    {
                        child.Color *= 0.8f;
                    }
                }
            }
            else
            {
                var classText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), frame.RectTransform, Anchor.CenterRight) { AbsoluteOffset = new Point(GUI.IntScale(20), 0) },
                TextManager.Get($"submarineclass.{sub.SubmarineClass}"), textAlignment: Alignment.CenterRight, font: GUI.SmallFont)
                {
                    UserData = "classtext",
                    TextColor = subTextBlock.TextColor * 0.8f,
                    ToolTip = subTextBlock.RawToolTip
                };
            }

        }

        public bool VotableClicked(GUIComponent component, object userData)
        {
            if (GameMain.Client == null) { return false; }

            VoteType voteType;
            if (component.Parent == GameMain.NetLobbyScreen.SubList.Content)
            {
                if (!GameMain.Client.ServerSettings.Voting.AllowSubVoting)
                {
                    var selectedSub = component.UserData as SubmarineInfo;
                    if (!selectedSub.RequiredContentPackagesInstalled)
                    {
                        var msgBox = new GUIMessageBox(TextManager.Get("ContentPackageMismatch"),
                            selectedSub.RequiredContentPackages.Any() ?
                            TextManager.GetWithVariable("ContentPackageMismatchWarning", "[requiredcontentpackages]", string.Join(", ", selectedSub.RequiredContentPackages)) :
                            TextManager.Get("ContentPackageMismatchWarningGeneric"),
                            new string[] { TextManager.Get("Yes"), TextManager.Get("No") });

                        msgBox.Buttons[0].OnClicked = msgBox.Close;
                        msgBox.Buttons[0].OnClicked += (button, obj) =>
                        {
                            GameMain.Client.RequestSelectSub(component.Parent.GetChildIndex(component), isShuttle: false);
                            return true;
                        };
                        msgBox.Buttons[1].OnClicked = msgBox.Close;
                        return false;
                    }
                    else if (GameMain.Client.HasPermission(ClientPermissions.SelectSub))
                    {
                        GameMain.Client.RequestSelectSub(component.Parent.GetChildIndex(component), isShuttle: false);
                        return true;
                    }
                    return false;
                }
                if (component.UserData is SubmarineInfo sub)
                {
                    CreateSubPreview(sub);
                }
                voteType = VoteType.Sub;
            }
            else if (component.Parent == GameMain.NetLobbyScreen.ModeList.Content)
            {
                if (!GameMain.Client.ServerSettings.Voting.AllowModeVoting)
                {
                    if (GameMain.Client.HasPermission(ClientPermissions.SelectMode))
                    {
                        string presetName = ((GameModePreset)component.UserData).Identifier;

                        //display a verification prompt when switching away from the campaign
                        if (HighlightedModeIndex == SelectedModeIndex &&
                            (GameMain.NetLobbyScreen.ModeList.SelectedData as GameModePreset) == GameModePreset.MultiPlayerCampaign &&
                            presetName != GameModePreset.MultiPlayerCampaign.Identifier)
                        {
                            var verificationBox = new GUIMessageBox("", TextManager.Get("endcampaignverification"), new string[] { TextManager.Get("yes"), TextManager.Get("no") });
                            verificationBox.Buttons[0].OnClicked += (btn, userdata) =>
                            {
                                GameMain.Client.RequestSelectMode(component.Parent.GetChildIndex(component));
                                HighlightMode(SelectedModeIndex);
                                verificationBox.Close(btn, userdata);
                                return true;
                            };
                            verificationBox.Buttons[1].OnClicked = verificationBox.Close;
                            return false;
                        }
                        GameMain.Client.RequestSelectMode(component.Parent.GetChildIndex(component));
                        HighlightMode(SelectedModeIndex);

                        if (presetName.Equals("multiplayercampaign", StringComparison.OrdinalIgnoreCase))
                        {
                            GUI.SetCursorWaiting(endCondition: () =>
                            {
                                return CampaignFrame.Visible || CampaignSetupFrame.Visible;
                            });
                        }

                        return !presetName.Equals("multiplayercampaign", StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                }
                else if (!((GameModePreset)userData).Votable)
                {
                    return false;
                }

                voteType = VoteType.Mode;
            }
            else
            {
                return false;
            }

            GameMain.Client.Vote(voteType, userData);

            return true;
        }

        public void AddPlayer(Client client)
        {
            GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), playerList.Content.RectTransform) { MinSize = new Point(0, (int)(30 * GUI.Scale)) },
                client.Name, textAlignment: Alignment.CenterLeft, font: GUI.SmallFont, style: null)
            {
                Padding = Vector4.One * 10.0f * GUI.Scale,
                Color = Color.White * 0.25f,
                HoverColor = Color.White * 0.5f,
                SelectedColor = Color.White * 0.85f,
                OutlineColor = Color.White * 0.5f,
                TextColor = Color.White,
                SelectedTextColor = Color.Black,
                UserData = client
            };
            var soundIcon = new GUIImage(new RectTransform(new Point((int)(textBlock.Rect.Height * 0.8f)), textBlock.RectTransform, Anchor.CenterRight) { AbsoluteOffset = new Point(5, 0) },
                sprite: GUI.Style.GetComponentStyle("GUISoundIcon").GetDefaultSprite(), scaleToFit: true)
            {
                UserData = new Pair<string, float>("soundicon", 0.0f),
                CanBeFocused = false,
                Visible = true,
                OverrideState = GUIComponent.ComponentState.None,
                HoverColor = Color.White
            };

            new GUIImage(new RectTransform(new Point((int)(textBlock.Rect.Height * 0.8f)), textBlock.RectTransform, Anchor.CenterRight) { AbsoluteOffset = new Point(5, 0) },
                "GUISoundIconDisabled")
            {
                UserData = "soundicondisabled",
                CanBeFocused = true,
                Visible = false,
                OverrideState = GUIComponent.ComponentState.None,
                HoverColor = Color.White
            };
            new GUIFrame(new RectTransform(new Vector2(0.6f, 0.6f), textBlock.RectTransform, Anchor.CenterRight, scaleBasis: ScaleBasis.BothHeight) { AbsoluteOffset = new Point(10 + soundIcon.Rect.Width, 0) }, style: "GUIReadyToStart")
            {
                Visible = false,
                CanBeFocused = false,
                ToolTip = TextManager.Get("ReadyToStartTickBox"),
                UserData = "clientready"
            };
        }

        public void SetPlayerNameAndJobPreference(Client client)
        {
            var playerFrame = (GUITextBlock)PlayerList.Content.FindChild(client);
            if (playerFrame == null) { return; }
            playerFrame.Text = client.Name;

            playerFrame.ToolTip = "";
            Color color = Color.White;
            if (SelectedMode == GameModePreset.PvP)
            {
                switch (client.PreferredTeam)
                {
                    case CharacterTeamType.Team1:
                        color = new Color(0, 110, 150, 255);
                        playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("teampreference.team1"));
                        break;
                    case CharacterTeamType.Team2:
                        color = new Color(150, 110, 0, 255);
                        playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("teampreference.team2"));
                        break;
                    default:
                        playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("none"));
                        break;
                }
            }
            else
            {
                if (JobPrefab.Prefabs.ContainsKey(client.PreferredJob))
                {
                    color = JobPrefab.Prefabs[client.PreferredJob].UIColor;
                    playerFrame.ToolTip = TextManager.GetWithVariable("jobpreference", "[job]", JobPrefab.Prefabs[client.PreferredJob].Name);
                }
                else
                {
                    playerFrame.ToolTip = TextManager.GetWithVariable("jobpreference", "[job]", TextManager.Get("none"));
                }
            }
            playerFrame.Color = color * 0.4f;
            playerFrame.HoverColor = color * 0.6f;
            playerFrame.SelectedColor = color * 0.8f;
            playerFrame.OutlineColor = color * 0.5f;
            playerFrame.TextColor = color;
        }

        public void SetPlayerVoiceIconState(Client client, bool muted, bool mutedLocally)
        {
            var PlayerFrame = PlayerList.Content.FindChild(client);
            if (PlayerFrame == null) { return; }
            var soundIcon = PlayerFrame.FindChild(c => c.UserData is Pair<string, float> pair && pair.First == "soundicon");
            var soundIconDisabled = PlayerFrame.FindChild("soundicondisabled");

            Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;

            if (!soundIcon.Visible)
            {
                userdata.Second = 0.0f;
            }
            soundIcon.Visible = !muted && !mutedLocally;
            soundIconDisabled.Visible = muted || mutedLocally;
            soundIconDisabled.ToolTip = TextManager.Get(mutedLocally ? "MutedLocally" : "MutedGlobally");
        }

        public void SetPlayerSpeaking(Client client)
        {
            var PlayerFrame = PlayerList.Content.FindChild(client);
            if (PlayerFrame == null) { return; }
            var soundIcon = PlayerFrame.FindChild(c => c.UserData is Pair<string, float> pair && pair.First == "soundicon");
            Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
            userdata.Second = Math.Max(userdata.Second,   0.18f);
            soundIcon.Visible = true;
        }

        public void RemovePlayer(Client client)
        {
            GUIComponent child = playerList.Content.GetChildByUserData(client);
            if (child != null) { playerList.RemoveChild(child); }
        }

        public void SelectPlayer(GUITextBlock component, GUITextBlock.ClickableArea area)
        {
            if (!UInt64.TryParse(area.Data.Metadata, out UInt64 id)) { return; }
            Client client = GameMain.Client.ConnectedClients.Find(c => c.SteamID == id)
                ?? GameMain.Client.ConnectedClients.Find(c => c.ID == id)
                ?? GameMain.Client.PreviouslyConnectedClients.FirstOrDefault(c => c.SteamID == id)
                ?? GameMain.Client.PreviouslyConnectedClients.FirstOrDefault(c => c.ID == id);
            if (client == null) { return; }
            GameMain.NetLobbyScreen.SelectPlayer(client);
        }

        public bool SelectPlayer(Client selectedClient)
        {
            bool myClient = selectedClient.ID == GameMain.Client.ID;
            bool hasManagePermissions = GameMain.Client.HasPermission(ClientPermissions.ManagePermissions);

            PlayerFrame = new GUIButton(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center), style: null)
            {
                OnClicked = (btn, userdata) => { if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock) ClosePlayerFrame(btn, userdata); return true; }
            };

            new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, PlayerFrame.RectTransform, Anchor.Center), style: "GUIBackgroundBlocker");
            Vector2 frameSize = hasManagePermissions ? new Vector2(.28f, .5f) : new Vector2(.28f, .15f);

            var playerFrameInner = new GUIFrame(new RectTransform(frameSize, PlayerFrame.RectTransform, Anchor.Center) { MinSize = new Point(550, 0) });
            var paddedPlayerFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.88f), playerFrameInner.RectTransform, Anchor.Center))
            {
                Stretch = true,
                RelativeSpacing = 0.03f
            };

            var headerContainer = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, hasManagePermissions ? 0.1f : 0.25f), paddedPlayerFrame.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft)
            {
                Stretch = true
            };

            var nameText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), headerContainer.RectTransform),
                text: selectedClient.Name, font: GUI.LargeFont);
            nameText.Text = ToolBox.LimitString(nameText.Text, nameText.Font, (int)(nameText.Rect.Width * 0.95f));

            if (hasManagePermissions)
            {
                PlayerFrame.UserData = selectedClient;

                new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.05f), paddedPlayerFrame.RectTransform),
                    TextManager.Get("Rank"), font: GUI.SubHeadingFont);
                var rankDropDown = new GUIDropDown(new RectTransform(new Vector2(1.0f, 0.1f), paddedPlayerFrame.RectTransform),
                    TextManager.Get("Rank"))
                {
                    UserData = selectedClient,
                    Enabled = !myClient
                };
                foreach (PermissionPreset permissionPreset in PermissionPreset.List)
                {
                    rankDropDown.AddItem(permissionPreset.Name, permissionPreset, permissionPreset.Description);
                }
                rankDropDown.AddItem(TextManager.Get("CustomRank"), null);

                PermissionPreset currentPreset = PermissionPreset.List.Find(p =>
                    p.Permissions == selectedClient.Permissions &&
                    p.PermittedCommands.Count == selectedClient.PermittedConsoleCommands.Count && !p.PermittedCommands.Except(selectedClient.PermittedConsoleCommands).Any());
                rankDropDown.SelectItem(currentPreset);

                rankDropDown.OnSelected += (c, userdata) =>
                {
                    PermissionPreset selectedPreset = (PermissionPreset)userdata;
                    if (selectedPreset != null)
                    {
                        var client = PlayerFrame.UserData as Client;
                        client.SetPermissions(selectedPreset.Permissions, selectedPreset.PermittedCommands);
                        GameMain.Client.UpdateClientPermissions(client);

                        PlayerFrame = null;
                        SelectPlayer(client);
                    }
                    return true;
                };

                var permissionLabels = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), paddedPlayerFrame.RectTransform), isHorizontal: true)
                {
                    Stretch = true,
                    RelativeSpacing = 0.05f
                };
                var permissionLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), permissionLabels.RectTransform), TextManager.Get("Permissions"), font: GUI.SubHeadingFont);
                var consoleCommandLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1.0f), permissionLabels.RectTransform),
                    TextManager.Get("PermittedConsoleCommands"), wrap: true, font: GUI.SubHeadingFont);
                GUITextBlock.AutoScaleAndNormalize(permissionLabel, consoleCommandLabel);

                var permissionContainer = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.4f), paddedPlayerFrame.RectTransform), isHorizontal: true)
                {
                    Stretch = true,
                    RelativeSpacing = 0.05f
                };

                var listBoxContainerLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), permissionContainer.RectTransform))
                {
                    Stretch = true,
                    RelativeSpacing = 0.05f
                };

                new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), listBoxContainerLeft.RectTransform), TextManager.Get("all", fallBackTag: "clientpermission.all"))
                {
                    Enabled = !myClient,
                    OnSelected = (tickbox) =>
                    {
                        //reset rank to custom
                        rankDropDown.SelectItem(null);

                        if (!(PlayerFrame.UserData is Client client)) { return false; }

                        foreach (GUIComponent child in tickbox.Parent.GetChild<GUIListBox>().Content.Children)
                        {
                            var permissionTickBox = child as GUITickBox;
                            permissionTickBox.Enabled = false;
                            permissionTickBox.Selected = tickbox.Selected;
                            permissionTickBox.Enabled = true;
                        }
                        GameMain.Client.UpdateClientPermissions(client);
                        return true;
                    }
                };
                var permissionsBox = new GUIListBox(new RectTransform(Vector2.One, listBoxContainerLeft.RectTransform))
                {
                    UserData = selectedClient
                };

                foreach (ClientPermissions permission in Enum.GetValues(typeof(ClientPermissions)))
                {
                    if (permission == ClientPermissions.None || permission == ClientPermissions.All) continue;

                    var permissionTick = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), permissionsBox.Content.RectTransform),
                        TextManager.Get("ClientPermission." + permission), font: GUI.SmallFont)
                    {
                        UserData = permission,
                        Selected = selectedClient.HasPermission(permission),
                        Enabled = !myClient,
                        OnSelected = (tickBox) =>
                        {
                            //reset rank to custom
                            rankDropDown.SelectItem(null);

                            if (!(PlayerFrame.UserData is Client client)) { return false; }

                            var thisPermission = (ClientPermissions)tickBox.UserData;
                            if (tickBox.Selected)
                            {
                                client.GivePermission(thisPermission);
                            }
                            else
                            {
                                client.RemovePermission(thisPermission);
                            }
                            if (tickBox.Enabled)
                            {
                                GameMain.Client.UpdateClientPermissions(client);
                            }
                            return true;
                        }
                    };
                }

                var listBoxContainerRight = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1.0f), permissionContainer.RectTransform))
                {
                    Stretch = true,
                    RelativeSpacing = 0.05f
                };

                new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), listBoxContainerRight.RectTransform), TextManager.Get("all", fallBackTag: "clientpermission.all"))
                {
                    Enabled = !myClient,
                    OnSelected = (tickbox) =>
                    {
                        //reset rank to custom
                        rankDropDown.SelectItem(null);

                        if (!(PlayerFrame.UserData is Client client)) { return false; }

                        foreach (GUIComponent child in tickbox.Parent.GetChild<GUIListBox>().Content.Children)
                        {
                            var commandTickBox = child as GUITickBox;
                            commandTickBox.Enabled = false;
                            commandTickBox.Selected = tickbox.Selected;
                            commandTickBox.Enabled = true;
                        }
                        GameMain.Client.UpdateClientPermissions(client);
                        return true;
                    }
                };
                var commandList = new GUIListBox(new RectTransform(Vector2.One, listBoxContainerRight.RectTransform))
                {
                    UserData = selectedClient
                };
                foreach (DebugConsole.Command command in DebugConsole.Commands)
                {
                    var commandTickBox = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), commandList.Content.RectTransform),
                        command.names[0], font: GUI.SmallFont)
                    {
                        Selected = selectedClient.PermittedConsoleCommands.Contains(command),
                        Enabled = !myClient,
                        ToolTip = command.help,
                        UserData = command
                    };
                    commandTickBox.OnSelected += (GUITickBox tickBox) =>
                    {
                        //reset rank to custom
                        rankDropDown.SelectItem(null);

                        DebugConsole.Command selectedCommand = tickBox.UserData as DebugConsole.Command;
                        if (!(PlayerFrame.UserData is Client client)) { return false; }

                        if (!tickBox.Selected)
                        {
                            client.PermittedConsoleCommands.Remove(selectedCommand);
                        }
                        else if (!client.PermittedConsoleCommands.Contains(selectedCommand))
                        {
                            client.PermittedConsoleCommands.Add(selectedCommand);
                        }
                        if (tickBox.Enabled)
                        {
                            GameMain.Client.UpdateClientPermissions(client);
                        }
                        return true;
                    };
                }
            }

            var buttonAreaTop = myClient ? null : new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.08f), paddedPlayerFrame.RectTransform), isHorizontal: true);
            var buttonAreaLower = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.08f), paddedPlayerFrame.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft);

            if (!myClient)
            {
                if (GameMain.Client.HasPermission(ClientPermissions.Ban))
                {
                    var banButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1.0f), buttonAreaTop.RectTransform),
                        TextManager.Get("Ban"))
                    {
                        UserData = selectedClient
                    };
                    banButton.OnClicked = (bt, userdata) => { BanPlayer(selectedClient); return true; };
                    banButton.OnClicked += ClosePlayerFrame;

                    var rangebanButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1.0f), buttonAreaTop.RectTransform),
                        TextManager.Get("BanRange"))
                    {
                        UserData = selectedClient
                    };
                    rangebanButton.OnClicked = (bt, userdata) => { BanPlayerRange(selectedClient); return true; };
                    rangebanButton.OnClicked += ClosePlayerFrame;
                }

                if (GameMain.Client != null && GameMain.Client.ConnectedClients.Contains(selectedClient))
                {
                    if (GameMain.Client.ServerSettings.Voting.AllowVoteKick &&
                        selectedClient != null && selectedClient.AllowKicking)
                    {
                        var kickVoteButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1.0f), buttonAreaLower.RectTransform),
                            TextManager.Get("VoteToKick"))
                        {
                            Enabled = !selectedClient.HasKickVoteFromID(GameMain.Client.ID),
                            OnClicked = (btn, userdata) => { GameMain.Client.VoteForKick(selectedClient); btn.Enabled = false; return true; },
                            UserData = selectedClient
                        };
                    }

                    if (GameMain.Client.HasPermission(ClientPermissions.Kick) &&
                        selectedClient != null && selectedClient.AllowKicking)
                    {
                        var kickButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1.0f), buttonAreaLower.RectTransform),
                            TextManager.Get("Kick"))
                        {
                            UserData = selectedClient
                        };
                        kickButton.OnClicked = (bt, userdata) => { KickPlayer(selectedClient); return true; };
                        kickButton.OnClicked += ClosePlayerFrame;
                    }

                    new GUITickBox(new RectTransform(new Vector2(0.175f, 1.0f), headerContainer.RectTransform, Anchor.TopRight),
                        TextManager.Get("Mute"))
                    {
                        Selected = selectedClient.MutedLocally,
                        OnSelected = (tickBox) => { selectedClient.MutedLocally = tickBox.Selected; return true; }
                    };
                }

                if (buttonAreaTop.CountChildren > 0)
                {
                    GUITextBlock.AutoScaleAndNormalize(buttonAreaTop.Children.Select(c => ((GUIButton)c).TextBlock).Concat(buttonAreaLower.Children.Select(c => ((GUIButton)c).TextBlock)));
                }
            }

            if (selectedClient.SteamID != 0 && Steam.SteamManager.IsInitialized)
            {
                var viewSteamProfileButton = new GUIButton(new RectTransform(new Vector2(0.3f, 1.0f), headerContainer.RectTransform, Anchor.TopCenter) { MaxSize = new Point(int.MaxValue, (int)(40 * GUI.Scale)) },
                        TextManager.Get("ViewSteamProfile"))
                {
                    UserData = selectedClient
                };
                viewSteamProfileButton.TextBlock.AutoScaleHorizontal = true;
                viewSteamProfileButton.OnClicked = (bt, userdata) =>
                {
                    Steamworks.SteamFriends.OpenWebOverlay("https://steamcommunity.com/profiles/" + selectedClient.SteamID.ToString());
                    return true;
                };
            }

            var closeButton = new GUIButton(new RectTransform(new Vector2(0f, 1.0f), buttonAreaLower.RectTransform, Anchor.CenterRight),
                TextManager.Get("Close"))
            {
                IgnoreLayoutGroups = true,
                OnClicked = ClosePlayerFrame
            };

            float xSize = 1f / buttonAreaLower.CountChildren;
            for (int i = 0; i < buttonAreaLower.CountChildren; i++)
            {
                buttonAreaLower.GetChild(i).RectTransform.RelativeSize = new Vector2(xSize, 1f);
            }

            buttonAreaLower.RectTransform.NonScaledSize = new Point(buttonAreaLower.Rect.Width, buttonAreaLower.RectTransform.Children.Max(c => c.NonScaledSize.Y));

            if (buttonAreaTop != null)
            {
                if (buttonAreaTop.CountChildren == 0)
                {
                    paddedPlayerFrame.RemoveChild(buttonAreaTop);
                }
                else
                {
                    for (int i = 0; i < buttonAreaTop.CountChildren; i++)
                    {
                        buttonAreaTop.GetChild(i).RectTransform.RelativeSize = new Vector2(1f / 3f, 1f);
                    }

                    buttonAreaTop.RectTransform.NonScaledSize =
                    buttonAreaLower.RectTransform.NonScaledSize =
                         new Point(buttonAreaLower.Rect.Width, Math.Max(buttonAreaLower.RectTransform.NonScaledSize.Y, buttonAreaTop.RectTransform.Children.Max(c => c.NonScaledSize.Y)));
                }
            }

            return false;
        }

        private bool ClosePlayerFrame(GUIButton button, object userData)
        {
            PlayerFrame = null;
            playerList.Deselect();
            return true;
        }

        public void KickPlayer(Client client)
        {
            if (GameMain.NetworkMember == null || client == null) { return; }
            GameMain.Client.CreateKickReasonPrompt(client.Name, false);
        }

        public void BanPlayer(Client client)
        {
            if (GameMain.NetworkMember == null || client == null) { return; }
            GameMain.Client.CreateKickReasonPrompt(client.Name, ban: true, rangeBan: false);
        }

        public void BanPlayerRange(Client client)
        {
            if (GameMain.NetworkMember == null || client == null) { return; }
            GameMain.Client.CreateKickReasonPrompt(client.Name, ban: true, rangeBan: true);
        }

        public override void AddToGUIUpdateList()
        {
            base.AddToGUIUpdateList();

            //CampaignSetupUI?.AddToGUIUpdateList();
            JobInfoFrame?.AddToGUIUpdateList();

            CharacterAppearanceCustomizationMenu?.AddToGUIUpdateList();
            JobSelectionFrame?.AddToGUIUpdateList();
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (GameMain.Client == null) { return; }

            string currMicStyle = micIcon.Style.Element.Name.LocalName;

            string targetMicStyle = "GUIMicrophoneEnabled";
            if (GameMain.Config.CaptureDeviceNames == null)
            {
                GameMain.Config.CaptureDeviceNames = OpenAL.Alc.GetStringList(IntPtr.Zero, OpenAL.Alc.CaptureDeviceSpecifier);
            }

            if (GameMain.Config.CaptureDeviceNames.Count == 0)
            {
                targetMicStyle = "GUIMicrophoneUnavailable";
            }
            else if (GameMain.Config.VoiceSetting == GameSettings.VoiceMode.Disabled)
            {
                targetMicStyle = "GUIMicrophoneDisabled";
            }

            if (!targetMicStyle.Equals(currMicStyle, StringComparison.OrdinalIgnoreCase))
            {
                GUI.Style.Apply(micIcon, targetMicStyle);
            }

            foreach (GUIComponent child in playerList.Content.Children)
            {
                if (child.UserData is Client client)
                {
                    if (child.FindChild(c => c.UserData is Pair<string, float> pair && pair.First == "soundicon") is GUIImage soundIcon)
                    {
                        double voipAmplitude = 0.0f;
                        if (client.ID != GameMain.Client.ID)
                        {
                            voipAmplitude = client.VoipSound?.CurrentAmplitude ?? 0.0f;
                        }
                        else
                        {
                            var voip = VoipCapture.Instance;
                            if (voip == null)
                            {
                                voipAmplitude = 0;
                            }
                            else if (voip.LastEnqueueAudio > DateTime.Now - new TimeSpan(0, 0, 0, 0, milliseconds: 100))
                            {
                                voipAmplitude = voip.LastAmplitude;
                            }
                        }
                        VoipClient.UpdateVoiceIndicator(soundIcon, (float)voipAmplitude, (float)deltaTime);
                    }
                }
            }

            autoRestartText.Visible = autoRestartTimer > 0.0f && autoRestartBox.Selected;
            if (!MathUtils.NearlyEqual(autoRestartTimer, 0.0f) && autoRestartBox.Selected)
            {
                autoRestartTimer = Math.Max(autoRestartTimer - (float)deltaTime, 0.0f);
                if (autoRestartTimer > 0.0f)
                {
                    autoRestartText.Text = TextManager.Get("RestartingIn") + " " + ToolBox.SecondsToReadableTime(Math.Max(autoRestartTimer, 0));
                }
            }

            CharacterAppearanceCustomizationMenu?.Update();
            if (JobSelectionFrame != null && PlayerInput.PrimaryMouseButtonDown() && !GUI.IsMouseOn(JobSelectionFrame))
            {
                JobList.Deselect();
                JobSelectionFrame.Visible = false;
            }

            if (GUI.MouseOn?.UserData is Pair<JobPrefab, int> jobPrefab && GUI.MouseOn.Style?.Name == "JobVariantButton")
            {
                var prevVisibleVariant = jobVariantTooltip?.UserData as Pair<JobPrefab, int>;
                if (jobVariantTooltip == null || prevVisibleVariant.First != jobPrefab.First || prevVisibleVariant.Second != jobPrefab.Second)
                {
                    CreateJobVariantTooltip(jobPrefab.First, jobPrefab.Second, GUI.MouseOn.Parent);
                }
            }
            if (jobVariantTooltip != null)
            {
                jobVariantTooltip?.AddToGUIUpdateList();
                Rectangle mouseRect = jobVariantTooltip.MouseRect;
                mouseRect.Inflate(60 * GUI.Scale, 60 * GUI.Scale);
                if (!mouseRect.Contains(PlayerInput.MousePosition)) { jobVariantTooltip = null; }
            }
        }
        public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            GUI.DrawBackgroundSprite(spriteBatch, backgroundSprite);

            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);

            GUI.Draw(Cam, spriteBatch);
            spriteBatch.End();
        }

        private PlayStyle? prevPlayStyle = null;
        private void DrawServerBanner(SpriteBatch spriteBatch, GUICustomComponent component)
        {
            if (GameMain.NetworkMember?.ServerSettings == null) { return; }

            PlayStyle playStyle = GameMain.NetworkMember.ServerSettings.PlayStyle;
            if ((int)playStyle < 0 ||
                (int)playStyle >= ServerListScreen.PlayStyleBanners.Length)
            {
                return;
            }

            Sprite sprite = ServerListScreen.PlayStyleBanners[(int)playStyle];
            float scale = component.Rect.Width / sprite.size.X;
            sprite.Draw(spriteBatch, component.Center, scale: scale);

            if (!prevPlayStyle.HasValue || playStyle != prevPlayStyle.Value)
            {
                var nameText = component.GetChild<GUITextBlock>();
                nameText.Text = TextManager.Get("servertag." + playStyle);
                nameText.Color = ServerListScreen.PlayStyleColors[(int)playStyle];
                nameText.RectTransform.NonScaledSize = (nameText.Font.MeasureString(nameText.Text) + new Vector2(25, 10) * GUI.Scale).ToPoint();
                prevPlayStyle = playStyle;

                component.ToolTip = TextManager.Get("servertagdescription." + playStyle);
            }

            publicOrPrivate.RectTransform.NonScaledSize = (publicOrPrivate.Font.MeasureString(publicOrPrivate.Text) + new Vector2(25, 8) * GUI.Scale).ToPoint();
        }

        private void DrawJobVariantItems(SpriteBatch spriteBatch, GUICustomComponent component, Pair<JobPrefab, int> jobPrefab, int itemsPerRow)
        {
            var itemIdentifiers = jobPrefab.First.PreviewItems[jobPrefab.Second]
                .Where(it => it.ShowPreview)
                .Select(it => it.ItemIdentifier)
                .Distinct();

            Point slotSize = new Point(component.Rect.Height);
            int spacing = (int)(5 * GUI.Scale);
            int slotCount = itemIdentifiers.Count();
            int slotCountPerRow = Math.Min(slotCount, itemsPerRow);
            int rows = (int)Math.Max(Math.Ceiling(itemIdentifiers.Count() / (float)itemsPerRow), 1);

            float totalWidth = slotSize.X * slotCountPerRow + spacing * (slotCountPerRow - 1);
            float totalHeight = slotSize.Y * rows + spacing * (rows - 1);
            if (totalWidth > component.Rect.Width)
            {
                slotSize = new Point(
                    Math.Min((int)Math.Floor((slotSize.X - spacing) * (component.Rect.Width / totalWidth)),
                        (int)Math.Floor((slotSize.Y - spacing) * (component.Rect.Height / totalHeight))));
            }
            int i = 0;
            Rectangle tooltipRect = Rectangle.Empty;
            string tooltip = null;
            foreach (var itemIdentifier in itemIdentifiers)
            {
                if (!(MapEntityPrefab.Find(null, identifier: itemIdentifier, showErrorMessages: false) is ItemPrefab itemPrefab)) { continue; }

                int row = (int)Math.Floor(i / (float)slotCountPerRow);
                int slotsPerThisRow = Math.Min((slotCount - row * slotCountPerRow), slotCountPerRow);
                Vector2 slotPos = new Vector2(
                    component.Rect.Center.X + (slotSize.X + spacing) * (i % slotCountPerRow - slotsPerThisRow * 0.5f),
                    component.Rect.Bottom - (rows * (slotSize.Y + spacing)) + (slotSize.Y + spacing) * row);

                Rectangle slotRect = new Rectangle(slotPos.ToPoint(), slotSize);
                Inventory.SlotSpriteSmall.Draw(spriteBatch, slotPos,
                    scale: slotSize.X / (float)Inventory.SlotSpriteSmall.SourceRect.Width,
                    color: slotRect.Contains(PlayerInput.MousePosition) ? Color.White : Color.White * 0.6f);

                Sprite icon = itemPrefab.InventoryIcon ?? itemPrefab.sprite;
                float iconScale = Math.Min(Math.Min(slotSize.X / icon.size.X, slotSize.Y / icon.size.Y), 2.0f) * 0.9f;
                icon.Draw(spriteBatch, slotPos + slotSize.ToVector2() * 0.5f, scale: iconScale);

                int count = jobPrefab.First.PreviewItems[jobPrefab.Second].Count(it => it.ShowPreview && it.ItemIdentifier == itemIdentifier);
                if (count > 1)
                {
                    string itemCountText = "x" + count;
                    GUI.Font.DrawString(spriteBatch, itemCountText, slotPos + slotSize.ToVector2() - GUI.Font.MeasureString(itemCountText) - Vector2.UnitX * 5, Color.White);
                }

                if (slotRect.Contains(PlayerInput.MousePosition))
                {
                    tooltipRect = slotRect;
                    tooltip = itemPrefab.Name + '\n' + itemPrefab.Description;
                }
                i++;
            }
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUIComponent.DrawToolTip(spriteBatch, tooltip, tooltipRect);
            }
        }

        public void NewChatMessage(ChatMessage message)
        {
            float prevSize = chatBox.BarSize;

            while (chatBox.Content.CountChildren > 60)
            {
                chatBox.RemoveChild(chatBox.Content.Children.First());
            }

            GUITextBlock msg = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.0f), chatBox.Content.RectTransform),
                text: ChatMessage.GetTimeStamp() + (message.Type == ChatMessageType.Private ? TextManager.Get("PrivateMessageTag") + " " : "") + message.TextWithSender,
                textColor: message.Color,
                color: ((chatBox.CountChildren % 2) == 0) ? Color.Transparent : Color.Black * 0.1f,
                wrap: true, font: GUI.SmallFont,
                parseRichText: true)
            {
                UserData = message,
                CanBeFocused = false
            };
            msg.CalculateHeightFromText();
            if (msg.RichTextData != null)
            {
                foreach (var data in msg.RichTextData)
                {
                    msg.ClickableAreas.Add(new GUITextBlock.ClickableArea()
                    {
                        Data = data,
                        OnClick = GameMain.NetLobbyScreen.SelectPlayer
                    });
                }
            }
            msg.RectTransform.SizeChanged += Recalculate;
            void Recalculate()
            {
                msg.RectTransform.SizeChanged -= Recalculate;
                msg.CalculateHeightFromText();
                msg.RectTransform.SizeChanged += Recalculate;
            }

            if ((prevSize == 1.0f && chatBox.BarScroll == 0.0f) || (prevSize < 1.0f && chatBox.BarScroll == 1.0f))
            {
                chatBox.BarScroll = 1.0f;
            }
        }

        private bool SelectJobPreferencesTab(GUIButton button, object userData)
        {
            jobPreferencesButton.Selected = true;
            appearanceButton.Selected = false;

            JobPreferenceContainer.Visible = true;
            appearanceFrame.Visible = false;

            return false;
        }

        private bool SelectAppearanceTab(GUIButton button, object _)
        {
            jobPreferencesButton.Selected = false;
            appearanceButton.Selected = true;

            JobPreferenceContainer.Visible = false;
            appearanceFrame.Visible = true;

            appearanceFrame.ClearChildren();

            var info = GameMain.Client.CharacterInfo;
            CharacterAppearanceCustomizationMenu = new CharacterInfo.AppearanceCustomizationMenu(info, appearanceFrame)
            {
                OnHeadSwitch = menu =>
                {
                    StoreHead(true);
                    UpdateJobPreferences();
                    SelectAppearanceTab(button, _);
                },
                OnSliderMoved = (bar, scroll) =>
                {
                    StoreHead(false);
                    return false;
                },
                OnSliderReleased = SaveHead
            };
            
            return false;
        }
        
        private bool SaveHead(GUIScrollBar scrollBar, float barScroll) => StoreHead(true);
        private bool StoreHead(bool save)
        {
            GameMain.Config.PlayerCharacterCustomization = GameMain.Client.CharacterInfo.Head;
            if (save)
            {
                if (GameMain.GameSession?.IsRunning ?? false)
                {
                    TabMenu.PendingChanges = true;
                    CreateChangesPendingText();
                }
                GameMain.Config.SaveNewPlayerConfig();
            }
            return true;
        }

        private bool SwitchJob(GUIButton _, object obj)
        {
            if (JobList == null) { return false; }

            int childIndex = JobList.SelectedIndex;
            var child = JobList.SelectedComponent;
            if (child == null) { return false; }

            bool moveToNext = obj != null;

            var jobPrefab = (obj as Pair<JobPrefab, int>)?.First;

            var prevObj = child.UserData;

            var existingChild = JobList.Content.FindChild(d => (d.UserData is Pair<JobPrefab, int> prefab) && (prefab.First == jobPrefab));
            if (existingChild != null && obj != null)
            {
                existingChild.UserData = prevObj;
            }
            child.UserData = obj;

            for (int i = 0; i < 2; i++)
            {
                if (i < 2 && JobList.Content.GetChild(i).UserData == null)
                {
                    JobList.Content.GetChild(i).UserData = JobList.Content.GetChild(i + 1).UserData;
                    JobList.Content.GetChild(i + 1).UserData = null;
                }
            }

            UpdateJobPreferences();

            if (moveToNext)
            {
                var emptyChild = JobList.Content.FindChild(c => c.UserData == null && c.CanBeFocused);
                if (emptyChild != null)
                {
                    JobList.Select(JobList.Content.GetChildIndex(emptyChild));
                }
                else
                {
                    JobList.Deselect();
                    if (JobSelectionFrame != null) { JobSelectionFrame.Visible = false; }
                }
            }
            else
            {
                OpenJobSelection(child, child.UserData);
            }

            return false;
        }

        private bool OpenJobSelection(GUIComponent _, object __)
        {
            if (JobSelectionFrame != null)
            {
                JobSelectionFrame.Visible = true;
                return true;
            }

            Point frameSize = new Point(characterInfoFrame.Rect.Width, (int)(characterInfoFrame.Rect.Height * 2 * 0.6f));
            JobSelectionFrame = new GUIFrame(new RectTransform(frameSize, GUI.Canvas, Anchor.TopLeft)
                { AbsoluteOffset = new Point(characterInfoFrame.Rect.Right - frameSize.X, characterInfoFrame.Rect.Bottom) }, style:"GUIFrameListBox");

            characterInfoFrame.RectTransform.SizeChanged += () =>
            {
                if (characterInfoFrame == null || JobSelectionFrame?.RectTransform == null) { return; }
                Point size = new Point(characterInfoFrame.Rect.Width, (int)(characterInfoFrame.Rect.Height * 2 * 0.6f));
                JobSelectionFrame.RectTransform.Resize(size);
                JobSelectionFrame.RectTransform.AbsoluteOffset = new Point(characterInfoFrame.Rect.Right - size.X, characterInfoFrame.Rect.Bottom);
            };

            new GUIFrame(new RectTransform(new Vector2(1.25f, 1.25f), JobSelectionFrame.RectTransform, anchor: Anchor.Center), style: "OuterGlow", color: Color.Black)
            {
                UserData = "outerglow",
                CanBeFocused = false
            };

            var rows = new GUILayoutGroup(new RectTransform(Vector2.One, JobSelectionFrame.RectTransform)) { Stretch = true };
            var row = new GUILayoutGroup(new RectTransform(Vector2.One, rows.RectTransform), true);

            GUIButton jobButton = null;

            var availableJobs = JobPrefab.Prefabs.Where(jobPrefab =>
                    jobPrefab.MaxNumber > 0 && JobList.Content.Children.All(c => !(c.UserData is Pair<JobPrefab, int> prefab) || prefab.First != jobPrefab)
            ).Select(j => new Pair<JobPrefab, int>(j, 0));

            availableJobs = availableJobs.Concat(
                JobPrefab.Prefabs.Where(jobPrefab =>
                    jobPrefab.MaxNumber > 0 && JobList.Content.Children.Any(c => (c.UserData is Pair<JobPrefab, int> prefab) && prefab.First == jobPrefab)
            ).Select(j => JobList.Content.FindChild(c => (c.UserData is Pair<JobPrefab, int> prefab) && prefab.First == j).UserData as Pair<JobPrefab, int>));

            availableJobs = availableJobs.ToList();

            int itemsInRow = 0;

            foreach (var jobPrefab in availableJobs)
            {
                if (itemsInRow >= 3)
                {
                    row = new GUILayoutGroup(new RectTransform(Vector2.One, rows.RectTransform), true);
                    itemsInRow = 0;
                }

                jobButton = new GUIButton(new RectTransform(new Vector2(1.0f / 3.0f, 1.0f), row.RectTransform), style: "ListBoxElementSquare")
                {
                    UserData = jobPrefab,
                    OnClicked = (btn, usdt) =>
                    {
                        if (btn.IsParentOf(GUI.MouseOn)) return false;
                        return SwitchJob(btn, usdt);
                    }
                };
                itemsInRow++;

                var images = AddJobSpritesToGUIComponent(jobButton, jobPrefab.First, selectedByPlayer: false);
                if (images != null && images.Length > 1)
                {
                    jobPrefab.Second = Math.Min(jobPrefab.Second, images.Length);
                    int currVisible = jobPrefab.Second;
                    GUIButton currSelected = null;
                    for (int variantIndex = 0; variantIndex < images.Length; variantIndex++)
                    {
                        foreach (GUIImage image in images[variantIndex])
                        {
                            image.Visible = currVisible == variantIndex;
                        }

                        var variantButton = CreateJobVariantButton(jobPrefab, variantIndex, images.Length, jobButton);
                        variantButton.OnClicked = (btn, obj) =>
                        {
                            if (currSelected != null) { currSelected.Selected = false; }
                            int k = ((Pair<JobPrefab, int>)obj).Second;
                            btn.Parent.UserData = obj;
                            for (int j = 0; j < images.Length; j++)
                            {
                                foreach (GUIImage image in images[j])
                                {
                                    image.Visible = k == j;
                                }
                            }
                            currSelected = btn;
                            currSelected.Selected = true;
                            return false;
                        };

                        if (currVisible == variantIndex)
                        {
                            currSelected = variantButton;
                        }
                    }

                    if (currSelected != null)
                    {
                        currSelected.Selected = true;
                    }
                }
            }

            return true;
        }

        private GUIImage[][] AddJobSpritesToGUIComponent(GUIComponent parent, JobPrefab jobPrefab, bool selectedByPlayer)
        {
            GUIFrame innerFrame = null;
            List<JobPrefab.OutfitPreview> outfitPreviews = jobPrefab.GetJobOutfitSprites(Gender.Male, useInventoryIcon: true, out var maxDimensions);

            innerFrame = new GUIFrame(new RectTransform(Vector2.One * 0.85f, parent.RectTransform, Anchor.Center), style: null)
            {
                CanBeFocused = false
            };

            GUIImage[][] retVal = new GUIImage[0][];
            if (outfitPreviews != null && outfitPreviews.Any())
            {
                retVal = new GUIImage[outfitPreviews.Count][];
                for (int i = 0; i < outfitPreviews.Count; i++)
                {
                    JobPrefab.OutfitPreview outfitPreview = outfitPreviews[i];
                    retVal[i] = new GUIImage[outfitPreview.Sprites.Count];
                    for (int j = 0; j < outfitPreview.Sprites.Count; j++)
                    {
                        Pair<Sprite, Vector2> sprite = outfitPreview.Sprites[j];
                        float aspectRatio = outfitPreview.Dimensions.Y / outfitPreview.Dimensions.X;
                        retVal[i][j] = new GUIImage(new RectTransform(new Vector2(0.7f / aspectRatio, 0.7f), innerFrame.RectTransform, Anchor.Center)
                            { RelativeOffset = sprite.Second / outfitPreview.Dimensions }, sprite.First, scaleToFit: true)
                        {
                            PressedColor = Color.White,
                            CanBeFocused = false
                        };
                    }
                }
            }

            new GUIFrame(new RectTransform(new Vector2(1.0f, 0.35f), parent.RectTransform, Anchor.BottomCenter), style: "OuterGlow")
            {
                Color = Color.Black,
                HoverColor = Color.Black,
                PressedColor = Color.Black,
                SelectedColor = Color.Black,
                CanBeFocused = false
            };

            var textBlock = new GUITextBlock(
              innerFrame.CountChildren == 0 ?
                  new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center) :
                  new RectTransform(new Vector2(selectedByPlayer ? 0.55f : 0.95f, 0.3f), parent.RectTransform, Anchor.BottomCenter),
              jobPrefab.Name, wrap: true, textAlignment: Alignment.BottomCenter)
            {
                Padding = Vector4.Zero,
                HoverColor = Color.Transparent,
                SelectedColor = Color.Transparent,
                TextColor = jobPrefab.UIColor,
                HoverTextColor = Color.Lerp(jobPrefab.UIColor, Color.White, 0.5f),
                CanBeFocused = false,
                AutoScaleHorizontal = true
            };
            textBlock.TextAlignment = textBlock.WrappedText.Contains('\n') ? Alignment.BottomCenter : Alignment.Center;
            textBlock.RectTransform.SizeChanged += () => { textBlock.TextScale = 1.0f; };

            return retVal;
        }

        public void SelectMode(int modeIndex)
        {
            if (modeIndex < 0 || modeIndex >= modeList.Content.CountChildren) { return; }

            if ((GameModePreset)modeList.Content.GetChild(modeIndex).UserData != GameModePreset.MultiPlayerCampaign)
            {
                ToggleCampaignMode(false);
            }

            var prevMode = modeList.Content.GetChild(selectedModeIndex).UserData as GameModePreset;

            if ((HighlightedModeIndex == selectedModeIndex || HighlightedModeIndex < 0) && modeList.SelectedIndex != modeIndex) { modeList.Select(modeIndex, true); }
            selectedModeIndex = modeIndex;

            if ((prevMode == GameModePreset.PvP) != (SelectedMode == GameModePreset.PvP))
            {
                UpdatePlayerFrame(null);
                GameMain.Client.ConnectedClients.ForEach(c => SetPlayerNameAndJobPreference(c));
            }

            if (SelectedMode != GameModePreset.MultiPlayerCampaign && GameMain.GameSession?.GameMode is CampaignMode && Selected == this)
            {
                GameMain.GameSession = null;
            }

            RefreshGameModeContent();
            RefreshEnabledElements();
        }

        public void HighlightMode(int modeIndex)
        {
            if (modeIndex < 0 || modeIndex >= modeList.Content.CountChildren) { return; }

            HighlightedModeIndex = modeIndex;
            RefreshGameModeContent();
            RefreshEnabledElements();
        }

        private void RefreshMissionTypes()
        {
            for (int i = 0; i < missionTypeTickBoxes.Length; i++)
            {
                MissionType missionType = (MissionType)(int)missionTypeTickBoxes[i].UserData;
                if (MissionPrefab.HiddenMissionClasses.Contains(missionType))
                {
                    missionTypeTickBoxes[i].Parent.Visible = false;
                    continue;
                }
                if (SelectedMode == GameModePreset.Mission)
                {
                    missionTypeTickBoxes[i].Parent.Visible = MissionPrefab.CoOpMissionClasses.ContainsKey(missionType);
                }
                else if (SelectedMode == GameModePreset.PvP)
                {
                    missionTypeTickBoxes[i].Parent.Visible = MissionPrefab.PvPMissionClasses.ContainsKey(missionType);
                }
            }
        }

        private void RefreshGameModeContent()
        {
            if (GameMain.Client == null) { return; }

            autoRestartBox.Parent.Visible = true;
            settingsBlocker.Visible = false;
            if (SelectedMode == GameModePreset.Mission || SelectedMode == GameModePreset.PvP)
            {
                MissionTypeFrame.Visible = true;
                CampaignFrame.Visible = CampaignSetupFrame.Visible = false;
                RefreshMissionTypes();
            }
            else if (SelectedMode == GameModePreset.MultiPlayerCampaign)
            {
                MissionTypeFrame.Visible = autoRestartBox.Parent.Visible = false;

                if (GameMain.GameSession?.GameMode is CampaignMode campaign && campaign.Map != null)
                {
                    //campaign running
                    settingsBlocker.Visible = true;
                    CampaignFrame.Visible = GameMain.Client.HasPermission(ClientPermissions.ManageCampaign);
                    ContinueCampaignButton.Enabled = !GameMain.Client.GameStarted && (GameMain.Client.HasPermission(ClientPermissions.ManageCampaign) || GameMain.Client.HasPermission(ClientPermissions.ManageRound));
                    QuitCampaignButton.Enabled = GameMain.Client.HasPermission(ClientPermissions.ManageCampaign);
                    CampaignSetupFrame.Visible = false;
                }
                else
                {
                    CampaignFrame.Visible = false;
                    CampaignSetupFrame.Visible = true;
                    if (!GameMain.Client.HasPermission(ClientPermissions.ManageCampaign))
                    {
                        CampaignSetupFrame.ClearChildren();
                        new GUITextBlock(new RectTransform(new Vector2(0.8f, 0.5f), CampaignSetupFrame.RectTransform, Anchor.Center),
                            TextManager.Get("campaignstarting"), font: GUI.SubHeadingFont, textAlignment: Alignment.Center, wrap: true);
                    }
                }
            }
            else
            {
                MissionTypeFrame.Visible = CampaignFrame.Visible = CampaignSetupFrame.Visible = false;
                CampaignFrame.Visible = CampaignSetupFrame.Visible = false;
            }

            ReadyToStartBox.Parent.Visible = !GameMain.Client.GameStarted;

            StartButton.Visible =
                GameMain.Client.HasPermission(ClientPermissions.ManageRound) &&
                !GameMain.Client.GameStarted &&
                !CampaignSetupFrame.Visible &&
                !CampaignFrame.Visible;
        }

        public void ToggleCampaignMode(bool enabled)
        {
            if (!enabled)
            {
                //remove campaign character from the panel
                if (campaignCharacterInfo != null) { UpdatePlayerFrame(null); }
                campaignCharacterInfo = null;
                CampaignCharacterDiscarded = false;
            }
            else
            {
                CampaignFrame.Visible = CampaignSetupFrame.Visible = false;
            }
            RefreshEnabledElements();
            if (enabled)
            {
                modeList.Select(GameModePreset.MultiPlayerCampaign, true);
            }
        }

        public void TryDisplayCampaignSubmarine(SubmarineInfo submarine)
        {
            string name = submarine?.Name;
            bool displayed = false;
            subList.OnSelected -= VotableClicked;
            subList.Deselect();
            subPreviewContainer.ClearChildren();
            foreach (GUIComponent child in subList.Content.Children)
            {
                if (!(child.UserData is SubmarineInfo sub)) { continue; }
                //just check the name, even though the campaign sub may not be the exact same version
                //we're selecting the sub just for show, the selection is not actually used for anything
                if (sub.Name == name)
                {
                    subList.Select(sub);
                    if (SubmarineInfo.SavedSubmarines.Contains(sub))
                    {
                        CreateSubPreview(sub);
                        displayed = true;
                    }
                    break;
                }
            }
            subList.OnSelected += VotableClicked;
            if (!displayed)
            {
                CreateSubPreview(submarine);
            }
        }

        private bool ViewJobInfo(GUIButton button, object obj)
        {
            if (!(button.UserData is Pair<JobPrefab, int> jobPrefab)) { return false; }

            JobInfoFrame = jobPrefab.First.CreateInfoFrame(out GUIComponent buttonContainer);
            GUIButton closeButton = new GUIButton(new RectTransform(new Vector2(0.25f, 0.05f), buttonContainer.RectTransform, Anchor.BottomRight),
                TextManager.Get("Close"))
            {
                OnClicked = CloseJobInfo
            };
            JobInfoFrame.OnClicked = (btn, userdata) => { if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock) CloseJobInfo(btn, userdata); return true; };

            return true;
        }

        private bool CloseJobInfo(GUIButton button, object obj)
        {
            JobInfoFrame = null;
            return true;
        }

        private void UpdateJobPreferences()
        {
            GUICustomComponent characterIcon = JobPreferenceContainer.GetChild<GUICustomComponent>();
            JobPreferenceContainer.RemoveChild(characterIcon);
            GameMain.Client.CharacterInfo.CreateIcon(new RectTransform(new Vector2(1.0f, 0.4f), JobPreferenceContainer.RectTransform, Anchor.TopCenter) { RelativeOffset = new Vector2(0.0f, 0.025f) });

            GUIListBox listBox = JobPreferenceContainer.GetChild<GUIListBox>();
            /*foreach (Sprite sprite in jobPreferenceSprites) { sprite.Remove(); }
            jobPreferenceSprites.Clear();*/

            List<Pair<string, int>> jobNamePreferences = new List<Pair<string, int>>();

            bool disableNext = false;
            for (int i = 0; i < listBox.Content.CountChildren; i++)
            {
                GUIComponent slot = listBox.Content.GetChild(i);

                slot.ClearChildren();

                slot.CanBeFocused = !disableNext;
                if (slot.UserData is Pair<JobPrefab, int> jobPrefab)
                {
                    var images = AddJobSpritesToGUIComponent(slot, jobPrefab.First, selectedByPlayer: true);
                    for (int variantIndex = 0; variantIndex < images.Length; variantIndex++)
                    {
                        foreach (GUIImage image in images[variantIndex])
                        {
                            //jobPreferenceSprites.Add(image.Sprite);
                            int selectedVariantIndex = Math.Min(jobPrefab.Second, images.Length);
                            image.Visible = images.Length == 1 || selectedVariantIndex == variantIndex;
                        }
                        if (images.Length > 1)
                        {
                            var variantButton = CreateJobVariantButton(jobPrefab, variantIndex, images.Length, slot);
                            variantButton.OnClicked = (btn, obj) =>
                            {
                                btn.Parent.UserData = obj;
                                UpdateJobPreferences();
                                return false;
                            };
                        }
                    }

                    // Info button
                    new GUIButton(new RectTransform(new Vector2(0.15f), slot.RectTransform, Anchor.BottomLeft, scaleBasis: ScaleBasis.BothWidth) { RelativeOffset = new Vector2(0.075f) },
                        style: "GUIButtonInfo")
                    {
                        UserData = jobPrefab,
                        OnClicked = ViewJobInfo
                    };

                    // Remove button
                    new GUIButton(new RectTransform(new Vector2(0.15f), slot.RectTransform, Anchor.BottomRight, scaleBasis: ScaleBasis.BothWidth) { RelativeOffset = new Vector2(0.075f) },
                        style: "GUICancelButton")
                    {
                        UserData = i,
                        OnClicked = (btn, obj) =>
                        {
                            JobList.Select((int)obj, true);
                            SwitchJob(btn, null);
                            if (JobSelectionFrame != null) { JobSelectionFrame.Visible = false; }
                            JobList.Deselect();

                            return false;
                        }
                    };

                    jobNamePreferences.Add(new Pair<string, int>(jobPrefab.First.Identifier, jobPrefab.Second));
                }
                else
                {
                    new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.6f), slot.RectTransform), (i + 1).ToString(),
                        textColor: Color.White * (disableNext ? 0.15f : 0.5f),
                        textAlignment: Alignment.Center,
                        font: GUI.LargeFont)
                    {
                        CanBeFocused = false
                    };

                    if (!disableNext)
                    {
                        new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.4f), slot.RectTransform, Anchor.BottomCenter), TextManager.Get("clicktoselectjob"),
                            font: GUI.SmallFont,
                            wrap: true,
                            textAlignment: Alignment.Center)
                        {
                            CanBeFocused = false
                        };
                    }

                    disableNext = true;
                }
            }
            GameMain.Client.ForceNameAndJobUpdate();

            if (!GameMain.Config.AreJobPreferencesEqual(jobNamePreferences))
            {
                if (GameMain.GameSession?.IsRunning ?? false)
                {
                    TabMenu.PendingChanges = true;
                    CreateChangesPendingText();
                }

                GameMain.Config.JobPreferences = jobNamePreferences;
                GameMain.Config.SaveNewPlayerConfig();
            }
        }

        private GUIButton CreateJobVariantButton(Pair<JobPrefab, int> jobPrefab, int variantIndex, int variantCount, GUIComponent slot)
        {
            float relativeSize = 0.15f;

            var btn = new GUIButton(new RectTransform(new Vector2(relativeSize), slot.RectTransform, Anchor.TopCenter, scaleBasis: ScaleBasis.BothHeight)
                { RelativeOffset = new Vector2(relativeSize * 1.3f * (variantIndex - (variantCount - 1) / 2.0f), 0.02f) },
                (variantIndex + 1).ToString(), style: "JobVariantButton")
            {
                Selected = jobPrefab.Second == variantIndex,
                UserData = new Pair<JobPrefab, int>(jobPrefab.First, variantIndex),
            };

            return btn;
        }

        public Pair<string, string> FailedSelectedSub;
        public Pair<string, string> FailedSelectedShuttle;

        public List<Pair<string, string>> FailedCampaignSubs = new List<Pair<string, string>>();
        public List<Pair<string, string>> FailedOwnedSubs = new List<Pair<string, string>>();

        public bool TrySelectSub(string subName, string md5Hash, GUIListBox subList)
        {
            if (GameMain.Client == null) { return false; }

            //already downloading the selected sub file
            if (GameMain.Client.FileReceiver.ActiveTransfers.Any(t => t.FileName == subName + ".sub"))
            {
                return false;
            }

            SubmarineInfo sub = subList.Content.Children
                .FirstOrDefault(c => c.UserData is SubmarineInfo s && s.Name == subName && s.MD5Hash?.Hash == md5Hash)?
                .UserData as SubmarineInfo;

            //matching sub found and already selected, all good
            if (sub != null)
            {
                if (subList == this.subList)
                {
                    CreateSubPreview(sub);
                }

                if (subList.SelectedData is SubmarineInfo selectedSub && selectedSub.MD5Hash?.Hash == md5Hash && Barotrauma.IO.File.Exists(sub.FilePath))
                {
                    return true;
                }
            }

            //sub not found, see if we have a sub with the same name
            if (sub == null)
            {
                sub = subList.Content.Children
                    .FirstOrDefault(c => c.UserData is SubmarineInfo s && s.Name == subName)?
                    .UserData as SubmarineInfo;
            }

            //found a sub that at least has the same name, select it
            if (sub != null)
            {
                if (subList.Parent is GUIDropDown subDropDown)
                {
                    subDropDown.SelectItem(sub);
                }
                else
                {
                    subList.OnSelected -= VotableClicked;
                    subList.Select(sub, force: true);
                    subList.OnSelected += VotableClicked;
                }

                if (subList == SubList)
                    FailedSelectedSub = null;
                else
                    FailedSelectedShuttle = null;

                //hashes match, all good
                if (sub.MD5Hash?.Hash == md5Hash && SubmarineInfo.SavedSubmarines.Contains(sub))
                {
                    return true;
                }
            }

            //-------------------------------------------------------------------------------------
            //if we get to this point, a matching sub was not found or it has an incorrect MD5 hash

            if (subList == SubList)
                FailedSelectedSub = new Pair<string, string>(subName, md5Hash);
            else
                FailedSelectedShuttle = new Pair<string, string>(subName, md5Hash);

            string errorMsg = "";
            if (sub == null || !SubmarineInfo.SavedSubmarines.Contains(sub))
            {
                errorMsg = TextManager.GetWithVariable("SubNotFoundError", "[subname]", subName) + " ";
            }
            else if (sub.MD5Hash?.Hash == null)
            {
                errorMsg = TextManager.GetWithVariable("SubLoadError", "[subname]", subName) + " ";
                GUITextBlock textBlock = subList.Content.GetChildByUserData(sub)?.GetChild<GUITextBlock>();
                if (textBlock != null) { textBlock.TextColor = GUI.Style.Red; }
            }
            else
            {
                errorMsg = TextManager.GetWithVariables("SubDoesntMatchError", new string[3] { "[subname]" , "[myhash]", "[serverhash]" },
                    new string[3] { sub.Name, sub.MD5Hash.ShortHash, Md5Hash.GetShortHash(md5Hash) }) + " ";
            }

            //already showing a message about the same sub
            if (GUIMessageBox.MessageBoxes.Any(mb => mb.UserData as string == "request" + subName))
            {
                return false;
            }

            if (GameMain.Client.ServerSettings.AllowFileTransfers)
            {
                errorMsg += TextManager.Get("DownloadSubQuestion");

                var requestFileBox = new GUIMessageBox(TextManager.Get("DownloadSubLabel"), errorMsg,
                    new string[] { TextManager.Get("Yes"), TextManager.Get("No") })
                {
                    UserData = "request" + subName
                };
                requestFileBox.Buttons[0].UserData = new string[] { subName, md5Hash };
                requestFileBox.Buttons[0].OnClicked += requestFileBox.Close;
                requestFileBox.Buttons[0].OnClicked += (GUIButton button, object userdata) =>
                {
                    string[] fileInfo = (string[])userdata;
                    GameMain.Client?.RequestFile(FileTransferType.Submarine, fileInfo[0], fileInfo[1]);
                    return true;
                };
                requestFileBox.Buttons[1].OnClicked += requestFileBox.Close;
            }
            else
            {
                new GUIMessageBox(TextManager.Get("DownloadSubLabel"), errorMsg);
            }
            return false;
        }

        public bool CheckIfCampaignSubMatches(SubmarineInfo serverSubmarine, string deliveryData)
        {
            if (GameMain.Client == null) return false;

            //already downloading the selected sub file
            if (GameMain.Client.FileReceiver.ActiveTransfers.Any(t => t.FileName == serverSubmarine.Name + ".sub"))
            {
                return false;
            }

            SubmarineInfo purchasableSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == serverSubmarine.Name && s.MD5Hash?.Hash == serverSubmarine.MD5Hash?.Hash);
            if (purchasableSub != null)
            {
                return true;
            }

            purchasableSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == serverSubmarine.Name);

            string errorMsg = "";
            if (purchasableSub == null)
            {
                errorMsg = TextManager.GetWithVariable("SubNotFoundError", "[subname]", serverSubmarine.Name) + " ";
            }
            else if (purchasableSub.MD5Hash?.Hash == null)
            {
                errorMsg = TextManager.GetWithVariable("SubLoadError", "[subname]", serverSubmarine.Name) + " ";
                /*GUITextBlock textBlock = subList.Content.GetChildByUserData(sub)?.GetChild<GUITextBlock>();
                if (textBlock != null) { textBlock.TextColor = GUI.Style.Red; }*/
            }
            else
            {
                errorMsg = TextManager.GetWithVariables("SubDoesntMatchError", new string[3] { "[subname]", "[myhash]", "[serverhash]" },
                    new string[3] { purchasableSub.Name, purchasableSub.MD5Hash.ShortHash, Md5Hash.GetShortHash(serverSubmarine.MD5Hash.Hash) }) + " ";
            }

            errorMsg += TextManager.Get("DownloadSubQuestion");

            //already showing a message about the same sub
            if (GUIMessageBox.MessageBoxes.Any(mb => mb.UserData as string == "request" + serverSubmarine.Name))
            {
                return false;
            }

            var requestFileBox = new GUIMessageBox(TextManager.Get("DownloadSubLabel"), errorMsg,
                new string[] { TextManager.Get("Yes"), TextManager.Get("No") })
            {
                UserData = "request" + serverSubmarine.Name
            };
            requestFileBox.Buttons[0].UserData = new string[] { serverSubmarine.Name, serverSubmarine.MD5Hash.Hash };
            requestFileBox.Buttons[0].OnClicked += requestFileBox.Close;
            requestFileBox.Buttons[0].OnClicked += (GUIButton button, object userdata) =>
            {
                string[] fileInfo = (string[])userdata;

                if (deliveryData == "owned")
                {
                    FailedOwnedSubs.Add(new Pair<string, string>(fileInfo[0], fileInfo[1]));
                }
                else if (deliveryData == "campaign")
                {
                    FailedCampaignSubs.Add(new Pair<string, string>(fileInfo[0], fileInfo[1]));
                }

                GameMain.Client?.RequestFile(FileTransferType.Submarine, fileInfo[0], fileInfo[1]);
                return true;
            };
            requestFileBox.Buttons[1].OnClicked += requestFileBox.Close;

            return false;
        }

        private void CreateSubPreview(SubmarineInfo sub)
        {
            subPreviewContainer?.ClearChildren();
            sub.CreatePreviewWindow(subPreviewContainer);
            RecalculateSubDescription();
        }

        private void RecalculateSubDescription()
        {
            var descriptionBox = subPreviewContainer?.FindChild("descriptionbox", recursive: true);
            if (descriptionBox != null && characterInfoFrame != null)
            {
                //if description box and character info box are roughly the same size, scale them to the same size
                if (Math.Abs(descriptionBox.Rect.Height - characterInfoFrame.Rect.Height) < 80 * GUI.Scale)
                {
                    descriptionBox.RectTransform.MaxSize = new Point(descriptionBox.Rect.Width, characterInfoFrame.Rect.Height);
                }
            }
        }

        public void OnRoundEnded()
        {
            CampaignCharacterDiscarded = false;
        }
    }
}

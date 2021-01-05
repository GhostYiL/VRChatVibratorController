﻿using Il2CppSystem.Collections.Generic;
using MelonLoader;
using PlagueButtonAPI;
using System;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private static string findButton = null;
        private static bool lockSpeed = false;
        private static bool requireHold;
        private static int buttonX;
        private static int buttonY;
        private static string subMenu;
        private static ButtonAPI.PlagueButton LockButtonUI;
        private static ButtonAPI.PlagueButton LockKeyBind;
        private static ButtonAPI.PlagueButton HoldButtonUI;
        private static ButtonAPI.PlagueButton HoldKeyBind;
        private static ButtonAPI.PlagueButton addButtonUI;
        private static KeyCode lockButton;//button to lock speed
        private static KeyCode holdButton;//button to hold with other controll to use toy (if enabled)
        private static GameObject quickMenu;
        private static GameObject menuContent;
        private bool pauseControl = false;//pause controls untill trigger is pressed

        public override void OnApplicationStart() {
            MelonPrefs.RegisterCategory("VibratorController", "Vibrator Controller");
            MelonPrefs.RegisterInt("VibratorController", "lockButton", 0, "Button to lock speed");
            MelonPrefs.RegisterInt("VibratorController", "holdButton", 0, "Button to hold to use toy");
            MelonPrefs.RegisterBool("VibratorController", "Requirehold", false, "If enabled you will need to hold set button to use toy");
            MelonPrefs.RegisterString("VibratorController", "subMenu", "ShortcutMenu", "Menu to put the mod button on");
            MelonPrefs.RegisterInt("VibratorController", "buttonX", 0, "x position to put the mod button");
            MelonPrefs.RegisterInt("VibratorController", "buttonY", 0, "y position to put the mod button");

            lockButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "lockButton");
            holdButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "holdButton");
            requireHold = MelonPrefs.GetBool("VibratorController", "Requirehold");
            subMenu = MelonPrefs.GetString("VibratorController", "subMenu");
            buttonX = MelonPrefs.GetInt("VibratorController", "buttonX");
            buttonY = MelonPrefs.GetInt("VibratorController", "buttonY");
        }

        public override void VRChat_OnUiManagerInit() {
            ButtonAPI.CustomTransform = GameObject.Find("/UserInterface/QuickMenu/" + subMenu).transform;
            ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Vibrator\nController", "Vibrator Controller Settings", buttonX - 4, buttonY + 3, null, delegate (bool a) {
                ButtonAPI.EnterSubMenu(ButtonAPI.MakeEmptyPage("SubMenu_1"));
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            //Back
            LockButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Back", "exit this menu", ButtonAPI.HorizontalPosition.RightOfMenu, ButtonAPI.VerticalPosition.BottomButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                ButtonAPI.EnterSubMenu(GameObject.Find("/UserInterface/QuickMenu/" + subMenu));
            }, Color.yellow, Color.magenta, null, true, false, false, false, null, true);

            //Lock button
            LockButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Lock Speed\nButton", "Click than press button on controller to set button to lock vibraton speed (click twice to disable)", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                if (findButton == "lockButton") {
                    lockButton = KeyCode.None;
                    findButton = null;
                    LockKeyBind.SetText("");
                    LockButtonUI.SetText("Lock Speed\nButton");
                    MelonPrefs.SetInt("VibratorController", "lockButton", lockButton.GetHashCode());
                    return;
                }
                findButton = "lockButton";
                LockButtonUI.SetText("Press Now");
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            LockKeyBind = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "", "Lock Speed Keybind", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
            }, Color.white, Color.grey, null, false, false, false, false, null, false);
            LockKeyBind.SetInteractivity(false);
            if (lockButton != 0)
                LockKeyBind.SetText(lockButton.ToString());

            //Hold button
            HoldButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Hold\nButton", "Click than press button on controller to set button to hold to use toy (click twice to disable)", ButtonAPI.HorizontalPosition.SecondButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                if (findButton == "holdButton") {
                    holdButton = KeyCode.None;
                    findButton = null;
                    HoldKeyBind.SetText("");
                    HoldButtonUI.SetText("Hold\nButton");
                    MelonPrefs.SetInt("VibratorController", "lockButton", holdButton.GetHashCode());
                    return;
                }
                findButton = "holdButton";
                HoldButtonUI.SetText("Press Now");
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            HoldKeyBind = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "", "Hold Keybind", ButtonAPI.HorizontalPosition.SecondButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
            }, Color.white, Color.grey, null, false, false, false, false, null, false);
            HoldKeyBind.SetInteractivity(false);
            if (holdButton != 0)
                HoldKeyBind.SetText(holdButton.ToString());

            //Add toy
            addButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Add\nToy", "Click to pair with a friend's toy", ButtonAPI.HorizontalPosition.ThirdButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                InputPopup("", delegate (string text) {
                    if (text.Length != 4) {
                        addButtonUI.SetText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    } else {
                        Client.send("join " + text);
                    }
                });
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            //How to use
            ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "How To Use", "Opens instructions to use", ButtonAPI.HorizontalPosition.ThirdButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                System.Diagnostics.Process.Start("https://github.com/markviews/VRChatVibratorController");
            }, Color.white, Color.grey, null, false, false, false, false, null, false);


            quickMenu = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            menuContent = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop");
        }

        //thanks to Plague#2850 for helping me with this
        internal static void InputPopup(string title, Action<string> okaction) {
            VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0
                .Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_0(
                    title, "", InputField.InputType.Standard, false, "Confirm",
                    DelegateSupport.ConvertDelegate<Il2CppSystem.Action<string, List<KeyCode>, Text>>(
                        (Action<string, List<KeyCode>, Text>)delegate (string s, List<KeyCode> k, Text t) {
                            okaction(s);
                        }), null, "...");
        }

        public override void OnUpdate() {
            if (RoomManager.prop_Boolean_3) {
                ButtonAPI.SubMenuHandler(); // Routine Delay Is Built In
            }

            if (findButton != null) getButton();

            if (Input.GetKeyDown(lockButton)) {
                if (lockSpeed) lockSpeed = false;
                else lockSpeed = true;
            }

            foreach (Toy toy in Toy.toys) {
                if (menuOpen()) {
                    toy.setSpeed((int)toy.speedSlider.value);

                    if (toy.maxSlider != null)
                        toy.setContraction();
                    if (toy.edgeSlider != null) {
                        if (toy.lastEdgeSpeed != toy.edgeSlider.value)
                            toy.setEdgeSpeed(toy.edgeSlider.value);
                    }
                    pauseControl = true;
                } else {
                    int speed = 0;
                    if (lockSpeed) return;
                    if (requireHold && !pauseControl)
                        if (!Input.GetKey(holdButton)) {
                            toy.setSpeed(0);
                            return;
                        }
                            int left = (int)(10 * Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"));
                            int right = (int)(10 * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));

                            if (pauseControl) {
                                 if (left != 0 || right != 0) {
                                     Console.WriteLine(left + " " + right);
                                     pauseControl = false;
                                 } else return;
                            }

                            switch (toy.hand) {
                                case "left":
                                    speed = left;
                                    break;
                                case "right":
                                    speed = right;
                                    break;
                                case "either":
                                    if (left > right) speed = left;
                                    else speed = right;
                                    break;
                                case "both":
                                    speed = left;
                                    toy.setEdgeSpeed(right);
                                    break;
                            }
                    toy.setSpeed(speed);
                }
            }
        }

        internal static bool menuOpen() {
            if (quickMenu.active || menuContent.active)
                return true;
            return false;
        }

        //message from server
        internal static void message(string msg) {
            String[] args = msg.Replace(((char)0).ToString(), "").Split(' ');
            switch (args[0]) {
                case "toys":
                case "add":
                    if (args[1] == "") {
                        MelonLogger.Log("Connected but no toys found..");
                        return;
                    }
                    for (int i = 1; i < args.Length; i++) {
                        string[] toyData = args[i].Split(':');
                        string name = toyData[0];
                        string id = toyData[1];

                            foreach (Toy toy in Toy.toys)
                                if (toy.id.Contains(id)) {
                                    toy.enable();
                                    return;
                                }

                        MelonLogger.Log("Adding: " + name + ":" + id);
                        new Toy(name, id);
                    }
                    break;
                case "remove": {
                    string[] toyData = args[1].Split(':');
                    string name = toyData[0];
                    string id = toyData[1];
                        foreach (Toy toy in Toy.toys)
                            if (toy.id.Contains(id)) {
                                toy.disable();//TODO display this somehow
                                break;
                            }
                    }
                    break;
                case "notFound":
                    MelonLogger.Log("Invalid code");
                    addButtonUI.SetText("Add\nToys\n<color=#FF0000>Invalid Code</color>");//TODO fix button text after a second
                    break;
                case "left":
                    MelonLogger.Log("User disconnected");//TODO display this somehow
                    foreach (Toy toy in Toy.toys)
                        toy.disable();
                    break;
            }
        }

        internal void getButton() {
            //A-Z
            for (int i = 97; i <= 122; i++)
                if (Input.GetKey((KeyCode)i)) {
                    setButton((KeyCode)i);
                    return;
                }

            //left vr controller buttons
            if (Input.GetKey(KeyCode.JoystickButton0)) setButton(KeyCode.JoystickButton0);
            else if (Input.GetKey(KeyCode.JoystickButton1)) setButton(KeyCode.JoystickButton1);
            else if (Input.GetKey(KeyCode.JoystickButton2)) setButton(KeyCode.JoystickButton2);
            else if (Input.GetKey(KeyCode.JoystickButton3)) setButton(KeyCode.JoystickButton3);
            else if (Input.GetKey(KeyCode.JoystickButton8)) setButton(KeyCode.JoystickButton8);
            else if (Input.GetKey(KeyCode.JoystickButton9)) setButton(KeyCode.JoystickButton9);

            //right vr controller buttons
            else if (Input.GetKey(KeyCode.Joystick1Button0)) setButton(KeyCode.Joystick1Button0);
            else if (Input.GetKey(KeyCode.Joystick1Button1)) setButton(KeyCode.Joystick1Button1);
            else if (Input.GetKey(KeyCode.Joystick1Button2)) setButton(KeyCode.Joystick1Button2);
            else if (Input.GetKey(KeyCode.Joystick1Button3)) setButton(KeyCode.Joystick1Button3);
            else if (Input.GetKey(KeyCode.Joystick1Button8)) setButton(KeyCode.Joystick1Button8);
            else if (Input.GetKey(KeyCode.Joystick1Button9)) setButton(KeyCode.Joystick1Button9);
        }

        internal void setButton(KeyCode button) {
            if (findButton.Equals("lockButton")) {
                lockButton = button;
                LockButtonUI.SetText("Lock Speed\nButton");
                LockKeyBind.SetText(lockButton.ToString());
                MelonPrefs.SetInt("VibratorController", "lockButton", button.GetHashCode());
            } else if (findButton.Equals("holdButton")) {
                holdButton = button;
                HoldButtonUI.SetText("Hold\nButton");
                HoldKeyBind.SetText(holdButton.ToString());
                MelonPrefs.SetInt("VibratorController", "holdButton", button.GetHashCode());
            }
            findButton = null;
        }

    }
}
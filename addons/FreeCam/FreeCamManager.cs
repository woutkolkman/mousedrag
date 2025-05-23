﻿using UnityEngine;

namespace FreeCam
{
    public static class FreeCamManager
    {
        public static FreeCam[] freeCams = new FreeCam[0];
        public static bool selectButtonDown() => (
            (Input.GetMouseButtonDown(0) && Options.selectLMB?.Value == true) ||
            (Input.GetMouseButtonDown(2) && Options.selectMMB?.Value == true) ||
            (Options.select?.Value != null && Input.GetKeyDown(Options.select.Value))
        );
        public static bool holdWasPressed = false;


        //for every RoomCamera, create a FreeCam object
        public static void Init(RainWorldGame game)
        {
            if (!(game?.cameras?.Length > 0)) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("FreeCamManager.Init, RoomCameras not initialized yet");
                return;
            }
            int length = game.cameras.Length;
            freeCams = new FreeCam[length];
            for (int i = 0; i < length; i++)
                freeCams[i] = new FreeCam(game.cameras[i]);
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCamManager.Init, initialized " + length + " FreeCam object(s)");
        }


        public static void Deinit()
        {
            freeCams = new FreeCam[0];
        }


        //toggle FreeCam for the RoomCamera the mouse is currently hovering over
        public static void Toggle(RainWorldGame game, int cameraNumber = -1)
        {
            int camNr;
            if (cameraNumber >= 0) {
                camNr = cameraNumber;
            } else {
                RoomCamera rcam = Cursor.MouseCamera(game);
                if (rcam == null)
                    return;
                camNr = rcam.cameraNumber;
            }
            if (!(camNr < freeCams.Length)) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCamManager.Toggle, no FreeCam for RoomCamera " + camNr);
                return;
            }
            freeCams[camNr].Toggle();
        }


        //return if FreeCam is enabled for the RoomCamera the mouse is currently hovering over
        public static bool IsEnabled(RainWorldGame game)
        {
            RoomCamera rcam = Cursor.MouseCamera(game);
            if (rcam == null)
                return false;
            if (!(rcam.cameraNumber < freeCams.Length)) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCamManager.IsEnabled, no FreeCam for RoomCamera " + rcam.cameraNumber);
                return false;
            }
            return freeCams[rcam.cameraNumber].enabled;
        }


        //return if FreeCam is enabled for a specific RoomCamera
        public static bool IsEnabled(int cameraNumber)
        {
            if (cameraNumber >= 0 && cameraNumber < freeCams.Length)
                return freeCams[cameraNumber].enabled;
            return false;
        }


        public static void Update(RainWorldGame game)
        {
            if (game == null)
                return;

            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                return;

            //toggle first freecam with hold keybind
            bool holdIsPressed = Options.holdKey?.Value != null && Input.GetKey(Options.holdKey.Value);
            if (holdIsPressed && !holdWasPressed && freeCams.Length > 0 && !freeCams[0].enabled)
                freeCams[0].Toggle();
            if (!holdIsPressed && holdWasPressed && freeCams.Length > 0 && freeCams[0].enabled)
                freeCams[0].Toggle();
            holdWasPressed = holdIsPressed;

            for (int i = 0; i < freeCams.Length; i++)
                freeCams[i].Update(game);
        }


        //manage pipe selection for FreeCams only for the RoomCamera the mouse is currently hovering over
        public static void RawUpdate(RainWorldGame game)
        {
            RoomCamera rcam = Cursor.MouseCamera(game);
            if (rcam == null || rcam.cameraNumber >= freeCams.Length)
                return;
            if (freeCams[rcam.cameraNumber].enabled)
                if (selectButtonDown())
                    freeCams[rcam.cameraNumber].mousePressed = true;
            for (int i = 0; i < freeCams.Length; i++)
                freeCams[i].RawUpdate();
        }
    }
}

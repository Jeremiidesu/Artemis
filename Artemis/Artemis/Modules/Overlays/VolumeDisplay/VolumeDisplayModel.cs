﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.Models;
using NAudio.CoreAudioApi;
using Open.WinKeyboardHook;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayModel : OverlayModel
    {
        private bool _enabled;

        public VolumeDisplayModel(VolumeDisplaySettings settings)
        {
            Settings = settings;
            Name = "VolumeDisplay";

            KeyboardInterceptor = new KeyboardInterceptor();
            KeyboardInterceptor.StartCapturing();

            Enabled = Settings.Enabled;

            VolumeDisplay = new VolumeDisplay();
        }

        public VolumeDisplay VolumeDisplay { get; set; }

        public VolumeDisplaySettings Settings { get; set; }
        public KeyboardInterceptor KeyboardInterceptor { get; set; }

        public override void Dispose()
        {
            KeyboardInterceptor.KeyUp -= HandleKeypress;
            KeyboardInterceptor.StopCapturing();
        }

        public override void Enable()
        {
            KeyboardInterceptor.StartCapturing();
            KeyboardInterceptor.KeyUp += HandleKeypress;
        }

        public override void Update()
        {
            // TODO: Get from settings
            var fps = 25;

            if (VolumeDisplay == null)
                return;
            if (VolumeDisplay.Ttl < 1)
                return;

            var decreaseAmount = 500/fps;
            VolumeDisplay.Ttl = VolumeDisplay.Ttl - decreaseAmount;
            if (VolumeDisplay.Ttl < 128)
                VolumeDisplay.Transparancy = (byte) (VolumeDisplay.Transparancy - 20);

            try
            {
                var enumerator = new MMDeviceEnumerator();
                var volumeFloat =
                    enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                        .AudioEndpointVolume.MasterVolumeLevelScalar;
                VolumeDisplay.Volume = (int) (volumeFloat*100);
            }
            catch (COMException)
            {
            }
        }

        public override Bitmap GenerateBitmap()
        {
            return GenerateBitmap(new Bitmap(21, 6));
        }

        public override Bitmap GenerateBitmap(Bitmap bitmap)
        {
            if (VolumeDisplay == null)
                return bitmap;
            if (VolumeDisplay.Ttl < 1)
                return bitmap;

            using (var g = Graphics.FromImage(bitmap))
            {
                VolumeDisplay.Draw(g);
            }

            return bitmap;
        }

        private void HandleKeypress(object sender, KeyEventArgs e)
        {
            Task.Factory.StartNew(() => KeyPressTask(e));
        }

        private void KeyPressTask(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.VolumeUp && e.KeyCode != Keys.VolumeDown)
                return;

            VolumeDisplay.Ttl = 1000;
            VolumeDisplay.Transparancy = 255;
        }
    }
}
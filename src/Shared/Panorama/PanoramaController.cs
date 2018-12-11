﻿using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.Swank.Panorama.ImageSources;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using Application = Urho.Application;
using View = Xamarin.Forms.View;

namespace Plugin.Swank.Panorama
{
    public class PanoramaController
    {
        private PanoramaApp _app;
        private float _fieldOfView, _yaw, _pitch;
        private PanoramaImageSource _imageSource;
        private UrhoSurface _urhoSurface;

        public View GetView()
        {
            return _urhoSurface ?? (_urhoSurface = new UrhoSurface
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                InputTransparent = true
            });
        }

        public void SetImage(PanoramaImageSource imageSource)
        {
            if (_imageSource != imageSource)
            {
                _imageSource = imageSource;
                UpdateImage();
            }
        }

        public void SetFieldOfView(float fieldOfView)
        {
            if (_fieldOfView != fieldOfView)
            {
                _fieldOfView = fieldOfView;
                UpdateFieldOfView();
            }
        }

        public void SetYaw(float yaw)
        {
            if (_yaw != yaw)
            {
                _yaw = yaw;
                UpdateYaw();
            }
        }

        public void SetPitch(float pitch)
        {
            if (_pitch != pitch)
            {
                _pitch = pitch;
                UpdatePitch();
            }
        }

        public void Initialize()
        {
            Device.BeginInvokeOnMainThread(InvokeSetup);
        }

        private async void InvokeSetup()
        {
            try
            {
                await Setup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing urho application: {ex.StackTrace}");
            }
        }

        private async Task Setup()
        {
            // sometimes, the scene is not big enough. we wait completion
            await Task.Delay(500);

            if (_app == null && _urhoSurface != null)
            {
                if (!Application.HasCurrent)
                {
                    var applicationOptions = new ApplicationOptions
                    {
                        Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait
                    };

                    _app = await _urhoSurface.Show<PanoramaApp>(applicationOptions);
                }
                else
                {
                    _app = (PanoramaApp) Application.Current;
                }
             
                UpdateImage();
                UpdateFieldOfView();
                UpdatePitch();
                UpdateYaw();    
            }

        }

        private async void UpdateImage()
        {
            if (_imageSource == null || _app == null)
            {
                return;
            }

            var imageStream = await _imageSource.GetStreamAsync();
            if (imageStream == null)
            {
                return;
            }

            _app.SetImage(new MemoryBuffer(GetMemoryStream(imageStream)));
        }

        private void UpdateFieldOfView()
        {
            if (_app != null)
            {
                _app.SetFieldOfView(_fieldOfView);
            }
        }

        private void UpdateYaw()
        {
            if (_app != null)
            {
                _app.SetYaw(_yaw);
            }
        }

        private void UpdatePitch()
        {
            if (_app != null)
            {
                _app.SetPitch(_pitch);
            }
        }

        private static MemoryStream GetMemoryStream(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return stream as MemoryStream;
            }

            var ms = new MemoryStream();

            stream.CopyTo(ms);
            ms.Position = 0;

            return ms;
        }
    }
}
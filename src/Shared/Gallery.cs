﻿using System;
using System.Collections;
using CarouselView.FormsPlugin.Abstractions;
using Plugin.Swank;
using Plugin.Swank.Panorama;
using Plugin.Swank.Panorama.ImageSources;
using Xamarin.Forms;

namespace Swank.FormsPlugin
{
    public class Gallery : ContentView
    {
        public int Position
        {
            get => (int)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(
                nameof(Position),
                typeof(int),
                typeof(Gallery),
                0,
                BindingMode.OneWay,
                null,
                (s, value, newValue) => (s as Gallery).PositionChanged(newValue),
                null,
                null,
                null
            );


        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(Gallery),
                null,
                BindingMode.OneWay,
                null,
                (s, value, newValue) => (s as Gallery).SourceChanged(newValue),
                null,
                null,
                null
            );

        public static readonly BindableProperty IsInfiniteProperty = 
            BindableProperty.Create(
                nameof(IsInfinite),
                typeof(bool),
                typeof(Gallery),
                default(bool),
                BindingMode.TwoWay
            );

        public bool IsInfinite
        {
            get => (bool) GetValue(IsInfiniteProperty);
            set => SetValue(IsInfiniteProperty, value);
        }

        private readonly PanGestureRecognizer _pan = new PanGestureRecognizer();
        private readonly Viewer _viewer;

        public PanoramaView _panorama;

        public Gallery()
        {
            _viewer = new Viewer
            {
                ItemsSource = ItemsSource,
                PositionSelectedCommand = new Command(() =>
                {
                    PositionSelected?.Invoke(this, new PositionSelectedEventArgs
                    {
                        NewValue = _viewer.Position
                    });
                })
            };

            _panorama = new PanoramaView
            {
                FieldOfView = 75.0f,
                Yaw = 0,
                Pitch = 0
            };

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;

            SetLayout();
        }

        public event EventHandler<PositionSelectedEventArgs> PositionSelected;

        private void SourceChanged(object newValue)
        {
            _viewer.ItemsSource = (IEnumerable)newValue;
        }

        private void PositionChanged(object newValue)
        {
            var newPosition = (int) newValue;
            if (newPosition != _viewer.Position)
            {
                _viewer.Position = newPosition;
            }
        }

        private void SetLayout()
        {
            var layout = new AbsoluteLayout();

            // Viewer
            AbsoluteLayout.SetLayoutBounds(_viewer, new Rectangle(.5, 1, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_viewer, AbsoluteLayoutFlags.All);

            // Panorama
            AbsoluteLayout.SetLayoutBounds(_panorama, new Rectangle(0, .45, 1, .7));
            AbsoluteLayout.SetLayoutFlags(_panorama, AbsoluteLayoutFlags.All);
            _panorama.IsVisible = false;

            // Gestures
            _pan.PanUpdated += OnPanUpdated;
            layout.GestureRecognizers.Add(_pan);

            layout.Children.Add(_viewer);
            layout.Children.Add(_panorama);

            Content = layout;
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!_panorama.IsVisible)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if (_panorama != null)
                    {
                        ComputeYaw(e.TotalX);
                        ComputePitch(e.TotalY);
                    }

                    break;
            }
        }

        private void ComputeYaw(double totalX)
        {
            if (totalX == 0)
            {
                return;
            }

            _panorama.Yaw += (float)(totalX / 100);
        }

        private void ComputePitch(double totalY)
        {
            if (totalY == 0)
            {
                return;
            }

            var newPitch = _panorama.Pitch + (float)(totalY / 100);
            if (newPitch <= 40 && newPitch >= -40)
            {
                _panorama.Pitch = newPitch;
            }
        }

        public void TogglePanoramaVisibility()
        {
            _panorama.IsVisible = !_panorama.IsVisible;

            if (_panorama.IsVisible)
            {
                _panorama.Initialize();
            }
        }

        public void SetPanoramaImage(PanoramaImageSource source)
        {
            _panorama.Image = source;
        }
    }
}
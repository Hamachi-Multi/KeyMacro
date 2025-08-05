using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleKeyMacro.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace SimpleKeyMacro.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IKeyboardHookService _keyboardHookService;
        private readonly IInputSimulator _inputSimulator;

        public MainViewModel(IKeyboardHookService keyboardHookService, IInputSimulator inputSimulator)
        {
            _keyboardHookService = keyboardHookService;
            _inputSimulator = inputSimulator;

            _keyboardHookService.KeyDown += OnGlobalKeyDown;
        }

        private void OnGlobalKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            var keyCode = e.KeyCode;

            if (keyCode == System.Windows.Forms.Keys.HanjaMode)
            {
                if (ToggleMacroCommand.CanExecute(null))
                {
                    ToggleMacroCommand.Execute(null);
                }
            }
            else if (keyCode == System.Windows.Forms.Keys.Up)
            {
                if (DelaySeconds == 0.1)
                {
                    DelaySeconds = 0.5;
                }
                else
                {
                    DelaySeconds += 0.5;
                }
            }
            else if (keyCode == System.Windows.Forms.Keys.Down)
            {
                DelaySeconds = Math.Max(0.1, DelaySeconds - 0.5);
            }
        }

        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationTokenSource? _delayChangeTokenSource;
        
        [ObservableProperty]
        private string keyStrokeText = "Right";


        private double _delaySeconds = 5.0;
        public double DelaySeconds
        {
            get => _delaySeconds;
            set
            {
                if (value > 0)
                {
                    SetProperty(ref _delaySeconds, value);
                    if (IsRunning)
                    {
                        _delayChangeTokenSource?.Cancel();
                    }
                }
            }
        }


        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value, nameof(StartButtonText));
        }

        public string StartButtonText => IsRunning ? "Stop" : "Start";


        [RelayCommand]
        private void ToggleMacro()
        {
            if (IsRunning)
            {
                StopMacro();
            }
            else
            {
                StartMacro();
            }
        }

        private void StartMacro()
        {
            IsRunning = true;
            Debug.WriteLine("Macro started");

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _delayChangeTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _delayChangeTokenSource.Token);
                        try
                        {
                            await Task.Delay((int)(DelaySeconds * 1000), linkedCts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                            _delayChangeTokenSource.Dispose();
                            _delayChangeTokenSource = new CancellationTokenSource();
                            continue;
                        }

                        if (!string.IsNullOrEmpty(KeyStrokeText))
                        {
                            if (KeyAliases.TryGetValue(KeyStrokeText, out var aliasKey))
                            {
                                _inputSimulator.Keyboard.KeyPress(aliasKey);
                                Debug.WriteLine($"Sent alias key: {aliasKey}");
                            }
                            else if (Enum.TryParse(KeyStrokeText, true, out VirtualKeyCode keyCode))
                            {
                                _inputSimulator.Keyboard.KeyPress(keyCode);
                                Debug.WriteLine($"Sent key: {keyCode}");
                            }
                            else
                            {
                                _inputSimulator.Keyboard.TextEntry(KeyStrokeText);
                                Debug.WriteLine($"Sent text: {KeyStrokeText}");
                            }
                        }

                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
                Debug.WriteLine("Macro execution stopped");
            }, token);
        }

        private static readonly Dictionary<string, VirtualKeyCode> KeyAliases = new Dictionary<string, VirtualKeyCode>(StringComparer.OrdinalIgnoreCase)
        {
            { "LeftShift", VirtualKeyCode.LSHIFT },
            { "RightShift", VirtualKeyCode.RSHIFT },
            { "LeftCtrl", VirtualKeyCode.LCONTROL },
            { "RightCtrl", VirtualKeyCode.RCONTROL },
            { "LeftAlt", VirtualKeyCode.LMENU },
            { "RightAlt", VirtualKeyCode.RMENU },
            { "LeftWin", VirtualKeyCode.LWIN },
            { "RightWin", VirtualKeyCode.RWIN },
            { "Space", VirtualKeyCode.SPACE },
            { "Enter", VirtualKeyCode.RETURN },
            { "Backspace", VirtualKeyCode.BACK },
            { "Tab", VirtualKeyCode.TAB }
        };

        private void StopMacro()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _delayChangeTokenSource?.Cancel();
            _delayChangeTokenSource = null;
            IsRunning = false;
            Debug.WriteLine("Macro stopped");
        }

        [RelayCommand]
        private void HandleMacroKey(object parameter)
        {
            if (parameter is string keyString)
            {
                KeyStrokeText = keyString;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _delayChangeTokenSource?.Cancel();
            _delayChangeTokenSource?.Dispose();
        }
    }
}

using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleKeyMacro.Services
{
    public class KeyboardHookService : IKeyboardHookService
    {
        private readonly IKeyboardMouseEvents _globalHook;

        public event KeyEventHandler? KeyDown;

        public KeyboardHookService()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += OnGlobalKeyDown;
        }

        private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
        {
            KeyDown?.Invoke(sender, e);
        }

        public void Dispose()
        {
            _globalHook.KeyDown -= OnGlobalKeyDown;
            _globalHook.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

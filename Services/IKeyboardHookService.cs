using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleKeyMacro.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        event KeyEventHandler KeyDown;
    }
}

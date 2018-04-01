using System;
using System.Collections.Generic;

public interface IVSingleton : IDisposable {
    void OnInit();
    void Initialize();
}

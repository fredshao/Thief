using System;
using System.Collections.Generic;
using UnityEngine;

public class HardwareUtil {

    public static bool IsWindows() {
        return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
    }

    public static bool IsOSX() {
        return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
    }

}

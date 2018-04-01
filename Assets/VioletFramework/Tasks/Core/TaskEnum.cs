using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public enum TaskChainType {
        RunOneByOne,            // 一个接一个Task执行，遇到任何一个执行失败的Task，返回失败，中终接下来的Task，全部成功返回成功
        RunOneByOneIgnoreError, // 一个接一个的Task执行，遇到失败也会继续执行下一个，只要有一个成功则返回成功，全部失败返回失败
        RunParallel,            // 同时执行，全部成功返回成功
        RunParallelIgnoreError, // 同时执行，只要有一个成功则返回成功，全部失败返回失败
    }
}

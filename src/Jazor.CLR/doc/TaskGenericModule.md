# TaskGenericModule.cs

> ⚠️ **注意**：签名 = `_` + SHA256Hash(成员)

## 类型映射

**类型**：`System.Threading.Tasks.Task<TResult>`  
**Op**：`Alias`  
**映射**：`Promise`

## 成员映射

**成员**：`System.Threading.Tasks.Task<TResult>.Result.get`  
**签名**：`_18af0aa87004bfcc`  
**Op**：`Discard`  
**说明**：当前 Jazor 运行时不支持同步阻塞读取 `Result`，推荐使用 `await`。  
**备注**：`Discard` 成员不会进入生成白名单。

**成员**：`System.Threading.Tasks.Task<TResult>.GetAwaiter()`  
**签名**：`_027217a9621e6f7b`  
**Op**：`Inline`  
**映射**：`Promise.resolve(__arg1)`

**成员**：`System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)`  
**签名**：`_0e17ea5f64ad914f`  
**Op**：`Inline`  
**映射**：`Promise.resolve(__arg1)`

**成员**：`System.Threading.Tasks.Task<TResult>.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)`  
**签名**：`_e315c5cff004ed53`  
**Op**：`Inline`  
**映射**：`Promise.resolve(__arg1)`

**成员**：`System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)`  
**签名**：`_a5adb3e12ef3a8bb`  
**Op**：`Inline`  
**映射**：`Promise.resolve(__arg1)`

**成员**：`System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan)`  
**签名**：`_408c4a7eefe8214c`  
**Op**：`Inline`  
**映射**：`Promise.race([Promise.resolve(__arg1), timeoutRejectPromise])`

**成员**：`System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider)`  
**签名**：`_35ae1f6899303439`  
**Op**：`Inline`  
**映射**：`Promise.race([Promise.resolve(__arg1), timeoutRejectPromise])`

**成员**：`System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)`  
**签名**：`_05fbcc037540ba42`  
**Op**：`Inline`  
**映射**：`Promise.race([Promise.resolve(__arg1), timeoutRejectPromise])`

**成员**：`System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)`  
**签名**：`_4b5b887e2099f8dd`  
**Op**：`Inline`  
**映射**：`Promise.race([Promise.resolve(__arg1), timeoutRejectPromise])`

namespace ECMAScript.VueContract.Descriptor;

/// <summary>描述 Vue emit 在组件 contract 中的语义类别。</summary>
/// <remarks>类别用于区分 model update、生命周期类和库特定事件的绑定规则。</remarks>
public enum VueEmitKind
{
    Normal,
    ModelUpdate,
    LifecycleLike,
    LibrarySpecific
}

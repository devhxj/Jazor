using System.Collections.Immutable;
using Acornima;
using Acornima.Ast;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Validates the final Vue ESTree before it is serialized. The validator follows lexical
/// bindings instead of scanning module text, so property keys, labels, import locals and
/// declaration names are not mistaken for free references.
/// 最终模块只允许声明、参数、import local、明确 ECMAScript global 和 generated runtime 名称；
/// 发现未知自由标识符时立即失败，避免把 lowering 回归留到浏览器 console。
/// </summary>
internal static class VueModuleIntegrityValidator
{
    private static readonly ImmutableHashSet<string> EcmaScriptGlobals =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "Array",
            "ArrayBuffer",
            "BigInt",
            "BigInt64Array",
            "BigUint64Array",
            "Boolean",
            "DataView",
            "Date",
            "decodeURI",
            "decodeURIComponent",
            "encodeURI",
            "encodeURIComponent",
            "Error",
            "EvalError",
            "FinalizationRegistry",
            "Float32Array",
            "Float64Array",
            "globalThis",
            // Browser host globals are explicit runtime dependencies. Keep this list narrow:
            // generated modules may use the standard browser surface, but an arbitrary free
            // identifier must still fail at build time instead of becoming a console error.
            "window",
            "document",
            "location",
            "history",
            "navigator",
            "Infinity",
            "Int8Array",
            "Int16Array",
            "Int32Array",
            "Intl",
            "isFinite",
            "isNaN",
            "JSON",
            "Map",
            "Math",
            "NaN",
            "Number",
            "Object",
            "parseFloat",
            "parseInt",
            "Promise",
            "Proxy",
            "RangeError",
            "ReferenceError",
            "Reflect",
            "RegExp",
            "Set",
            "String",
            "Symbol",
            "SyntaxError",
            "TypeError",
            "Uint8Array",
            "Uint8ClampedArray",
            "Uint16Array",
            "Uint32Array",
            "URIError",
            "WeakMap",
            "WeakRef",
            "WeakSet",
            "undefined",
            "console",
            "setTimeout",
            "clearTimeout",
            "queueMicrotask");

    internal static ImmutableArray<string> FindUnboundIdentifiers(Module module)
    {
        if (module is null)
            throw new ArgumentNullException(nameof(module));

        var collector = new FreeIdentifierCollector();
        collector.Visit(module);
        return collector.Names
            .Where(name => !EcmaScriptGlobals.Contains(name))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static void Validate(Module module)
    {
        var unbound = FindUnboundIdentifiers(module);
        if (unbound.Length == 0)
            return;

        throw new InvalidOperationException(
            "RazorVue generated Vue module contains unbound identifier(s): " +
            string.Join(", ", unbound) + ". Ensure the lowering path declares or imports every runtime helper.");
    }

    private sealed class FreeIdentifierCollector : AstVisitor
    {
        private readonly Scope _root = new(null, isVarScope: true);
        private Scope _current;

        internal FreeIdentifierCollector()
        {
            _current = _root;
        }

        internal HashSet<string> Names { get; } = new(StringComparer.Ordinal);

        protected override object VisitProgram(Program node)
        {
            DeclareBlockBindings(node.Body, _current);
            DeclareHoistedBindings(node.Body, _current);
            foreach (var statement in node.Body)
                Visit(statement);
            return node;
        }

        protected override object VisitBlockStatement(BlockStatement node)
        {
            var scope = new Scope(_current);
            DeclareBlockBindings(node.Body, scope);
            DeclareHoistedBindings(node.Body, scope);
            using var lease = PushScope(scope);
            foreach (var statement in node.Body)
                Visit(statement);
            return node;
        }

        protected override object VisitFunctionBody(FunctionBody node)
        {
            DeclareBlockBindings(node.Body, _current);
            DeclareHoistedBindings(node.Body, _current);
            foreach (var statement in node.Body)
                Visit(statement);
            return node;
        }

        protected override object VisitFunctionDeclaration(FunctionDeclaration node)
        {
            var scope = CreateFunctionScope(node.Params, node.Body.Body);
            if (node.Id is not null)
                scope.Add(node.Id.Name);
            using var lease = PushScope(scope);
            VisitParameterDefaults(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitFunctionExpression(FunctionExpression node)
        {
            var scope = CreateFunctionScope(node.Params, node.Body.Body);
            if (node.Id is not null)
                scope.Add(node.Id.Name);

            using var lease = PushScope(scope);
            VisitParameterDefaults(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node)
        {
            var scope = new Scope(_current, isVarScope: true);
            foreach (var parameter in node.Params)
                AddBindingNames(parameter, scope);

            using var lease = PushScope(scope);
            VisitParameterDefaults(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitVariableDeclaration(VariableDeclaration node)
        {
            foreach (var declaration in node.Declarations)
            {
                VisitBindingSideEffects(declaration.Id);
                if (declaration.Init is not null)
                    Visit(declaration.Init);
            }

            return node;
        }

        protected override object VisitImportDeclaration(ImportDeclaration node)
        {
            foreach (var specifier in node.Specifiers)
            {
                switch (specifier)
                {
                    case ImportSpecifier named:
                        _current.Add(named.Local.Name);
                        break;
                    case ImportDefaultSpecifier defaultSpecifier:
                        _current.Add(defaultSpecifier.Local.Name);
                        break;
                    case ImportNamespaceSpecifier namespaceSpecifier:
                        _current.Add(namespaceSpecifier.Local.Name);
                        break;
                }
            }

            return node;
        }

        protected override object VisitExportNamedDeclaration(ExportNamedDeclaration node)
        {
            if (node.Declaration is not null)
            {
                Visit(node.Declaration);
                return node;
            }

            if (node.Source is null)
            {
                foreach (var specifier in node.Specifiers)
                    Visit(specifier.Local);
            }

            return node;
        }

        protected override object VisitExportDefaultDeclaration(ExportDefaultDeclaration node)
        {
            switch (node.Declaration)
            {
                case FunctionDeclaration { Id: not null } function:
                    _current.Add(function.Id.Name);
                    break;
                case ClassDeclaration { Id: not null } @class:
                    _current.Add(@class.Id.Name);
                    break;
            }
            if (node.Declaration is not null)
                Visit(node.Declaration);
            return node;
        }

        protected override object VisitExportAllDeclaration(ExportAllDeclaration node)
            => node;

        protected override object VisitMemberExpression(MemberExpression node)
        {
            Visit(node.Object);
            if (node.Computed)
                Visit(node.Property);
            return node;
        }

        protected override object VisitObjectProperty(ObjectProperty node)
        {
            if (node.Computed)
                Visit(node.Key);
            Visit(node.Value);
            return node;
        }

        protected override object VisitAssignmentProperty(AssignmentProperty node)
        {
            if (node.Computed)
                Visit(node.Key);
            Visit(node.Value);
            return node;
        }

        protected override object VisitMethodDefinition(MethodDefinition node)
        {
            if (node.Computed)
                Visit(node.Key);
            foreach (var decorator in node.Decorators)
                Visit(decorator);
            Visit(node.Value);
            return node;
        }

        protected override object VisitPropertyDefinition(PropertyDefinition node)
        {
            if (node.Computed)
                Visit(node.Key);
            foreach (var decorator in node.Decorators)
                Visit(decorator);
            if (node.Value is not null)
                Visit(node.Value);
            return node;
        }

        protected override object VisitClassDeclaration(ClassDeclaration node)
        {
            var scope = new Scope(_current);
            if (node.Id is not null)
                scope.Add(node.Id.Name);

            using var lease = PushScope(scope);
            foreach (var decorator in node.Decorators)
                Visit(decorator);
            if (node.SuperClass is not null)
                Visit(node.SuperClass);
            Visit(node.Body);
            return node;
        }

        protected override object VisitClassExpression(ClassExpression node)
        {
            var scope = new Scope(_current);
            if (node.Id is not null)
                scope.Add(node.Id.Name);

            using var lease = PushScope(scope);
            foreach (var decorator in node.Decorators)
                Visit(decorator);
            if (node.SuperClass is not null)
                Visit(node.SuperClass);
            Visit(node.Body);
            return node;
        }

        protected override object VisitStaticBlock(StaticBlock node)
        {
            // Static blocks have a lexical scope of their own. Do not let a declaration inside
            // the class body mask a module/setup binding outside the class.
            var scope = new Scope(_current, isVarScope: true);
            DeclareBlockBindings(node.Body, scope);
            DeclareHoistedBindings(node.Body, scope);
            using var lease = PushScope(scope);
            foreach (var statement in node.Body)
                Visit(statement);
            return node;
        }

        protected override object VisitLabeledStatement(LabeledStatement node)
        {
            // Labels are control-flow metadata, never value references.
            Visit(node.Body);
            return node;
        }

        protected override object VisitBreakStatement(BreakStatement node)
            => node;

        protected override object VisitContinueStatement(ContinueStatement node)
            => node;

        protected override object VisitCatchClause(CatchClause node)
        {
            var scope = new Scope(_current);
            if (node.Param is not null)
                AddBindingNames(node.Param, scope);

            using var lease = PushScope(scope);
            if (node.Param is not null)
                VisitBindingSideEffects(node.Param);
            Visit(node.Body);
            return node;
        }

        protected override object VisitForStatement(ForStatement node)
        {
            if (node.Init is VariableDeclaration declaration &&
                declaration.Kind != VariableDeclarationKind.Var)
            {
                var scope = new Scope(_current);
                foreach (var item in declaration.Declarations)
                    AddBindingNames(item.Id, scope);
                using var lease = PushScope(scope);
                Visit(node.Init);
                if (node.Test is not null)
                    Visit(node.Test);
                if (node.Update is not null)
                    Visit(node.Update);
                Visit(node.Body);
                return node;
            }

            if (node.Init is VariableDeclaration varDeclaration)
            {
                foreach (var item in varDeclaration.Declarations)
                    AddBindingNamesToVarScope(item.Id, _current);
            }

            if (node.Init is not null)
                Visit(node.Init);
            if (node.Test is not null)
                Visit(node.Test);
            if (node.Update is not null)
                Visit(node.Update);
            Visit(node.Body);
            return node;
        }

        protected override object VisitForOfStatement(ForOfStatement node)
            => VisitForBindingLoop(node.Left, node.Right, node.Body);

        protected override object VisitForInStatement(ForInStatement node)
            => VisitForBindingLoop(node.Left, node.Right, node.Body);

        protected override object VisitSwitchStatement(SwitchStatement node)
        {
            var scope = new Scope(_current);
            foreach (var @case in node.Cases)
                DeclareBlockBindings(@case.Consequent, scope);

            using var lease = PushScope(scope);
            Visit(node.Discriminant);
            foreach (var @case in node.Cases)
                Visit(@case);
            return node;
        }

        protected override object VisitAssignmentPattern(AssignmentPattern node)
        {
            // AssignmentPattern is normally reached through a binding, where its left side
            // has already been declared. Only the default expression is a runtime reference.
            Visit(node.Right);
            return node;
        }

        protected override object VisitIdentifier(Identifier node)
        {
            if (!_current.Contains(node.Name))
                Names.Add(node.Name);
            return node;
        }

        private object VisitForBindingLoop(Node left, Expression right, Statement body)
        {
            if (left is VariableDeclaration declaration &&
                declaration.Kind != VariableDeclarationKind.Var)
            {
                var scope = new Scope(_current);
                foreach (var item in declaration.Declarations)
                    AddBindingNames(item.Id, scope);
                using var lease = PushScope(scope);
                Visit(left);
                Visit(right);
                Visit(body);
                return body;
            }

            if (left is VariableDeclaration varDeclaration)
            {
                foreach (var item in varDeclaration.Declarations)
                    AddBindingNamesToVarScope(item.Id, _current);
            }

            Visit(left);
            Visit(right);
            Visit(body);
            return body;
        }

        private Scope CreateFunctionScope(
            NodeList<Node> parameters,
            NodeList<Statement> body)
        {
            var scope = new Scope(_current, isVarScope: true);
            foreach (var parameter in parameters)
                AddBindingNames(parameter, scope);
            return scope;
        }

        private void VisitParameterDefaults(NodeList<Node> parameters)
        {
            foreach (var parameter in parameters)
                VisitBindingSideEffects(parameter);
        }

        private static void DeclareBlockBindings(
            IEnumerable<Statement> statements,
            Scope scope)
        {
            foreach (var statement in statements)
            {
                switch (statement)
                {
                    case VariableDeclaration declaration:
                        if (declaration.Kind != VariableDeclarationKind.Var)
                        {
                            foreach (var item in declaration.Declarations)
                                AddBindingNames(item.Id, scope);
                        }
                        break;
                    case FunctionDeclaration function when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                    case ClassDeclaration @class when @class.Id is not null:
                        scope.Add(@class.Id.Name);
                        break;
                    case ExportNamedDeclaration { Declaration: VariableDeclaration declaration }:
                        if (declaration.Kind != VariableDeclarationKind.Var)
                        {
                            foreach (var item in declaration.Declarations)
                                AddBindingNames(item.Id, scope);
                        }
                        break;
                    case ExportNamedDeclaration { Declaration: FunctionDeclaration function }
                        when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                    case ExportNamedDeclaration { Declaration: ClassDeclaration @class }
                        when @class.Id is not null:
                        scope.Add(@class.Id.Name);
                        break;
                    case ExportDefaultDeclaration { Declaration: FunctionDeclaration function }
                        when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                    case ExportDefaultDeclaration { Declaration: ClassDeclaration @class }
                        when @class.Id is not null:
                        scope.Add(@class.Id.Name);
                        break;
                }
            }
        }

        private static void DeclareHoistedBindings(
            IEnumerable<Statement> statements,
            Scope scope)
        {
            foreach (var statement in statements)
            {
                switch (statement)
                {
                    case VariableDeclaration { Kind: VariableDeclarationKind.Var } varDeclaration:
                        foreach (var item in varDeclaration.Declarations)
                            AddBindingNamesToVarScope(item.Id, scope);
                        break;
                    case BlockStatement block:
                        DeclareHoistedBindings(block.Body, scope);
                        break;
                    case IfStatement conditional:
                        DeclareHoistedBindings([conditional.Consequent], scope);
                        if (conditional.Alternate is not null)
                            DeclareHoistedBindings([conditional.Alternate], scope);
                        break;
                    case ForStatement @for:
                        if (@for.Init is VariableDeclaration forDeclaration &&
                            forDeclaration.Kind == VariableDeclarationKind.Var)
                        {
                            foreach (var item in forDeclaration.Declarations)
                                AddBindingNamesToVarScope(item.Id, scope);
                        }
                        DeclareHoistedBindings([@for.Body], scope);
                        break;
                    case ForInStatement forIn:
                        if (forIn.Left is VariableDeclaration { Kind: VariableDeclarationKind.Var } declarationIn)
                        {
                            foreach (var item in declarationIn.Declarations)
                                AddBindingNamesToVarScope(item.Id, scope);
                        }
                        DeclareHoistedBindings([forIn.Body], scope);
                        break;
                    case ForOfStatement forOf:
                        if (forOf.Left is VariableDeclaration { Kind: VariableDeclarationKind.Var } declarationOf)
                        {
                            foreach (var item in declarationOf.Declarations)
                                AddBindingNamesToVarScope(item.Id, scope);
                        }
                        DeclareHoistedBindings([forOf.Body], scope);
                        break;
                    case WhileStatement @while:
                        DeclareHoistedBindings([@while.Body], scope);
                        break;
                    case DoWhileStatement @do:
                        DeclareHoistedBindings([@do.Body], scope);
                        break;
                    case LabeledStatement labeled:
                        DeclareHoistedBindings([labeled.Body], scope);
                        break;
                    case WithStatement @with:
                        DeclareHoistedBindings([@with.Body], scope);
                        break;
                    case TryStatement @try:
                        DeclareHoistedBindings(@try.Block.Body, scope);
                        if (@try.Handler is not null)
                            DeclareHoistedBindings(@try.Handler.Body.Body, scope);
                        if (@try.Finalizer is not null)
                            DeclareHoistedBindings(@try.Finalizer.Body, scope);
                        break;
                    case SwitchStatement @switch:
                        foreach (var @case in @switch.Cases)
                            DeclareHoistedBindings(@case.Consequent, scope);
                        break;
                    case FunctionDeclaration function when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                    case ExportNamedDeclaration { Declaration: FunctionDeclaration function }
                        when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                    case ExportDefaultDeclaration { Declaration: FunctionDeclaration function }
                        when function.Id is not null:
                        scope.Add(function.Id.Name);
                        break;
                }
            }
        }

        private static void AddBindingNames(Node node, Scope scope)
        {
            switch (node)
            {
                case Identifier identifier:
                    scope.Add(identifier.Name);
                    break;
                case AssignmentPattern assignment:
                    AddBindingNames(assignment.Left, scope);
                    break;
                case RestElement rest:
                    AddBindingNames(rest.Argument, scope);
                    break;
                case ArrayPattern array:
                    foreach (var element in array.Elements)
                    {
                        if (element is not null)
                            AddBindingNames(element, scope);
                    }
                    break;
                case ObjectPattern @object:
                    foreach (var property in @object.Properties)
                    {
                        switch (property)
                        {
                            case ObjectProperty objectProperty:
                                AddBindingNames(objectProperty.Value, scope);
                                break;
                            case AssignmentProperty assignmentProperty:
                                AddBindingNames(assignmentProperty.Value, scope);
                                break;
                            case RestElement restProperty:
                                AddBindingNames(restProperty.Argument, scope);
                                break;
                        }
                    }
                    break;
            }
        }

        private static void AddBindingNamesToVarScope(Node node, Scope scope)
            => AddBindingNames(node, scope.GetVarScope());

        private void VisitBindingSideEffects(Node node)
        {
            switch (node)
            {
                case Identifier:
                    return;
                case AssignmentPattern assignment:
                    VisitBindingSideEffects(assignment.Left);
                    Visit(assignment.Right);
                    return;
                case RestElement rest:
                    VisitBindingSideEffects(rest.Argument);
                    return;
                case ArrayPattern array:
                    foreach (var element in array.Elements)
                    {
                        if (element is not null)
                            VisitBindingSideEffects(element);
                    }
                    return;
                case ObjectPattern @object:
                    foreach (var property in @object.Properties)
                    {
                        switch (property)
                        {
                            case ObjectProperty objectProperty:
                                if (objectProperty.Computed)
                                    Visit(objectProperty.Key);
                                VisitBindingSideEffects(objectProperty.Value);
                                break;
                            case AssignmentProperty assignmentProperty:
                                if (assignmentProperty.Computed)
                                    Visit(assignmentProperty.Key);
                                VisitBindingSideEffects(assignmentProperty.Value);
                                break;
                            case RestElement restProperty:
                                VisitBindingSideEffects(restProperty.Argument);
                                break;
                        }
                    }
                    return;
                default:
                    Visit(node);
                    return;
            }
        }

        private IDisposable PushScope(Scope scope)
        {
            var previous = _current;
            _current = scope;
            return new ScopeLease(() => _current = previous);
        }

        private sealed class Scope(Scope? parent, bool isVarScope = false)
        {
            private readonly Scope? _parent = parent;
            private readonly bool _isVarScope = isVarScope;
            private readonly HashSet<string> _names = new(StringComparer.Ordinal);

            internal void Add(string name) => _names.Add(name);

            internal bool Contains(string name)
                => _names.Contains(name) || (_parent is not null && _parent.Contains(name));

            internal Scope GetVarScope()
                => _isVarScope || _parent is null ? this : _parent.GetVarScope();
        }

        private sealed class ScopeLease(Action release) : IDisposable
        {
            private Action? _release = release;

            public void Dispose()
                => Interlocked.Exchange(ref _release, null)?.Invoke();
        }
    }
}

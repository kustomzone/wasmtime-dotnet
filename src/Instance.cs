using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Wasmtime.Externs;

namespace Wasmtime
{
    /// <summary>
    /// Represents an instantiated WebAssembly module.
    /// </summary>
    public class Instance : DynamicObject, IDisposable, IImportable
    {
        /// <summary>
        /// The exported functions of the instance.
        /// </summary>
        public IReadOnlyList<ExternFunction> Functions => _externs.Functions;

        /// <summary>
        /// The exported globals of the instance.
        /// </summary>
        public IReadOnlyList<ExternGlobal> Globals => _externs.Globals;

        /// <summary>
        /// The exported tables of the instance.
        /// </summary>
        public IReadOnlyList<ExternTable> Tables => _externs.Tables;

        /// <summary>
        /// The exported memories of the instance.
        /// </summary>
        public IReadOnlyList<ExternMemory> Memories => _externs.Memories;

        /// <summary>
        /// The exported instances of the instance.
        /// </summary>
        public IReadOnlyList<ExternInstance> Instances => _externs.Instances;

        /// <summary>
        /// The exported modules of the instance.
        /// </summary>
        public IReadOnlyList<ExternModule> Modules => _externs.Modules;

        /// <inheritdoc/>
        public unsafe void Dispose()
        {
            if (!Handle.IsInvalid)
            {
                Handle.Dispose();
                Handle.SetHandleAsInvalid();
            }

            foreach (var instance in Instances)
            {
                instance.Dispose();
            }

            if (!(_externs is null))
            {
                _externs.Dispose();
            }
        }

        IntPtr IImportable.GetHandle()
        {
            return Interop.wasm_instance_as_extern(Handle.DangerousGetHandle());
        }

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (_globals.TryGetValue(binder.Name, out var global))
            {
                result = global.Value;
                return true;
            }
            result = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_globals.TryGetValue(binder.Name, out var global))
            {
                global.Value = value;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[] args, out object? result)
        {
            if (!_functions.TryGetValue(binder.Name, out var func))
            {
                result = null;
                return false;
            }

            result = func.Invoke(args);
            return true;
        }

        internal Instance(Interop.InstanceHandle handle, IntPtr module)
        {
            Handle = handle;

            if (Handle.IsInvalid)
            {
                throw new WasmtimeException("Failed to create Wasmtime instance.");
            }

            Interop.wasm_exporttype_vec_t exportsVec;
            Interop.wasm_module_exports(module, out exportsVec);

            try
            {
                var exports = new Wasmtime.Exports.Exports(exportsVec);
                _externs = new Wasmtime.Externs.Externs(exports, Handle.DangerousGetHandle());
            }
            finally
            {
                Interop.wasm_exporttype_vec_delete(ref exportsVec);
            }

            _functions = Functions.ToDictionary(f => f.Name);
            _globals = Globals.ToDictionary(g => g.Name);
        }

        internal Interop.InstanceHandle Handle { get; private set; }
        private Externs.Externs _externs;
        private Dictionary<string, ExternFunction> _functions;
        private Dictionary<string, ExternGlobal> _globals;
    }
}

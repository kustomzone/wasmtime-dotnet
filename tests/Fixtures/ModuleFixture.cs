using System;
using System.IO;
using Wasmtime;

namespace Wasmtime.Tests
{
    public abstract class ModuleFixture : IDisposable
    {
        public ModuleFixture()
        {
            Engine = new EngineBuilder()
                .WithMultiValue(true)
                .WithReferenceTypes(true)
                .Build();

            Store = new Store(Engine);

            Module = Store.LoadModuleText(Path.Combine("Modules", ModuleFileName));
        }

        public void Dispose()
        {
            if (!(Module is null))
            {
                Module.Dispose();
                Module = null;
            }

            if (!(Store is null))
            {
                Store.Dispose();
                Store = null;
            }
        }

        public Engine Engine { get; set; }
        public Store Store { get; set; }
        public Module Module { get; set; }

        protected abstract string ModuleFileName { get; }
    }
}

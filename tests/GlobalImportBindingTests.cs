using System;
using FluentAssertions;
using Xunit;

namespace Wasmtime.Tests
{
    public class GlobalImportBindingFixture : ModuleFixture
    {
        protected override string ModuleFileName => "GlobalImportBindings.wat";
    }

    public class GlobalImportBindingTests : IClassFixture<GlobalImportBindingFixture>, IDisposable
    {
        private Host Host { get; set; }

        public GlobalImportBindingTests(GlobalImportBindingFixture fixture)
        {
            Fixture = fixture;
            Host = new Host(Fixture.Engine);
        }

        private GlobalImportBindingFixture Fixture { get; set; }

        [Fact]
        public void ItFailsToInstantiateWithMissingImport()
        {
            Action action = () => { using var instance = Host.Instantiate(Fixture.Module); };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("unknown import: `::global_i32_mut` has not been defined");
        }

        [Fact]
        public void ItFailsToDefineAGlobalWithInvalidType()
        {
            int? i = 0;
            Action action = () => { Host.DefineGlobal("", "global_i32_mut", i); };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Global variables cannot be of type 'System.Nullable`1[System.Int32]'.");
        }

        [Fact]
        public void ItFailsToInstantiateWithGlobalTypeMismatch()
        {
            Host.DefineGlobal("", "global_i32_mut", 0L);
            Action action = () => { using var instance = Host.Instantiate(Fixture.Module); };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("incompatible import type for `::global_i32_mut` specified*");
        }

        [Fact]
        public void ItFailsToInstantiateWhenGlobalIsNotMut()
        {
            Host.DefineGlobal("", "global_i32_mut", 1);
            Action action = () => { using var instance = Host.Instantiate(Fixture.Module); };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("incompatible import type for `::global_i32_mut` specified*");
        }

        [Fact]
        public void ItFailsToInstantiateWhenGlobalIsMut()
        {
            Host.DefineMutableGlobal("", "global_i32_mut", 0);
            Host.DefineMutableGlobal("", "global_i32", 0);
            Action action = () => { using var instance = Host.Instantiate(Fixture.Module); };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("incompatible import type for `::global_i32` specified*");
        }

        [Fact]
        public void ItBindsTheGlobalsCorrectly()
        {
            var global_i32_mut = Host.DefineMutableGlobal("", "global_i32_mut", 0);
            var global_i32 = Host.DefineGlobal("", "global_i32", 1);
            var global_i64_mut = Host.DefineMutableGlobal("", "global_i64_mut", 2L);
            var global_i64 = Host.DefineGlobal("", "global_i64", 3L);
            var global_f32_mut = Host.DefineMutableGlobal("", "global_f32_mut", 4f);
            var global_f32 = Host.DefineGlobal("", "global_f32", 5f);
            var global_f64_mut = Host.DefineMutableGlobal("", "global_f64_mut", 6.0);
            var global_f64 = Host.DefineGlobal("", "global_f64", 7.0);

            using dynamic instance = Host.Instantiate(Fixture.Module);

            global_i32_mut.Value.Should().Be(0);
            ((int)instance.get_global_i32_mut()).Should().Be(0);
            global_i32.Value.Should().Be(1);
            ((int)instance.get_global_i32()).Should().Be(1);
            global_i64_mut.Value.Should().Be(2);
            ((long)instance.get_global_i64_mut()).Should().Be(2);
            global_i64.Value.Should().Be(3);
            ((long)instance.get_global_i64()).Should().Be(3);
            global_f32_mut.Value.Should().Be(4);
            ((float)instance.get_global_f32_mut()).Should().Be(4);
            global_f32.Value.Should().Be(5);
            ((float)instance.get_global_f32()).Should().Be(5);
            global_f64_mut.Value.Should().Be(6);
            ((double)instance.get_global_f64_mut()).Should().Be(6);
            global_f64.Value.Should().Be(7);
            ((double)instance.get_global_f64()).Should().Be(7);

            global_i32_mut.Value = 10;
            global_i32_mut.Value.Should().Be(10);
            ((int)instance.get_global_i32_mut()).Should().Be(10);
            instance.set_global_i32_mut(11);
            global_i32_mut.Value.Should().Be(11);
            ((int)instance.get_global_i32_mut()).Should().Be(11);

            global_i64_mut.Value = 12;
            global_i64_mut.Value.Should().Be(12);
            ((long)instance.get_global_i64_mut()).Should().Be(12);
            instance.set_global_i64_mut(13);
            global_i64_mut.Value.Should().Be(13);
            ((long)instance.get_global_i64_mut()).Should().Be(13);

            global_f32_mut.Value = 14;
            global_f32_mut.Value.Should().Be(14);
            ((float)instance.get_global_f32_mut()).Should().Be(14);
            instance.set_global_f32_mut(15);
            global_f32_mut.Value.Should().Be(15);
            ((float)instance.get_global_f32_mut()).Should().Be(15);

            global_f64_mut.Value = 16;
            global_f64_mut.Value.Should().Be(16);
            ((double)instance.get_global_f64_mut()).Should().Be(16);
            instance.set_global_f64_mut(17);
            global_f64_mut.Value.Should().Be(17);
            ((double)instance.get_global_f64_mut()).Should().Be(17);
        }

        public void Dispose()
        {
            Host.Dispose();
        }
    }
}

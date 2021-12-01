#!/usr/bin/env python
# 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM

from sys import argv, stderr, exit
from wasmer import engine, Module, wat2wasm, Store, Instance
from wasmer_compiler_cranelift import Compiler
from falaklib import make_import_object

def main():
    if len(argv) != 2:
        print('Please specify the name of the input Wat file.', file=stderr)
        exit(1)

    # Create a store
    store = Store(engine.JIT(Compiler))

    # Convert Wat file contents into Wasm binary code
    wat_file_name = argv[1]
    with open(wat_file_name) as wat_file:
        wat_source_code = wat_file.read()
    wasm_bytes = wat2wasm(wat_source_code)

    # Compile the Wasm module
    module = Module(store, wasm_bytes)

    # Obtain functions to be imported from the Wasm module
    import_object = make_import_object(store)

    # Instantiate the module
    instance = Instance(module, import_object)

    # Run start function and return to OS its exit code
    exit(instance.exports.main())

if __name__ == '__main__':
    main()

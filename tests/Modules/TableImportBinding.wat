(module
  (import "" "table" (table $t 10 funcref))
  (func (export "is_null") (param i32) (result i32)
    (table.get $t (local.get 0))
    (ref.is_null)
  )
  (func (export "call") (param i32)
    (call_indirect $t (local.get 0))
  )
  (func (export "grow") (param i32)
    (table.grow $t (ref.null func) (local.get 0))
    (drop)
  )
)

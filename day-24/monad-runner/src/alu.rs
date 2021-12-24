#[allow(unused_assignments)]
#[macro_export]
macro_rules! alu_program {
    () => {};
    (add $a:ident $b:ident $($tail:tt)*) => {
        $a += $b;
        alu_program!($($tail)*);
    };
    (add $a:ident $b:literal $($tail:tt)*) => {
        $a += $b;
        alu_program!($($tail)*);
    };
    (sub $a:ident $b:ident $($tail:tt)*) => {
        $a += $b;
        alu_program!($($tail)*);
    };
    (sub $a:ident $b:literal $($tail:tt)*) => {
        $a -= $b;
        alu_program!($($tail)*);
    };    
    (mul $a:ident $b:ident $($tail:tt)*) => {
        $a *= $b;
        alu_program!($($tail)*);
    };
    (mul $a:ident $b:literal $($tail:tt)*) => {
        $a *= $b;
        alu_program!($($tail)*);
    };
    (inp $a:ident $name:ident $($tail:tt)*) => {
        $a = $name.remove(0) as i64;
        alu_program!($($tail)*);
    };
    (div $a:ident $b:ident $($tail:tt)*) => {
        $a /= $b;
        alu_program!($($tail)*);
    };
    (div $a:ident $b:literal $($tail:tt)*) => {
        $a /= $b;
        alu_program!($($tail)*);
    };
    (mod $a:ident $b:ident $($tail:tt)*) => {
        $a = $a % $b;
        alu_program!($($tail)*);
    };
    (mod $a:ident $b:literal $($tail:tt)*) => {
        $a = $a % $b;
        alu_program!($($tail)*);
    };
    (eql $a:ident $b:ident $($tail:tt)*) => {
        $a = if $a == $b { 1_i64 } else { 0_i64 };
        alu_program!($($tail)*);
    };
    (eql $a:ident $b:literal $($tail:tt)*) => {
        $a = if $a == $b { 1_i64 } else { 0_i64 };
        alu_program!($($tail)*);
    };
}

#[macro_export]
macro_rules! alu_init {
    () => {};
    (.data $d:ident $name:ident $($tail:tt)*) => {
        let mut $name = $d;
        alu_init!($($tail)*);
    };
    (.register $id:ident $($tail:tt)*) => {
        let mut $id: i64 = 0_i64;
        alu_init!($($tail)*);
    };
}

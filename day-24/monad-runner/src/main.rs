#![recursion_limit = "512"]
#![allow(unused_assignments)]

mod alu;

fn main() {
    let input = std::io::stdin();
    let mut buffer = String::new();
    input.read_line(&mut buffer).expect("put your input on the stdin!");
    let input = buffer
        .trim()
        .chars()
        .map(|c| c.to_digit(10).unwrap() as u8)
        .collect::<Vec<_>>();

    let result = run_monad(input);
    println!("{}", result);
}

fn run_monad(input_data: Vec<u8>) -> bool {
    alu_init! {
        .data input_data data
        .register x
        .register y    
        .register z
        .register w  
    }
    
    alu_program! {
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 11
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 6
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 11
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 12
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 15
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 8
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -11
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 7
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 15
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 7
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 15
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 12
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 14
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 2
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -7
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 15
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 1
        add x 12
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 4
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -6
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 5
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -10
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 12
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -15
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 11
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x -9
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 13
        mul y x
        add z y
        inp w data
        mul x 0
        add x z
        mod x 26
        div z 26
        add x 0
        eql x w
        eql x 0
        mul y 0
        add y 25
        mul y x
        add y 1
        mul z y
        mul y 0
        add y w
        add y 7
        mul y x
        add z y
    }
    z == 0
}
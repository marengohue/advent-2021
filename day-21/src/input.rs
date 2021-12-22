pub fn read_input() -> (u8, u8) {
    let stdin = std::io::stdin();
    let mut in_str = String::new();
    stdin.read_line(&mut in_str).expect("Oh no!");
    let p1 = in_str.split(": ").last().unwrap().trim().parse::<u8>().unwrap();
    stdin.read_line(&mut in_str).expect("Oh no 2!");
    let p2 = in_str.split(": ").last().unwrap().trim().parse::<u8>().unwrap();

    (p1, p2)
}

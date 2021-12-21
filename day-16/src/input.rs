use hex::FromHex;

pub fn read_input() -> Vec<u8> {
    let stdin = std::io::stdin();
    let mut hex_str = String::new();

    stdin
        .read_line(&mut hex_str)
        .expect("Couldnt read data");

    let mut trimmed_hex_str = hex_str.trim().to_string();

    if trimmed_hex_str.len() % 2 == 1 {
        // Make sure input string is even-size.
        trimmed_hex_str.insert(0, '0');
    }

    Vec::from_hex(trimmed_hex_str).unwrap()
}

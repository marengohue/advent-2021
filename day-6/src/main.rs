use std::io::{stdin, BufRead};

fn fish(time_till_spawn: u8, days: u32) -> u64 {
    if time_till_spawn > 0 {
        fish(0, days - time_till_spawn as u32)
    } else {
        let mut fishes: Vec<u64> = vec![0;9];
        fishes[0] = 1;
        
        for _ in 0..days {
            let mut to_spawn: u64 = 0;
            for age in 0..8 {
                if age == 0 { to_spawn = fishes[0]; }
                if age == 8 { break; }
                fishes.swap(age, age + 1);
            }

            fishes[6] += to_spawn;
            fishes[8] = to_spawn;
        }

        fishes.iter().sum()
    }
}

    
fn main() {
    let stdin = stdin();
    let mut handle = stdin.lock();
    let mut str_in = String::new();
    handle.read_line(&mut str_in).expect("oh uh...");

    let total: u64 = str_in
        .split(',')
        .map(|num| num.trim().parse::<u8>().unwrap())
        .map(|f| fish(f, 256))
        .sum();

    println!("{:?}", total);
}

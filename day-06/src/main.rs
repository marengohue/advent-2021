use std::io::{stdin, BufRead};

const MAX_FISH_AGE: usize = 8;
const AGE_AFTER_SPAWN: usize = 6;

// I've done nothing but spin fish for three days.
fn spin_fish(fish: Vec<u8>, days: u32) -> u64 {    
    let mut fish_ring: Vec<u64> = vec![0; MAX_FISH_AGE + 1];
    fish.iter()
        .for_each(|&fish| fish_ring[fish as usize] += 1);
        
    for _ in 0..days {
        let to_spawn = fish_ring[0];
        fish_ring.rotate_left(1);
        fish_ring[MAX_FISH_AGE] = to_spawn;
        fish_ring[AGE_AFTER_SPAWN] += to_spawn;
    }

    fish_ring.iter().sum()
}

    
fn main() {
    let stdin = stdin();
    let mut handle = stdin.lock();
    let mut str_in = String::new();
    handle.read_line(&mut str_in).expect("oh uh...");

    let fishes = str_in
        .split(',')
        .map(|num| num.trim().parse::<u8>().unwrap())
        .collect::<Vec<u8>>();
        
    println!("Total fish: {:?}", spin_fish(fishes, 256));
}

use std::io::{BufRead, stdin};

fn main() {
    let mut last_sum = u32::MAX;
    let mut count: u32 = 0;
    let stdin = stdin();
    let handle = stdin.lock();
    let mut window = Vec::<u32>::new();
    let window_size = 3;
    
    for line in handle.lines() {
        let next_depth = line.unwrap().trim().parse::<u32>().unwrap();
        window.push(next_depth);
        if window.len() == window_size {
            // Required window size reached - can start measuring
            let current_sum: u32 = window.iter().sum();
            if current_sum > last_sum {
                count += 1;
            }
            last_sum = current_sum;
            window.remove(0);
        }
    }

    println!("{}", count);
}

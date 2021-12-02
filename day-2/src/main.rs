use std::io::{BufRead, stdin};

pub struct Submarine {
    pos: (i32, i32),
    aim: i32,
}

impl Submarine {
    pub fn new() -> Self {
        Submarine {
            pos: (0, 0),
            aim: 0
        }
    }
    
    pub fn forward(&mut self, by: i32) {
        let (x, y) = self.pos;
        self.pos = (
            x + by,
            y + self.aim * by
        )
    }

    pub fn steer(&mut self, by: i32) {
        self.aim += by;
    }
}

fn main() {
    let stdin = stdin();
    let handle = stdin.lock();
    let mut sub = Submarine::new();

    for maybe_line in handle.lines() {
        let line = maybe_line.unwrap();
        let parts = line.split(' ').collect::<Vec<_>>();

        let &cmd = parts.first().unwrap();
        let distance = parts.last().unwrap().parse::<i32>().unwrap();
        
        match cmd {
            "forward" => sub.forward(distance),
            "up" => sub.steer(-distance),
            "down" => sub.steer(distance),
            _ => panic!("???")
        };
    }

    println!("{}", sub.pos.0 * sub.pos.1);
}

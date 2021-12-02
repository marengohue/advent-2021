use std::io::{BufRead, stdin};

pub enum SubOrderMode {
    Polar,
    Eucledian
}

pub struct Submarine {
    mode: SubOrderMode,
    pos: (i32, i32),
    aim: i32,
}

impl Submarine {
    pub fn new(mode: SubOrderMode) -> Self {
        Submarine {
            pos: (0, 0),
            aim: 0,
            mode
        }
    }
    
    pub fn forward(&mut self, by: i32) {
        match self.mode {
            SubOrderMode::Polar => {
                self.pos.0 += by;
                self.pos.1 += self.aim * by;
            },
            SubOrderMode::Eucledian => {
                self.pos.0 += by;
            }
        }
    }

    pub fn steer(&mut self, by: i32) {
        match self.mode {
            SubOrderMode::Polar => {
                self.aim += by;
            },
            SubOrderMode::Eucledian => {
                self.pos.1 += by;
            }
        }

    }
}

fn main() {
    let stdin = stdin();
    let handle = stdin.lock();
    let mut sub = Submarine::new(SubOrderMode::Eucledian);

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

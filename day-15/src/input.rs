use std::{
    io::{stdin, BufRead},
    collections::HashMap
};
use crate::{Board, Point};

pub fn read_input() -> Board {
    let stdin = stdin();
    let handle = stdin.lock();

    let map = handle
        .lines()
        .enumerate()
        .flat_map(|(y, maybe_line)| {
            let line = maybe_line.unwrap();
            line
                .chars()
                .enumerate()
                .map(|(x, c)| {
                    (Point(x as i32, y as i32), char::to_digit(c, 10).unwrap() as u32)
                })
                .collect::<Vec<_>>()
        })
        .collect::<HashMap<Point, u32>>();
        
    
    Board {
        map
    }
}

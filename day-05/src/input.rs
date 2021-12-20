use std::io::{stdin, BufRead};

use crate::{Point, Line};

fn parse_point(string_point: &str) -> Point {
    let vals = string_point
        .split(',')
        .map(|str_number| str_number.trim().parse::<i32>().unwrap())
        .collect::<Vec<_>>();

    (vals[0], vals[1])
}

pub fn read_inputs() -> Vec<Line> {
    let stdin = stdin();
    let handle = stdin.lock();
    handle
        .lines()
        .map(|maybe_line| {
            let line = maybe_line.unwrap();
            line.split(" -> ")
                .map(parse_point)
                .collect::<Vec<_>>()
        })
        .map(|points| {
            Line::new(points[0], points[1])
        })
        .collect::<Vec<_>>()

}

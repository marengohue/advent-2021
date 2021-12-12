use std::{collections::HashMap, io::{BufRead, stdin}};
use crate::{Point, Board};

pub fn read_input() -> Board {
    let stdin = stdin();
    let handle = stdin.lock();

    handle
        .lines()
        .into_iter()
        .filter_map(|it| {
            match it {
                Ok(line) => Some(line),
                _ => None
            }
        })
        .enumerate()
        .flat_map(move |(line_no, line)| {
            line
                .trim()
                .chars()
                .enumerate()
                .map(move |(col_no, next_char)| {
                    (
                        (col_no as i8, line_no as i8) as Point,
                        next_char.to_digit(10).unwrap() as u8
                    )
                })
                .collect::<Vec<_>>()
        })
        .collect::<HashMap<Point, u8>>()
}

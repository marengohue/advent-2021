use std::{collections::HashMap, io::{BufRead, Lines, StdinLock, stdin}};
use itertools::{Chunk, Itertools};
use crate::{Bingo, Point, Cell};

fn read_draws(stdin: &std::io::Stdin) -> Vec<u32> {
    let mut handle = stdin.lock();
    let mut line = String::new();
    handle.read_line(&mut line).unwrap();

    let draws = line
        .split(',')
        .map(|str_number| {
            str_number.trim().parse::<u32>().unwrap()
        })
        .collect::<Vec<_>>();

    draws
}

fn read_board_line(line: String, row_no: usize) -> Vec<(Point, Cell)>{
    line
        .split(' ')
        .filter(|&it| !it.is_empty())
        .enumerate()
        .map(move |(col_no, substr)| {
            let key = (row_no, col_no) as Point;
            let cell = (substr.trim().parse::<u32>().unwrap(), false) as Cell;
            (key, cell)
        })
        .collect::<Vec<_>>()
}

fn read_board(board_lines: Chunk<Lines<StdinLock>>) -> Bingo {
    let board_map = board_lines
        .skip(1) // Skip one empty line before every board
        .enumerate()
        .flat_map(move |(row_no, maybe_line)| {
            let line = maybe_line.unwrap();
            read_board_line(line, row_no)
        })
        .collect::<HashMap<Point, Cell>>();

    Bingo::new(board_map)
}

fn read_boards(stdin: &std::io::Stdin) -> Vec<Bingo> {
    let handle = stdin.lock();
    let lines = handle.lines();

    lines.into_iter()
        .chunks(6) // 1 empty line + 5 rows per board
        .into_iter()
        .map(read_board)
        .collect::<Vec<Bingo>>()
}

pub fn read_inputs() -> (Vec<u32>, Vec<Bingo>) {
    let stdin = stdin();
    let draws = read_draws(&stdin);
    let bingos = read_boards(&stdin);

    (draws, bingos)
}

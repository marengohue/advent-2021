use std::collections::HashMap;

mod input;

const FLASH_AT: u8 = 10;
const MIN_STEPS: usize = 100;
const BOUND: i8 = 10;

pub type Point = (i8, i8);
pub type Board = HashMap<Point, u8>;

fn adjacent(&(x, y): &Point) -> Vec<Point> {
    vec![
        (x - 1, y - 1),
        (x,     y - 1),
        (x + 1, y - 1),
        (x - 1, y    ),
        (x + 1, y    ),
        (x - 1, y + 1),
        (x    , y + 1),
        (x + 1, y + 1)
    ]
}

fn power_up(board: &mut Board, point: &Point) {
    if let Some(cell) = board.get_mut(point) {
        *cell += 1;
        if *cell == FLASH_AT {
            for adjacent_point in adjacent(point) {
                power_up(board, &adjacent_point);
            }
        }
    }
}

fn step(board: &mut Board) -> u32 {
    for row in 0..BOUND {
        for col in 0..BOUND {
            power_up(board, &(col, row));
        }
    }

    let mut flashes: u32 = 0;
    for row in 0..BOUND {
        for col in 0..BOUND {
            board.entry((col, row)).and_modify(|it| {
                if *it >= FLASH_AT {
                    *it = 0;
                    flashes += 1;
                }
            });
        }
    }

    flashes
}

fn main() {
    let mut board = input::read_input();
    let total_flashes = (0..)
        .map(|_| {
            step(&mut board)
        })
        .enumerate()
        .take_while(|&(step_no, flash_count)| {
            flash_count < (BOUND as u32 * BOUND as u32)
                ||
            step_no < MIN_STEPS
        })
        .scan(0_u32, |state, (_, flash_count)| {
            *state += flash_count;
            Some(*state)
        })
        .collect::<Vec<_>>();

    let sum_at_minsteps = total_flashes[MIN_STEPS - 1];
    let first_full_flash = total_flashes.len() + 1;
    
    println!("{}, {}", sum_at_minsteps, first_full_flash);
}

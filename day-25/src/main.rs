#![feature(hash_drain_filter)]

use std::collections::HashSet;

mod input;

#[derive(Eq, PartialEq, Hash, Debug, Copy, Clone)]
pub struct Point(u8, u8);

pub enum MoveType { East, South }

pub fn left_of(p: &Point, bounds: &Point) -> Point {
    let Point(x, y) = *p;
    if x == 0 {
        Point(bounds.0, y)
    } else {
        Point(x - 1, y)
    }
}

pub fn right_of(p: &Point, bounds: &Point) -> Point {
    let Point(x, y) = *p;
    if x == bounds.0 {
        Point(0, y)
    } else {
        Point(x + 1, y)
    }
}

pub fn top_of(p: &Point, bounds: &Point) -> Point {
    let Point(x, y) = *p;
    if y == 0 {
        Point(x, bounds.1)
    } else {
        Point(x, y - 1)
    }
}

pub fn bottom_of(p: &Point, bounds: &Point) -> Point {
    let Point(x, y) = *p;
    if y == bounds.1 {
        Point(x, 0)
    } else {
        Point(x, y + 1)
    }
}

#[derive(Debug)]
pub struct BoardState {
    pub south_facing: HashSet<Point>,
    pub east_facing: HashSet<Point>,
    pub free_spots: HashSet<Point>,

    pub bounds: Point
}

fn move_once(board: &mut BoardState, move_type: MoveType) -> usize {
    let mut free_spot_updates = Vec::<(Option<Point>, Point)>::new();

    let (move_coll, other_coll) = match move_type {
        MoveType::East => (&mut board.east_facing, &board.south_facing),
        MoveType::South => (&mut board.south_facing, &board.east_facing)
    };
    
    for &free_spot in board.free_spots.iter() {
        let move_from = match move_type {
            MoveType::East => left_of(&free_spot, &board.bounds),
            MoveType::South => top_of(&free_spot, &board.bounds)
        };
        
        if move_coll.contains(&move_from) {
            free_spot_updates.push((Some(free_spot), move_from));
            let new_free_spot = match move_type {
                MoveType::East => right_of(&free_spot, &board.bounds),
                MoveType::South => bottom_of(&free_spot, &board.bounds)
            };
            if !move_coll.contains(&new_free_spot) && !other_coll.contains(&new_free_spot) {
                free_spot_updates.push((None, new_free_spot));
            }

        }
    }

    for (to_remove, to_add) in &free_spot_updates {
        if let Some(to_remove) = &to_remove {
            board.free_spots.remove(to_remove);
            move_coll.insert(*to_remove);
            move_coll.remove(to_add);
        }
        board.free_spots.insert(*to_add);
    }
    
    free_spot_updates.len()
}

fn turn(mut board_state: &mut BoardState) -> usize {
    let move_count_fst = move_once(&mut board_state, MoveType::East);
    let move_count_snd = move_once(&mut board_state, MoveType::South);
    move_count_fst + move_count_snd
}

fn main() {

    let mut board_state = input::read_input();
    println!("Initial state: ");

    let mut turn_count: u64 = 0;
    loop {
        turn_count += 1;
        let count = turn(&mut board_state);
        if count == 0 {
            println!("stopped at: {}", turn_count);
            break
        }
    }
}

mod input;
use std::collections::HashMap;

pub type Point = (usize, usize);

pub type Cell = (u32, bool);

#[derive(Debug)]
pub struct Bingo {
    board: HashMap<Point, Cell>,
    state: GameState,
    place: Option<usize>
}

#[derive(Debug, PartialEq, Clone, Copy)]
pub enum GameState {
    Win(u32),
    Playing
}

impl Bingo {
    fn new(board: HashMap<Point, Cell>) -> Self {
        Bingo {
            board,
            place: None,
            state: GameState::Playing
        }
    }

    fn is_playing(&self) -> bool {
        self.state == GameState::Playing
    }
    
    fn find(&self, val: u32) -> Option<Point> {
        let found = self.board
            .iter()
            .find(|&(_, &(val_in_cell, _))| {
                val_in_cell == val
            });
        found.map(|(&pos, _)| pos)
    }

    fn get_unmarked_score(&self) -> u32 {
        self.board.iter()
            .filter_map(|(_, &(val, marked))| {
                if !marked { Some(val) } else { None }
            })
            .sum()
    }

    fn get_turn_result(&self, last_marked: Point) -> GameState {
        // Either row or column is fully marked.
        // Looks wonky to take advantage of bool logic short circuits
        // and not allocate extra vectors without need.
        let reached_win = self.row(last_marked.0)
            .iter().fold(true, |acc, (_, marked)| acc & marked)
            ||
            self.col(last_marked.1).iter()
            .fold(true, |acc, (_, marked)| acc & marked);

        if reached_win {
            let &(last_marked_score, _) = self.board.get(&last_marked).unwrap();
            GameState::Win(self.get_unmarked_score() * last_marked_score)
        } else {
            GameState::Playing
        }
    }

    fn mark_at(&mut self, pos: Point) -> GameState {
        self.board.entry(pos).and_modify(|(_, marked)| {
            *marked = true;
        });

        self.get_turn_result(pos)
    }

    fn mark_num(&mut self, num: u32) -> GameState {
        let result = if let Some(pos) = self.find(num) {
            self.mark_at(pos)
        } else {
            GameState::Playing
        };
        self.state = result;
        self.state
    }

    fn row(&self, row_no: usize) -> Vec<(u32, bool)> {
        self.board.iter()
            .filter_map(|(&(row, _), &(val, marked))| {
                if row == row_no { Some((val, marked)) } else { None }
            })
            .collect::<Vec<_>>()
    }

    fn col(&self, col_no: usize) -> Vec<(u32, bool)> {
        self.board.iter()
            .filter_map(|(&(_, col), &(val, marked))| {
                if col == col_no { Some((val, marked)) } else { None }
            })
            .collect::<Vec<_>>()
    }
}

fn main() {
    let (draws, mut bingos) = input::read_inputs();
    let mut place = 0_usize;
    for draw in draws {
        for bingo in bingos.iter_mut().filter(|b| b.is_playing()) {
            if let GameState::Win(_) = bingo.mark_num(draw) {
                bingo.place = Some(place);
                place += 1;
            }
        }
    }

    let mut result = bingos.iter()
        .map(|b| (b.place, b.state))
        .collect::<Vec<_>>();
    
    result.sort_unstable_by(|&(p0, _), &(p1, _)| {
        p0.unwrap_or(usize::MAX).partial_cmp(&p1.unwrap_or(usize::MAX)).unwrap()
    });
    
    println!("{:?}", result);
}
    

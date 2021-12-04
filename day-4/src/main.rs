use itertools::Itertools;
use std::collections::hash_map::HashMap;
use std::io::{BufRead, stdin};

type Point = (usize, usize);
type Cell = (u32, bool);

#[derive(Debug)]
struct Bingo {
    map: HashMap<Point, Cell>,
    state: TurnResult,
    place: Option<usize>
}

#[derive(Debug, Clone, Copy)]
enum TurnResult {
    Win(u32),
    Playing
}

impl Bingo {
    fn new(map: HashMap<Point, Cell>) -> Self {
        Bingo {
            map,
            place: None,
            state: TurnResult::Playing
        }
    }

    fn is_playing(&self) -> bool {
        if let TurnResult::Playing = self.state {
            true
        } else {
            false
        }
    }
    
    fn find(&self, val: u32) -> Option<Point> {
        let found = self.map
            .iter()
            .find(|&(_, &(val_in_cell, _))| {
                val_in_cell == val
            });
        found.map(|(&pos, _)| pos)
    }

    fn get_unmarked_score(&self) -> u32 {
        self.map.iter()
            .filter_map(|(_, &(val, marked))| {
                if !marked { Some(val) } else { None }
            })
            .sum()
    }

    fn get_turn_result(&self, last_marked: Point) -> TurnResult {
        // Either row or column is fully marked.
        // Looks wonky to take advantage of bool logic short circuits
        // and not allocate extra vectors without need.
        let reached_win = self.row(last_marked.0)
            .iter().fold(true, |acc, (_, marked)| acc & marked)
            ||
            self.col(last_marked.1).iter()
            .fold(true, |acc, (_, marked)| acc & marked);

        if reached_win {
            let &(last_marked_score, _) = self.map.get(&last_marked).unwrap();
            TurnResult::Win(self.get_unmarked_score() * last_marked_score)
        } else {
            TurnResult::Playing
        }
    }

    fn mark_at(&mut self, pos: Point) -> TurnResult {
        self.map.entry(pos).and_modify(|(_, marked)| {
            *marked = true;
        });

        self.get_turn_result(pos)
    }

    fn mark_num(&mut self, num: u32) -> TurnResult {
        let result = if let Some(pos) = self.find(num) {
            self.mark_at(pos)
        } else {
            TurnResult::Playing
        };
        self.state = result;
        self.state
    }

    fn row(&self, row_no: usize) -> Vec<(u32, bool)> {
        self.map.iter()
            .filter_map(|(&(row, _), &(val, marked))| {
                if row == row_no { Some((val, marked)) } else { None }
            })
            .collect::<Vec<_>>()
    }

    fn col(&self, col_no: usize) -> Vec<(u32, bool)> {
        self.map.iter()
            .filter_map(|(&(_, col), &(val, marked))| {
                if col == col_no { Some((val, marked)) } else { None }
            })
            .collect::<Vec<_>>()
    }
}

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

fn read_inputs() -> (Vec<u32>, Vec<Bingo>) {
    let stdin = stdin();
    let draws = read_draws(&stdin);
    let handle = stdin.lock();
    let lines = handle.lines();
    
    let mut bingos = Vec::<Bingo>::new();    
    for chunk in &lines.into_iter().chunks(6) {
        let map = chunk
            .skip(1)
            .enumerate()
            .flat_map(move |(row_no, maybe_line)| {
                let line = maybe_line.unwrap();
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
            })
            .collect::<HashMap<Point, Cell>>();

        bingos.push(Bingo::new(map));
    }

    (draws, bingos)
}

fn main() {
    let (draws, mut bingos) = read_inputs();
    let mut place = 0_usize;
    
    for draw in draws {
        for bingo in bingos.iter_mut().filter(|b|b.is_playing()) {
            if let TurnResult::Win(_) = bingo.mark_num(draw) {
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
    

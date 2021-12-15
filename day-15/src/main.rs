use std::{collections::HashMap};
use itertools::Itertools;
use pathfinding::prelude::dijkstra;
mod input;

#[derive(Clone, Copy, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct Point(i32, i32);

impl Point {
    pub fn manhattan_to_origin(&self) -> u32 {
        self.0.abs() as u32 + self.1.abs() as u32
    }
}

pub struct Board {
    map: HashMap<Point, u32>
}

fn adjacent(p: Point) -> Vec<Point> {
    let Point(x, y) = p;
    vec![
        Point(x - 1, y),
        Point(x + 1, y),
        Point(x, y - 1),
        Point(x, y + 1)
    ]
}

impl Board {
    pub fn bounds(&self) -> (Point, Point) {
        self.map
            .keys()
            .fold((Point(0, 0), Point(0, 0)),
                |(min, max), val| {
                    (
                        Point(min.0.min(val.0), min.1.min(val.1)),
                        Point(max.0.max(val.0), max.1.max(val.1))
                    )
                })
    }
    
    pub fn find_shortest(&self, from: Point, to: Point) -> u64 {
        let result = dijkstra(
            &from,
            |&p| {
                adjacent(p)
                    .iter()
                    .filter_map(|potential| {
                        self.map
                            .get(potential)
                            .map(|&risk| (*potential, risk as u64))
                    })
                    .collect::<Vec<_>>()
            },
            |&p| p == to
        );

        result.unwrap().1
    }

    pub fn enlarge(self, factor: i32) -> Self {
        let (_, Point(max_x, max_y)) = self.bounds();
        
        let enlarged_map = self.map
            .iter()
            .flat_map(|(&Point(x, y), &risk)| {
                (0..factor)
                    .cartesian_product(0..factor)
                    .map(move |(scale_x, scale_y)| {
                        let new_x = x + scale_x * (max_x + 1);
                        let new_y = y + scale_y * (max_y + 1);
                        let sector_manhattan = Point(scale_x, scale_y);
                        let new_risk = risk + sector_manhattan.manhattan_to_origin();
                        let wrapped_risk = if new_risk > 9 { new_risk - 9 } else { new_risk };
                        
                        (Point(new_x, new_y), wrapped_risk)
                    })
            })
            .collect::<HashMap<Point, u32>>();
        
        Self { map: enlarged_map }
    }
}

fn main() {
    let board = input::read_input().enlarge(5);
    let (min, max) = board.bounds();
    println!("{}", board.find_shortest(min, max));
}

use std::{collections::HashSet, io::BufRead};

use crate::{BoardState, Point, top_of, left_of};


fn calculate_bounds(south_facing: &HashSet<Point>, east_facing: &HashSet<Point>, free_spots: &HashSet<Point>) -> Point {
    let max_x = south_facing.iter()
        .chain(east_facing.iter())
        .chain(free_spots.iter())
        .map(|Point(x, _)| *x)
        .max()
        .unwrap();

    let max_y = south_facing.iter()
        .chain(east_facing.iter())
        .chain(free_spots.iter())
        .map(|Point(_, y)| *y)
        .max()
        .unwrap();
    
    Point(max_x, max_y)
}
    
pub fn read_input() -> BoardState {
    let stdin = std::io::stdin();
    let handle = stdin.lock();

    let mut south_facing = HashSet::<Point>::new();
    let mut east_facing = HashSet::<Point>::new();
    let mut free_spots = HashSet::<Point>::new();
    
    handle
        .lines()
        .enumerate()
        .for_each(|(y, maybe_line)| {
            let line = maybe_line.unwrap();
            line
                .trim()
                .chars()
                .enumerate()
                .for_each(|(x, c)| {
                    let point = Point(x as u8, y as u8);
                    if c == 'v' {
                        south_facing.insert(point);
                    } else if c =='>' {
                        east_facing.insert(point);
                    } else {
                        free_spots.insert(point);
                    }
                });
        });

    let bounds = calculate_bounds(&south_facing, &east_facing, &free_spots);
    
    // Remove free spots which dont
    // have cucumbers coming into them
    // Since we don't need to look at them
    free_spots.drain_filter(|p| {
        let top = top_of(p, &bounds);
        let left = left_of(p, &bounds);
        
        !south_facing.contains(&top) && !east_facing.contains(&left)
    });


    BoardState {
        south_facing,
        east_facing,
        free_spots,
        bounds
    }
}
